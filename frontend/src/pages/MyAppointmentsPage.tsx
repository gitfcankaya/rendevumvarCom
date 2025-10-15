import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  appointmentService,
  type AppointmentDto,
  type AppointmentDetailsDto,
  AppointmentStatus,
} from '../services/appointmentService';

type TabType = 'upcoming' | 'past';

const MyAppointmentsPage: React.FC = () => {
  const navigate = useNavigate();

  // State
  const [activeTab, setActiveTab] = useState<TabType>('upcoming');
  const [appointments, setAppointments] = useState<AppointmentDto[]>([]);
  const [filteredAppointments, setFilteredAppointments] = useState<AppointmentDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');

  // Filter state
  const [startDateFilter, setStartDateFilter] = useState<string>('');
  const [endDateFilter, setEndDateFilter] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<number | undefined>(undefined);

  // Modal state
  const [selectedAppointment, setSelectedAppointment] = useState<AppointmentDetailsDto | null>(null);
  const [showDetailsModal, setShowDetailsModal] = useState(false);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [showRescheduleModal, setShowRescheduleModal] = useState(false);
  const [cancellationReason, setCancellationReason] = useState('');
  const [rescheduleDate, setRescheduleDate] = useState('');
  const [rescheduleTime, setRescheduleTime] = useState('');
  const [availableSlots, setAvailableSlots] = useState<string[]>([]);

  // Load appointments
  useEffect(() => {
    loadAppointments();
  }, []);

  // Filter appointments when tab or filters change
  useEffect(() => {
    filterAppointments();
  }, [activeTab, appointments, startDateFilter, endDateFilter, statusFilter]);

  const loadAppointments = async () => {
    try {
      setLoading(true);
      setError('');
      const data = await appointmentService.getMyAppointments();
      setAppointments(data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Randevular yüklenemedi.');
      console.error('Error loading appointments:', err);
    } finally {
      setLoading(false);
    }
  };

  const filterAppointments = () => {
    let filtered = [...appointments];
    const now = new Date();

    // Tab filter
    if (activeTab === 'upcoming') {
      filtered = filtered.filter((apt) => {
        const startTime = new Date(apt.startTime);
        return (
          startTime >= now &&
          (apt.status === AppointmentStatus.Pending ||
            apt.status === AppointmentStatus.Confirmed ||
            apt.status === AppointmentStatus.CheckedIn ||
            apt.status === AppointmentStatus.InProgress)
        );
      });
    } else {
      filtered = filtered.filter((apt) => {
        const startTime = new Date(apt.startTime);
        return (
          startTime < now ||
          apt.status === AppointmentStatus.Completed ||
          apt.status === AppointmentStatus.Cancelled ||
          apt.status === AppointmentStatus.NoShow
        );
      });
    }

    // Date filters
    if (startDateFilter) {
      const startDate = new Date(startDateFilter);
      filtered = filtered.filter((apt) => new Date(apt.startTime) >= startDate);
    }
    if (endDateFilter) {
      const endDate = new Date(endDateFilter);
      endDate.setHours(23, 59, 59);
      filtered = filtered.filter((apt) => new Date(apt.startTime) <= endDate);
    }

    // Status filter
    if (statusFilter !== undefined) {
      filtered = filtered.filter((apt) => apt.status === statusFilter);
    }

    // Sort by date
    filtered.sort((a, b) => {
      const dateA = new Date(a.startTime).getTime();
      const dateB = new Date(b.startTime).getTime();
      return activeTab === 'upcoming' ? dateA - dateB : dateB - dateA;
    });

    setFilteredAppointments(filtered);
  };

  const handleViewDetails = async (appointmentId: string) => {
    try {
      setLoading(true);
      const details = await appointmentService.getAppointmentDetails(appointmentId);
      setSelectedAppointment(details);
      setShowDetailsModal(true);
    } catch (err) {
      setError('Randevu detayları yüklenemedi.');
      console.error('Error loading appointment details:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleCancelClick = (appointment: AppointmentDto) => {
    setSelectedAppointment(appointment as any);
    setShowCancelModal(true);
  };

  const handleCancelConfirm = async () => {
    if (!selectedAppointment) return;

    try {
      setLoading(true);
      await appointmentService.cancelAppointment(selectedAppointment.id, cancellationReason);
      setShowCancelModal(false);
      setCancellationReason('');
      setSelectedAppointment(null);
      await loadAppointments();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Randevu iptal edilemedi.');
      console.error('Error cancelling appointment:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleRescheduleClick = async (appointment: AppointmentDto) => {
    setSelectedAppointment(appointment as any);
    setShowRescheduleModal(true);
    setRescheduleDate('');
    setRescheduleTime('');
    setAvailableSlots([]);
  };

  const handleRescheduleDateChange = async (date: string) => {
    setRescheduleDate(date);
    setRescheduleTime('');
    
    if (!selectedAppointment) return;

    try {
      setLoading(true);
      const slots = await appointmentService.getAvailableTimeSlots(
        selectedAppointment.staffId,
        date,
        selectedAppointment.serviceDuration
      );
      const times = slots.map((slot) => slot.startTime);
      setAvailableSlots(times);
    } catch (err) {
      console.error('Error loading available slots:', err);
      setAvailableSlots([]);
    } finally {
      setLoading(false);
    }
  };

  const handleRescheduleConfirm = async () => {
    if (!selectedAppointment || !rescheduleDate || !rescheduleTime) return;

    try {
      setLoading(true);
      const newStartTime = new Date(`${rescheduleDate}T${rescheduleTime}`).toISOString();
      
      await appointmentService.rescheduleAppointment(selectedAppointment.id, {
        newStartTime,
        newStaffId: selectedAppointment.staffId,
      });

      setShowRescheduleModal(false);
      setRescheduleDate('');
      setRescheduleTime('');
      setSelectedAppointment(null);
      await loadAppointments();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Randevu değiştirilemedi.');
      console.error('Error rescheduling appointment:', err);
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadge = (status: number) => {
    const statusMap: Record<number, { text: string; class: string }> = {
      [AppointmentStatus.Pending]: { text: 'Beklemede', class: 'bg-yellow-100 text-yellow-800' },
      [AppointmentStatus.Confirmed]: { text: 'Onaylandı', class: 'bg-green-100 text-green-800' },
      [AppointmentStatus.CheckedIn]: { text: 'Giriş Yapıldı', class: 'bg-blue-100 text-blue-800' },
      [AppointmentStatus.InProgress]: { text: 'Devam Ediyor', class: 'bg-purple-100 text-purple-800' },
      [AppointmentStatus.Completed]: { text: 'Tamamlandı', class: 'bg-gray-100 text-gray-800' },
      [AppointmentStatus.Cancelled]: { text: 'İptal Edildi', class: 'bg-red-100 text-red-800' },
      [AppointmentStatus.NoShow]: { text: 'Gelmedi', class: 'bg-orange-100 text-orange-800' },
    };

    const badge = statusMap[status] || { text: 'Bilinmiyor', class: 'bg-gray-100 text-gray-800' };
    return (
      <span className={`px-2 py-1 rounded-full text-xs font-medium ${badge.class}`}>
        {badge.text}
      </span>
    );
  };

  const canCancel = (appointment: AppointmentDto) => {
    return (
      appointment.status === AppointmentStatus.Pending ||
      appointment.status === AppointmentStatus.Confirmed
    );
  };

  const canReschedule = (appointment: AppointmentDto) => {
    return (
      appointment.status === AppointmentStatus.Pending ||
      appointment.status === AppointmentStatus.Confirmed
    );
  };

  const formatDateTime = (dateString: string) => {
    const date = new Date(dateString);
    return {
      date: date.toLocaleDateString('tr-TR', {
        day: 'numeric',
        month: 'long',
        year: 'numeric',
      }),
      time: date.toLocaleTimeString('tr-TR', {
        hour: '2-digit',
        minute: '2-digit',
      }),
      weekday: date.toLocaleDateString('tr-TR', { weekday: 'long' }),
    };
  };

  const formatTime = (timeString: string) => {
    const [hours, minutes] = timeString.split(':');
    return `${hours}:${minutes}`;
  };

  const clearFilters = () => {
    setStartDateFilter('');
    setEndDateFilter('');
    setStatusFilter(undefined);
  };

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Randevularım</h1>
          <p className="mt-2 text-gray-600">Tüm randevularınızı buradan görüntüleyip yönetebilirsiniz.</p>
        </div>

        {/* Error Display */}
        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg flex justify-between items-center">
            <p className="text-red-800">{error}</p>
            <button onClick={() => setError('')} className="text-red-800 hover:text-red-900">
              ✕
            </button>
          </div>
        )}

        {/* Tabs */}
        <div className="mb-6 border-b border-gray-200">
          <nav className="flex space-x-8">
            <button
              onClick={() => setActiveTab('upcoming')}
              className={`pb-4 px-1 border-b-2 font-medium text-sm ${
                activeTab === 'upcoming'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Yaklaşan Randevular
            </button>
            <button
              onClick={() => setActiveTab('past')}
              className={`pb-4 px-1 border-b-2 font-medium text-sm ${
                activeTab === 'past'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Geçmiş Randevular
            </button>
          </nav>
        </div>

        {/* Filters */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Başlangıç Tarihi
              </label>
              <input
                type="date"
                value={startDateFilter}
                onChange={(e) => setStartDateFilter(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Bitiş Tarihi
              </label>
              <input
                type="date"
                value={endDateFilter}
                onChange={(e) => setEndDateFilter(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Durum
              </label>
              <select
                value={statusFilter ?? ''}
                onChange={(e) => setStatusFilter(e.target.value ? Number(e.target.value) : undefined)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">Tümü</option>
                <option value={AppointmentStatus.Pending}>Beklemede</option>
                <option value={AppointmentStatus.Confirmed}>Onaylandı</option>
                <option value={AppointmentStatus.CheckedIn}>Giriş Yapıldı</option>
                <option value={AppointmentStatus.InProgress}>Devam Ediyor</option>
                <option value={AppointmentStatus.Completed}>Tamamlandı</option>
                <option value={AppointmentStatus.Cancelled}>İptal Edildi</option>
                <option value={AppointmentStatus.NoShow}>Gelmedi</option>
              </select>
            </div>
            <div className="flex items-end">
              <button
                onClick={clearFilters}
                className="w-full px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200"
              >
                Filtreleri Temizle
              </button>
            </div>
          </div>
        </div>

        {/* Loading */}
        {loading && !showDetailsModal && !showCancelModal && !showRescheduleModal && (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
            <p className="mt-4 text-gray-600">Yükleniyor...</p>
          </div>
        )}

        {/* Appointments List */}
        {!loading && filteredAppointments.length === 0 ? (
          <div className="bg-white rounded-lg shadow-md p-12 text-center">
            <div className="mx-auto w-24 h-24 bg-gray-100 rounded-full flex items-center justify-center mb-4">
              <svg
                className="w-12 h-12 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth="2"
                  d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                />
              </svg>
            </div>
            <h3 className="text-lg font-medium text-gray-900 mb-2">Randevu Bulunamadı</h3>
            <p className="text-gray-600 mb-6">
              {activeTab === 'upcoming'
                ? 'Henüz yaklaşan bir randevunuz yok.'
                : 'Geçmiş randevunuz bulunmuyor.'}
            </p>
            <button
              onClick={() => navigate('/book')}
              className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              Yeni Randevu Al
            </button>
          </div>
        ) : (
          <div className="grid grid-cols-1 gap-4">
            {filteredAppointments.map((appointment) => {
              const dateTime = formatDateTime(appointment.startTime);
              return (
                <div
                  key={appointment.id}
                  className="bg-white rounded-lg shadow-md p-6 hover:shadow-lg transition"
                >
                  <div className="flex flex-col md:flex-row md:items-center md:justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-3">
                        <h3 className="text-xl font-semibold text-gray-900">
                          {appointment.salonName}
                        </h3>
                        {getStatusBadge(appointment.status)}
                      </div>
                      
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                        <div>
                          <p className="text-gray-600">
                            <span className="font-medium">Hizmet:</span> {appointment.serviceName}
                          </p>
                          <p className="text-gray-600">
                            <span className="font-medium">Personel:</span> {appointment.staffName}
                          </p>
                        </div>
                        <div>
                          <p className="text-gray-600">
                            <span className="font-medium">Tarih:</span> {dateTime.date}
                          </p>
                          <p className="text-gray-600">
                            <span className="font-medium">Saat:</span> {dateTime.time}
                          </p>
                        </div>
                      </div>

                      <div className="mt-3 flex items-center gap-4 text-sm text-gray-500">
                        <span>⏱️ {appointment.serviceDuration} dakika</span>
                        <span>💰 {appointment.totalPrice} ₺</span>
                      </div>
                    </div>

                    <div className="mt-4 md:mt-0 md:ml-6 flex flex-col gap-2">
                      <button
                        onClick={() => handleViewDetails(appointment.id)}
                        className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 text-sm font-medium"
                      >
                        Detaylar
                      </button>
                      {canReschedule(appointment) && (
                        <button
                          onClick={() => handleRescheduleClick(appointment)}
                          className="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 text-sm font-medium"
                        >
                          Değiştir
                        </button>
                      )}
                      {canCancel(appointment) && (
                        <button
                          onClick={() => handleCancelClick(appointment)}
                          className="px-4 py-2 bg-red-100 text-red-700 rounded-lg hover:bg-red-200 text-sm font-medium"
                        >
                          İptal Et
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>

      {/* Details Modal */}
      {showDetailsModal && selectedAppointment && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <div className="flex justify-between items-center mb-6">
                <h2 className="text-2xl font-bold">Randevu Detayları</h2>
                <button
                  onClick={() => setShowDetailsModal(false)}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>

              <div className="space-y-6">
                {/* Status */}
                <div>
                  <h3 className="text-sm font-medium text-gray-500 mb-2">Durum</h3>
                  {getStatusBadge(selectedAppointment.status)}
                </div>

                {/* Salon Info */}
                <div className="border-t pt-4">
                  <h3 className="text-sm font-medium text-gray-500 mb-2">Salon Bilgileri</h3>
                  <p className="text-lg font-semibold">{selectedAppointment.salon.name}</p>
                  <p className="text-gray-600">{selectedAppointment.salon.address}</p>
                  <p className="text-gray-600">{selectedAppointment.salon.city}</p>
                  <p className="text-gray-600">{selectedAppointment.salon.phone}</p>
                </div>

                {/* Service Info */}
                <div className="border-t pt-4">
                  <h3 className="text-sm font-medium text-gray-500 mb-2">Hizmet Bilgileri</h3>
                  <p className="text-lg font-semibold">{selectedAppointment.service.name}</p>
                  {selectedAppointment.service.description && (
                    <p className="text-gray-600">{selectedAppointment.service.description}</p>
                  )}
                  <div className="mt-2 flex gap-4">
                    <span className="text-gray-600">⏱️ {selectedAppointment.service.durationMinutes} dakika</span>
                    <span className="text-gray-600">💰 {selectedAppointment.service.price} ₺</span>
                  </div>
                </div>

                {/* Staff Info */}
                <div className="border-t pt-4">
                  <h3 className="text-sm font-medium text-gray-500 mb-2">Personel Bilgileri</h3>
                  <div className="flex items-center gap-4">
                    {selectedAppointment.staff.profilePictureUrl && (
                      <img
                        src={selectedAppointment.staff.profilePictureUrl}
                        alt={selectedAppointment.staff.fullName}
                        className="w-16 h-16 rounded-full object-cover"
                      />
                    )}
                    <div>
                      <p className="text-lg font-semibold">{selectedAppointment.staff.fullName}</p>
                      {selectedAppointment.staff.roleName && (
                        <p className="text-gray-600">{selectedAppointment.staff.roleName}</p>
                      )}
                      <div className="flex items-center mt-1">
                        <span className="text-yellow-500">★</span>
                        <span className="ml-1 text-sm">{selectedAppointment.staff.averageRating.toFixed(1)}</span>
                      </div>
                    </div>
                  </div>
                </div>

                {/* DateTime Info */}
                <div className="border-t pt-4">
                  <h3 className="text-sm font-medium text-gray-500 mb-2">Tarih ve Saat</h3>
                  {(() => {
                    const dt = formatDateTime(selectedAppointment.startTime);
                    return (
                      <>
                        <p className="text-lg font-semibold">{dt.date}</p>
                        <p className="text-gray-600">{dt.weekday}, {dt.time}</p>
                      </>
                    );
                  })()}
                </div>

                {/* Notes */}
                {selectedAppointment.customerNotes && (
                  <div className="border-t pt-4">
                    <h3 className="text-sm font-medium text-gray-500 mb-2">Notlarınız</h3>
                    <p className="text-gray-700">{selectedAppointment.customerNotes}</p>
                  </div>
                )}

                {/* Cancellation Reason */}
                {selectedAppointment.cancellationReason && (
                  <div className="border-t pt-4">
                    <h3 className="text-sm font-medium text-gray-500 mb-2">İptal Nedeni</h3>
                    <p className="text-gray-700">{selectedAppointment.cancellationReason}</p>
                  </div>
                )}

                {/* Price Summary */}
                <div className="border-t pt-4 bg-gray-50 -mx-6 px-6 py-4">
                  <div className="flex justify-between items-center">
                    <span className="text-lg font-semibold">Toplam Tutar:</span>
                    <span className="text-2xl font-bold text-blue-600">{selectedAppointment.totalPrice} ₺</span>
                  </div>
                  {selectedAppointment.depositPaid > 0 && (
                    <div className="flex justify-between items-center mt-2 text-sm">
                      <span className="text-gray-600">Ödenen Depozito:</span>
                      <span className="text-green-600">{selectedAppointment.depositPaid} ₺</span>
                    </div>
                  )}
                </div>
              </div>

              <div className="mt-6">
                <button
                  onClick={() => setShowDetailsModal(false)}
                  className="w-full px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200"
                >
                  Kapat
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Cancel Modal */}
      {showCancelModal && selectedAppointment && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <h2 className="text-xl font-bold mb-4">Randevuyu İptal Et</h2>
            <p className="text-gray-600 mb-4">
              Bu randevuyu iptal etmek istediğinize emin misiniz? Bu işlem geri alınamaz.
            </p>
            
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                İptal Nedeni (İsteğe Bağlı)
              </label>
              <textarea
                value={cancellationReason}
                onChange={(e) => setCancellationReason(e.target.value)}
                placeholder="İptal nedeninizi belirtebilirsiniz..."
                rows={3}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            <div className="flex gap-3">
              <button
                onClick={() => {
                  setShowCancelModal(false);
                  setCancellationReason('');
                  setSelectedAppointment(null);
                }}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
                disabled={loading}
              >
                Vazgeç
              </button>
              <button
                onClick={handleCancelConfirm}
                className="flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
                disabled={loading}
              >
                {loading ? 'İptal Ediliyor...' : 'İptal Et'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Reschedule Modal */}
      {showRescheduleModal && selectedAppointment && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <h2 className="text-xl font-bold mb-4">Randevuyu Değiştir</h2>
            
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Yeni Tarih
              </label>
              <input
                type="date"
                value={rescheduleDate}
                onChange={(e) => handleRescheduleDateChange(e.target.value)}
                min={new Date().toISOString().split('T')[0]}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>

            {rescheduleDate && (
              <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Yeni Saat
                </label>
                {loading ? (
                  <p className="text-gray-600">Müsait saatler yükleniyor...</p>
                ) : availableSlots.length === 0 ? (
                  <p className="text-gray-600">Bu tarih için müsait saat bulunamadı.</p>
                ) : (
                  <div className="grid grid-cols-3 gap-2">
                    {availableSlots.map((slot) => (
                      <button
                        key={slot}
                        onClick={() => setRescheduleTime(slot)}
                        className={`px-3 py-2 border rounded-lg text-sm ${
                          rescheduleTime === slot
                            ? 'bg-blue-600 text-white border-blue-600'
                            : 'bg-white text-gray-700 border-gray-300 hover:border-blue-500'
                        }`}
                      >
                        {formatTime(slot)}
                      </button>
                    ))}
                  </div>
                )}
              </div>
            )}

            <div className="flex gap-3">
              <button
                onClick={() => {
                  setShowRescheduleModal(false);
                  setRescheduleDate('');
                  setRescheduleTime('');
                  setSelectedAppointment(null);
                }}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
                disabled={loading}
              >
                Vazgeç
              </button>
              <button
                onClick={handleRescheduleConfirm}
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                disabled={loading || !rescheduleDate || !rescheduleTime}
              >
                {loading ? 'Değiştiriliyor...' : 'Onayla'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default MyAppointmentsPage;
