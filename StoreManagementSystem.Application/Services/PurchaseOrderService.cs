using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Application.Services
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _repository;

        public PurchaseOrderService(IPurchaseOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PurchaseOrder>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder.SupplierId <= 0)
                return false;

            if (purchaseOrder.Items == null || !purchaseOrder.Items.Any())
                return false;

            purchaseOrder.PONumber =
                await _repository.GeneratePONumberAsync();

            purchaseOrder.OrderDate = DateTime.Now;

            purchaseOrder.Status = PurchaseOrderStatus.Draft;

            purchaseOrder.CreatedDate = DateTime.Now;

            purchaseOrder.IsDeleted = false;

            decimal subTotal = 0;

            decimal tax = 0;

            decimal discount = 0;

            foreach (var item in purchaseOrder.Items)
            {
                item.Total =
                    (item.Quantity * item.UnitPrice)
                    - item.Discount
                    + item.TaxAmount;

                subTotal += item.Quantity * item.UnitPrice;

                tax += item.TaxAmount;

                discount += item.Discount;
            }

            purchaseOrder.SubTotal = subTotal;

            purchaseOrder.TaxAmount = tax;

            purchaseOrder.Discount = discount;

            purchaseOrder.GrandTotal =
                subTotal + tax - discount;

            await _repository.AddAsync(purchaseOrder);

            return true;
        }

        public async Task<bool> UpdateAsync(PurchaseOrder purchaseOrder)
        {
            var existing =
                await _repository.GetByIdAsync(purchaseOrder.Id);

            if (existing == null)
                return false;

            if (existing.Status != PurchaseOrderStatus.Draft)
                return false;

            existing.SupplierId = purchaseOrder.SupplierId;

            existing.ExpectedDate = purchaseOrder.ExpectedDate;

            existing.Remarks = purchaseOrder.Remarks;

            existing.Items = purchaseOrder.Items;

            decimal subTotal = 0;
            decimal tax = 0;
            decimal discount = 0;

            foreach (var item in existing.Items)
            {
                item.Total =
                    (item.Quantity * item.UnitPrice)
                    - item.Discount
                    + item.TaxAmount;

                subTotal += item.Quantity * item.UnitPrice;

                tax += item.TaxAmount;

                discount += item.Discount;
            }

            existing.SubTotal = subTotal;
            existing.TaxAmount = tax;
            existing.Discount = discount;
            existing.GrandTotal = subTotal + tax - discount;

            existing.UpdatedDate = DateTime.Now;

            await _repository.UpdateAsync(existing);

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var po = await _repository.GetByIdAsync(id);

            if (po == null)
                return false;

            if (po.Status != PurchaseOrderStatus.Draft)
                return false;

            await _repository.DeleteAsync(id);

            return true;
        }

        public async Task<bool> ApproveAsync(int id)
        {
            var po = await _repository.GetByIdAsync(id);

            if (po == null)
                return false;

            if (po.Status != PurchaseOrderStatus.Draft)
                return false;

            await _repository.ApproveAsync(id);

            return true;
        }

        public async Task<bool> CancelAsync(int id)
        {
            var po = await _repository.GetByIdAsync(id);

            if (po == null)
                return false;

            if (po.Status == PurchaseOrderStatus.Received)
                return false;

            await _repository.CancelAsync(id);

            return true;
        }
    }
}
