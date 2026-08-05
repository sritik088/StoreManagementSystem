namespace StoreManagementSystem.Domain.Enums
{
    public enum PurchaseOrderStatus
    {
        Draft = 1,

        Approved = 2,

        Received = 3,

        Cancelled = 4,
        PartiallyReceived = 5,
        FullyReceived = 6
    }
}
