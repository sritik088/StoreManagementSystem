using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Application.Services
{
    public class PurchaseOrderService
        : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _repository;

        public PurchaseOrderService(
            IPurchaseOrderRepository repository)
        {
            _repository = repository;
        }


        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<IEnumerable<PurchaseOrder>>
            GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }


        // =====================================================
        // GET BY ID
        // =====================================================

        public async Task<PurchaseOrder?>
            GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }


        // =====================================================
        // CREATE
        // =====================================================

        public async Task<bool>
            CreateAsync(
                PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder.SupplierId <= 0)
                return false;

            if (purchaseOrder.Items == null ||
                !purchaseOrder.Items.Any())
                return false;

            purchaseOrder.PONumber =
                await _repository.GeneratePONumberAsync();

            purchaseOrder.OrderDate =
                DateTime.Now;

            purchaseOrder.Status =
                PurchaseOrderStatus.Draft;

            purchaseOrder.CreatedDate =
                DateTime.Now;

            purchaseOrder.IsDeleted =
                false;

            decimal subTotal = 0;

            decimal tax = 0;

            decimal discount = 0;

            foreach (var item in purchaseOrder.Items)
            {
                item.Total =
                    (item.Quantity *
                     item.UnitPrice)
                    - item.Discount
                    + item.TaxAmount;

                subTotal +=
                    item.Quantity *
                    item.UnitPrice;

                tax +=
                    item.TaxAmount;

                discount +=
                    item.Discount;
            }

            purchaseOrder.SubTotal =
                subTotal;

            purchaseOrder.TaxAmount =
                tax;

            purchaseOrder.Discount =
                discount;

            purchaseOrder.GrandTotal =
                subTotal +
                tax -
                discount;

            await _repository
                .AddAsync(purchaseOrder);

            return true;
        }


        // =====================================================
        // UPDATE
        // =====================================================

        public async Task<bool>
            UpdateAsync(
                PurchaseOrder purchaseOrder)
        {
            var existing =
                await _repository
                    .GetByIdAsync(
                        purchaseOrder.Id);

            if (existing == null)
                return false;

            if (existing.Status !=
                PurchaseOrderStatus.Draft)
                return false;

            existing.SupplierId =
                purchaseOrder.SupplierId;

            existing.ExpectedDate =
                purchaseOrder.ExpectedDate;

            existing.Remarks =
                purchaseOrder.Remarks;

            existing.Items =
                purchaseOrder.Items;

            decimal subTotal = 0;

            decimal tax = 0;

            decimal discount = 0;

            foreach (var item in existing.Items)
            {
                item.Total =
                    (item.Quantity *
                     item.UnitPrice)
                    - item.Discount
                    + item.TaxAmount;

                subTotal +=
                    item.Quantity *
                    item.UnitPrice;

                tax +=
                    item.TaxAmount;

                discount +=
                    item.Discount;
            }

            existing.SubTotal =
                subTotal;

            existing.TaxAmount =
                tax;

            existing.Discount =
                discount;

            existing.GrandTotal =
                subTotal +
                tax -
                discount;

            existing.UpdatedDate =
                DateTime.Now;

            await _repository
                .UpdateAsync(existing);

            return true;
        }


        // =====================================================
        // DELETE
        // =====================================================
        //
        // RULE:
        //
        // 1. PO must exist.
        // 2. PO must not already be deleted.
        // 3. PO must be Draft.
        // 4. PO must NOT have an active GRN.
        //
        // If GRN exists:
        //      deletion is blocked.
        //
        // User must reverse/delete the GRN first.
        //
        // =====================================================

        public async Task<bool>
            DeleteAsync(int id)
        {
            // -------------------------------------------------
            // LOAD ACTIVE PO
            // -------------------------------------------------

            var po =
                await _repository
                    .GetByIdAsync(id);

            if (po == null)
                return false;


            // -------------------------------------------------
            // ONLY DRAFT PO CAN BE DELETED
            // -------------------------------------------------

            if (po.Status !=
                PurchaseOrderStatus.Draft)
            {
                return false;
            }


            // -------------------------------------------------
            // CHECK ACTIVE GRN
            // -------------------------------------------------

            var hasActiveGRN =
                await _repository
                    .HasActiveGoodsReceiptsAsync(id);

            if (hasActiveGRN)
            {
                // GRN exists.
                //
                // DO NOT delete the PO.
                //
                // User must first reverse/delete
                // the Goods Receipt.
                //
                return false;
            }


            // -------------------------------------------------
            // DELETE PO
            // -------------------------------------------------

            await _repository
                .DeleteAsync(id);

            return true;
        }


        // =====================================================
        // APPROVE
        // =====================================================

        public async Task<bool>
            ApproveAsync(int id)
        {
            var po =
                await _repository
                    .GetByIdAsync(id);

            if (po == null)
                return false;

            if (po.Status !=
                PurchaseOrderStatus.Draft)
                return false;

            await _repository
                .ApproveAsync(id);

            return true;
        }


        // =====================================================
        // GET LATEST UNIT PRICE
        // =====================================================

        public async Task<decimal>
            GetLatestUnitPriceAsync(
                int productId)
        {
            if (productId <= 0)
                return 0m;

            return await _repository
                .GetLatestUnitPriceAsync(
                    productId);
        }


        // =====================================================
        // CANCEL
        // =====================================================

        public async Task<bool>
            CancelAsync(int id)
        {
            var po =
                await _repository
                    .GetByIdAsync(id);

            if (po == null)
                return false;

            if (po.Status ==
                PurchaseOrderStatus.Received)
                return false;

            await _repository
                .CancelAsync(id);

            return true;
        }

        // =====================================================
        // RESTORE
        // =====================================================

        public async Task<bool>
            RestoreAsync(int id)
        {
            var po =
                await _repository
                    .GetByIdAsync(id);

            if (po == null)
                return false;

            // -------------------------------------------------
            // ONLY DELETED PO CAN BE RESTORED
            // -------------------------------------------------

            if (!po.IsDeleted)
                return false;

            await _repository
                .RestoreAsync(id);

            return true;
        }


        // =====================================================
        // CHECK ACTIVE GRN
        // =====================================================

        public async Task<bool>
            HasActiveGoodsReceiptsAsync(
                int purchaseOrderId)
        {
            if (purchaseOrderId <= 0)
                return false;

            return await _repository
                .HasActiveGoodsReceiptsAsync(
                    purchaseOrderId);
        }
    }
}