import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Box,
  Typography,
  Alert,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Grid,
  Paper,
  Chip,
  Divider,
  CircularProgress,
} from '@mui/material';
import {
  CreditCard as CreditCardIcon,
  Info as InfoIcon,
} from '@mui/icons-material';
import { TEST_CARDS, formatCardNumber, formatCurrency } from '../types/payment';
import type { CreatePaymentDto } from '../types/payment';

interface PaymentFormDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (paymentData: CreatePaymentDto) => Promise<void>;
  amount: number;
  currency?: string;
  appointmentId?: string;
  subscriptionId?: string;
  title?: string;
}

const PaymentFormDialog: React.FC<PaymentFormDialogProps> = ({
  open,
  onClose,
  onSubmit,
  amount,
  currency = 'TRY',
  appointmentId,
  subscriptionId,
  title = 'Ödeme Bilgileri',
}) => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showTestCards, setShowTestCards] = useState(true);
  
  const [formData, setFormData] = useState<CreatePaymentDto>({
    appointmentId,
    subscriptionId,
    amount,
    currency,
    cardHolderName: '',
    cardNumber: '',
    expiryMonth: '',
    expiryYear: '',
    cvv: '',
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  const handleChange = (field: keyof CreatePaymentDto) => (
    event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
  ) => {
    let value = event.target.value;

    // Format card number
    if (field === 'cardNumber') {
      value = value.replace(/\s/g, '').replace(/(\d{4})/g, '$1 ').trim();
      if (value.replace(/\s/g, '').length > 16) return;
    }

    // Format CVV
    if (field === 'cvv' && value.length > 3) return;

    // Format expiry
    if (field === 'expiryMonth' || field === 'expiryYear') {
      value = value.replace(/\D/g, '');
      if (field === 'expiryMonth' && value.length > 2) return;
      if (field === 'expiryYear' && value.length > 2) return;
    }

    setFormData((prev) => ({ ...prev, [field]: value }));
    
    // Clear error for this field
    if (errors[field]) {
      setErrors((prev) => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  };

  const selectTestCard = (cardNumber: string) => {
    setFormData((prev) => ({
      ...prev,
      cardNumber: formatCardNumber(cardNumber),
      cardHolderName: 'Test User',
      expiryMonth: '12',
      expiryYear: '25',
      cvv: '123',
    }));
    setShowTestCards(false);
  };

  const validateForm = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.cardHolderName?.trim()) {
      newErrors.cardHolderName = 'Kart sahibi adı gereklidir';
    }

    const cardNumber = formData.cardNumber?.replace(/\s/g, '');
    if (!cardNumber) {
      newErrors.cardNumber = 'Kart numarası gereklidir';
    } else if (cardNumber.length !== 16) {
      newErrors.cardNumber = 'Kart numarası 16 haneli olmalıdır';
    }

    if (!formData.expiryMonth) {
      newErrors.expiryMonth = 'Ay gereklidir';
    } else {
      const month = parseInt(formData.expiryMonth);
      if (month < 1 || month > 12) {
        newErrors.expiryMonth = 'Geçerli bir ay giriniz (01-12)';
      }
    }

    if (!formData.expiryYear) {
      newErrors.expiryYear = 'Yıl gereklidir';
    }

    if (!formData.cvv) {
      newErrors.cvv = 'CVV gereklidir';
    } else if (formData.cvv.length !== 3) {
      newErrors.cvv = 'CVV 3 haneli olmalıdır';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validateForm()) return;

    setLoading(true);
    setError(null);

    try {
      await onSubmit({
        ...formData,
        cardNumber: formData.cardNumber?.replace(/\s/g, ''),
      });
      onClose();
    } catch (err: any) {
      setError(err.response?.data?.error || err.message || 'Ödeme işlemi başarısız oldu');
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    if (!loading) {
      setFormData({
        appointmentId,
        subscriptionId,
        amount,
        currency,
        cardHolderName: '',
        cardNumber: '',
        expiryMonth: '',
        expiryYear: '',
        cvv: '',
      });
      setErrors({});
      setError(null);
      setShowTestCards(true);
      onClose();
    }
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        <Box display="flex" alignItems="center" gap={1}>
          <CreditCardIcon />
          {title}
        </Box>
      </DialogTitle>
      
      <DialogContent>
        {/* Amount Display */}
        <Paper elevation={0} sx={{ p: 2, mb: 3, bgcolor: 'primary.50' }}>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            Ödenecek Tutar
          </Typography>
          <Typography variant="h5" color="primary" fontWeight="bold">
            {formatCurrency(amount, currency)}
          </Typography>
        </Paper>

        {/* Test Cards Info */}
        {showTestCards && (
          <Alert severity="info" icon={<InfoIcon />} sx={{ mb: 3 }}>
            <Typography variant="subtitle2" gutterBottom>
              Test Kartları (Geliştirme Modu)
            </Typography>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1, mt: 1 }}>
              {TEST_CARDS.map((card) => (
                <Chip
                  key={card.number}
                  label={card.description}
                  size="small"
                  onClick={() => selectTestCard(card.number)}
                  sx={{ cursor: 'pointer' }}
                  color={card.expectedResult === 'success' ? 'success' : 'default'}
                />
              ))}
            </Box>
            <Typography variant="caption" display="block" sx={{ mt: 1 }}>
              Bir test kartına tıklayarak formu otomatik doldurun
            </Typography>
          </Alert>
        )}

        {error && (
          <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        {/* Payment Form */}
        <Box component="form" noValidate>
          <TextField
            fullWidth
            label="Kart Sahibi Adı"
            value={formData.cardHolderName || ''}
            onChange={handleChange('cardHolderName')}
            error={!!errors.cardHolderName}
            helperText={errors.cardHolderName}
            disabled={loading}
            sx={{ mb: 2 }}
            inputProps={{ style: { textTransform: 'uppercase' } }}
          />

          <TextField
            fullWidth
            label="Kart Numarası"
            value={formData.cardNumber || ''}
            onChange={handleChange('cardNumber')}
            error={!!errors.cardNumber}
            helperText={errors.cardNumber}
            disabled={loading}
            placeholder="1234 5678 9012 3456"
            sx={{ mb: 2 }}
            InputProps={{
              startAdornment: <CreditCardIcon sx={{ mr: 1, color: 'action.active' }} />,
            }}
          />

          <Grid container spacing={2}>
            <Grid item xs={6}>
              <Box display="flex" gap={1}>
                <TextField
                  label="Ay"
                  value={formData.expiryMonth || ''}
                  onChange={handleChange('expiryMonth')}
                  error={!!errors.expiryMonth}
                  helperText={errors.expiryMonth}
                  disabled={loading}
                  placeholder="MM"
                  inputProps={{ maxLength: 2 }}
                  fullWidth
                />
                <TextField
                  label="Yıl"
                  value={formData.expiryYear || ''}
                  onChange={handleChange('expiryYear')}
                  error={!!errors.expiryYear}
                  helperText={errors.expiryYear}
                  disabled={loading}
                  placeholder="YY"
                  inputProps={{ maxLength: 2 }}
                  fullWidth
                />
              </Box>
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="CVV"
                value={formData.cvv || ''}
                onChange={handleChange('cvv')}
                error={!!errors.cvv}
                helperText={errors.cvv || 'Kartın arkasındaki 3 haneli kod'}
                disabled={loading}
                placeholder="123"
                inputProps={{ maxLength: 3 }}
                type="password"
              />
            </Grid>
          </Grid>

          <Divider sx={{ my: 3 }} />

          <Typography variant="caption" color="text.secondary" display="block" sx={{ mb: 1 }}>
            🔒 Ödeme bilgileriniz güvenli bir şekilde işlenmektedir.
          </Typography>
        </Box>
      </DialogContent>

      <DialogActions>
        <Button onClick={handleClose} disabled={loading}>
          İptal
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={loading}
          startIcon={loading ? <CircularProgress size={20} /> : <CreditCardIcon />}
        >
          {loading ? 'İşleniyor...' : `${formatCurrency(amount, currency)} Öde`}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default PaymentFormDialog;
