// Admin Panel Types

export enum UserRole {
  Customer = 0,
  SalonOwner = 1,
  Staff = 2,
  Admin = 3
}

export enum SalonStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Suspended = 3,
  Closed = 4
}

// User Management Types
export interface AdminUserListDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phone?: string;
  role: UserRole;
  roleName: string;
  isActive: boolean;
  emailConfirmed: boolean;
  createdAt: string;
  lastLoginAt?: string;
}

export interface AdminUserDetailDto {
  id: string;
  tenantId?: string;
  email: string;
  firstName: string;
  lastName: string;
  phone?: string;
  role: UserRole;
  profilePictureUrl?: string;
  emailConfirmed: boolean;
  phoneConfirmed: boolean;
  isActive: boolean;
  lastLoginAt?: string;
  createdAt: string;
  updatedAt: string;
  totalAppointments: number;
  totalReviews: number;
  totalSpent: number;
}

export interface UpdateUserRoleDto {
  role: UserRole;
}

export interface UserFilterDto {
  searchTerm?: string;
  role?: UserRole;
  isActive?: boolean;
  emailConfirmed?: boolean;
  createdFrom?: string;
  createdTo?: string;
  page: number;
  pageSize: number;
  sortBy: string;
  sortDescending: boolean;
}

export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// Salon Approval Types
export interface PendingSalonDto {
  id: string;
  name: string;
  description?: string;
  address: string;
  city: string;
  phone?: string;
  email?: string;
  ownerId: string;
  ownerName: string;
  ownerEmail: string;
  status: SalonStatus;
  createdAt: string;
  approvedAt?: string;
  rejectionReason?: string;
}

export interface ApproveSalonDto {
  notes?: string;
}

export interface RejectSalonDto {
  rejectionReason: string;
}

// System Settings Types
export interface SystemSettingsDto {
  maintenanceMode: boolean;
  maintenanceMessage?: string;
  allowNewRegistrations: boolean;
  allowNewSalons: boolean;
  emailNotificationsEnabled: boolean;
  smsNotificationsEnabled: boolean;
  maxAppointmentsPerDay: number;
  appointmentCancellationHours: number;
  featureFlags: Record<string, boolean>;
}

export interface UpdateSystemSettingsDto {
  maintenanceMode?: boolean;
  maintenanceMessage?: string;
  allowNewRegistrations?: boolean;
  allowNewSalons?: boolean;
  emailNotificationsEnabled?: boolean;
  smsNotificationsEnabled?: boolean;
  maxAppointmentsPerDay?: number;
  appointmentCancellationHours?: number;
}

export interface FeatureFlagDto {
  key: string;
  enabled: boolean;
  description: string;
}

// Helper functions
export const getUserRoleName = (role: UserRole): string => {
  switch (role) {
    case UserRole.Customer:
      return 'Customer';
    case UserRole.SalonOwner:
      return 'Salon Owner';
    case UserRole.Staff:
      return 'Staff';
    case UserRole.Admin:
      return 'Admin';
    default:
      return 'Unknown';
  }
};

export const getSalonStatusName = (status: SalonStatus): string => {
  switch (status) {
    case SalonStatus.Pending:
      return 'Pending';
    case SalonStatus.Approved:
      return 'Approved';
    case SalonStatus.Rejected:
      return 'Rejected';
    case SalonStatus.Suspended:
      return 'Suspended';
    case SalonStatus.Closed:
      return 'Closed';
    default:
      return 'Unknown';
  }
};

export const getSalonStatusColor = (status: SalonStatus): 'warning' | 'success' | 'error' | 'default' => {
  switch (status) {
    case SalonStatus.Pending:
      return 'warning';
    case SalonStatus.Approved:
      return 'success';
    case SalonStatus.Rejected:
    case SalonStatus.Closed:
      return 'error';
    case SalonStatus.Suspended:
      return 'default';
    default:
      return 'default';
  }
};
