namespace RendevumVar.Core.Enums;

public enum SubscriptionStatus
{
    Active = 1,
    Cancelled = 2,
    Expired = 3,
    Suspended = 4,
    PendingPayment = 5,
    FreeTrial = 6
}

public enum PaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    Refunded = 5
}

public enum PaymentMethod
{
    CreditCard = 1,
    DebitCard = 2,
    BankTransfer = 3,
    PayTR = 4,
    Other = 5
}