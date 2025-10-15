import axios from 'axios';
import type {
  StaffDto,
  InviteStaffDto,
  AcceptInvitationDto,
  UpdateStaffProfileDto,
  RoleDto,
  CreateRoleDto,
  AssignRoleDto,
  StaffScheduleDto,
  SetStaffScheduleDto,
  AvailableTimeSlotDto,
  TimeOffRequestDto,
  CreateTimeOffRequestDto,
  RejectTimeOffDto
} from './staffTypes';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

// Get auth token from localStorage
const getAuthToken = (): string | null => {
  return localStorage.getItem('accessToken');
};

// Create axios instance with auth interceptor
const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token to requests
api.interceptors.request.use((config) => {
  const token = getAuthToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Staff Management API
export const staffService = {
  // Invite new staff member
  inviteStaff: async (data: InviteStaffDto): Promise<StaffDto> => {
    const response = await api.post<StaffDto>('/staff/invite', data);
    return response.data;
  },

  // Accept staff invitation
  acceptInvitation: async (data: AcceptInvitationDto): Promise<void> => {
    await api.post('/staff/accept-invitation', data);
  },

  // Resend invitation
  resendInvitation: async (staffId: string): Promise<void> => {
    await api.post(`/staff/${staffId}/resend-invitation`);
  },

  // Get all staff members
  getStaffList: async (): Promise<StaffDto[]> => {
    const response = await api.get<StaffDto[]>('/staff');
    return response.data;
  },

  // Get staff details
  getStaffDetails: async (staffId: string): Promise<StaffDto> => {
    const response = await api.get<StaffDto>(`/staff/${staffId}`);
    return response.data;
  },

  // Update staff profile
  updateStaffProfile: async (staffId: string, data: UpdateStaffProfileDto): Promise<StaffDto> => {
    const response = await api.put<StaffDto>(`/staff/${staffId}`, data);
    return response.data;
  },

  // Deactivate staff
  deactivateStaff: async (staffId: string): Promise<void> => {
    await api.post(`/staff/${staffId}/deactivate`);
  },

  // Reactivate staff
  reactivateStaff: async (staffId: string): Promise<void> => {
    await api.post(`/staff/${staffId}/reactivate`);
  },

  // Assign role to staff
  assignRole: async (staffId: string, data: AssignRoleDto): Promise<StaffDto> => {
    const response = await api.post<StaffDto>(`/staff/${staffId}/assign-role`, data);
    return response.data;
  },

  // Get all roles
  getRoles: async (): Promise<RoleDto[]> => {
    const response = await api.get<RoleDto[]>('/staff/roles');
    return response.data;
  },

  // Create new role
  createRole: async (data: CreateRoleDto): Promise<RoleDto> => {
    const response = await api.post<RoleDto>('/staff/roles', data);
    return response.data;
  },
};

// Schedule Management API
export const scheduleService = {
  // Set/Create staff schedule
  setStaffSchedule: async (staffId: string, data: SetStaffScheduleDto): Promise<StaffScheduleDto> => {
    const response = await api.post<StaffScheduleDto>(`/schedule/staff/${staffId}`, data);
    return response.data;
  },

  // Get staff schedules
  getStaffSchedule: async (staffId: string): Promise<StaffScheduleDto[]> => {
    const response = await api.get<StaffScheduleDto[]>(`/schedule/staff/${staffId}`);
    return response.data;
  },

  // Update schedule
  updateStaffSchedule: async (scheduleId: string, data: SetStaffScheduleDto): Promise<StaffScheduleDto> => {
    const response = await api.put<StaffScheduleDto>(`/schedule/${scheduleId}`, data);
    return response.data;
  },

  // Delete schedule
  deleteStaffSchedule: async (scheduleId: string): Promise<void> => {
    await api.delete(`/schedule/${scheduleId}`);
  },

  // Check availability
  checkStaffAvailability: async (staffId: string, dateTime: string, durationMinutes: number): Promise<boolean> => {
    const response = await api.post<{ available: boolean }>('/schedule/availability/check', {
      staffId,
      dateTime,
      durationMinutes,
    });
    return response.data.available;
  },

  // Get available time slots
  getAvailableTimeSlots: async (
    staffId: string,
    date: string,
    slotDurationMinutes: number = 30
  ): Promise<AvailableTimeSlotDto[]> => {
    const response = await api.get<AvailableTimeSlotDto[]>('/schedule/availability/slots', {
      params: { staffId, date, slotDurationMinutes },
    });
    return response.data;
  },
};

// Time Off Management API
export const timeOffService = {
  // Request time off
  requestTimeOff: async (data: CreateTimeOffRequestDto): Promise<TimeOffRequestDto> => {
    const response = await api.post<TimeOffRequestDto>('/timeoff/request', data);
    return response.data;
  },

  // Get staff time off
  getStaffTimeOff: async (staffId: string): Promise<TimeOffRequestDto[]> => {
    const response = await api.get<TimeOffRequestDto[]>(`/timeoff/staff/${staffId}`);
    return response.data;
  },

  // Get pending requests
  getPendingRequests: async (): Promise<TimeOffRequestDto[]> => {
    const response = await api.get<TimeOffRequestDto[]>('/timeoff/pending');
    return response.data;
  },

  // Approve time off request
  approveTimeOffRequest: async (requestId: string): Promise<TimeOffRequestDto> => {
    const response = await api.post<TimeOffRequestDto>(`/timeoff/${requestId}/approve`);
    return response.data;
  },

  // Reject time off request
  rejectTimeOffRequest: async (requestId: string, data: RejectTimeOffDto): Promise<TimeOffRequestDto> => {
    const response = await api.post<TimeOffRequestDto>(`/timeoff/${requestId}/reject`, data);
    return response.data;
  },

  // Cancel time off request
  cancelTimeOffRequest: async (requestId: string): Promise<void> => {
    await api.post(`/timeoff/${requestId}/cancel`);
  },
};

export default staffService;
