namespace RendevumVar.Core.Enums;

public enum SubscriptionStatus
{
    Trialing = 1,
    Active = 2,
    PastDue = 3,
    Suspended = 4,
    Cancelled = 5,
    Expired = 6,
    FreeTrial = 7, // Legacy
    PendingPayment = 8 // Legacy
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