// Payment enums
export const PaymentStatus = {
  Pending: 1,
  Completed: 2,
  Failed: 3,
  Cancelled: 4,
  Refunded: 5,
} as const;

export type PaymentStatus = typeof PaymentStatus[keyof typeof PaymentStatus];

export const PaymentMethod = {
  CreditCard: 1,
  DebitCard: 2,
  BankTransfer: 3,
  PayTR: 4,
  Other: 5,
} as const;

export type PaymentMethod = typeof PaymentMethod[keyof typeof PaymentMethod];

// Payment interfaces
export interface Payment {
  id: string;
  userId?: string;
  appointmentId?: string;
  subscriptionId?: string;
  amount: number;
  currency: string;
  method: PaymentMethod;
  status: PaymentStatus;
  transactionId?: string;
  paymentGateway?: string;
  paymentReference?: string;
  failureReason?: string;
  createdAt: string;
  paymentDate?: string;
  refundDate?: string;
  refundAmount?: number;
  paymentDetails?: string;
}

export interface PaymentDetail extends Payment {
  userEmail?: string;
  userName?: string;
}

// DTOs
export interface CreatePaymentDto {
  appointmentId?: string;
  subscriptionId?: string;
  amount: number;
  currency?: string;
  method?: PaymentMethod;
  userEmail?: string;
  
  // Card details (for fake POS)
  cardHolderName?: string;
  cardNumber?: string;
  expiryMonth?: string;
  expiryYear?: string;
  cvv?: string;
  
  // Callback URLs
  successUrl?: string;
  failureUrl?: string;
}

export interface PaymentResponseDto {
  paymentId: string;
  status: PaymentStatus;
  transactionId?: string;
  paymentGateway?: string;
  paymentReference?: string;
  failureReason?: string;
  amount: number;
  currency: string;
  createdAt: string;
  paymentDate?: string;
  
  // For redirect-based payments (like PayTR)
  paymentUrl?: string;
  token?: string;
}

export interface RefundPaymentDto {
  refundAmount?: number; // If null, full refund
  reason?: string;
}

export interface PaymentStatisticsDto {
  totalRevenue: number;
  totalPayments: number;
  successfulPayments: number;
  failedPayments: number;
  refundedPayments: number;
  averagePaymentAmount: number;
  paymentsByMethod: Record<string, number>;
  revenueByMonth: Record<string, number>;
}

// Test card numbers for FakePOS
export interface TestCard {
  number: string;
  description: string;
  expectedResult: 'success' | 'declined' | 'insufficient_funds' | 'expired_card' | 'incorrect_cvc' | 'processing_error';
}

export const TEST_CARDS: TestCard[] = [
  {
    number: '4242424242424242',
    description: 'Successful payment',
    expectedResult: 'success',
  },
  {
    number: '4000000000000002',
    description: 'Card declined',
    expectedResult: 'declined',
  },
  {
    number: '4000000000009995',
    description: 'Insufficient funds',
    expectedResult: 'insufficient_funds',
  },
  {
    number: '4000000000000069',
    description: 'Expired card',
    expectedResult: 'expired_card',
  },
  {
    number: '4000000000000127',
    description: 'Incorrect CVC',
    expectedResult: 'incorrect_cvc',
  },
  {
    number: '4000000000000119',
    description: 'Processing error',
    expectedResult: 'processing_error',
  },
];

// Helper functions
export const getPaymentStatusText = (status: PaymentStatus): string => {
  switch (status) {
    case PaymentStatus.Pending:
      return 'Beklemede';
    case PaymentStatus.Completed:
      return 'Tamamlandı';
    case PaymentStatus.Failed:
      return 'Başarısız';
    case PaymentStatus.Cancelled:
      return 'İptal Edildi';
    case PaymentStatus.Refunded:
      return 'İade Edildi';
    default:
      return 'Bilinmiyor';
  }
};

export const getPaymentStatusColor = (status: PaymentStatus): string => {
  switch (status) {
    case PaymentStatus.Pending:
      return 'warning';
    case PaymentStatus.Completed:
      return 'success';
    case PaymentStatus.Failed:
      return 'error';
    case PaymentStatus.Cancelled:
      return 'default';
    case PaymentStatus.Refunded:
      return 'info';
    default:
      return 'default';
  }
};

export const getPaymentMethodText = (method: PaymentMethod): string => {
  switch (method) {
    case PaymentMethod.CreditCard:
      return 'Kredi Kartı';
    case PaymentMethod.DebitCard:
      return 'Banka Kartı';
    case PaymentMethod.BankTransfer:
      return 'Banka Transferi';
    case PaymentMethod.PayTR:
      return 'PayTR';
    case PaymentMethod.Other:
      return 'Diğer';
    default:
      return 'Bilinmiyor';
  }
};

export const formatCurrency = (amount: number, currency: string = 'TRY'): string => {
  return new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency,
  }).format(amount);
};

export const formatCardNumber = (cardNumber: string): string => {
  return cardNumber.replace(/(\d{4})/g, '$1 ').trim();
};

export const maskCardNumber = (cardNumber: string): string => {
  if (cardNumber.length < 4) return cardNumber;
  const lastFour = cardNumber.slice(-4);
  return `**** **** **** ${lastFour}`;
};
