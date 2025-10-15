import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

export interface SubscriptionPlan {
  id: string;
  name: string;
  description: string;
  monthlyPrice: number;
  annualPrice: number;
  trialDays: number;
  isActive: boolean;
  isPopular: boolean;
  sortOrder: number;
  features: string; // JSON string
  badge?: string;
  color?: string;
  maxStaff: number;
  maxAppointmentsPerMonth: number;
  maxLocations: number;
  maxServices: number;
  hasAdvancedAnalytics: boolean;
  hasSMSNotifications: boolean;
  hasEmailNotifications: boolean;
  hasCustomBranding: boolean;
  hasAPIAccess: boolean;
  hasMultiLocation: boolean;
  hasPackageManagement: boolean;
  hasGoogleCalendarIntegration: boolean;
  hasPaymentIntegration: boolean;
}

export interface CurrentSubscription {
  id: string;
  plan: SubscriptionPlan;
  status: number; // SubscriptionStatus enum
  billingCycle: number; // BillingCycle enum (1=Monthly, 2=Annual)
  startDate: string;
  endDate?: string;
  trialEndDate?: string;
  nextBillingDate?: string;
  autoRenew: boolean;
  daysUntilExpiry: number;
  isTrialing: boolean;
}

export interface FeatureLimits {
  maxStaff: number;
  maxAppointmentsPerMonth: number;
  maxLocations: number;
  maxServices: number;
  currentStaffCount: number;
  currentAppointmentsThisMonth: number;
  currentLocationsCount: number;
  currentServicesCount: number;
  canAddStaff: boolean;
  canAddAppointment: boolean;
  canAddLocation: boolean;
  canAddService: boolean;
}

export interface UsageStats {
  staff: {
    current: number;
    max: number;
    percentage: number;
    canAdd: boolean;
  };
  appointments: {
    current: number;
    max: number;
    percentage: number;
    canAdd: boolean;
  };
  locations: {
    current: number;
    max: number;
    percentage: number;
    canAdd: boolean;
  };
  services: {
    current: number;
    max: number;
    percentage: number;
    canAdd: boolean;
  };
}

export interface Invoice {
  id: string;
  invoiceNumber: string;
  invoiceDate: string;
  dueDate: string;
  totalAmount: number;
  currency: string;
  status: number; // InvoiceStatus enum
  paidAt?: string;
  lineItems: InvoiceLineItem[];
}

export interface InvoiceLineItem {
  description: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface ProrationCalculation {
  currentPlanCost: number;
  newPlanCost: number;
  prorationCredit: number;
  amountToPay: number;
  daysRemainingInCycle: number;
  totalDaysInCycle: number;
}

class SubscriptionService {
  private getAuthHeaders() {
    const token = localStorage.getItem('token');
    return {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    };
  }

  async getPlans(): Promise<SubscriptionPlan[]> {
    const response = await axios.get(`${API_URL}/subscriptions/plans`);
    return response.data;
  }

  async getPlan(planId: string): Promise<SubscriptionPlan> {
    const response = await axios.get(`${API_URL}/subscriptions/plans/${planId}`);
    return response.data;
  }

  async getCurrentSubscription(): Promise<CurrentSubscription> {
    const response = await axios.get(
      `${API_URL}/subscriptions/current`,
      this.getAuthHeaders()
    );
    return response.data;
  }

  async createTrialSubscription(planId: string): Promise<CurrentSubscription> {
    const response = await axios.post(
      `${API_URL}/subscriptions/trial`,
      { subscriptionPlanId: planId },
      this.getAuthHeaders()
    );
    return response.data;
  }

  async upgradeSubscription(
    newPlanId: string,
    billingCycle: number,
    paymentMethodId?: string
  ): Promise<CurrentSubscription> {
    const response = await axios.post(
      `${API_URL}/subscriptions/upgrade`,
      {
        newPlanId,
        billingCycle,
        paymentMethodId,
      },
      this.getAuthHeaders()
    );
    return response.data;
  }

  async downgradeSubscription(newPlanId: string): Promise<CurrentSubscription> {
    const response = await axios.post(
      `${API_URL}/subscriptions/downgrade`,
      { newPlanId },
      this.getAuthHeaders()
    );
    return response.data;
  }

  async cancelSubscription(reason: string): Promise<void> {
    await axios.post(
      `${API_URL}/subscriptions/cancel`,
      { reason },
      this.getAuthHeaders()
    );
  }

  async getFeatureLimits(): Promise<FeatureLimits> {
    const response = await axios.get(
      `${API_URL}/subscriptions/feature-limits`,
      this.getAuthHeaders()
    );
    return response.data;
  }

  async checkFeatureLimit(featureName: string): Promise<boolean> {
    const response = await axios.get(
      `${API_URL}/subscriptions/feature-limits/${featureName}`,
      this.getAuthHeaders()
    );
    return response.data.available;
  }

  async getBillingHistory(): Promise<Invoice[]> {
    const response = await axios.get(
      `${API_URL}/subscriptions/billing-history`,
      this.getAuthHeaders()
    );
    return response.data;
  }

  async getInvoice(invoiceId: string): Promise<Invoice> {
    const response = await axios.get(
      `${API_URL}/subscriptions/invoices/${invoiceId}`,
      this.getAuthHeaders()
    );
    return response.data;
  }

  async getUsageStats(): Promise<UsageStats> {
    const response = await axios.get(
      `${API_URL}/subscriptions/usage`,
      this.getAuthHeaders()
    );
    return response.data;
  }

  async calculateProration(newPlanId: string): Promise<ProrationCalculation> {
    const response = await axios.post(
      `${API_URL}/subscriptions/calculate-proration`,
      newPlanId,
      this.getAuthHeaders()
    );
    return response.data;
  }
}

export default new SubscriptionService();
