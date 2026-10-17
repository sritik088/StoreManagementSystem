
using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Application.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
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

        public async Task<IEnumerable<PurchaseOrder>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        // =====================================================
        // GET BY ID
        // =====================================================

        public async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _repository.GetByIdAsync(id);
        }

        // =====================================================
        // VALIDATE PURCHASE ORDER
        // =====================================================

        private static bool IsValidPurchaseOrder(
            PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null)
                return false;

            if (purchaseOrder.SupplierId <= 0)
                return false;

            if (purchaseOrder.Items == null ||
                !purchaseOrder.Items.Any())
            {
                return false;
            }

            foreach (var item in purchaseOrder.Items)
            {
                if (item.ProductId <= 0 ||
                    item.Quantity <= 0 ||
                    item.UnitPrice < 0 ||
                    item.Discount < 0 ||
                    item.TaxAmount < 0)
                {
                    return false;
                }

                decimal grossAmount =
                    item.Quantity * item.UnitPrice;

                if (item.Discount > grossAmount)
                    return false;
            }

            // A single product should appear only once
            // in a purchase order.
            if (purchaseOrder.Items
                .GroupBy(x => x.ProductId)
                .Any(group => group.Count() > 1))
            {
                return false;
            }

            return true;
        }

        // =====================================================
        // CALCULATE PURCHASE ORDER TOTALS
        // =====================================================

        private static void CalculateTotals(
            PurchaseOrder purchaseOrder)
        {
            decimal subTotal = 0m;
            decimal tax = 0m;
            decimal discount = 0m;

            foreach (var item in purchaseOrder.Items)
            {
                decimal grossAmount =
                    item.Quantity * item.UnitPrice;

                item.Total =
                    grossAmount
                    - item.Discount
                    + item.TaxAmount;

                subTotal += grossAmount;
                tax += item.TaxAmount;
                discount += item.Discount;
            }

            purchaseOrder.SubTotal = subTotal;
            purchaseOrder.TaxAmount = tax;
            purchaseOrder.Discount = discount;

            purchaseOrder.GrandTotal =
                subTotal + tax - discount;
        }

        // =====================================================
        // CREATE
        // =====================================================

        public async Task<bool> CreateAsync(
            PurchaseOrder purchaseOrder)
        {
            if (!IsValidPurchaseOrder(purchaseOrder))
                return false;

            DateTime now = DateTime.Now;

            purchaseOrder.PONumber =
                await _repository.GeneratePONumberAsync();

            purchaseOrder.OrderDate = now;
            purchaseOrder.Status = PurchaseOrderStatus.Draft;
            purchaseOrder.CreatedDate = now;
            purchaseOrder.UpdatedDate = null;
            purchaseOrder.IsDeleted = false;

            foreach (var item in purchaseOrder.Items)
            {
                item.Id = 0;
                item.PurchaseOrderId = 0;
            }

            CalculateTotals(purchaseOrder);

            await _repository.AddAsync(purchaseOrder);

            return true;
        }

        // =====================================================
        // UPDATE
        // =====================================================

        public async Task<bool> UpdateAsync(
            PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder == null ||
                purchaseOrder.Id <= 0)
            {
                return false;
            }

            if (!IsValidPurchaseOrder(purchaseOrder))
                return false;

            var existing =
                await _repository.GetByIdAsync(
                    purchaseOrder.Id);

            if (existing == null ||
                existing.IsDeleted ||
                existing.Status != PurchaseOrderStatus.Draft)
            {
                return false;
            }

            existing.SupplierId = purchaseOrder.SupplierId;
            existing.ExpectedDate = purchaseOrder.ExpectedDate;
            existing.Remarks = purchaseOrder.Remarks;

            // Replace existing line items safely.
            // The repository loads existing.Items, so EF can
            // track the removed and added entities.
            existing.Items.Clear();

            foreach (var incomingItem in purchaseOrder.Items)
            {
                existing.Items.Add(new PurchaseOrderItem
                {
                    ProductId = incomingItem.ProductId,
                    Quantity = incomingItem.Quantity,
                    UnitPrice = incomingItem.UnitPrice,
                    Discount = incomingItem.Discount,
                    TaxAmount = incomingItem.TaxAmount
                });
            }

            CalculateTotals(existing);

            existing.UpdatedDate = DateTime.Now;

            await _repository.UpdateAsync(existing);

            return true;
        }

        // =====================================================
        // DELETE - SOFT DELETE
        // =====================================================

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                return false;

            var po = await _repository.GetByIdAsync(id);

            if (po == null ||
                po.IsDeleted ||
                po.Status != PurchaseOrderStatus.Draft)
            {
                return false;
            }

            bool hasActiveGRN =
                await _repository.HasActiveGoodsReceiptsAsync(id);

            if (hasActiveGRN)
                return false;

            await _repository.DeleteAsync(id);

            return true;
        }

        // =====================================================
        // APPROVE
        // =====================================================

        public async Task<bool> ApproveAsync(int id)
        {
            if (id <= 0)
                return false;

            var po = await _repository.GetByIdAsync(id);

            if (po == null ||
                po.IsDeleted ||
                po.Status != PurchaseOrderStatus.Draft)
            {
                return false;
            }

            await _repository.ApproveAsync(id);

            return true;
        }

        // =====================================================
        // GET LATEST UNIT PRICE
        // =====================================================

        public async Task<decimal> GetLatestUnitPriceAsync(
            int productId)
        {
            if (productId <= 0)
                return 0m;

            return await _repository.GetLatestUnitPriceAsync(
                productId);
        }

        // =====================================================
        // CANCEL
        // =====================================================

        public async Task<bool> CancelAsync(int id)
        {
            if (id <= 0)
                return false;

            var po = await _repository.GetByIdAsync(id);

            if (po == null || po.IsDeleted)
                return false;

            if (po.Status == PurchaseOrderStatus.Cancelled ||
                po.Status == PurchaseOrderStatus.Received ||
                po.Status == PurchaseOrderStatus.PartiallyReceived ||
                po.Status == PurchaseOrderStatus.FullyReceived)
            {
                return false;
            }

            await _repository.CancelAsync(id);

            return true;
        }

        // =====================================================
        // RESTORE
        // =====================================================

        public async Task<bool> RestoreAsync(int id)
        {
            if (id <= 0)
                return false;

            var po = await _repository.GetByIdAsync(id);

            if (po == null || !po.IsDeleted)
                return false;

            await _repository.RestoreAsync(id);

            return true;
        }

        // =====================================================
        // CHECK ACTIVE GOODS RECEIPTS
        // =====================================================

        public async Task<bool> HasActiveGoodsReceiptsAsync(
            int purchaseOrderId)
        {
            if (purchaseOrderId <= 0)
                return false;

            return await _repository.HasActiveGoodsReceiptsAsync(
                purchaseOrderId);
        }
    }
}
