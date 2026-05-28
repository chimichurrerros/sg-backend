namespace BackEnd.Models;

public enum PurchaseOrderStateEnum
{
    Pending = 1,
    Confirmed = 2,
    PartiallyReceived = 3,
    Received = 4,
    Cancelled = 5
}
