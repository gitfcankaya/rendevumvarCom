import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { Provider } from 'react-redux';
import { store } from './store/store';
import Layout from './components/Layout';
import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './pages/DashboardPage';
import AppointmentPage from './pages/AppointmentPage';
import BookAppointmentPage from './pages/BookAppointmentPage';
import MyAppointmentsPage from './pages/MyAppointmentsPage';
import AppointmentCalendarPage from './pages/AppointmentCalendarPage';
import ServicesPage from './pages/ServicesPage';
import DynamicContentPage from './pages/DynamicContentPage';
import PricingPage from './pages/PricingPage';
import SubscriptionDashboard from './pages/SubscriptionDashboard';
import SalonListPage from './pages/SalonListPage';
import SalonProfilePage from './pages/SalonProfilePage';
import SalonDashboard from './pages/SalonDashboard';
import ManageSalonPage from './pages/ManageSalonPage';
import ServiceManagementPage from './pages/ServiceManagementPage';
import './App.css';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

function App() {
  return (
    <Provider store={store}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <Router>
          <Routes>
            {/* Public routes - no layout */}
            <Route path="/" element={<HomePage />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/pricing" element={<PricingPage />} />
            
            {/* Public salon routes */}
            <Route path="/salons" element={<SalonListPage />} />
            <Route path="/salons/:id" element={<SalonProfilePage />} />
            
            {/* Booking routes - public */}
            <Route path="/book" element={<BookAppointmentPage />} />
            <Route path="/book/:salonId" element={<BookAppointmentPage />} />
            
            {/* Protected routes - with layout */}
            <Route path="/dashboard" element={<Layout><DashboardPage /></Layout>} />
            <Route path="/appointments" element={<Layout><AppointmentPage /></Layout>} />
            <Route path="/my-appointments" element={<Layout><MyAppointmentsPage /></Layout>} />
            <Route path="/appointment-calendar" element={<Layout><AppointmentCalendarPage /></Layout>} />
            <Route path="/services" element={<Layout><ServicesPage /></Layout>} />
            <Route path="/subscription" element={<Layout><SubscriptionDashboard /></Layout>} />
            <Route path="/sayfa/:slug" element={<Layout><DynamicContentPage /></Layout>} />
            
            {/* Salon management routes - protected */}
            <Route path="/salon-dashboard" element={<Layout><SalonDashboard /></Layout>} />
            <Route path="/salons/:id/manage" element={<Layout><ManageSalonPage /></Layout>} />
            <Route path="/salons/:salonId/services" element={<Layout><ServiceManagementPage /></Layout>} />
          </Routes>
        </Router>
      </ThemeProvider>
    </Provider>
  );
}

export default App;
