import React, { useEffect, useState } from 'react';
import {
  Container,
  Typography,
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Button,
  Alert,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  Receipt as ReceiptIcon,
  Undo as UndoIcon,
} from '@mui/icons-material';
import paymentService from '../services/paymentService';
import type {
  PaymentDetail,
} from '../types/payment';
import {
  PaymentStatus,
  getPaymentStatusText,
  getPaymentStatusColor,
  getPaymentMethodText,
  formatCurrency,
} from '../types/payment';

const PaymentHistoryPage: React.FC = () => {
  const [payments, setPayments] = useState<PaymentDetail[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refundDialogOpen, setRefundDialogOpen] = useState(false);
  const [selectedPayment, setSelectedPayment] = useState<PaymentDetail | null>(null);
  const [refundReason, setRefundReason] = useState('');
  const [refunding, setRefunding] = useState(false);

  useEffect(() => {
    fetchPayments();
  }, []);

  const fetchPayments = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await paymentService.getMyPayments();
      setPayments(data);
    } catch (err: any) {
      setError(err.response?.data?.error || err.message || 'Ödemeler yüklenirken hata oluştu');
    } finally {
      setLoading(false);
    }
  };

  const handleRefundClick = (payment: PaymentDetail) => {
    setSelectedPayment(payment);
    setRefundReason('');
    setRefundDialogOpen(true);
  };

  const handleRefundSubmit = async () => {
    if (!selectedPayment) return;

    try {
      setRefunding(true);
      await paymentService.refundPayment(selectedPayment.id, {
        reason: refundReason || undefined,
      });
      
      setRefundDialogOpen(false);
      setSelectedPayment(null);
      setRefundReason('');
      
      // Refresh payments list
      await fetchPayments();
    } catch (err: any) {
      setError(err.response?.data?.error || err.message || 'İade işlemi başarısız oldu');
    } finally {
      setRefunding(false);
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleString('tr-TR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4, textAlign: 'center' }}>
        <CircularProgress />
        <Typography sx={{ mt: 2 }}>Ödemeler yükleniyor...</Typography>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Box display="flex" alignItems="center" gap={2}>
          <ReceiptIcon sx={{ fontSize: 40, color: 'primary.main' }} />
          <Typography variant="h4" component="h1">
            Ödeme Geçmişi
          </Typography>
        </Box>
        <Tooltip title="Yenile">
          <IconButton onClick={fetchPayments} disabled={loading}>
            <RefreshIcon />
          </IconButton>
        </Tooltip>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {payments.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <ReceiptIcon sx={{ fontSize: 64, color: 'text.disabled', mb: 2 }} />
          <Typography variant="h6" color="text.secondary" gutterBottom>
            Henüz ödeme geçmişiniz bulunmuyor
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Randevu oluşturduktan sonra ödemeleriniz burada görünecek
          </Typography>
        </Paper>
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Tarih</TableCell>
                <TableCell>İşlem No</TableCell>
                <TableCell>Tutar</TableCell>
                <TableCell>Ödeme Yöntemi</TableCell>
                <TableCell>Durum</TableCell>
                <TableCell>Ödeme Gateway</TableCell>
                <TableCell align="right">İşlemler</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {payments.map((payment) => (
                <TableRow key={payment.id} hover>
                  <TableCell>
                    <Typography variant="body2">
                      {formatDate(payment.createdAt)}
                    </Typography>
                    {payment.paymentDate && payment.paymentDate !== payment.createdAt && (
                      <Typography variant="caption" color="text.secondary" display="block">
                        Ödendi: {formatDate(payment.paymentDate)}
                      </Typography>
                    )}
                  </TableCell>
                  
                  <TableCell>
                    <Typography variant="body2" fontFamily="monospace">
                      {payment.transactionId || '-'}
                    </Typography>
                    {payment.paymentReference && (
                      <Typography variant="caption" color="text.secondary" display="block">
                        Ref: {payment.paymentReference}
                      </Typography>
                    )}
                  </TableCell>
                  
                  <TableCell>
                    <Typography variant="body1" fontWeight="bold">
                      {formatCurrency(payment.amount, payment.currency)}
                    </Typography>
                    {payment.refundAmount && (
                      <Typography variant="caption" color="error" display="block">
                        İade: {formatCurrency(payment.refundAmount, payment.currency)}
                      </Typography>
                    )}
                  </TableCell>
                  
                  <TableCell>
                    {getPaymentMethodText(payment.method)}
                  </TableCell>
                  
                  <TableCell>
                    <Chip
                      label={getPaymentStatusText(payment.status)}
                      color={getPaymentStatusColor(payment.status) as any}
                      size="small"
                    />
                    {payment.failureReason && (
                      <Typography variant="caption" color="error" display="block" sx={{ mt: 0.5 }}>
                        {payment.failureReason}
                      </Typography>
                    )}
                  </TableCell>
                  
                  <TableCell>
                    {payment.paymentGateway || '-'}
                  </TableCell>
                  
                  <TableCell align="right">
                    {payment.status === PaymentStatus.Completed && !payment.refundDate && (
                      <Button
                        size="small"
                        startIcon={<UndoIcon />}
                        onClick={() => handleRefundClick(payment)}
                        color="error"
                        variant="outlined"
                      >
                        İade
                      </Button>
                    )}
                    {payment.refundDate && (
                      <Typography variant="caption" color="text.secondary">
                        İade edildi: {formatDate(payment.refundDate)}
                      </Typography>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      {/* Refund Dialog */}
      <Dialog open={refundDialogOpen} onClose={() => !refunding && setRefundDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Ödeme İadesi</DialogTitle>
        <DialogContent>
          {selectedPayment && (
            <>
              <Alert severity="warning" sx={{ mb: 2 }}>
                Bu ödemeyi iade etmek istediğinizden emin misiniz?
              </Alert>
              
              <Box sx={{ mb: 2 }}>
                <Typography variant="body2" color="text.secondary">
                  İade Tutarı
                </Typography>
                <Typography variant="h6">
                  {formatCurrency(selectedPayment.amount, selectedPayment.currency)}
                </Typography>
              </Box>

              <TextField
                fullWidth
                label="İade Nedeni (Opsiyonel)"
                multiline
                rows={3}
                value={refundReason}
                onChange={(e) => setRefundReason(e.target.value)}
                disabled={refunding}
                placeholder="İade nedeninizi yazınız..."
              />
            </>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRefundDialogOpen(false)} disabled={refunding}>
            İptal
          </Button>
          <Button
            onClick={handleRefundSubmit}
            color="error"
            variant="contained"
            disabled={refunding}
            startIcon={refunding ? <CircularProgress size={20} /> : <UndoIcon />}
          >
            {refunding ? 'İşleniyor...' : 'İade Et'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default PaymentHistoryPage;
