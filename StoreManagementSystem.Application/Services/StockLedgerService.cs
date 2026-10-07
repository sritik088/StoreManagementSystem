using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;

namespace StoreManagementSystem.Application.Services
{
    public class StockLedgerService
        : IStockLedgerService
    {
        private readonly IStockLedgerRepository
            _repository;

        public StockLedgerService(
            IStockLedgerRepository repository)
        {
            _repository = repository;
        }

        public async Task AddStockMovementAsync(
            int productId,
            int warehouseId,
            decimal quantity,
            StockTransactionType transactionType,
            decimal unitCost,
            string referenceNo,
            string? remarks = null,
            string? createdBy = null)
        {
            if (productId <= 0)
                throw new ArgumentException(
                    "ProductId is required.");

            if (warehouseId <= 0)
                throw new ArgumentException(
                    "WarehouseId is required.");

            if (quantity == 0)
                throw new ArgumentException(
                    "Stock movement cannot be zero.");

            if (string.IsNullOrWhiteSpace(
                referenceNo))
            {
                throw new ArgumentException(
                    "Reference number is required.");
            }

            var previousBalance =
                await _repository
                    .GetCurrentBalanceAsync(
                        productId,
                        warehouseId);

            var newBalance =
                previousBalance + quantity;

            if (newBalance < 0)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock. " +
                    $"Current balance: {previousBalance}, " +
                    $"Movement: {quantity}.");
            }

            var ledger = new StockLedger
            {
                ProductId =
                    productId,

                WarehouseId =
                    warehouseId,

                TransactionType =
                    transactionType,

                Quantity =
                    quantity,

                BalanceAfterTransaction =
                    newBalance,

                UnitCost =
                    unitCost,

                ReferenceNo =
                    referenceNo,

                Remarks =
                    remarks,

                CreatedBy =
                    createdBy,

                TransactionDate =
                    DateTime.Now
            };

            await _repository
                .AddAsync(ledger);
        }
    }
}