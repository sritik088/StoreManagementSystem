using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Application.Services
{
    public class SalesOrderService : ISalesOrderService
    {
        private readonly ISalesOrderRepository _salesOrderRepository;

        public SalesOrderService(
            ISalesOrderRepository salesOrderRepository)
        {
            _salesOrderRepository =
                salesOrderRepository;
        }

        // =========================================================
        // GET ALL
        // =========================================================

        public async Task<IEnumerable<SalesOrder>> GetAllAsync()
        {
            return await _salesOrderRepository
                .GetAllAsync();
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        public async Task<SalesOrder?> GetByIdAsync(
            int id)
        {
            if (id <= 0)
                return null;

            return await _salesOrderRepository
                .GetByIdWithDetailsAsync(id);
        }

        // =========================================================
        // CREATE
        // =========================================================

        public async Task<bool> CreateAsync(
            SalesOrder salesOrder)
        {
            if (salesOrder == null)
                return false;

            if (string.IsNullOrWhiteSpace(
                salesOrder.OrderNumber))
            {
                return false;
            }

            if (salesOrder.WarehouseId <= 0)
                return false;

            if (salesOrder.SalesOrderItems == null ||
                !salesOrder.SalesOrderItems.Any())
            {
                return false;
            }

            CalculateTotals(salesOrder);

            salesOrder.Status =
                SalesOrderStatus.Draft;

            salesOrder.IsDeleted =
                false;

            salesOrder.CreatedDate =
                DateTime.Now;

            salesOrder.UpdatedDate =
                null;

            await _salesOrderRepository
                .AddAsync(salesOrder);

            return true;
        }

        // =========================================================
        // UPDATE DRAFT
        // =========================================================

        public async Task<bool> UpdateAsync(
            SalesOrder salesOrder)
        {
            if (salesOrder == null)
                return false;

            if (salesOrder.Id <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(
                salesOrder.OrderNumber))
            {
                return false;
            }

            if (salesOrder.WarehouseId <= 0)
                return false;

            if (salesOrder.Status !=
                SalesOrderStatus.Draft)
            {
                return false;
            }

            if (salesOrder.SalesOrderItems == null ||
                !salesOrder.SalesOrderItems.Any())
            {
                return false;
            }

            CalculateTotals(salesOrder);

            salesOrder.IsDeleted =
                false;

            salesOrder.UpdatedDate =
                DateTime.Now;

            await _salesOrderRepository
                .UpdateAsync(salesOrder);

            return true;
        }

        // =========================================================
        // CONFIRM
        // =========================================================
        //
        // Draft
        //    ↓
        // Confirmed
        //
        // Confirmed orders are counted by Analytics.
        // =========================================================

        public async Task<bool> ConfirmAsync(
            int id)
        {
            if (id <= 0)
                return false;

            var salesOrder =
                await _salesOrderRepository
                    .GetByIdWithDetailsAsync(id);

            if (salesOrder == null)
                return false;

            if (salesOrder.IsDeleted)
                return false;

            if (salesOrder.Status !=
                SalesOrderStatus.Draft)
            {
                return false;
            }

            if (salesOrder.SalesOrderItems == null ||
                !salesOrder.SalesOrderItems.Any())
            {
                return false;
            }

            // Recalculate everything on the server
            // before confirmation.
            CalculateTotals(salesOrder);

            salesOrder.Status =
                SalesOrderStatus.Confirmed;

            salesOrder.IsDeleted =
                false;

            salesOrder.UpdatedDate =
                DateTime.Now;

            await _salesOrderRepository
                .UpdateAsync(salesOrder);

            return true;
        }

        // =========================================================
        // CANCEL
        // =========================================================

        public async Task<bool> CancelAsync(
            int id)
        {
            if (id <= 0)
                return false;

            var salesOrder =
                await _salesOrderRepository
                    .GetByIdWithDetailsAsync(id);

            if (salesOrder == null)
                return false;

            if (salesOrder.IsDeleted)
                return false;

            // Only Draft orders can be cancelled.
            if (salesOrder.Status !=
                SalesOrderStatus.Draft)
            {
                return false;
            }

            salesOrder.Status =
                SalesOrderStatus.Cancelled;

            salesOrder.UpdatedDate =
                DateTime.Now;

            await _salesOrderRepository
                .UpdateAsync(salesOrder);

            return true;
        }

        // =========================================================
        // CALCULATE TOTALS
        // =========================================================

        private static void CalculateTotals(
            SalesOrder salesOrder)
        {
            if (salesOrder.SalesOrderItems == null ||
                !salesOrder.SalesOrderItems.Any())
            {
                throw new InvalidOperationException(
                    "Sales Order must contain at least one item.");
            }

            decimal subTotal = 0m;
            decimal discountAmount = 0m;
            decimal taxAmount = 0m;
            decimal grandTotal = 0m;
            decimal grossProfit = 0m;

            foreach (var item
                in salesOrder.SalesOrderItems)
            {
                if (item.ProductId <= 0)
                {
                    throw new InvalidOperationException(
                        "Invalid product.");
                }

                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        "Quantity must be greater than zero.");
                }

                if (item.UnitPrice < 0)
                {
                    throw new InvalidOperationException(
                        "Cost price cannot be negative.");
                }

                if (item.SalePrice < 0)
                {
                    throw new InvalidOperationException(
                        "Sale price cannot be negative.");
                }

                if (item.TaxPercent < 0 ||
                    item.TaxPercent > 100)
                {
                    throw new InvalidOperationException(
                        "Tax percentage must be between 0 and 100.");
                }

                if (item.DiscountPercent < 0 ||
                    item.DiscountPercent > 100)
                {
                    throw new InvalidOperationException(
                        "Discount percentage must be between 0 and 100.");
                }

                // =================================================
                // PURCHASE / GRN COST
                // =================================================

                decimal costAmount =
                    item.Quantity *
                    item.UnitPrice;

                // =================================================
                // GROSS SALES
                // =================================================

                decimal grossSales =
                    item.Quantity *
                    item.SalePrice;

                // =================================================
                // DISCOUNT
                // =================================================

                decimal lineDiscount =
                    grossSales *
                    item.DiscountPercent /
                    100m;

                // =================================================
                // NET SALES
                // =================================================

                decimal netSales =
                    Math.Max(
                        grossSales -
                        lineDiscount,
                        0m);

                // =================================================
                // TAX
                // =================================================

                decimal lineTax =
                    netSales *
                    item.TaxPercent /
                    100m;

                // =================================================
                // CUSTOMER TOTAL
                // =================================================

                decimal lineTotal =
                    netSales +
                    lineTax;

                // =================================================
                // GROSS PROFIT
                //
                // Tax is excluded.
                //
                // Gross Profit =
                // Net Sales - Purchase Cost
                // =================================================

                decimal lineGrossProfit =
                    netSales -
                    costAmount;

                item.LineTotal =
                    Math.Round(
                        lineTotal,
                        2);

                item.GrossProfit =
                    Math.Round(
                        lineGrossProfit,
                        2);

                subTotal +=
                    grossSales;

                discountAmount +=
                    lineDiscount;

                taxAmount +=
                    lineTax;

                grandTotal +=
                    lineTotal;

                grossProfit +=
                    lineGrossProfit;
            }

            salesOrder.SubTotal =
                Math.Round(
                    subTotal,
                    2);

            salesOrder.DiscountAmount =
                Math.Round(
                    discountAmount,
                    2);

            salesOrder.TaxAmount =
                Math.Round(
                    taxAmount,
                    2);

            salesOrder.GrandTotal =
                Math.Round(
                    grandTotal,
                    2);

            salesOrder.GrossProfit =
                Math.Round(
                    grossProfit,
                    2);
        }

        // =========================================================
        // DELETE
        // =========================================================

        public async Task<bool> DeleteAsync(
            int id)
        {
            if (id <= 0)
                return false;

            var salesOrder =
                await _salesOrderRepository
                    .GetByIdAsync(id);

            if (salesOrder == null)
                return false;

            // Confirmed and Cancelled orders cannot be deleted.
            if (salesOrder.Status !=
                SalesOrderStatus.Draft)
            {
                return false;
            }

            salesOrder.IsDeleted =
                true;

            salesOrder.UpdatedDate =
                DateTime.Now;

            await _salesOrderRepository
                .UpdateAsync(salesOrder);

            return true;
        }

        // =========================================================
        // GET LATEST COST PRICE
        // =========================================================

        public async Task<decimal?> GetLatestCostPriceAsync(
            int productId)
        {
            if (productId <= 0)
                return null;

            return await _salesOrderRepository
                .GetLatestCostPriceAsync(productId);
        }
    }
}