// Staff Management Types

export enum StaffStatus {
  Active = 0,
  Inactive = 1,
  PendingInvitation = 2
}

export interface StaffDto {
  id: string;
  userId: string;
  tenantId: string;
  roleId?: string;
  roleName?: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  status: StaffStatus;
  invitationToken?: string;
  invitationExpiresAt?: string;
  invitedAt?: string;
  invitedBy?: string;
  joinedAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface InviteStaffDto {
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  roleId?: string;
}

export interface AcceptInvitationDto {
  invitationToken: string;
  password: string;
}

export interface UpdateStaffProfileDto {
  firstName: string;
  lastName: string;
  phoneNumber?: string;
}

export interface RoleDto {
  id: string;
  tenantId: string;
  name: string;
  description?: string;
  permissions: string[];
  isSystemRole: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateRoleDto {
  name: string;
  description?: string;
  permissions: string[];
}

export interface AssignRoleDto {
  roleId: string;
}

// Staff Schedule Types
export enum DayOfWeek {
  Sunday = 0,
  Monday = 1,
  Tuesday = 2,
  Wednesday = 3,
  Thursday = 4,
  Friday = 5,
  Saturday = 6
}

export interface StaffScheduleDto {
  id: string;
  staffId: string;
  dayOfWeek: DayOfWeek;
  startTime: string; // TimeOnly as string "HH:mm:ss"
  endTime: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SetStaffScheduleDto {
  dayOfWeek: DayOfWeek;
  startTime: string; // "HH:mm:ss"
  endTime: string;
}

export interface AvailableTimeSlotDto {
  startTime: string; // DateTime
  endTime: string;
  isAvailable: boolean;
}

// Time Off Types
export enum TimeOffType {
  Vacation = 0,
  Sick = 1,
  Personal = 2,
  Other = 3
}

export enum TimeOffStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Cancelled = 3
}

export interface TimeOffRequestDto {
  id: string;
  staffId: string;
  staffName: string;
  type: TimeOffType;
  startDate: string;
  endDate: string;
  reason?: string;
  status: TimeOffStatus;
  approvedBy?: string;
  approvedAt?: string;
  rejectedReason?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTimeOffRequestDto {
  type: TimeOffType;
  startDate: string;
  endDate: string;
  reason?: string;
}

export interface RejectTimeOffDto {
  reason: string;
}
