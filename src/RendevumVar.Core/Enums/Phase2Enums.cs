namespace RendevumVar.Core.Enums;

// Billing Cycle for subscriptions
public enum BillingCycle
{
    Monthly = 1,
    Annual = 2
}

// Invoice status
public enum InvoiceStatus
{
    Draft = 1,
    Sent = 2,
    Paid = 3,
    Overdue = 4,
    Cancelled = 5,
    Refunded = 6
}

// Payment gateway providers
public enum PaymentGateway
{
    PayTR = 1,
    Iyzico = 2,
    Stripe = 3,
    Manual = 4
}

// Invitation types
public enum InvitationType
{
    QR = 1,
    Link = 2,
    SMS = 3,
    Code = 4
}

// Connection status between customer and business
public enum ConnectionStatus
{
    PendingCustomerApproval = 1,
    PendingBusinessApproval = 2,
    Approved = 3,
    Rejected = 4,
    Blocked = 5,
    Disconnected = 6
}

// Connection initiated by
public enum ConnectionInitiator
{
    Customer = 1,
    Business = 2
}

// Staff roles
public enum StaffRole
{
    Owner = 1,
    Manager = 2,
    Staff = 3,
    Receptionist = 4
}

// Time off types
public enum TimeOffType
{
    Vacation = 1,
    SickLeave = 2,
    Emergency = 3,
    Personal = 4
}

// Time off status
public enum TimeOffStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}

// Appointment request status
public enum AppointmentRequestStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Expired = 4,
    Cancelled = 5
}

// Booking mode
public enum BookingMode
{
    StaffFirst = 1,      // Customer selects staff first
    ServiceFirst = 2,    // Customer selects service first
    AutoAssignment = 3   // System assigns staff automatically
}

// Staff assignment strategy
public enum AssignmentStrategy
{
    RoundRobin = 1,
    LeastBusy = 2,
    HighestRated = 3,
    Random = 4
}

// Service package types
public enum PackageType
{
    MultiSession = 1,    // Same service multiple times (e.g., 10 laser sessions)
    MixedService = 2,    // Multiple different services
    Unlimited = 3,       // Unlimited sessions within period
    Membership = 4       // Monthly membership
}

// Customer package status
public enum CustomerPackageStatus
{
    Active = 1,
    Suspended = 2,
    Expired = 3,
    Cancelled = 4,
    Completed = 5
}

// Package payment type
public enum PackagePaymentType
{
    FullPayment = 1,
    Installments = 2,
    Deposit = 3
}

// Installment status
public enum InstallmentStatus
{
    Pending = 1,
    Paid = 2,
    Overdue = 3,
    Failed = 4,
    Waived = 5
}

// Notification types
public enum NotificationType
{
    BookingConfirmation = 1,
    BookingReminder24h = 2,
    BookingReminder2h = 3,
    BookingCancelled = 4,
    BookingRescheduled = 5,
    SessionUsed = 6,
    PackageExpiring = 7,
    PackageExpired = 8,
    PackageCompleted = 9,
    PaymentReceived = 10,
    PaymentFailed = 11,
    InvoiceGenerated = 12,
    SubscriptionExpiring = 13,
    SubscriptionExpired = 14,
    ConnectionRequest = 15,
    ConnectionApproved = 16,
    ConnectionRejected = 17,
    StaffAssigned = 18,
    TimeOffApproved = 19,
    TimeOffRejected = 20,
    Custom = 99
}

// Notification channels
public enum NotificationChannel
{
    Email = 1,
    SMS = 2,
    Push = 3,
    InApp = 4
}

// Notification status
public enum NotificationStatus
{
    Queued = 1,
    Sending = 2,
    Sent = 3,
    Failed = 4,
    Read = 5
}

// Cancellation category
public enum CancellationCategory
{
    Personal = 1,
    Emergency = 2,
    ScheduleConflict = 3,
    Weather = 4,
    Illness = 5,
    Transportation = 6,
    Other = 7
}

// Verification purpose for SMS
public enum VerificationPurpose
{
    Registration = 1,
    Login = 2,
    PasswordReset = 3,
    InvitationAcceptance = 4,
    PhoneChange = 5
}

// Staff Status
public enum StaffStatus
{
    Invited = 1,      // Invitation sent, not accepted yet
    Active = 2,       // Active and working
    Inactive = 3,     // Temporarily inactive
    OnLeave = 4,      // On leave/vacation
    Terminated = 5    // Employment terminated
}

// Invitation Status
public enum InvitationStatus
{
    Pending = 1,      // Invitation sent, waiting for response
    Accepted = 2,     // Invitation accepted
    Expired = 3,      // Invitation link expired
    Cancelled = 4     // Invitation cancelled by admin
}

// Day of Week (for schedules) - renamed to avoid conflict with System.DayOfWeek
public enum WorkDayOfWeek
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7
}
