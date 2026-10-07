namespace StoreManagementSystem.Domain.Enums
{
    public enum StockTransactionType
    {
        OpeningStock = 1,


    PurchaseReceipt = 2,

        SalesIssue = 3,

        SalesReturn = 4,

        PurchaseReturn = 5,

        StockTransferIn = 6,

        StockTransferOut = 7,

        StockAdjustment = 8,

        GiftReceipt = 9,

        // =====================================================
        // REVERSAL TRANSACTIONS
        // =====================================================

        PurchaseReceiptReversal = 10,

        GiftReceiptReversal = 11
    }


}
