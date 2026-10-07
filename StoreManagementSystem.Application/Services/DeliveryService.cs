using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;

namespace StoreManagementSystem.Application.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly ISalesOrderRepository _salesOrderRepository;
        private readonly IWarehouseStockService _stockService;
        private readonly IStockLedgerService _ledgerService;

        public DeliveryService(
            IDeliveryRepository deliveryRepository,
            ISalesOrderRepository salesOrderRepository,
            IWarehouseStockService stockService,
            IStockLedgerService ledgerService)
        {
            _deliveryRepository = deliveryRepository;
            _salesOrderRepository = salesOrderRepository;
            _stockService = stockService;
            _ledgerService = ledgerService;
        }

        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<List<Delivery>> GetAllAsync()
        {
            return await _deliveryRepository.GetAllAsync();
        }

        // =====================================================
        // GET BY ID
        // =====================================================

        public async Task<Delivery?> GetByIdAsync(int id)
        {
            return await _deliveryRepository.GetByIdAsync(id);
        }

        // =====================================================
        // GET BY SALES ORDER
        // =====================================================

        public async Task<List<Delivery>> GetBySalesOrderIdAsync(
            int salesOrderId)
        {
            return await _deliveryRepository
                .GetBySalesOrderIdAsync(salesOrderId);
        }

        // =====================================================
        // GET DELIVERED QUANTITY
        // =====================================================

        public async Task<decimal> GetDeliveredQuantityAsync(
            int salesOrderItemId)
        {
            return await _deliveryRepository
                .GetDeliveredQuantityAsync(
                    salesOrderItemId);
        }

        // =====================================================
        // CREATE DELIVERY
        // =====================================================

        public async Task<bool> CreateAsync(
            Delivery delivery)
        {
            if (delivery == null)
                return false;

            if (delivery.SalesOrderId <= 0)
                return false;

            if (delivery.WarehouseId <= 0)
                return false;

            if (delivery.Items == null ||
                !delivery.Items.Any())
            {
                return false;
            }

            // =================================================
            // GET SALES ORDER
            // =================================================

            var salesOrder =
                await _salesOrderRepository
                    .GetByIdAsync(
                        delivery.SalesOrderId);

            if (salesOrder == null)
                return false;

            if (salesOrder.SalesOrderItems == null ||
                !salesOrder.SalesOrderItems.Any())
            {
                return false;
            }

            // =================================================
            // VALIDATE ALL DELIVERY ITEMS
            // =================================================

            foreach (var item in delivery.Items)
            {
                if (item.SalesOrderItemId <= 0)
                    return false;

                if (item.ProductId <= 0)
                    return false;

                if (item.Quantity <= 0)
                    return false;

                // ---------------------------------------------
                // FIND ORDER ITEM
                // ---------------------------------------------

                var orderItem =
                    salesOrder.SalesOrderItems
                        .FirstOrDefault(x =>
                            x.Id ==
                            item.SalesOrderItemId);

                if (orderItem == null)
                    return false;

                // ---------------------------------------------
                // PRODUCT MUST MATCH
                // ---------------------------------------------

                if (orderItem.ProductId !=
                    item.ProductId)
                {
                    return false;
                }

                // ---------------------------------------------
                // ALREADY DELIVERED
                // ---------------------------------------------

                var alreadyDelivered =
                    await _deliveryRepository
                        .GetDeliveredQuantityAsync(
                            item.SalesOrderItemId);

                // ---------------------------------------------
                // REMAINING QUANTITY
                // ---------------------------------------------

                var remaining =
                    orderItem.Quantity -
                    alreadyDelivered;

                if (remaining <= 0)
                    return false;

                if (item.Quantity > remaining)
                    return false;

                // ---------------------------------------------
                // CHECK WAREHOUSE STOCK
                // ---------------------------------------------

                var availableStock =
                    await _stockService
                        .GetAvailableStockAsync(
                            delivery.WarehouseId,
                            item.ProductId);

                if (availableStock <
                    item.Quantity)
                {
                    return false;
                }
            }

            // =================================================
            // DELIVERY NUMBER
            // =================================================

            delivery.DeliveryNumber =
                await GenerateDeliveryNumberAsync();

            delivery.DeliveryDate =
                delivery.DeliveryDate == default
                    ? DateTime.Now
                    : delivery.DeliveryDate;

            // =================================================
            // ADD DELIVERY
            // =================================================

            await _deliveryRepository
                .AddAsync(delivery);

            // =================================================
            // STOCK DEDUCTION
            // =================================================

            foreach (var item in delivery.Items)
            {
                // ---------------------------------------------
                // DECREASE WAREHOUSE STOCK
                // ---------------------------------------------

                await _stockService
                    .DecreaseStockAsync(
                        delivery.WarehouseId,
                        item.ProductId,
                        item.Quantity);

                // ---------------------------------------------
                // STOCK LEDGER
                // ---------------------------------------------

                await _ledgerService
                    .AddStockMovementAsync(
                        item.ProductId,
                        delivery.WarehouseId,
                        -item.Quantity,
                        Domain.Enums.StockTransactionType.SalesIssue,
                        item.UnitPrice,
                        delivery.DeliveryNumber,
                        "Stock deducted for final delivery.",
                        "System");
            }

            // =================================================
            // STATUS
            // =================================================

            bool fullyDelivered = true;

            foreach (var orderItem
                in salesOrder.SalesOrderItems)
            {
                var alreadyDelivered =
                    await _deliveryRepository
                        .GetDeliveredQuantityAsync(
                            orderItem.Id);

                var currentDelivery =
                    delivery.Items
                        .Where(x =>
                            x.SalesOrderItemId ==
                            orderItem.Id)
                        .Sum(x => x.Quantity);

                var totalDelivered =
                    alreadyDelivered +
                    currentDelivery;

                if (totalDelivered <
                    orderItem.Quantity)
                {
                    fullyDelivered = false;
                    break;
                }
            }

            delivery.Status =
                fullyDelivered
                    ? Domain.Enums.DeliveryStatus.Delivered
                    : Domain.Enums.DeliveryStatus.PartiallyDelivered;

            return true;
        }

        // =====================================================
        // GENERATE DELIVERY NUMBER
        // =====================================================

        private async Task<string>
            GenerateDeliveryNumberAsync()
        {
            var deliveries =
                await _deliveryRepository
                    .GetAllAsync();

            var lastNumber = 0;

            foreach (var delivery in deliveries)
            {
                if (string.IsNullOrWhiteSpace(
                    delivery.DeliveryNumber))
                {
                    continue;
                }

                var numberText =
                    delivery.DeliveryNumber
                        .Replace("DEL-", "");

                if (int.TryParse(
                    numberText,
                    out int number))
                {
                    if (number > lastNumber)
                        lastNumber = number;
                }
            }

            lastNumber++;

            return $"DEL-{lastNumber:0000}";
        }
    }
}