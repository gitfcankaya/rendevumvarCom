using Microsoft.EntityFrameworkCore;
using RendevumVar.Core.Entities;
using RendevumVar.Core.Enums;
using RendevumVar.Infrastructure.Data;

namespace RendevumVar.Infrastructure.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<Payment?> GetByTransactionIdAsync(string transactionId);
    Task<Payment?> GetByAppointmentIdAsync(Guid appointmentId);
    Task<Payment?> GetBySubscriptionIdAsync(Guid subscriptionId);
    Task<List<Payment>> GetByUserIdAsync(Guid userId);
    Task<List<Payment>> GetBySalonIdAsync(Guid salonId);
    Task<List<Payment>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<Payment> CreateAsync(Payment payment);
    Task<Payment> UpdateAsync(Payment payment);
    Task DeleteAsync(Guid id);
    Task<int> CountAsync();
    Task<decimal> GetTotalRevenueAsync(Guid? salonId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<Dictionary<PaymentStatus, int>> GetPaymentStatsByStatusAsync(Guid? salonId = null);
}

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _context.Payments
            .Include(p => p.User)
            .Include(p => p.Appointment)
                .ThenInclude(a => a!.Salon)
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
    {
        return await _context.Payments
            .Include(p => p.User)
            .Include(p => p.Appointment)
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
    }

    public async Task<Payment?> GetByAppointmentIdAsync(Guid appointmentId)
    {
        return await _context.Payments
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
    }

    public async Task<Payment?> GetBySubscriptionIdAsync(Guid subscriptionId)
    {
        return await _context.Payments
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.SubscriptionId == subscriptionId);
    }

    public async Task<List<Payment>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Payments
            .Include(p => p.Appointment)
                .ThenInclude(a => a!.Salon)
            .Include(p => p.Subscription)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetBySalonIdAsync(Guid salonId)
    {
        return await _context.Payments
            .Include(p => p.User)
            .Include(p => p.Appointment)
            .Where(p => p.Appointment!.SalonId == salonId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        return await _context.Payments
            .Include(p => p.User)
            .Include(p => p.Appointment)
                .ThenInclude(a => a!.Salon)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Payment> CreateAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment> UpdateAsync(Payment payment)
    {
        payment.UpdatedAt = DateTime.UtcNow;
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task DeleteAsync(Guid id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment != null)
        {
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> CountAsync()
    {
        return await _context.Payments.CountAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync(Guid? salonId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed);

        if (salonId.HasValue)
        {
            query = query.Where(p => p.Appointment!.SalonId == salonId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= endDate.Value);
        }

        var total = await query.SumAsync(p => (decimal?)p.Amount) ?? 0;

        // Subtract refunded amounts
        var refunded = await query
            .Where(p => p.RefundAmount.HasValue)
            .SumAsync(p => (decimal?)p.RefundAmount) ?? 0;

        return total - refunded;
    }

    public async Task<Dictionary<PaymentStatus, int>> GetPaymentStatsByStatusAsync(Guid? salonId = null)
    {
        var query = _context.Payments.AsQueryable();

        if (salonId.HasValue)
        {
            query = query.Where(p => p.Appointment!.SalonId == salonId.Value);
        }

        return await query
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }
}
