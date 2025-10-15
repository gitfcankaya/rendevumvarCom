import axios from 'axios';

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

// Request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    const token = getAuthToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export interface SearchSalonDto {
  searchTerm?: string;
  city?: string;
  minRating?: number;
  latitude?: number;
  longitude?: number;
  radiusKm?: number;
  serviceIds?: string[];
  pageNumber?: number;
  pageSize?: number;
}

export interface SalonDto {
  id: string;
  tenantId: string;
  name: string;
  description?: string;
  phone: string;
  email?: string;
  website?: string;
  address: string;
  city: string;
  state?: string;
  postalCode?: string;
  latitude?: number;
  longitude?: number;
  businessHours?: string;
  averageRating: number;
  reviewCount: number;
  isActive: boolean;
  createdAt: string;
  primaryImageUrl?: string;
}

export interface SalonImageDto {
  id: string;
  salonId: string;
  imageUrl: string;
  isPrimary: boolean;
  displayOrder: number;
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
  staffIds?: string[];
  isActive: boolean;
}

export interface StaffDto {
  id: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone?: string;
  bio?: string;
  averageRating: number;
  profilePictureUrl?: string;
  roleId?: string;
  roleName?: string;
  status: string;
}

export interface SalonDetailsDto extends SalonDto {
  images: SalonImageDto[];
  services: ServiceDto[];
  staff: StaffDto[];
}

export interface SalonSearchResultDto {
  salons: SalonDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateSalonDto {
  name: string;
  description?: string;
  phone: string;
  email?: string;
  website?: string;
  address: string;
  city: string;
  state?: string;
  postalCode?: string;
  latitude?: number;
  longitude?: number;
  businessHours?: string;
}

export interface UpdateSalonDto extends CreateSalonDto {
  id: string;
  isActive: boolean;
}

export interface UploadImageDto {
  imageUrl: string;
  isPrimary: boolean;
}

export interface UpdateBusinessHoursDto {
  salonId: string;
  businessHours: string;
}

const salonService = {
  // Public endpoints
  searchSalons: async (params: SearchSalonDto): Promise<SalonSearchResultDto> => {
    const response = await api.get('/salons/search', { params });
    return response.data;
  },

  getSalonDetails: async (id: string): Promise<SalonDetailsDto> => {
    const response = await api.get(`/salons/${id}`);
    return response.data;
  },

  // Authenticated endpoints
  getMySalons: async (): Promise<SalonDto[]> => {
    const response = await api.get('/salons');
    return response.data;
  },

  createSalon: async (dto: CreateSalonDto): Promise<SalonDto> => {
    const response = await api.post('/salons', dto);
    return response.data;
  },

  updateSalon: async (id: string, dto: UpdateSalonDto): Promise<SalonDto> => {
    const response = await api.put(`/salons/${id}`, dto);
    return response.data;
  },

  deleteSalon: async (id: string): Promise<void> => {
    await api.delete(`/salons/${id}`);
  },

  uploadSalonImage: async (salonId: string, dto: UploadImageDto): Promise<SalonImageDto> => {
    const response = await api.post(`/salons/${salonId}/images`, dto);
    return response.data;
  },

  deleteSalonImage: async (imageId: string): Promise<void> => {
    await api.delete(`/salons/images/${imageId}`);
  },

  updateBusinessHours: async (salonId: string, dto: UpdateBusinessHoursDto): Promise<void> => {
    await api.put(`/salons/${salonId}/business-hours`, dto);
  },

  // Service-related endpoints
  getSalonServices: async (salonId: string): Promise<ServiceDto[]> => {
    const response = await api.get(`/services/salon/${salonId}`);
    return response.data;
  },

  // Staff-related endpoints
  getSalonStaff: async (salonId: string): Promise<StaffDto[]> => {
    const response = await api.get(`/staff/salon/${salonId}`);
    return response.data;
  },
};

export default salonService;
