// Analytics Types

export interface DashboardAnalytics {
  totalRevenue: number;
  monthlyRevenue: number;
  revenueGrowthPercentage: number;
  totalAppointments: number;
  monthlyAppointments: number;
  appointmentGrowthPercentage: number;
  totalCustomers: number;
  newCustomersThisMonth: number;
  customerGrowthPercentage: number;
  averageRating: number;
  totalReviews: number;
  recentAppointments: RecentAppointment[];
  topServices: TopService[];
  topStaff: TopStaff[];
}

export interface RecentAppointment {
  id: string;
  startTime: string;
  customerName: string;
  serviceName: string;
  staffName: string;
  status: number;
  totalPrice: number;
}

export interface TopService {
  serviceId: string;
  serviceName: string;
  bookingCount: number;
  revenue: number;
}

export interface TopStaff {
  staffId: string;
  staffName: string;
  photoUrl?: string;
  appointmentCount: number;
  revenue: number;
  averageRating: number;
}

export interface RevenueReport {
  startDate: string;
  endDate: string;
  totalRevenue: number;
  totalRefunds: number;
  netRevenue: number;
  totalTransactions: number;
  averageTransactionValue: number;
  dailyBreakdown: DailyRevenue[];
  revenueByService: ServiceRevenue[];
  revenueByPaymentMethod: PaymentMethodRevenue[];
}

export interface DailyRevenue {
  date: string;
  revenue: number;
  transactionCount: number;
}

export interface ServiceRevenue {
  serviceId: string;
  serviceName: string;
  revenue: number;
  bookingCount: number;
  percentage: number;
}

export interface PaymentMethodRevenue {
  paymentMethod: number;
  paymentMethodName: string;
  revenue: number;
  transactionCount: number;
  percentage: number;
}

export interface AppointmentAnalytics {
  startDate: string;
  endDate: string;
  totalAppointments: number;
  completedAppointments: number;
  cancelledAppointments: number;
  noShowAppointments: number;
  completionRate: number;
  cancellationRate: number;
  noShowRate: number;
  statusBreakdown: AppointmentStatusBreakdown[];
  hourlyDistribution: HourlyAppointment[];
  dayOfWeekDistribution: DayOfWeekAppointment[];
}

export interface AppointmentStatusBreakdown {
  status: number;
  statusName: string;
  count: number;
  percentage: number;
}

export interface HourlyAppointment {
  hour: number;
  count: number;
}

export interface DayOfWeekAppointment {
  dayOfWeek: number;
  dayName: string;
  count: number;
}

export interface StaffPerformance {
  staffId: string;
  staffName: string;
  photoUrl?: string;
  totalAppointments: number;
  completedAppointments: number;
  completionRate: number;
  totalRevenue: number;
  averageRevenuePerAppointment: number;
  averageRating: number;
  reviewCount: number;
  totalWorkingHours: number;
  revenuePerHour: number;
}

export interface CustomerInsights {
  totalCustomers: number;
  newCustomersThisMonth: number;
  returningCustomers: number;
  customerRetentionRate: number;
  averageLifetimeValue: number;
  averageBookingsPerCustomer: number;
  topCustomers: TopCustomer[];
  customerSegments: CustomerSegment[];
}

export interface TopCustomer {
  customerId: string;
  customerName: string;
  email: string;
  totalBookings: number;
  totalSpent: number;
  lastBookingDate: string;
}

export interface CustomerSegment {
  segmentName: string;
  customerCount: number;
  averageSpend: number;
  percentage: number;
}

export interface DateRangeFilter {
  startDate?: string;
  endDate?: string;
  salonId?: string;
  staffId?: string;
}

// Helper functions
export const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency: 'TRY',
    minimumFractionDigits: 0,
    maximumFractionDigits: 2
  }).format(amount);
};

export const formatPercentage = (value: number): string => {
  return `${value >= 0 ? '+' : ''}${value.toFixed(1)}%`;
};

export const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('tr-TR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
};
