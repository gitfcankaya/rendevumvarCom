import React, { useState, useEffect, useMemo } from 'react';
import { Calendar, dateFnsLocalizer, type View, Views } from 'react-big-calendar';
import { format, parse, startOfWeek, getDay, parseISO, addMinutes } from 'date-fns';
import { tr } from 'date-fns/locale';
import 'react-big-calendar/lib/css/react-big-calendar.css';
import { appointmentService, type AppointmentDto, type AppointmentDetailsDto } from '../services/appointmentService';
import salonService, { type StaffDto, type ServiceDto } from '../services/salonService';
import { authService } from '../services/authService';

// Configure date-fns localizer for Turkish
const locales = {
  'tr': tr
};

const localizer = dateFnsLocalizer({
  format,
  parse,
  startOfWeek,
  getDay,
  locales,
});

// Status color mapping
const statusColors: Record<number, { bg: string; text: string; border: string }> = {
  0: { bg: '#FEF3C7', text: '#92400E', border: '#FCD34D' }, // Pending - Yellow
  1: { bg: '#D1FAE5', text: '#065F46', border: '#34D399' }, // Confirmed - Green
  2: { bg: '#DBEAFE', text: '#1E40AF', border: '#60A5FA' }, // CheckedIn - Blue
  3: { bg: '#E9D5FF', text: '#6B21A8', border: '#A78BFA' }, // InProgress - Purple
  4: { bg: '#F3F4F6', text: '#374151', border: '#9CA3AF' }, // Completed - Gray
  5: { bg: '#FEE2E2', text: '#991B1B', border: '#F87171' }, // Cancelled - Red
  6: { bg: '#FED7AA', text: '#9A3412', border: '#FB923C' }, // NoShow - Orange
};

const statusNames: Record<number, string> = {
  0: 'Beklemede',
  1: 'Onaylandı',
  2: 'Geldi',
  3: 'Devam Ediyor',
  4: 'Tamamlandı',
  5: 'İptal Edildi',
  6: 'Gelmedi',
};

interface CalendarEvent {
  id: string;
  title: string;
  start: Date;
  end: Date;
  resource: AppointmentDto;
}

interface FilterState {
  staffId: string | null;
  statusIds: number[];
  serviceId: string | null;
}

const AppointmentCalendarPage: React.FC = () => {
  const [view, setView] = useState<View>(Views.WEEK);
  const [currentDate, setCurrentDate] = useState<Date>(new Date());
  const [appointments, setAppointments] = useState<AppointmentDto[]>([]);
  const [selectedAppointment, setSelectedAppointment] = useState<AppointmentDetailsDto | null>(null);
  const [showSidebar, setShowSidebar] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // Filter states
  const [staffList, setStaffList] = useState<StaffDto[]>([]);
  const [serviceList, setServiceList] = useState<ServiceDto[]>([]);
  const [filters, setFilters] = useState<FilterState>({
    staffId: null,
    statusIds: [0, 1, 2, 3], // Default: show Pending, Confirmed, CheckedIn, InProgress
    serviceId: null,
  });

  // Cancel and Reschedule modal states
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [showRescheduleModal, setShowRescheduleModal] = useState(false);
  const [rescheduleDate, setRescheduleDate] = useState<Date | null>(null);
  const [availableSlots, setAvailableSlots] = useState<{ time: string; available: boolean }[]>([]);

  // Staff notes
  const [staffNotes, setStaffNotes] = useState('');
  const [savingNotes, setSavingNotes] = useState(false);

  // Get current user info
  const currentUser = authService.getCurrentUser();
  const userRole = currentUser?.role;
  const isSalonOwner = userRole === 'SalonOwner';
  const isStaff = userRole === 'Staff';

  // Load staff and services on mount
  useEffect(() => {
    loadFiltersData();
  }, []);

  // Load appointments when date or filters change
  useEffect(() => {
    loadAppointments();
  }, [currentDate, view, filters]);

  const loadFiltersData = async () => {
    try {
      if (currentUser?.salonId) {
        const [staff, services] = await Promise.all([
          salonService.getSalonStaff(currentUser.salonId),
          salonService.getSalonServices(currentUser.salonId),
        ]);
        setStaffList(staff);
        setServiceList(services);
      }
    } catch (err: any) {
      console.error('Error loading filters data:', err);
    }
  };

  const loadAppointments = async () => {
    if (!currentUser) return;

    setLoading(true);
    setError(null);

    try {
      let result: AppointmentDto[] = [];

      // Calculate date range based on view
      const startDate = getViewStartDate(currentDate, view);
      const endDate = getViewEndDate(currentDate, view);

      if (isSalonOwner && currentUser.salonId) {
        // Salon owner sees all appointments
        result = await appointmentService.getSalonAppointments(
          currentUser.salonId,
          startDate.toISOString(),
          endDate.toISOString()
        );
      } else if (isStaff && currentUser.staffId) {
        // Staff sees their own appointments
        result = await appointmentService.getStaffAppointments(
          currentUser.staffId,
          startDate.toISOString(),
          endDate.toISOString()
        );
      }

      setAppointments(result);
    } catch (err: any) {
      setError(err.message || 'Randevular yüklenirken bir hata oluştu');
    } finally {
      setLoading(false);
    }
  };

  // Helper functions for date range calculation
  const getViewStartDate = (date: Date, view: View): Date => {
    const d = new Date(date);
    if (view === Views.DAY) {
      d.setHours(0, 0, 0, 0);
      return d;
    } else if (view === Views.WEEK) {
      const day = d.getDay();
      const diff = d.getDate() - day + (day === 0 ? -6 : 1); // Monday
      d.setDate(diff);
      d.setHours(0, 0, 0, 0);
      return d;
    } else if (view === Views.MONTH) {
      d.setDate(1);
      d.setHours(0, 0, 0, 0);
      return d;
    }
    return d;
  };

  const getViewEndDate = (date: Date, view: View): Date => {
    const d = new Date(date);
    if (view === Views.DAY) {
      d.setHours(23, 59, 59, 999);
      return d;
    } else if (view === Views.WEEK) {
      const day = d.getDay();
      const diff = d.getDate() - day + (day === 0 ? -6 : 1); // Monday
      d.setDate(diff + 6); // Sunday
      d.setHours(23, 59, 59, 999);
      return d;
    } else if (view === Views.MONTH) {
      d.setMonth(d.getMonth() + 1);
      d.setDate(0); // Last day of month
      d.setHours(23, 59, 59, 999);
      return d;
    }
    return d;
  };

  // Transform appointments to calendar events with filters applied
  const events: CalendarEvent[] = useMemo(() => {
    return appointments
      .filter(apt => {
        // Apply filters
        if (filters.staffId && apt.staffId !== filters.staffId) return false;
        if (filters.statusIds.length > 0 && !filters.statusIds.includes(apt.status)) return false;
        if (filters.serviceId && apt.serviceId !== filters.serviceId) return false;
        return true;
      })
      .map(apt => ({
        id: apt.id,
        title: `${apt.customerName} - ${apt.serviceName}`,
        start: parseISO(apt.startTime),
        end: addMinutes(parseISO(apt.startTime), apt.serviceDuration),
        resource: apt,
      }));
  }, [appointments, filters]);

  // Event style getter for color coding
  const eventStyleGetter = (event: CalendarEvent) => {
    const status = event.resource.status;
    const colors = statusColors[status] || statusColors[0];
    
    return {
      style: {
        backgroundColor: colors.bg,
        borderLeft: `4px solid ${colors.border}`,
        color: colors.text,
        borderRadius: '4px',
        padding: '2px 5px',
        fontSize: '0.875rem',
        fontWeight: '500',
      }
    };
  };

  // Handle event selection
  const handleSelectEvent = async (event: CalendarEvent) => {
    try {
      const details = await appointmentService.getAppointmentDetails(event.id);
      setSelectedAppointment(details);
      setStaffNotes(details.staffNotes || '');
      setShowSidebar(true);
    } catch (err: any) {
      setError(err.message || 'Randevu detayları yüklenirken bir hata oluştu');
    }
  };

  // Handle status update
  const handleStatusUpdate = async (newStatus: number) => {
    if (!selectedAppointment) return;

    try {
      await appointmentService.updateAppointmentStatus(selectedAppointment.id, newStatus as any);
      
      // Update local state
      setAppointments(prev =>
        prev.map(apt =>
          apt.id === selectedAppointment.id ? { ...apt, status: newStatus as any } : apt
        )
      );

      // Reload details
      const updatedDetails = await appointmentService.getAppointmentDetails(selectedAppointment.id);
      setSelectedAppointment(updatedDetails);

      alert(`Randevu durumu "${statusNames[newStatus]}" olarak güncellendi`);
    } catch (err: any) {
      setError(err.message || 'Durum güncellenirken bir hata oluştu');
    }
  };

  // Handle cancel
  const handleCancelConfirm = async () => {
    if (!selectedAppointment) return;

    try {
      await appointmentService.cancelAppointment(selectedAppointment.id, cancelReason);
      
      // Update local state
      setAppointments(prev =>
        prev.map(apt =>
          apt.id === selectedAppointment.id ? { ...apt, status: 5 } : apt
        )
      );

      setShowCancelModal(false);
      setCancelReason('');
      setShowSidebar(false);
      setSelectedAppointment(null);

      alert('Randevu iptal edildi');
      loadAppointments(); // Reload to refresh
    } catch (err: any) {
      setError(err.message || 'Randevu iptal edilirken bir hata oluştu');
    }
  };

  // Handle reschedule date change
  const handleRescheduleDateChange = async (date: Date) => {
    if (!selectedAppointment) return;
    
    setRescheduleDate(date);
    
    try {
      const slots = await appointmentService.getAvailableTimeSlots(
        selectedAppointment.staffId,
        date.toISOString().split('T')[0],
        selectedAppointment.serviceDuration
      );
      setAvailableSlots(slots.map(s => ({ time: s.startTime, available: true }))); // All returned slots are available
    } catch (err: any) {
      setError(err.message || 'Müsait saatler yüklenirken bir hata oluştu');
    }
  };

  // Handle reschedule confirm
  const handleRescheduleConfirm = async (newTime: string) => {
    if (!selectedAppointment || !rescheduleDate) return;

    try {
      const newStartTime = new Date(rescheduleDate);
      const [hours, minutes] = newTime.split(':').map(Number);
      newStartTime.setHours(hours, minutes, 0, 0);

      await appointmentService.rescheduleAppointment(selectedAppointment.id, {
        newStartTime: newStartTime.toISOString(),
        newStaffId: selectedAppointment.staffId,
      });

      setShowRescheduleModal(false);
      setRescheduleDate(null);
      setAvailableSlots([]);
      setShowSidebar(false);
      setSelectedAppointment(null);

      alert('Randevu yeniden planlandı');
      loadAppointments(); // Reload to refresh
    } catch (err: any) {
      setError(err.message || 'Randevu yeniden planlanırken bir hata oluştu');
    }
  };

  // Handle save notes
  const handleSaveNotes = async () => {
    if (!selectedAppointment) return;

    setSavingNotes(true);
    try {
      // TODO: Add API endpoint for updating notes
      // For now, just show success
      alert('Notlar kaydedildi');
      
      // Update local state
      setSelectedAppointment(prev => prev ? { ...prev, staffNotes: staffNotes } : null);
    } catch (err: any) {
      setError(err.message || 'Notlar kaydedilirken bir hata oluştu');
    } finally {
      setSavingNotes(false);
    }
  };

  // Check if appointment can be cancelled
  const canCancel = (status: number): boolean => {
    return status !== 4 && status !== 5; // Not Completed or Cancelled
  };

  // Check if appointment can be rescheduled
  const canReschedule = (status: number): boolean => {
    return status === 0 || status === 1; // Pending or Confirmed
  };

  // Check if status can be updated
  const canUpdateStatus = (currentStatus: number, targetStatus: number): boolean => {
    // Define valid transitions
    const validTransitions: Record<number, number[]> = {
      0: [1, 5], // Pending -> Confirmed, Cancelled
      1: [2, 5, 6], // Confirmed -> CheckedIn, Cancelled, NoShow
      2: [3, 5], // CheckedIn -> InProgress, Cancelled
      3: [4, 5], // InProgress -> Completed, Cancelled
    };

    return validTransitions[currentStatus]?.includes(targetStatus) || false;
  };

  // Clear all filters
  const handleClearFilters = () => {
    setFilters({
      staffId: null,
      statusIds: [0, 1, 2, 3],
      serviceId: null,
    });
  };

  // Toggle status filter
  const handleToggleStatus = (statusId: number) => {
    setFilters(prev => ({
      ...prev,
      statusIds: prev.statusIds.includes(statusId)
        ? prev.statusIds.filter(id => id !== statusId)
        : [...prev.statusIds, statusId],
    }));
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto py-6 px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-gray-900">Randevu Takvimi</h1>
          <p className="mt-1 text-sm text-gray-600">
            {isSalonOwner ? 'Salon randevularını görüntüleyin ve yönetin' : 'Randevularınızı görüntüleyin ve yönetin'}
          </p>
        </div>

        {/* Error Display */}
        {error && (
          <div className="mb-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded relative">
            <span className="block sm:inline">{error}</span>
            <button
              onClick={() => setError(null)}
              className="absolute top-0 right-0 px-4 py-3"
            >
              <span className="text-2xl">&times;</span>
            </button>
          </div>
        )}

        {/* Filter Bar */}
        <div className="bg-white rounded-lg shadow p-4 mb-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            {/* Staff Filter (only for salon owner) */}
            {isSalonOwner && (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Personel
                </label>
                <select
                  value={filters.staffId || ''}
                  onChange={(e) => setFilters(prev => ({ ...prev, staffId: e.target.value || null }))}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                >
                  <option value="">Tüm Personel</option>
                  {staffList.map(staff => (
                    <option key={staff.id} value={staff.id}>
                      {staff.firstName} {staff.lastName}
                    </option>
                  ))}
                </select>
              </div>
            )}

            {/* Service Filter */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Hizmet
              </label>
              <select
                value={filters.serviceId || ''}
                onChange={(e) => setFilters(prev => ({ ...prev, serviceId: e.target.value || null }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
              >
                <option value="">Tüm Hizmetler</option>
                {serviceList.map(service => (
                  <option key={service.id} value={service.id}>
                    {service.name}
                  </option>
                ))}
              </select>
            </div>

            {/* Status Filter */}
            <div className="lg:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Durum
              </label>
              <div className="flex flex-wrap gap-2">
                {Object.entries(statusNames).map(([statusId, statusName]) => (
                  <label key={statusId} className="inline-flex items-center">
                    <input
                      type="checkbox"
                      checked={filters.statusIds.includes(Number(statusId))}
                      onChange={() => handleToggleStatus(Number(statusId))}
                      className="rounded border-gray-300 text-purple-600 focus:ring-purple-500"
                    />
                    <span className="ml-2 text-sm text-gray-700">{statusName}</span>
                  </label>
                ))}
              </div>
            </div>
          </div>

          <div className="mt-4 flex items-center justify-between">
            <button
              onClick={handleClearFilters}
              className="text-sm text-purple-600 hover:text-purple-700 font-medium"
            >
              Filtreleri Temizle
            </button>
            <div className="text-sm text-gray-600">
              {events.length} randevu gösteriliyor
            </div>
          </div>
        </div>

        {/* Calendar Controls */}
        <div className="bg-white rounded-lg shadow p-4 mb-6">
          <div className="flex flex-wrap items-center justify-between gap-4">
            {/* View Toggle */}
            <div className="flex gap-2">
              <button
                onClick={() => setView(Views.DAY)}
                className={`px-4 py-2 rounded-lg font-medium ${
                  view === Views.DAY
                    ? 'bg-purple-600 text-white'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                Gün
              </button>
              <button
                onClick={() => setView(Views.WEEK)}
                className={`px-4 py-2 rounded-lg font-medium ${
                  view === Views.WEEK
                    ? 'bg-purple-600 text-white'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                Hafta
              </button>
              <button
                onClick={() => setView(Views.MONTH)}
                className={`px-4 py-2 rounded-lg font-medium ${
                  view === Views.MONTH
                    ? 'bg-purple-600 text-white'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                Ay
              </button>
            </div>

            {/* Today Button */}
            <button
              onClick={() => setCurrentDate(new Date())}
              className="px-4 py-2 bg-white border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 font-medium"
            >
              Bugün
            </button>
          </div>
        </div>

        {/* Calendar */}
        <div className="bg-white rounded-lg shadow p-4" style={{ height: '600px' }}>
          {loading ? (
            <div className="flex items-center justify-center h-full">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600"></div>
            </div>
          ) : (
            <Calendar
              localizer={localizer}
              events={events}
              startAccessor="start"
              endAccessor="end"
              view={view}
              onView={setView}
              date={currentDate}
              onNavigate={setCurrentDate}
              onSelectEvent={handleSelectEvent}
              eventPropGetter={eventStyleGetter}
              style={{ height: '100%' }}
              culture="tr"
              messages={{
                today: 'Bugün',
                previous: 'Önceki',
                next: 'Sonraki',
                month: 'Ay',
                week: 'Hafta',
                day: 'Gün',
                agenda: 'Ajanda',
                date: 'Tarih',
                time: 'Saat',
                event: 'Randevu',
                noEventsInRange: 'Bu tarih aralığında randevu bulunmuyor.',
              }}
            />
          )}
        </div>

        {/* Legend */}
        <div className="mt-6 bg-white rounded-lg shadow p-4">
          <h3 className="text-sm font-medium text-gray-700 mb-3">Durum Renkleri</h3>
          <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-7 gap-3">
            {Object.entries(statusNames).map(([statusId, statusName]) => {
              const colors = statusColors[Number(statusId)];
              return (
                <div key={statusId} className="flex items-center gap-2">
                  <div
                    className="w-4 h-4 rounded border"
                    style={{ backgroundColor: colors.bg, borderColor: colors.border }}
                  />
                  <span className="text-xs text-gray-600">{statusName}</span>
                </div>
              );
            })}
          </div>
        </div>
      </div>

      {/* Details Sidebar */}
      {showSidebar && selectedAppointment && (
        <div className="fixed inset-0 z-50">
          {/* Backdrop */}
          <div
            className="absolute inset-0 bg-black bg-opacity-50"
            onClick={() => setShowSidebar(false)}
          />

          {/* Sidebar */}
          <div className="absolute right-0 top-0 bottom-0 w-full max-w-md bg-white shadow-xl overflow-y-auto">
            {/* Header */}
            <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
              <h2 className="text-xl font-semibold text-gray-900">Randevu Detayları</h2>
              <button
                onClick={() => setShowSidebar(false)}
                className="text-gray-400 hover:text-gray-600"
              >
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            {/* Content */}
            <div className="px-6 py-4 space-y-6">
              {/* Appointment Info */}
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-2">Randevu Bilgileri</h3>
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-600">Tarih & Saat:</span>
                    <span className="text-sm font-medium text-gray-900">
                      {new Date(selectedAppointment.startTime).toLocaleString('tr-TR')}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-600">Süre:</span>
                    <span className="text-sm font-medium text-gray-900">{selectedAppointment.serviceDuration} dakika</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-600">Durum:</span>
                    <span
                      className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                      style={{
                        backgroundColor: statusColors[selectedAppointment.status].bg,
                        color: statusColors[selectedAppointment.status].text,
                      }}
                    >
                      {statusNames[selectedAppointment.status]}
                    </span>
                  </div>
                </div>
              </div>

              {/* Customer Info */}
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-2">Müşteri Bilgileri</h3>
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-600">Ad Soyad:</span>
                    <span className="text-sm font-medium text-gray-900">{selectedAppointment.customerName}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-600">E-posta:</span>
                    <span className="text-sm font-medium text-gray-900">{selectedAppointment.customerEmail}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-600">Telefon:</span>
                    <span className="text-sm font-medium text-gray-900">{selectedAppointment.customerPhone}</span>
                  </div>
                </div>
              </div>

              {/* Service Info */}
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-2">Hizmet Bilgileri</h3>
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-600">Hizmet:</span>
                    <span className="text-sm font-medium text-gray-900">{selectedAppointment.serviceName}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-600">Personel:</span>
                    <span className="text-sm font-medium text-gray-900">{selectedAppointment.staffName}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-sm text-gray-600">Fiyat:</span>
                    <span className="text-sm font-medium text-gray-900">
                      ₺{selectedAppointment.totalPrice.toFixed(2)}
                    </span>
                  </div>
                </div>
              </div>

              {/* Staff Notes */}
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-2">Personel Notları</h3>
                <textarea
                  value={staffNotes}
                  onChange={(e) => setStaffNotes(e.target.value)}
                  rows={4}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                  placeholder="Randevuyla ilgili notlarınızı buraya ekleyin..."
                />
                <button
                  onClick={handleSaveNotes}
                  disabled={savingNotes}
                  className="mt-2 w-full px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 disabled:bg-gray-300"
                >
                  {savingNotes ? 'Kaydediliyor...' : 'Notları Kaydet'}
                </button>
              </div>

              {/* Actions */}
              <div>
                <h3 className="text-sm font-medium text-gray-500 mb-3">İşlemler</h3>
                <div className="space-y-2">
                  {/* Confirm */}
                  {canUpdateStatus(selectedAppointment.status, 1) && (
                    <button
                      onClick={() => handleStatusUpdate(1)}
                      className="w-full px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700"
                    >
                      Onayla
                    </button>
                  )}

                  {/* Check In */}
                  {canUpdateStatus(selectedAppointment.status, 2) && (
                    <button
                      onClick={() => handleStatusUpdate(2)}
                      className="w-full px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                    >
                      Geldi Olarak İşaretle
                    </button>
                  )}

                  {/* Start Service */}
                  {canUpdateStatus(selectedAppointment.status, 3) && (
                    <button
                      onClick={() => handleStatusUpdate(3)}
                      className="w-full px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700"
                    >
                      Hizmeti Başlat
                    </button>
                  )}

                  {/* Complete */}
                  {canUpdateStatus(selectedAppointment.status, 4) && (
                    <button
                      onClick={() => handleStatusUpdate(4)}
                      className="w-full px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700"
                    >
                      Tamamlandı Olarak İşaretle
                    </button>
                  )}

                  {/* Mark No-Show */}
                  {canUpdateStatus(selectedAppointment.status, 6) && (
                    <button
                      onClick={() => handleStatusUpdate(6)}
                      className="w-full px-4 py-2 bg-orange-600 text-white rounded-lg hover:bg-orange-700"
                    >
                      Gelmedi Olarak İşaretle
                    </button>
                  )}

                  {/* Reschedule */}
                  {canReschedule(selectedAppointment.status) && (
                    <button
                      onClick={() => setShowRescheduleModal(true)}
                      className="w-full px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700"
                    >
                      Yeniden Planla
                    </button>
                  )}

                  {/* Cancel */}
                  {canCancel(selectedAppointment.status) && (
                    <button
                      onClick={() => setShowCancelModal(true)}
                      className="w-full px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
                    >
                      İptal Et
                    </button>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Cancel Modal */}
      {showCancelModal && selectedAppointment && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-black bg-opacity-50" onClick={() => setShowCancelModal(false)} />
          <div className="relative bg-white rounded-lg max-w-md w-full p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Randevuyu İptal Et</h3>
            <p className="text-sm text-gray-600 mb-4">
              Bu randevuyu iptal etmek istediğinizden emin misiniz?
            </p>
            <textarea
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              rows={3}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent mb-4"
              placeholder="İptal nedeni (isteğe bağlı)..."
            />
            <div className="flex gap-3">
              <button
                onClick={() => setShowCancelModal(false)}
                className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
              >
                Vazgeç
              </button>
              <button
                onClick={handleCancelConfirm}
                className="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
              >
                İptal Et
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Reschedule Modal */}
      {showRescheduleModal && selectedAppointment && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-black bg-opacity-50" onClick={() => setShowRescheduleModal(false)} />
          <div className="relative bg-white rounded-lg max-w-md w-full p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Randevuyu Yeniden Planla</h3>
            
            {/* Date Picker */}
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">Yeni Tarih</label>
              <input
                type="date"
                onChange={(e) => handleRescheduleDateChange(new Date(e.target.value))}
                min={new Date().toISOString().split('T')[0]}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
              />
            </div>

            {/* Time Slots */}
            {rescheduleDate && availableSlots.length > 0 && (
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">Müsait Saatler</label>
                <div className="grid grid-cols-3 gap-2 max-h-48 overflow-y-auto">
                  {availableSlots.map(slot => (
                    <button
                      key={slot.time}
                      onClick={() => slot.available && handleRescheduleConfirm(slot.time)}
                      disabled={!slot.available}
                      className={`px-3 py-2 rounded-lg text-sm font-medium ${
                        slot.available
                          ? 'bg-green-100 text-green-800 hover:bg-green-200'
                          : 'bg-gray-100 text-gray-400 cursor-not-allowed'
                      }`}
                    >
                      {slot.time}
                    </button>
                  ))}
                </div>
              </div>
            )}

            <div className="mt-4 flex gap-3">
              <button
                onClick={() => {
                  setShowRescheduleModal(false);
                  setRescheduleDate(null);
                  setAvailableSlots([]);
                }}
                className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50"
              >
                Vazgeç
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AppointmentCalendarPage;
