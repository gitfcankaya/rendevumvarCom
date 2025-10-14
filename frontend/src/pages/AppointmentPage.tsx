import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { AppointmentCalendar } from '../components/AppointmentCalendar';
import { AppointmentList } from '../components/AppointmentList';
import { appointmentService, type Appointment } from '../services/appointmentService';

const AppointmentPage: React.FC = () => {
  // State
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [filteredAppointments, setFilteredAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState(false);
  const [view, setView] = useState<'daily' | 'weekly' | 'monthly'>('daily');
  const [selectedDate, setSelectedDate] = useState(new Date());
  const [statusFilter, setStatusFilter] = useState<'all' | 'scheduled' | 'in-progress' | 'completed' | 'cancelled'>('all');

  // Load appointments based on view and date
  useEffect(() => {
    const loadAppointments = async () => {
      setLoading(true);
      try {
        let appointmentData: Appointment[] = [];

        if (view === 'daily') {
          const dateStr = selectedDate.toISOString().split('T')[0];
          appointmentData = await appointmentService.getDailyAppointments(dateStr);
        } else if (view === 'weekly') {
          // Get start of week
          const startOfWeek = new Date(selectedDate);
          const day = startOfWeek.getDay();
          const diff = startOfWeek.getDate() - day + (day === 0 ? -6 : 1);
          startOfWeek.setDate(diff);
          const startDateStr = startOfWeek.toISOString().split('T')[0];
          
          appointmentData = await appointmentService.getWeeklyAppointments(startDateStr);
        } else if (view === 'monthly') {
          appointmentData = await appointmentService.getMonthlyAppointments(
            selectedDate.getFullYear(),
            selectedDate.getMonth()
          );
        }

        setAppointments(appointmentData);
      } catch (error) {
        console.error('Error loading appointments:', error);
      } finally {
        setLoading(false);
      }
    };

    loadAppointments();
  }, [view, selectedDate]);

  // Filter appointments by status
  useEffect(() => {
    let filtered = appointments;
    
    if (statusFilter !== 'all') {
      filtered = appointments.filter(apt => apt.status === statusFilter);
    }

    setFilteredAppointments(filtered);
  }, [appointments, statusFilter]);

  const handleDateChange = (date: Date) => {
    setSelectedDate(date);
  };

  const handleViewChange = (newView: 'daily' | 'weekly' | 'monthly') => {
    setView(newView);
  };

  const handleStatusFilterChange = (status: 'all' | 'scheduled' | 'in-progress' | 'completed' | 'cancelled') => {
    setStatusFilter(status);
  };

  const handleStatusChange = async (appointmentId: string, newStatus: Appointment['status']) => {
    try {
      await appointmentService.updateAppointmentStatus(appointmentId, newStatus);
      
      // Update local state
      const updatedAppointments = appointments.map(apt => 
        apt.id === appointmentId ? { ...apt, status: newStatus } : apt
      );
      setAppointments(updatedAppointments);
    } catch (error) {
      console.error('Error updating appointment status:', error);
    }
  };

  return (
    <div className="appointment-page">
      {/* Navigation Header */}
      <div className="appointment-nav-header">
        <div className="nav-content">
          <div className="nav-left">
            <h2>Rendevum Admin</h2>
          </div>
          <div className="nav-center">
            <Link to="/dashboard" className="nav-link">
              📊 Dashboard
            </Link>
            <Link to="/appointments" className="nav-link active">
              📅 Randevular
            </Link>
            <span className="nav-link">
              👥 Müşteriler
            </span>
            <span className="nav-link">
              ⚙️ Ayarlar
            </span>
          </div>
          <div className="nav-right">
            <span className="user-info">Admin Kullanıcı</span>
            <Link to="/login" className="logout-btn">Çıkış</Link>
          </div>
        </div>
      </div>

      <div className="appointment-main">
        <div className="page-header">
          <h1>Randevu Yönetimi</h1>
          <p>Günlük, haftalık ve aylık randevularınızı görüntüleyin ve yönetin</p>
        </div>

        {/* Top Controls - Mobil Uyumlu */}
        <div className="appointment-controls">
          <div className="controls-row">
            <div className="view-controls">
              <h3>Görünüm</h3>
              <div className="view-buttons-horizontal">
                <button 
                  className={`view-btn-compact ${view === 'daily' ? 'active' : ''}`}
                  onClick={() => handleViewChange('daily')}
                >
                  📅 Günlük
                </button>
                <button 
                  className={`view-btn-compact ${view === 'weekly' ? 'active' : ''}`}
                  onClick={() => handleViewChange('weekly')}
                >
                  📊 Haftalık
                </button>
                <button 
                  className={`view-btn-compact ${view === 'monthly' ? 'active' : ''}`}
                  onClick={() => handleViewChange('monthly')}
                >
                  📋 Aylık
                </button>
              </div>
            </div>
            
            <div className="filter-controls">
              <h3>Filtrele</h3>
              <select 
                value={statusFilter} 
                onChange={(e) => handleStatusFilterChange(e.target.value as any)}
                className="status-select-compact"
              >
                <option value="all">Tüm Durumlar</option>
                <option value="scheduled">Planlandı</option>
                <option value="in-progress">Devam Ediyor</option>
                <option value="completed">Tamamlandı</option>
                <option value="cancelled">İptal Edildi</option>
              </select>
            </div>

            <div className="stats-controls">
              <div className="quick-stats-horizontal">
                <div className="stat-item-compact">
                  <span className="stat-value">{filteredAppointments.length}</span>
                  <span className="stat-label">Toplam</span>
                </div>
                <div className="stat-item-compact">
                  <span className="stat-value">
                    {filteredAppointments.filter(apt => apt.status === 'scheduled').length}
                  </span>
                  <span className="stat-label">Planlandı</span>
                </div>
                <div className="stat-item-compact">
                  <span className="stat-value">
                    {filteredAppointments.filter(apt => apt.status === 'completed').length}
                  </span>
                  <span className="stat-label">Tamamlandı</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Content Area */}
        <div className="appointment-content-new">
          {/* Calendar Section - Daha Kompakt */}
          <div className="calendar-section-new">
            <AppointmentCalendar
              appointments={filteredAppointments}
              selectedDate={selectedDate}
              onDateChange={handleDateChange}
              view={view}
            />
          </div>

          {/* Appointments List - İyileştirilmiş */}
          <div className="appointments-section-new">
            {loading ? (
              <div className="loading">
                <p>Randevular yükleniyor...</p>
              </div>
            ) : (
              <AppointmentList
                appointments={filteredAppointments}
                view={view}
                onStatusChange={handleStatusChange}
              />
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default AppointmentPage;