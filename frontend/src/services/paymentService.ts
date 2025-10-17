import apiClient from './apiClient';
import type {
  CreatePaymentDto,
  PaymentResponseDto,
  PaymentDetail,
  RefundPaymentDto,
  PaymentStatisticsDto,
} from '../types/payment';

const paymentService = {
  // Create a new payment
  async createPayment(data: CreatePaymentDto): Promise<PaymentResponseDto> {
    const response = await apiClient.post<PaymentResponseDto>('/payments', data);
    return response.data;
  },

  // Get payment by ID
  async getPaymentById(id: string): Promise<PaymentDetail> {
    const response = await apiClient.get<PaymentDetail>(`/payments/${id}`);
    return response.data;
  },

  // Get user's payment history
  async getMyPayments(): Promise<PaymentDetail[]> {
    const response = await apiClient.get<PaymentDetail[]>('/payments/my-payments');
    return response.data;
  },

  // Get salon's payment history (salon owner only)
  async getSalonPayments(salonId: string): Promise<PaymentDetail[]> {
    const response = await apiClient.get<PaymentDetail[]>(`/payments/salon/${salonId}`);
    return response.data;
  },

  // Refund a payment
  async refundPayment(paymentId: string, data: RefundPaymentDto): Promise<PaymentResponseDto> {
    const response = await apiClient.post<PaymentResponseDto>(
      `/payments/${paymentId}/refund`,
      data
    );
    return response.data;
  },

  // Get payment statistics (salon owner/admin only)
  async getPaymentStatistics(salonId?: string): Promise<PaymentStatisticsDto> {
    const params = salonId ? { salonId } : {};
    const response = await apiClient.get<PaymentStatisticsDto>('/payments/statistics', {
      params,
    });
    return response.data;
  },

  // Get test card numbers
  async getTestCards(): Promise<{ cards: Record<string, string>; note: string }> {
    const response = await apiClient.get<{ cards: Record<string, string>; note: string }>(
      '/payments/test-cards'
    );
    return response.data;
  },
};

export default paymentService;
