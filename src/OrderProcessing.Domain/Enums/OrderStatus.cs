namespace OrderProcessing.Domain;

public enum OrderStatus
{
    Pending = 1,
    Processed = 2,
    Cancelled = 3,
    MissingProducts = 4
}
