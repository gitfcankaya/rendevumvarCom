import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Appointment Types
export interface CreateAppointmentDto {
  salonId: string;
  serviceId: string;
  staffId: string;
  startTime: string; // ISO date string
  notes?: string;
  customerNotes?: string;
}

export interface AppointmentDto {
  id: string;
  tenantId: string;
  salonId: string;
  salonName: string;
  customerId: string;
  customerName: string;
  customerEmail: string;
  customerPhone?: string;
  staffId: string;
  staffName: string;
  serviceId: string;
  serviceName: string;
  serviceDuration: number;
  servicePrice: number;
  startTime: string;
  endTime: string;
  status: AppointmentStatus;
  notes?: string;
  customerNotes?: string;
  staffNotes?: string;
  cancellationReason?: string;
  cancelledAt?: string;
  totalPrice: number;
  depositPaid: number;
  reminderSent: boolean;
  createdAt: string;
  // Computed fields
  duration?: number; // serviceDuration alias for compatibility
}

export interface AppointmentDetailsDto extends AppointmentDto {
  salon: SalonDto;
  service: ServiceDto;
  staff: StaffDto;
  payments: PaymentDto[];
}

export interface SalonDto {
  id: string;
  tenantId: string;
  name: string;
  address: string;
  city: string;
  phone: string;
  email: string;
  state?: string;
  postalCode?: string;
  latitude?: number;
  longitude?: number;
  businessHours?: string;
  isActive: boolean;
  averageRating: number;
  reviewCount: number;
  createdAt: string;
}

export interface ServiceDto {
  id: string;
  name: string;
  description?: string;
  durationMinutes: number;
  price: number;
  imageUrl?: string;
  categoryId?: string;
  categoryName?: string;
  isActive: boolean;
}

export interface StaffDto {
  id: string;
  tenantId: string;
  salonId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone?: string;
  bio?: string;
  photoUrl?: string;
  profilePictureUrl?: string;
  averageRating: number;
  status: string;
  invitationStatus: string;
  roleId: string;
  roleName?: string;
}

export interface PaymentDto {
  id: string;
  amount: number;
  method: string;
  status: string;
}

export interface AvailableTimeSlotDto {
  date: string;
  startTime: string; // TimeSpan format
  endTime: string;
  durationMinutes: number;
}

export interface RescheduleAppointmentDto {
  newStartTime: string;
  newStaffId?: string;
}

export interface CancelAppointmentDto {
  cancellationReason?: string;
}

export const AppointmentStatus = {
  Pending: 0,
  Confirmed: 1,
  CheckedIn: 2,
  InProgress: 3,
  Completed: 4,
  Cancelled: 5,
  NoShow: 6,
} as const;

export type AppointmentStatus = typeof AppointmentStatus[keyof typeof AppointmentStatus];

// Legacy types for backward compatibility with dashboard
export interface Appointment {
  id: string;
  customerName: string;
  customerPhone: string;
  customerEmail: string;
  serviceName: string;
  servicePrice: number;
  staffName: string;
  appointmentDate: string;
  appointmentTime: string;
  status: 'scheduled' | 'completed' | 'cancelled' | 'in-progress';
  duration: number;
  notes?: string;
}

export interface Service {
  id: string;
  name: string;
  description: string;
  price: number;
  duration: number;
  category: string;
  isActive: boolean;
}

export interface Staff {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  specialties: string[];
  isActive: boolean;
  avatar?: string;
}

export interface Customer {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  lastVisit?: string;
  totalAppointments: number;
  totalSpent: number;
}

export interface DashboardStats {
  todayAppointments: number;
  totalCustomers: number;
  monthlyRevenue: number;
  averageRating: number;
  appointmentGrowth: number;
  customerGrowth: number;
  revenueGrowth: number;
  ratingGrowth: number;
}

// Helper function to convert AppointmentDto to legacy Appointment format
const convertToLegacyAppointment = (dto: AppointmentDto): Appointment => {
  const startTime = new Date(dto.startTime);
  const statusMap: Record<number, 'scheduled' | 'completed' | 'cancelled' | 'in-progress'> = {
    0: 'scheduled', // Pending
    1: 'scheduled', // Confirmed
    2: 'in-progress', // CheckedIn
    3: 'in-progress', // InProgress
    4: 'completed', // Completed
    5: 'cancelled', // Cancelled
    6: 'cancelled', // NoShow
  };

  return {
    id: dto.id,
    customerName: dto.customerName,
    customerPhone: dto.customerPhone || '',
    customerEmail: dto.customerEmail,
    serviceName: dto.serviceName,
    servicePrice: dto.servicePrice,
    staffName: dto.staffName,
    appointmentDate: startTime.toISOString().split('T')[0],
    appointmentTime: startTime.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' }),
    status: statusMap[dto.status] || 'scheduled',
    duration: dto.serviceDuration,
    notes: dto.notes,
  };
};

// API Service
export const appointmentService = {
  // Create appointment
  createAppointment: async (data: CreateAppointmentDto): Promise<AppointmentDto> => {
    const response = await api.post('/appointments', data);
    return response.data;
  },

  // Get appointment details
  getAppointmentDetails: async (id: string): Promise<AppointmentDetailsDto> => {
    const response = await api.get(`/appointments/${id}`);
    return response.data;
  },

  // Get customer's appointments
  getMyAppointments: async (
    startDate?: string,
    endDate?: string,
    status?: AppointmentStatus
  ): Promise<AppointmentDto[]> => {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);
    if (status !== undefined) params.append('status', status.toString());

    const response = await api.get(`/appointments/my?${params.toString()}`);
    return response.data;
  },

  // Get staff appointments
  getStaffAppointments: async (
    staffId: string,
    startDate?: string,
    endDate?: string
  ): Promise<AppointmentDto[]> => {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    const response = await api.get(`/appointments/staff/${staffId}?${params.toString()}`);
    return response.data;
  },

  // Get salon appointments
  getSalonAppointments: async (
    salonId: string,
    startDate?: string,
    endDate?: string
  ): Promise<AppointmentDto[]> => {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    const response = await api.get(`/appointments/salon/${salonId}?${params.toString()}`);
    return response.data;
  },

  // Update appointment status
  updateAppointmentStatus: async (
    id: string,
    status: AppointmentStatus,
    cancellationReason?: string
  ): Promise<AppointmentDto> => {
    const response = await api.put(`/appointments/${id}/status`, {
      status,
      cancellationReason,
    });
    return response.data;
  },

  // Reschedule appointment
  rescheduleAppointment: async (
    id: string,
    data: RescheduleAppointmentDto
  ): Promise<AppointmentDto> => {
    const response = await api.put(`/appointments/${id}/reschedule`, data);
    return response.data;
  },

  // Cancel appointment
  cancelAppointment: async (id: string, cancellationReason?: string): Promise<void> => {
    await api.delete(`/appointments/${id}`, {
      data: { cancellationReason },
    });
  },

  // Get available time slots
  getAvailableTimeSlots: async (
    staffId: string,
    date: string,
    serviceDurationMinutes: number = 60
  ): Promise<AvailableTimeSlotDto[]> => {
    const params = new URLSearchParams({
      staffId,
      date,
      serviceDurationMinutes: serviceDurationMinutes.toString(),
    });

    const response = await api.get(`/appointments/availability?${params.toString()}`);
    return response.data;
  },

  // Get available staff for a service at a specific time
  getAvailableStaff: async (
    salonId: string,
    serviceId: string,
    dateTime: string,
    durationMinutes: number = 60
  ): Promise<StaffDto[]> => {
    const params = new URLSearchParams({
      salonId,
      serviceId,
      dateTime,
      durationMinutes: durationMinutes.toString(),
    });

    const response = await api.get(`/appointments/availability/staff?${params.toString()}`);
    return response.data;
  },

  // Get salon-wide availability
  getSalonAvailability: async (
    salonId: string,
    serviceId: string,
    date: string
  ): Promise<Record<string, AvailableTimeSlotDto[]>> => {
    const params = new URLSearchParams({
      serviceId,
      date,
    });

    const response = await api.get(`/appointments/availability/salon/${salonId}?${params.toString()}`);
    return response.data;
  },

  // Legacy methods for dashboard compatibility
  getAllAppointments: async (): Promise<Appointment[]> => {
    const response = await api.get('/appointments/my');
    const appointments: AppointmentDto[] = response.data;
    return appointments.map(convertToLegacyAppointment);
  },

  getTodayAppointments: async (): Promise<Appointment[]> => {
    const today = new Date().toISOString().split('T')[0];
    const response = await api.get(`/appointments/my?startDate=${today}&endDate=${today}`);
    const appointments: AppointmentDto[] = response.data;
    return appointments.map(convertToLegacyAppointment);
  },

  getDailyAppointments: async (date: string): Promise<Appointment[]> => {
    const response = await api.get(`/appointments/my?startDate=${date}&endDate=${date}`);
    const appointments: AppointmentDto[] = response.data;
    return appointments.map(convertToLegacyAppointment);
  },

  getWeeklyAppointments: async (startDate: string): Promise<Appointment[]> => {
    const start = new Date(startDate);
    const end = new Date(start);
    end.setDate(start.getDate() + 6);
    const endDate = end.toISOString().split('T')[0];

    const response = await api.get(`/appointments/my?startDate=${startDate}&endDate=${endDate}`);
    const appointments: AppointmentDto[] = response.data;
    return appointments.map(convertToLegacyAppointment).sort((a, b) => {
      if (a.appointmentDate === b.appointmentDate) {
        return a.appointmentTime.localeCompare(b.appointmentTime);
      }
      return a.appointmentDate.localeCompare(b.appointmentDate);
    });
  },

  getMonthlyAppointments: async (year: number, month: number): Promise<Appointment[]> => {
    const startDate = new Date(year, month, 1).toISOString().split('T')[0];
    const endDate = new Date(year, month + 1, 0).toISOString().split('T')[0];

    const response = await api.get(`/appointments/my?startDate=${startDate}&endDate=${endDate}`);
    const appointments: AppointmentDto[] = response.data;
    return appointments.map(convertToLegacyAppointment).sort((a, b) => {
      if (a.appointmentDate === b.appointmentDate) {
        return a.appointmentTime.localeCompare(b.appointmentTime);
      }
      return a.appointmentDate.localeCompare(b.appointmentDate);
    });
  },

  getAppointmentsByDateRange: async (startDate: string, endDate: string): Promise<Appointment[]> => {
    const response = await api.get(`/appointments/my?startDate=${startDate}&endDate=${endDate}`);
    const appointments: AppointmentDto[] = response.data;
    return appointments.map(convertToLegacyAppointment).sort((a, b) => {
      if (a.appointmentDate === b.appointmentDate) {
        return a.appointmentTime.localeCompare(b.appointmentTime);
      }
      return a.appointmentDate.localeCompare(b.appointmentDate);
    });
  },

  // Placeholder methods - these need proper implementation
  getAllServices: async (): Promise<Service[]> => {
    // TODO: Implement with real service API
    return [];
  },

  createService: async (service: Omit<Service, 'id'>): Promise<Service> => {
    // TODO: Implement with real service API
    return { ...service, id: '' };
  },

  getAllStaff: async (): Promise<Staff[]> => {
    // TODO: Implement with real staff API
    return [];
  },

  createStaff: async (staff: Omit<Staff, 'id'>): Promise<Staff> => {
    // TODO: Implement with real staff API
    return { ...staff, id: '' };
  },

  getAllCustomers: async (): Promise<Customer[]> => {
    // TODO: Implement with real customer API
    return [];
  },

  getDashboardStats: async (): Promise<DashboardStats> => {
    // TODO: Implement with real dashboard API
    return {
      todayAppointments: 0,
      totalCustomers: 0,
      monthlyRevenue: 0,
      averageRating: 0,
      appointmentGrowth: 0,
      customerGrowth: 0,
      revenueGrowth: 0,
      ratingGrowth: 0,
    };
  },
};

export default appointmentService;
