import React from 'react';

interface AppointmentFilterProps {
  view: 'daily' | 'weekly' | 'monthly';
  onViewChange: (view: 'daily' | 'weekly' | 'monthly') => void;
  selectedDate: Date;
  totalAppointments: number;
  statusFilter: 'all' | 'scheduled' | 'in-progress' | 'completed' | 'cancelled';
  onStatusFilterChange: (status: 'all' | 'scheduled' | 'in-progress' | 'completed' | 'cancelled') => void;
}

export const AppointmentFilter: React.FC<AppointmentFilterProps> = ({
  view,
  onViewChange,
  selectedDate,
  totalAppointments,
  statusFilter,
  onStatusFilterChange
}) => {
  const formatDateRange = () => {
    if (view === 'daily') {
      return selectedDate.toLocaleDateString('tr-TR', { 
        weekday: 'long', 
        day: 'numeric', 
        month: 'long' 
      });
    }
    
    if (view === 'weekly') {
      const startOfWeek = new Date(selectedDate);
      const day = startOfWeek.getDay();
      const diff = startOfWeek.getDate() - day + (day === 0 ? -6 : 1);
      startOfWeek.setDate(diff);
      
      const endOfWeek = new Date(startOfWeek);
      endOfWeek.setDate(startOfWeek.getDate() + 6);
      
      return `${startOfWeek.toLocaleDateString('tr-TR', { day: 'numeric', month: 'short' })} - ${endOfWeek.toLocaleDateString('tr-TR', { day: 'numeric', month: 'short' })}`;
    }
    
    if (view === 'monthly') {
      return selectedDate.toLocaleDateString('tr-TR', { 
        month: 'long', 
        year: 'numeric' 
      });
    }
  };

  return (
    <div className="appointment-filter">
      <div className="filter-section">
        <div className="view-selector">
          <h3>Görünüm</h3>
          <div className="view-buttons">
            <button 
              className={`view-btn ${view === 'daily' ? 'active' : ''}`}
              onClick={() => onViewChange('daily')}
            >
              📅 Günlük
            </button>
            <button 
              className={`view-btn ${view === 'weekly' ? 'active' : ''}`}
              onClick={() => onViewChange('weekly')}
            >
              📊 Haftalık
            </button>
            <button 
              className={`view-btn ${view === 'monthly' ? 'active' : ''}`}
              onClick={() => onViewChange('monthly')}
            >
              📋 Aylık
            </button>
          </div>
        </div>

        <div className="date-info">
          <h4>{formatDateRange()}</h4>
          <p className="appointment-count">
            <strong>{totalAppointments}</strong> randevu
          </p>
        </div>
      </div>

      <div className="filter-section">
        <div className="status-filter">
          <h3>Durum Filtresi</h3>
          <select 
            value={statusFilter} 
            onChange={(e) => onStatusFilterChange(e.target.value as any)}
            className="status-select"
          >
            <option value="all">Tüm Durumlar</option>
            <option value="scheduled">Planlandı</option>
            <option value="in-progress">Devam Ediyor</option>
            <option value="completed">Tamamlandı</option>
            <option value="cancelled">İptal Edildi</option>
          </select>
        </div>
      </div>

      <div className="filter-section">
        <div className="quick-stats">
          <h3>Hızlı İstatistikler</h3>
          <div className="stats-grid">
            <div className="stat-item">
              <span className="stat-label">Bu Hafta</span>
              <span className="stat-value">24</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Bu Ay</span>
              <span className="stat-value">127</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Tamamlanan</span>
              <span className="stat-value">89%</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};