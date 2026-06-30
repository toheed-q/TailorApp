namespace TailorApp.Domain.Enums;

/// <summary>Lifecycle state of an order.</summary>
public enum OrderStatus
{
    Pending = 0,
    InProgress = 1,
    Ready = 2,
    Delivered = 3,
    Cancelled = 4
}
