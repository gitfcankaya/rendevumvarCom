using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Entities;

/// <summary>
/// Invoice for subscription billing
/// </summary>
public class Invoice : BaseEntity
{
    // Invoice details
    public string InvoiceNumber { get; set; } = string.Empty; // Unique invoice number
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    
    // Dates
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    
    // Amounts
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    
    // Status
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    
    // Payment tracking
    public DateTime? PaidAt { get; set; }
    public string? PaymentTransactionId { get; set; }
    
    // File storage
    public string? PdfUrl { get; set; }
    
    // Relations
    public Guid? TenantSubscriptionId { get; set; }
    public TenantSubscription? TenantSubscription { get; set; }
    
    // Line items
    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
}

/// <summary>
/// Individual line item in an invoice
/// </summary>
public class InvoiceLineItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
