import apiClient from './apiClient';
import type { 
  DashboardAnalytics, 
  RevenueReport, 
  AppointmentAnalytics, 
  StaffPerformance, 
  CustomerInsights,
  DateRangeFilter 
} from '../types/analytics';

const analyticsService = {
  // Get dashboard analytics overview
  getDashboard: async (filters?: DateRangeFilter): Promise<DashboardAnalytics> => {
    const params = new URLSearchParams();
    if (filters?.startDate) params.append('startDate', filters.startDate);
    if (filters?.endDate) params.append('endDate', filters.endDate);
    if (filters?.salonId) params.append('salonId', filters.salonId);
    
    const response = await apiClient.get(`/analytics/dashboard?${params.toString()}`);
    return response.data;
  },

  // Get revenue report
  getRevenueReport: async (startDate: string, endDate: string, salonId?: string): Promise<RevenueReport> => {
    const params = new URLSearchParams({ startDate, endDate });
    if (salonId) params.append('salonId', salonId);
    
    const response = await apiClient.get(`/analytics/revenue?${params.toString()}`);
    return response.data;
  },

  // Get appointment analytics
  getAppointmentAnalytics: async (startDate: string, endDate: string, salonId?: string): Promise<AppointmentAnalytics> => {
    const params = new URLSearchParams({ startDate, endDate });
    if (salonId) params.append('salonId', salonId);
    
    const response = await apiClient.get(`/analytics/appointments?${params.toString()}`);
    return response.data;
  },

  // Get staff performance
  getStaffPerformance: async (startDate: string, endDate: string, salonId?: string, staffId?: string): Promise<StaffPerformance[]> => {
    const params = new URLSearchParams({ startDate, endDate });
    if (salonId) params.append('salonId', salonId);
    if (staffId) params.append('staffId', staffId);
    
    const response = await apiClient.get(`/analytics/staff-performance?${params.toString()}`);
    return response.data;
  },

  // Get customer insights
  getCustomerInsights: async (startDate: string, endDate: string, salonId?: string): Promise<CustomerInsights> => {
    const params = new URLSearchParams({ startDate, endDate });
    if (salonId) params.append('salonId', salonId);
    
    const response = await apiClient.get(`/analytics/customers?${params.toString()}`);
    return response.data;
  }
};

export default analyticsService;
