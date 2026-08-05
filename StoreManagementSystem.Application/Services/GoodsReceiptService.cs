using StoreManagementSystem.Application.Interfaces;
using StoreManagementSystem.Domain.Entities;
using StoreManagementSystem.Domain.Enums;

public class GoodsReceiptService : IGoodsReceiptService
{
    private readonly IGoodsReceiptRepository _repository;

    private readonly IProductRepository _productRepository;

    private readonly IWarehouseStockRepository _warehouseRepository;

    private readonly IStockLedgerRepository _stockLedgerRepository;

    private readonly IPurchaseOrderRepository _purchaseRepository;

    public GoodsReceiptService(
        IGoodsReceiptRepository repository,
        IProductRepository productRepository,
        IWarehouseStockRepository warehouseRepository,
        IStockLedgerRepository stockLedgerRepository,
        IPurchaseOrderRepository purchaseRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
        _warehouseRepository = warehouseRepository;
        _stockLedgerRepository = stockLedgerRepository;
        _purchaseRepository = purchaseRepository;
    }

    public async Task<bool> CreateAsync(GoodsReceipt receipt)
    {
        var po = await _purchaseRepository
    .GetByIdAsync(receipt.PurchaseOrderId);

        if (po == null)
            return false;

        if (po.Status != PurchaseOrderStatus.Approved &&
            po.Status != PurchaseOrderStatus.PartiallyReceived)
            return false;

        receipt.GRNNumber =
    await _repository.GenerateGRNNumberAsync();

        receipt.ReceiptDate = DateTime.Now;

        receipt.Status = GoodsReceiptStatus.Received;

        receipt.CreatedDate = DateTime.Now;

        foreach (var item in receipt.Items)
        {
            if (item.ReceivedQuantity <= 0)
                return false;

            if (item.ReceivedQuantity >
               item.OrderedQuantity)
                return false;
        }

        decimal subtotal = 0;

        decimal discount = 0;

        decimal tax = 0;

        foreach (var item in receipt.Items)
        {
            item.Total =
                item.ReceivedQuantity *
                item.UnitPrice;

            item.Total -=
                item.Discount;

            item.Total +=
                item.TaxAmount;

            subtotal +=
                item.ReceivedQuantity *
                item.UnitPrice;

            discount +=
                item.Discount;

            tax +=
                item.TaxAmount;
        }

        receipt.SubTotal = subtotal;

        receipt.Discount = discount;

        receipt.TaxAmount = tax;

        receipt.GrandTotal =
        subtotal + tax - discount;

        await _repository.AddAsync(receipt);

        foreach (var item in receipt.Items)
        {
            var product =
                await _productRepository
                .GetByIdAsync(item.ProductId);

            if (product == null)
                continue;

            product.CurrentStock +=
                item.ReceivedQuantity;

            await _productRepository
                .UpdateAsync(product);
        }

        foreach (var item in receipt.Items)
        {
            await _warehouseRepository
                .IncreaseStockAsync(

                item.WarehouseId,

                item.ProductId,

                item.ReceivedQuantity
                );
        }

        foreach (var item in receipt.Items)
        {
            await _stockLedgerRepository
                .AddAsync(new StockLedger
                {
                    ProductId = item.ProductId,

                    WarehouseId = item.WarehouseId,

                    TransactionType =
                        StockTransactionType.GRN,

                    Quantity =
                        item.ReceivedQuantity,

                    ReferenceNo =
                        receipt.GRNNumber,

                    TransactionDate =
                        DateTime.Now
                });
        }

        bool completed = true;

        foreach (var poItem in po.Items)
        {
            decimal received =
                receipt.Items
                .Where(x => x.ProductId ==
                          poItem.ProductId)
                .Sum(x => x.ReceivedQuantity);

            if (received < poItem.Quantity)
            {
                completed = false;
                break;
            }
        }

        po.Status =
        completed
        ? PurchaseOrderStatus.FullyReceived
        : PurchaseOrderStatus.PartiallyReceived;

        await _purchaseRepository.UpdateAsync(po);

        return true;

    }

    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GoodsReceipt>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<GoodsReceipt?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }
}
