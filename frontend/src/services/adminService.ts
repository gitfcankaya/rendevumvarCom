import apiClient from './apiClient';
import type {
  AdminUserListDto,
  AdminUserDetailDto,
  UpdateUserRoleDto,
  UserFilterDto,
  PagedResultDto,
  PendingSalonDto,
  ApproveSalonDto,
  RejectSalonDto,
  SystemSettingsDto,
  UpdateSystemSettingsDto
} from '../types/admin';

// User Management
export const getUsers = async (filter: UserFilterDto): Promise<PagedResultDto<AdminUserListDto>> => {
  const params = new URLSearchParams();
  
  if (filter.searchTerm) params.append('searchTerm', filter.searchTerm);
  if (filter.role !== undefined) params.append('role', filter.role.toString());
  if (filter.isActive !== undefined) params.append('isActive', filter.isActive.toString());
  if (filter.emailConfirmed !== undefined) params.append('emailConfirmed', filter.emailConfirmed.toString());
  if (filter.createdFrom) params.append('createdFrom', filter.createdFrom);
  if (filter.createdTo) params.append('createdTo', filter.createdTo);
  params.append('page', filter.page.toString());
  params.append('pageSize', filter.pageSize.toString());
  params.append('sortBy', filter.sortBy);
  params.append('sortDescending', filter.sortDescending.toString());

  const response = await apiClient.get<PagedResultDto<AdminUserListDto>>(
    `/admin/users?${params.toString()}`
  );
  return response.data;
};

export const getUserById = async (id: string): Promise<AdminUserDetailDto> => {
  const response = await apiClient.get<AdminUserDetailDto>(`/admin/users/${id}`);
  return response.data;
};

export const updateUserRole = async (id: string, data: UpdateUserRoleDto): Promise<void> => {
  await apiClient.put(`/admin/users/${id}/role`, data);
};

export const deleteUser = async (id: string): Promise<void> => {
  await apiClient.delete(`/admin/users/${id}`);
};

// Salon Approval
export const getPendingSalons = async (): Promise<PendingSalonDto[]> => {
  const response = await apiClient.get<PendingSalonDto[]>('/admin/salons/pending');
  return response.data;
};

export const approveSalon = async (id: string, data: ApproveSalonDto): Promise<void> => {
  await apiClient.put(`/admin/salons/${id}/approve`, data);
};

export const rejectSalon = async (id: string, data: RejectSalonDto): Promise<void> => {
  await apiClient.put(`/admin/salons/${id}/reject`, data);
};

// System Settings (placeholder for future implementation)
export const getSystemSettings = async (): Promise<SystemSettingsDto> => {
  const response = await apiClient.get<SystemSettingsDto>('/admin/settings');
  return response.data;
};

export const updateSystemSettings = async (data: UpdateSystemSettingsDto): Promise<void> => {
  await apiClient.put('/admin/settings', data);
};

const adminService = {
  // Users
  getUsers,
  getUserById,
  updateUserRole,
  deleteUser,
  
  // Salons
  getPendingSalons,
  approveSalon,
  rejectSalon,
  
  // Settings
  getSystemSettings,
  updateSystemSettings
};

export default adminService;
