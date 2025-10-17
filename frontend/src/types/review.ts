export interface Review {
  id: string;
  appointmentId: string;
  customerId: string;
  customerName: string;
  salonId: string;
  salonName: string;
  staffId?: string;
  staffName?: string;
  rating: number;
  comment?: string;
  response?: string;
  responseBy?: string;
  responseByName?: string;
  responseAt?: string;
  isPublished: boolean;
  createdAt: string;
}

export interface CreateReviewDto {
  appointmentId: string;
  salonId: string;
  staffId?: string;
  rating: number;
  comment?: string;
}

export interface UpdateReviewDto {
  rating: number;
  comment?: string;
}

export interface ReviewResponseDto {
  response: string;
}

export interface SalonRating {
  salonId: string;
  averageRating: number;
  totalReviews: number;
  ratingDistribution: {
    [key: number]: number;
  };
}

export interface StaffRating {
  staffId: string;
  averageRating: number;
  totalReviews: number;
}
