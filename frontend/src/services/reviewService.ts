import apiClient from './apiClient';
import type {
  Review,
  CreateReviewDto,
  UpdateReviewDto,
  ReviewResponseDto,
  SalonRating,
  StaffRating,
} from '../types/review';

const API_URL = '/reviews';

export const reviewService = {
  // Create a new review
  createReview: async (dto: CreateReviewDto): Promise<Review> => {
    const response = await apiClient.post<Review>(API_URL, dto);
    return response.data;
  },

  // Update an existing review
  updateReview: async (id: string, dto: UpdateReviewDto): Promise<Review> => {
    const response = await apiClient.put<Review>(`${API_URL}/${id}`, dto);
    return response.data;
  },

  // Delete a review
  deleteReview: async (id: string): Promise<void> => {
    await apiClient.delete(`${API_URL}/${id}`);
  },

  // Get a specific review by ID
  getReviewById: async (id: string): Promise<Review> => {
    const response = await apiClient.get<Review>(`${API_URL}/${id}`);
    return response.data;
  },

  // Get all reviews for a salon
  getReviewsBySalon: async (
    salonId: string,
    includeUnpublished = false
  ): Promise<Review[]> => {
    const response = await apiClient.get<Review[]>(
      `${API_URL}/salon/${salonId}`,
      {
        params: { includeUnpublished },
      }
    );
    return response.data;
  },

  // Get all reviews by the current customer
  getMyReviews: async (): Promise<Review[]> => {
    const response = await apiClient.get<Review[]>(`${API_URL}/my-reviews`);
    return response.data;
  },

  // Get all reviews for a staff member
  getReviewsByStaff: async (
    staffId: string,
    includeUnpublished = false
  ): Promise<Review[]> => {
    const response = await apiClient.get<Review[]>(
      `${API_URL}/staff/${staffId}`,
      {
        params: { includeUnpublished },
      }
    );
    return response.data;
  },

  // Get review for a specific appointment
  getReviewByAppointment: async (appointmentId: string): Promise<Review> => {
    const response = await apiClient.get<Review>(
      `${API_URL}/appointment/${appointmentId}`
    );
    return response.data;
  },

  // Get rating statistics for a salon
  getSalonRating: async (salonId: string): Promise<SalonRating> => {
    const response = await apiClient.get<SalonRating>(
      `${API_URL}/salon/${salonId}/rating`
    );
    return response.data;
  },

  // Get rating statistics for a staff member
  getStaffRating: async (staffId: string): Promise<StaffRating> => {
    const response = await apiClient.get<StaffRating>(
      `${API_URL}/staff/${staffId}/rating`
    );
    return response.data;
  },

  // Add a response to a review (salon owner only)
  addResponse: async (
    id: string,
    dto: ReviewResponseDto
  ): Promise<Review> => {
    const response = await apiClient.post<Review>(
      `${API_URL}/${id}/response`,
      dto
    );
    return response.data;
  },

  // Toggle publish status of a review (salon owner only)
  togglePublish: async (id: string): Promise<Review> => {
    const response = await apiClient.patch<Review>(
      `${API_URL}/${id}/toggle-publish`
    );
    return response.data;
  },
};

export default reviewService;
