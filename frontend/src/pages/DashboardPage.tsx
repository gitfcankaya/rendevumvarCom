import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Avatar,
  Button,
  Chip,
  List,
  ListItem,
  ListItemText,
  ListItemAvatar,
  Divider,
  LinearProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  ListItemButton,
  Grid
} from '@mui/material';
import {
  Person,
  CalendarToday,
  TrendingUp,
  Assessment,
  Today,
  ArrowUpward,
  ArrowDownward,
  Add,
  Edit,
  Delete,
  AccessTime,
  Build,
  Star
} from '@mui/icons-material';

// Import the appointment service
import { 
  appointmentService,
  type Appointment,
  type Service,
  type Staff,
  type DashboardStats
} from '../services/appointmentService';

const DashboardPage: React.FC = () => {
  // State for data
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [services, setServices] = useState<Service[]>([]);
  const [staff, setStaff] = useState<Staff[]>([]);

  // UI state
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogType, setDialogType] = useState<'appointment' | 'service' | 'staff' | 'customer'>('appointment');

  // Load data on component mount
  useEffect(() => {
    const loadData = async () => {
      try {
        const [statsData, appointmentsData, servicesData, staffData] = await Promise.all([
          appointmentService.getDashboardStats(),
          appointmentService.getAllAppointments(),
          appointmentService.getAllServices(),
          appointmentService.getAllStaff()
        ]);

        setStats(statsData);
        setAppointments(appointmentsData);
        setServices(servicesData);
        setStaff(staffData);
      } catch (error) {
        console.error('Error loading data:', error);
      }
    };

    loadData();
  }, []);

  const openNewDialog = (type: 'appointment' | 'service' | 'staff' | 'customer') => {
    setDialogType(type);
    setOpenDialog(true);
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'scheduled':
        return 'info';
      case 'completed':
        return 'success';
      case 'cancelled':
        return 'error';
      case 'in-progress':
        return 'warning';
      default:
        return 'default';
    }
  };

  const getStatusText = (status: string) => {
    switch (status) {
      case 'scheduled':
        return 'Planlandı';
      case 'completed':
        return 'Tamamlandı';
      case 'cancelled':
        return 'İptal';
      case 'in-progress':
        return 'Devam Ediyor';
      default:
        return status;
    }
  };

  return (
    <Box sx={{ flexGrow: 1, minHeight: '100vh', backgroundColor: '#f8f9fa' }}>
      <Container maxWidth="xl" sx={{ py: 4 }}>
        {/* Page Title */}
        <Box sx={{ mb: 4 }}>
          <Typography variant="h4" sx={{ fontWeight: 700, color: '#2d3748', mb: 1 }}>
            Dashboard
          </Typography>
          <Typography variant="body1" sx={{ color: '#718096' }}>
            Salon yönetim panelinize hoş geldiniz
          </Typography>
        </Box>

        {/* Navigation Buttons */}
        <Box sx={{ mb: 4, display: 'flex', gap: 2, flexWrap: 'wrap' }}>
          <Button
            component={Link}
            to="/appointments"
            variant="contained"
            startIcon={<CalendarToday />}
            sx={{
              borderRadius: 3,
              textTransform: 'none',
              fontWeight: 600,
              px: 3,
              py: 1.5,
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              '&:hover': {
                background: 'linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%)',
              }
            }}
          >
            Randevu Yönetimi
          </Button>
          <Button
            component={Link}
            to="/services"
            variant="contained"
            startIcon={<Build />}
            sx={{
              borderRadius: 3,
              textTransform: 'none',
              fontWeight: 600,
              px: 3,
              py: 1.5,
              background: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
              '&:hover': {
                background: 'linear-gradient(135deg, #0e8074 0%, #2dd36f 100%)',
              }
            }}
          >
            Hizmet Yönetimi
          </Button>
          <Button
            variant="outlined"
            startIcon={<Assessment />}
            sx={{
              borderRadius: 3,
              textTransform: 'none',
              fontWeight: 600,
              px: 3,
              py: 1.5,
              borderColor: '#667eea',
              color: '#667eea',
              '&:hover': {
                borderColor: '#5a6fd8',
                background: 'rgba(102, 126, 234, 0.04)',
              }
            }}
          >
            Raporlar
          </Button>
          <Button
            variant="outlined"
            startIcon={<Person />}
            sx={{
              borderRadius: 3,
              textTransform: 'none',
              fontWeight: 600,
              px: 3,
              py: 1.5,
              borderColor: '#667eea',
              color: '#667eea',
              '&:hover': {
                borderColor: '#5a6fd8',
                background: 'rgba(102, 126, 234, 0.04)',
              }
            }}
          >
            Müşteri Yönetimi
          </Button>
        </Box>

        {/* Stats Cards */}
        <Grid container spacing={3} sx={{ mb: 4 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card 
              sx={{ 
                borderRadius: 3,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                color: 'white',
                position: 'relative',
                overflow: 'hidden'
              }}
            >
              <Box sx={{
                position: 'absolute',
                top: 0,
                right: 0,
                width: 100,
                height: 100,
                borderRadius: '50%',
                background: 'rgba(255,255,255,0.1)',
                transform: 'translate(30px, -30px)'
              }} />
              <CardContent sx={{ position: 'relative', zIndex: 1 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <Box>
                    <Typography variant="h3" sx={{ fontWeight: 700, mb: 1 }}>
                      {stats?.todayAppointments || 0}
                    </Typography>
                    <Typography variant="body2" sx={{ opacity: 0.9 }}>
                      Bugünkü Randevular
                    </Typography>
                  </Box>
                  <Today sx={{ fontSize: 40, opacity: 0.7 }} />
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', mt: 2 }}>
                  {stats?.appointmentGrowth && stats.appointmentGrowth > 0 ? (
                    <>
                      <ArrowUpward sx={{ fontSize: 16, mr: 0.5 }} />
                      <Typography variant="body2">+{stats.appointmentGrowth}% bu hafta</Typography>
                    </>
                  ) : (
                    <>
                      <ArrowDownward sx={{ fontSize: 16, mr: 0.5 }} />
                      <Typography variant="body2">{stats?.appointmentGrowth || 0}% bu hafta</Typography>
                    </>
                  )}
                </Box>
              </CardContent>
            </Card>
          </Grid>

          <Grid xs={12} sm={6} md={3}>
            <Card 
              sx={{ 
                borderRadius: 3,
                background: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
                color: 'white',
                position: 'relative',
                overflow: 'hidden'
              }}
            >
              <Box sx={{
                position: 'absolute',
                top: 0,
                right: 0,
                width: 100,
                height: 100,
                borderRadius: '50%',
                background: 'rgba(255,255,255,0.1)',
                transform: 'translate(30px, -30px)'
              }} />
              <CardContent sx={{ position: 'relative', zIndex: 1 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <Box>
                    <Typography variant="h3" sx={{ fontWeight: 700, mb: 1 }}>
                      {stats?.totalCustomers || 0}
                    </Typography>
                    <Typography variant="body2" sx={{ opacity: 0.9 }}>
                      Toplam Müşteri
                    </Typography>
                  </Box>
                  <Person sx={{ fontSize: 40, opacity: 0.7 }} />
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', mt: 2 }}>
                  {stats?.customerGrowth && stats.customerGrowth > 0 ? (
                    <>
                      <ArrowUpward sx={{ fontSize: 16, mr: 0.5 }} />
                      <Typography variant="body2">+{stats.customerGrowth}% bu ay</Typography>
                    </>
                  ) : (
                    <>
                      <ArrowDownward sx={{ fontSize: 16, mr: 0.5 }} />
                      <Typography variant="body2">{stats?.customerGrowth || 0}% bu ay</Typography>
                    </>
                  )}
                </Box>
              </CardContent>
            </Card>
          </Grid>

          <Grid xs={12} sm={6} md={3}>
            <Card 
              sx={{ 
                borderRadius: 3,
                background: 'linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%)',
                color: 'white',
                position: 'relative',
                overflow: 'hidden'
              }}
            >
              <Box sx={{
                position: 'absolute',
                top: 0,
                right: 0,
                width: 100,
                height: 100,
                borderRadius: '50%',
                background: 'rgba(255,255,255,0.1)',
                transform: 'translate(30px, -30px)'
              }} />
              <CardContent sx={{ position: 'relative', zIndex: 1 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <Box>
                    <Typography variant="h3" sx={{ fontWeight: 700, mb: 1 }}>
                      ₺{stats?.monthlyRevenue?.toLocaleString() || '0'}
                    </Typography>
                    <Typography variant="body2" sx={{ opacity: 0.9 }}>
                      Bu Ay Gelir
                    </Typography>
                  </Box>
                  <TrendingUp sx={{ fontSize: 40, opacity: 0.7 }} />
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', mt: 2 }}>
                  {stats?.revenueGrowth && stats.revenueGrowth > 0 ? (
                    <>
                      <ArrowUpward sx={{ fontSize: 16, mr: 0.5 }} />
                      <Typography variant="body2">+{stats.revenueGrowth}% geçen ay</Typography>
                    </>
                  ) : (
                    <>
                      <ArrowDownward sx={{ fontSize: 16, mr: 0.5 }} />
                      <Typography variant="body2">{stats?.revenueGrowth}% geçen ay</Typography>
                    </>
                  )}
                </Box>
              </CardContent>
            </Card>
          </Grid>

          <Grid xs={12} sm={6} md={3}>
            <Card 
              sx={{ 
                borderRadius: 3,
                background: 'linear-gradient(135deg, #ffeaa7 0%, #fab1a0 100%)',
                color: 'white',
                position: 'relative',
                overflow: 'hidden'
              }}
            >
              <Box sx={{
                position: 'absolute',
                top: 0,
                right: 0,
                width: 100,
                height: 100,
                borderRadius: '50%',
                background: 'rgba(255,255,255,0.1)',
                transform: 'translate(30px, -30px)'
              }} />
              <CardContent sx={{ position: 'relative', zIndex: 1 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <Box>
                    <Typography variant="h3" sx={{ fontWeight: 700, mb: 1 }}>
                      {stats?.averageRating || '0.0'}
                    </Typography>
                    <Typography variant="body2" sx={{ opacity: 0.9 }}>
                      Ortalama Puan
                    </Typography>
                  </Box>
                  <Assessment sx={{ fontSize: 40, opacity: 0.7 }} />
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', mt: 2 }}>
                  {stats?.ratingGrowth && stats.ratingGrowth > 0 ? (
                    <>
                      <ArrowUpward sx={{ fontSize: 16, mr: 0.5 }} />
                      <Typography variant="body2">+{stats.ratingGrowth} bu ay</Typography>
                    </>
                  ) : (
                    <>
                      <ArrowDownward sx={{ fontSize: 16, mr: 0.5 }} />
                      <Typography variant="body2">{stats?.ratingGrowth || 0} bu ay</Typography>
                    </>
                  )}
                </Box>
              </CardContent>
            </Card>
          </Grid>
        </Grid>

        {/* Quick Actions */}
        <Typography variant="h5" sx={{ mb: 3, fontWeight: 600, color: 'text.primary' }}>
          Hızlı İşlemler
        </Typography>
        <Grid container spacing={2} sx={{ mb: 4 }}>
          <Grid xs={12} sm={6} md={3}>
            <Button
              fullWidth
              variant="contained"
              startIcon={<Add />}
              onClick={() => openNewDialog('appointment')}
              sx={{ 
                py: 2, 
                borderRadius: 2,
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                '&:hover': {
                  background: 'linear-gradient(135deg, #5a67d8 0%, #6b46c1 100%)',
                }
              }}
            >
              Yeni Randevu
            </Button>
          </Grid>
          <Grid xs={12} sm={6} md={3}>
            <Button
              fullWidth
              variant="contained"
              startIcon={<Add />}
              onClick={() => openNewDialog('customer')}
              sx={{ 
                py: 2, 
                borderRadius: 2,
                background: 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)',
                '&:hover': {
                  background: 'linear-gradient(135deg, #0d8378 0%, #2dd4bf 100%)',
                }
              }}
            >
              Yeni Müşteri
            </Button>
          </Grid>
          <Grid xs={12} sm={6} md={3}>
            <Button
              fullWidth
              variant="contained"
              startIcon={<Add />}
              onClick={() => openNewDialog('service')}
              sx={{ 
                py: 2, 
                borderRadius: 2,
                background: 'linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%)',
                '&:hover': {
                  background: 'linear-gradient(135deg, #ff6b6b 0%, #ffeaa7 100%)',
                }
              }}
            >
              Yeni Hizmet
            </Button>
          </Grid>
          <Grid xs={12} sm={6} md={3}>
            <Button
              fullWidth
              variant="contained"
              startIcon={<Add />}
              onClick={() => openNewDialog('staff')}
              sx={{ 
                py: 2, 
                borderRadius: 2,
                background: 'linear-gradient(135deg, #ffeaa7 0%, #fab1a0 100%)',
                '&:hover': {
                  background: 'linear-gradient(135deg, #fdcb6e 0%, #e17055 100%)',
                }
              }}
            >
              Yeni Personel
            </Button>
          </Grid>
        </Grid>

        {/* Main Content */}
        <Grid container spacing={3}>
          {/* Today's Appointments */}
          <Grid xs={12} lg={8}>
            <Card sx={{ borderRadius: 3, boxShadow: 'rgba(0,0,0,0.1) 0px 4px 12px' }}>
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                  <Typography variant="h6" sx={{ fontWeight: 600 }}>
                    Bugünkü Randevular
                  </Typography>
                  <Button startIcon={<Add />} variant="outlined" size="small">
                    Yeni Randevu
                  </Button>
                </Box>
                
                <TableContainer>
                  <Table>
                    <TableHead>
                      <TableRow>
                        <TableCell>Müşteri</TableCell>
                        <TableCell>Hizmet</TableCell>
                        <TableCell>Personel</TableCell>
                        <TableCell>Saat</TableCell>
                        <TableCell>Durum</TableCell>
                        <TableCell>İşlemler</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {appointments.slice(0, 5).map((appointment) => (
                        <TableRow key={appointment.id}>
                          <TableCell>
                            <Box sx={{ display: 'flex', alignItems: 'center' }}>
                              <Avatar sx={{ mr: 2, bgcolor: 'primary.main' }}>
                                {appointment.customerName.charAt(0)}
                              </Avatar>
                              <Box>
                                <Typography variant="body2" fontWeight={600}>
                                  {appointment.customerName}
                                </Typography>
                                <Typography variant="caption" color="text.secondary">
                                  {appointment.customerPhone}
                                </Typography>
                              </Box>
                            </Box>
                          </TableCell>
                          <TableCell>
                            <Typography variant="body2">
                              {appointment.serviceName}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              ₺{appointment.servicePrice}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <Typography variant="body2">
                              {appointment.staffName}
                            </Typography>
                          </TableCell>
                          <TableCell>
                            <Box sx={{ display: 'flex', alignItems: 'center' }}>
                              <AccessTime sx={{ fontSize: 16, mr: 1, color: 'text.secondary' }} />
                              <Typography variant="body2">
                                {appointment.appointmentTime}
                              </Typography>
                            </Box>
                          </TableCell>
                          <TableCell>
                            <Chip 
                              label={getStatusText(appointment.status)} 
                              color={getStatusColor(appointment.status) as any}
                              size="small"
                            />
                          </TableCell>
                          <TableCell>
                            <IconButton size="small">
                              <Edit />
                            </IconButton>
                            <IconButton size="small">
                              <Delete />
                            </IconButton>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </CardContent>
            </Card>
          </Grid>

          {/* Recent Activities */}
          <Grid xs={12} lg={4}>
            <Card sx={{ borderRadius: 3, boxShadow: 'rgba(0,0,0,0.1) 0px 4px 12px', mb: 3 }}>
              <CardContent>
                <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
                  Son Aktiviteler
                </Typography>
                <List>
                  <ListItem disablePadding>
                    <ListItemButton>
                      <ListItemAvatar>
                        <Avatar sx={{ bgcolor: 'success.main' }}>
                          <CalendarToday />
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText
                        primary="Yeni randevu oluşturuldu"
                        secondary="Ayşe Kaya - Saç Kesimi"
                      />
                    </ListItemButton>
                  </ListItem>
                  <Divider />
                  <ListItem disablePadding>
                    <ListItemButton>
                      <ListItemAvatar>
                        <Avatar sx={{ bgcolor: 'info.main' }}>
                          <Person />
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText
                        primary="Yeni müşteri kaydı"
                        secondary="Mehmet Demir"
                      />
                    </ListItemButton>
                  </ListItem>
                  <Divider />
                  <ListItem disablePadding>
                    <ListItemButton>
                      <ListItemAvatar>
                        <Avatar sx={{ bgcolor: 'warning.main' }}>
                          <Star />
                        </Avatar>
                      </ListItemAvatar>
                      <ListItemText
                        primary="Yeni değerlendirme"
                        secondary="5 yıldız - Harika hizmet!"
                      />
                    </ListItemButton>
                  </ListItem>
                </List>
              </CardContent>
            </Card>

            {/* Quick Stats */}
            <Card sx={{ borderRadius: 3, boxShadow: 'rgba(0,0,0,0.1) 0px 4px 12px' }}>
              <CardContent>
                <Typography variant="h6" sx={{ fontWeight: 600, mb: 3 }}>
                  Personel Performansı
                </Typography>
                {staff.map((member) => (
                  <Box key={member.id} sx={{ mb: 2 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                      <Typography variant="body2" fontWeight={600}>
                        {member.firstName} {member.lastName}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        2 randevu
                      </Typography>
                    </Box>
                    <LinearProgress 
                      variant="determinate" 
                      value={40} 
                      sx={{ borderRadius: 1 }}
                    />
                  </Box>
                ))}
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      </Container>

      {/* Dialog for adding new items */}
      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          Yeni {dialogType === 'appointment' ? 'Randevu' : 
                 dialogType === 'customer' ? 'Müşteri' :
                 dialogType === 'service' ? 'Hizmet' : 'Personel'} Ekle
        </DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 1 }}>
            {dialogType === 'appointment' && (
              <>
                <TextField fullWidth label="Müşteri Adı" sx={{ mb: 2 }} />
                <TextField fullWidth label="Telefon" sx={{ mb: 2 }} />
                <FormControl fullWidth sx={{ mb: 2 }}>
                  <InputLabel>Hizmet</InputLabel>
                  <Select>
                    {services.map((service) => (
                      <MenuItem key={service.id} value={service.id}>
                        {service.name} - ₺{service.price}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
                <FormControl fullWidth sx={{ mb: 2 }}>
                  <InputLabel>Personel</InputLabel>
                  <Select>
                    {staff.map((member) => (
                      <MenuItem key={member.id} value={member.id}>
                        {member.firstName} {member.lastName}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
                <TextField fullWidth type="datetime-local" label="Tarih ve Saat" sx={{ mb: 2 }} />
              </>
            )}
            {dialogType === 'customer' && (
              <>
                <TextField fullWidth label="Ad Soyad" sx={{ mb: 2 }} />
                <TextField fullWidth label="Telefon" sx={{ mb: 2 }} />
                <TextField fullWidth label="E-posta" sx={{ mb: 2 }} />
                <TextField fullWidth label="Adres" multiline rows={3} sx={{ mb: 2 }} />
              </>
            )}
            {dialogType === 'service' && (
              <>
                <TextField fullWidth label="Hizmet Adı" sx={{ mb: 2 }} />
                <TextField fullWidth label="Açıklama" multiline rows={3} sx={{ mb: 2 }} />
                <TextField fullWidth label="Fiyat" type="number" sx={{ mb: 2 }} />
                <TextField fullWidth label="Süre (dakika)" type="number" sx={{ mb: 2 }} />
              </>
            )}
            {dialogType === 'staff' && (
              <>
                <TextField fullWidth label="Ad Soyad" sx={{ mb: 2 }} />
                <TextField fullWidth label="Telefon" sx={{ mb: 2 }} />
                <TextField fullWidth label="E-posta" sx={{ mb: 2 }} />
                <TextField fullWidth label="Uzmanlık Alanı" sx={{ mb: 2 }} />
              </>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)}>İptal</Button>
          <Button variant="contained" onClick={() => setOpenDialog(false)}>
            Kaydet
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default DashboardPage;