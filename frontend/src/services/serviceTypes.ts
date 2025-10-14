export interface Service {
  id: string;
  name: string;
  description?: string;
  price: number;
  durationMinutes: number;
  category?: string;
  isActive: boolean;
  notes?: string;
  imageUrl?: string;
  createdAt: string;
  createdBy: string;
  updatedAt: string;
  updatedBy?: string;
}

export interface ServiceListItem {
  id: string;
  name: string;
  category?: string;
  price: number;
  durationMinutes: number;
  isActive: boolean;
  createdAt: string;
  priceDisplay: string;
  durationDisplay: string;
  statusDisplay: string;
}

export interface CreateServiceRequest {
  name: string;
  description?: string;
  price: number;
  durationMinutes: number;
  category?: string;
  notes?: string;
  imageUrl?: string;
}

export interface UpdateServiceRequest {
  name: string;
  description?: string;
  price: number;
  durationMinutes: number;
  category?: string;
  notes?: string;
  imageUrl?: string;
  isActive: boolean;
}

// Category types
export interface ServiceCategory {
  id: string;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  serviceCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface ServiceCategoryListItem {
  id: string;
  name: string;
  description?: string;
  displayOrder: number;
  isActive: boolean;
  serviceCount: number;
  statusDisplay: string;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
  displayOrder?: number;
}

export interface UpdateCategoryRequest {
  name: string;
  description?: string;
  displayOrder?: number;
  isActive: boolean;
}