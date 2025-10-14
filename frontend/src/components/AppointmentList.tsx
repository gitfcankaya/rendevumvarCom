import React from 'react';
import type { Appointment } from '../services/appointmentService';

interface AppointmentListProps {
  appointments: Appointment[];
  view: 'daily' | 'weekly' | 'monthly';
  onStatusChange?: (appointmentId: string, status: Appointment['status']) => void;
}

export const AppointmentList: React.FC<AppointmentListProps> = ({
  appointments,
  view,
  onStatusChange
}) => {
  const getStatusColor = (status: Appointment['status']) => {
    switch (status) {
      case 'scheduled': return '#007bff';
      case 'in-progress': return '#ffc107';
      case 'completed': return '#28a745';
      case 'cancelled': return '#dc3545';
      default: return '#6c757d';
    }
  };

  const getStatusText = (status: Appointment['status']) => {
    switch (status) {
      case 'scheduled': return 'Planlandı';
      case 'in-progress': return 'Devam Ediyor';
      case 'completed': return 'Tamamlandı';
      case 'cancelled': return 'İptal Edildi';
      default: return 'Bilinmiyor';
    }
  };

  const formatDate = (date: string) => {
    return new Date(date).toLocaleDateString('tr-TR', {
      weekday: 'short',
      day: 'numeric',
      month: 'short'
    });
  };

  const formatTime = (time: string) => {
    return time.slice(0, 5); // HH:MM formatında
  };

  const groupAppointmentsByDate = (appointments: Appointment[]) => {
    const grouped: { [key: string]: Appointment[] } = {};
    appointments.forEach(apt => {
      if (!grouped[apt.appointmentDate]) {
        grouped[apt.appointmentDate] = [];
      }
      grouped[apt.appointmentDate].push(apt);
    });
    return grouped;
  };

  const renderAppointmentCard = (appointment: Appointment) => (
    <div key={appointment.id} className="appointment-card">
      <div className="appointment-header">
        <div className="appointment-time">
          <span className="time">{formatTime(appointment.appointmentTime)}</span>
          <span className="duration">({appointment.duration} dk)</span>
        </div>
        <div 
          className="appointment-status" 
          style={{ backgroundColor: getStatusColor(appointment.status) }}
        >
          {getStatusText(appointment.status)}
        </div>
      </div>
      
      <div className="appointment-content">
        <div className="customer-info">
          <h4>{appointment.customerName}</h4>
          <p className="customer-contact">
            📞 {appointment.customerPhone} | ✉️ {appointment.customerEmail}
          </p>
        </div>
        
        <div className="service-info">
          <h5>{appointment.serviceName}</h5>
          <p className="service-details">
            💰 {appointment.servicePrice}₺ | 👨‍💼 {appointment.staffName}
          </p>
        </div>
        
        {appointment.notes && (
          <div className="appointment-notes">
            <p><strong>Notlar:</strong> {appointment.notes}</p>
          </div>
        )}
        
        {onStatusChange && appointment.status !== 'completed' && appointment.status !== 'cancelled' && (
          <div className="appointment-actions">
            <button 
              onClick={() => onStatusChange(appointment.id, 'in-progress')}
              className="btn btn-sm btn-warning"
              disabled={appointment.status === 'in-progress'}
            >
              Başlat
            </button>
            <button 
              onClick={() => onStatusChange(appointment.id, 'completed')}
              className="btn btn-sm btn-success"
            >
              Tamamla
            </button>
            <button 
              onClick={() => onStatusChange(appointment.id, 'cancelled')}
              className="btn btn-sm btn-danger"
            >
              İptal Et
            </button>
          </div>
        )}
      </div>
    </div>
  );

  if (appointments.length === 0) {
    return (
      <div className="appointment-list empty">
        <div className="empty-state">
          <h3>Randevu Bulunamadı</h3>
          <p>Seçilen tarih aralığında herhangi bir randevu bulunmamaktadır.</p>
        </div>
      </div>
    );
  }

  if (view === 'daily') {
    const sortedAppointments = [...appointments].sort((a, b) => 
      a.appointmentTime.localeCompare(b.appointmentTime)
    );

    return (
      <div className="appointment-list daily">
        <div className="list-header">
          <h3>Günün Randevuları ({appointments.length})</h3>
        </div>
        <div className="appointment-cards">
          {sortedAppointments.map(renderAppointmentCard)}
        </div>
      </div>
    );
  }

  if (view === 'weekly' || view === 'monthly') {
    const groupedAppointments = groupAppointmentsByDate(appointments);
    const sortedDates = Object.keys(groupedAppointments).sort();

    return (
      <div className={`appointment-list ${view}`}>
        <div className="list-header">
          <h3>
            {view === 'weekly' ? 'Haftalık' : 'Aylık'} Randevular ({appointments.length})
          </h3>
        </div>
        
        {sortedDates.map(date => (
          <div key={date} className="date-group">
            <div className="date-header">
              <h4>{formatDate(date)}</h4>
              <span className="appointment-count">
                {groupedAppointments[date].length} randevu
              </span>
            </div>
            <div className="appointment-cards">
              {groupedAppointments[date]
                .sort((a, b) => a.appointmentTime.localeCompare(b.appointmentTime))
                .map(renderAppointmentCard)}
            </div>
          </div>
        ))}
      </div>
    );
  }

  return null;
};