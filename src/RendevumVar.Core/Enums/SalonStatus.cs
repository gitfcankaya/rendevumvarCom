namespace RendevumVar.Core.Enums;

public enum SalonStatus
{
    Pending = 0,      // Waiting for admin approval
    Approved = 1,     // Approved and active
    Rejected = 2,     // Rejected by admin
    Suspended = 3,    // Temporarily suspended
    Closed = 4        // Permanently closed
}
