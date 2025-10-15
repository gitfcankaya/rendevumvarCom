using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;

namespace RendevumVar.Core.Repositories;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
    Task<Invoice?> GetByIdWithLineItemsAsync(Guid id);
    Task<IEnumerable<Invoice>> GetByTenantIdAsync(Guid tenantId);
    Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
    Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status);
    Task<string> GenerateInvoiceNumberAsync();
}
