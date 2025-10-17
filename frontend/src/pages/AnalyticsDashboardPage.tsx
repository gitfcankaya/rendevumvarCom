import React, { useState, useEffect } from 'react';
import { 
  Container, 
  Paper, 
  Typography, 
  Box,
  Card,
  CardContent,
  CircularProgress
} from '@mui/material';
import { 
  TrendingUp as TrendingUpIcon,
  TrendingDown as TrendingDownIcon,
  CalendarToday as CalendarIcon,
  People as PeopleIcon,
  AttachMoney as MoneyIcon,
  Star as StarIcon
} from '@mui/icons-material';
import analyticsService from '../services/analyticsService';
import type { DashboardAnalytics } from '../types/analytics';
import { formatCurrency, formatPercentage } from '../types/analytics';

const AnalyticsDashboardPage: React.FC = () => {
  const [loading, setLoading] = useState(true);
  const [dashboard, setDashboard] = useState<DashboardAnalytics | null>(null);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    fetchDashboard();
  }, []);

  const fetchDashboard = async () => {
    try {
      setLoading(true);
      const data = await analyticsService.getDashboard();
      setDashboard(data);
    } catch (err: any) {
      setError('Dashboard verileri yüklenirken hata oluştu');
      console.error('Error fetching dashboard:', err);
    } finally {
      setLoading(false);
    }
  };

  const StatCard: React.FC<{
    title: string;
    value: string | number;
    growth?: number;
    icon: React.ReactNode;
    color: string;
  }> = ({ title, value, growth, icon, color }) => (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Box>
            <Typography color="textSecondary" variant="body2" gutterBottom>
              {title}
            </Typography>
            <Typography variant="h4" component="div" fontWeight="bold">
              {value}
            </Typography>
            {growth !== undefined && (
              <Box display="flex" alignItems="center" mt={1}>
                {growth >= 0 ? (
                  <TrendingUpIcon fontSize="small" sx={{ color: 'success.main', mr: 0.5 }} />
                ) : (
                  <TrendingDownIcon fontSize="small" sx={{ color: 'error.main', mr: 0.5 }} />
                )}
                <Typography
                  variant="body2"
                  color={growth >= 0 ? 'success.main' : 'error.main'}
                >
                  {formatPercentage(growth)}
                </Typography>
              </Box>
            )}
          </Box>
          <Box
            sx={{
              backgroundColor: `${color}.light`,
              borderRadius: 2,
              p: 1.5,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}
          >
            {icon}
          </Box>
        </Box>
      </CardContent>
    </Card>
  );

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (error || !dashboard) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
        <Paper sx={{ p: 3, textAlign: 'center' }}>
          <Typography color="error">{error || 'Veri bulunamadı'}</Typography>
        </Paper>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Typography variant="h4" component="h1" gutterBottom fontWeight="bold">
        Dashboard & Analytics
      </Typography>
      <Typography variant="body1" color="textSecondary" paragraph>
        İşletme performansınızın genel görünümü
      </Typography>

      {/* Key Metrics */}
      <Box display="grid" gridTemplateColumns="repeat(auto-fit, minmax(250px, 1fr))" gap={3} sx={{ mb: 4 }}>
        <StatCard
          title="Toplam Gelir"
          value={formatCurrency(dashboard.totalRevenue)}
          growth={dashboard.revenueGrowthPercentage}
          icon={<MoneyIcon sx={{ color: 'primary.main', fontSize: 40 }} />}
          color="primary"
        />
        <StatCard
          title="Randevular"
          value={dashboard.totalAppointments}
          growth={dashboard.appointmentGrowthPercentage}
          icon={<CalendarIcon sx={{ color: 'info.main', fontSize: 40 }} />}
          color="info"
        />
        <StatCard
          title="Müşteriler"
          value={dashboard.totalCustomers}
          growth={dashboard.customerGrowthPercentage}
          icon={<PeopleIcon sx={{ color: 'success.main', fontSize: 40 }} />}
          color="success"
        />
        <StatCard
          title="Ortalama Puan"
          value={dashboard.averageRating.toFixed(1)}
          icon={<StarIcon sx={{ color: 'warning.main', fontSize: 40 }} />}
          color="warning"
        />
      </Box>

      {/* Top Services & Staff */}
      <Box display="grid" gridTemplateColumns={{ xs: '1fr', md: '1fr 1fr' }} gap={3} sx={{ mb: 4 }}>
        <Paper sx={{ p: 3, height: '100%' }}>
          <Typography variant="h6" gutterBottom fontWeight="bold">
            En Çok Tercih Edilen Hizmetler
          </Typography>
          {dashboard.topServices.slice(0, 5).map((service, index) => (
            <Box
              key={service.serviceId}
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                py: 2,
                borderBottom: index < 4 ? '1px solid #eee' : 'none'
              }}
            >
              <Box>
                <Typography variant="body1" fontWeight="medium">
                  {service.serviceName}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  {service.bookingCount} randevu
                </Typography>
              </Box>
              <Typography variant="h6" color="primary.main" fontWeight="bold">
                {formatCurrency(service.revenue)}
              </Typography>
            </Box>
          ))}
        </Paper>

        {/* Top Staff */}
        <Paper sx={{ p: 3, height: '100%' }}>
          <Typography variant="h6" gutterBottom fontWeight="bold">
            En Başarılı Personeller
          </Typography>
          {dashboard.topStaff.slice(0, 5).map((staff, index) => (
            <Box
              key={staff.staffId}
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                py: 2,
                borderBottom: index < 4 ? '1px solid #eee' : 'none'
              }}
            >
              <Box display="flex" alignItems="center" gap={2}>
                {staff.photoUrl ? (
                  <img
                    src={staff.photoUrl}
                    alt={staff.staffName}
                    style={{ width: 40, height: 40, borderRadius: '50%' }}
                  />
                ) : (
                  <Box
                    sx={{
                      width: 40,
                      height: 40,
                      borderRadius: '50%',
                      bgcolor: 'primary.light',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center'
                    }}
                  >
                    <Typography variant="body2" fontWeight="bold">
                      {staff.staffName.charAt(0)}
                    </Typography>
                  </Box>
                )}
                <Box>
                  <Typography variant="body1" fontWeight="medium">
                    {staff.staffName}
                  </Typography>
                  <Typography variant="body2" color="textSecondary">
                    {staff.appointmentCount} randevu • ★ {staff.averageRating.toFixed(1)}
                  </Typography>
                </Box>
              </Box>
              <Typography variant="h6" color="primary.main" fontWeight="bold">
                {formatCurrency(staff.revenue)}
              </Typography>
            </Box>
          ))}
        </Paper>
      </Box>

      {/* Recent Appointments */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom fontWeight="bold">
          Son Randevular
        </Typography>
        <Box sx={{ overflowX: 'auto' }}>
          {dashboard.recentAppointments.map((appointment, index) => (
            <Box
              key={appointment.id}
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                py: 2,
                borderBottom: index < dashboard.recentAppointments.length - 1 ? '1px solid #eee' : 'none',
                flexWrap: 'wrap',
                gap: 2
              }}
            >
              <Box flex={1} minWidth={200}>
                <Typography variant="body1" fontWeight="medium">
                  {appointment.customerName}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  {appointment.serviceName} • {appointment.staffName}
                </Typography>
              </Box>
              <Typography variant="body2" color="textSecondary" minWidth={120}>
                {new Date(appointment.startTime).toLocaleString('tr-TR', {
                  day: 'numeric',
                  month: 'short',
                  hour: '2-digit',
                  minute: '2-digit'
                })}
              </Typography>
              <Typography variant="body1" fontWeight="bold" minWidth={80} textAlign="right">
                {formatCurrency(appointment.totalPrice)}
              </Typography>
            </Box>
          ))}
        </Box>
      </Paper>
    </Container>
  );
};

export default AnalyticsDashboardPage;
