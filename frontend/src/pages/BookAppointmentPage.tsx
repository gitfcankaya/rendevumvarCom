import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, useSearchParams } from 'react-router-dom';
import {
  appointmentService,
  type CreateAppointmentDto,
  type AvailableTimeSlotDto,
} from '../services/appointmentService';
import salonService, {
  type SalonDto,
  type ServiceDto,
  type StaffDto,
} from '../services/salonService';
import PaymentFormDialog from '../components/PaymentFormDialog';
import paymentService, { type CreatePaymentDto } from '../services/paymentService';

interface BookingFormData {
  salonId: string;
  serviceId: string;
  staffId: string;
  startTime: string;
  customerNotes: string;
}

const BookAppointmentPage: React.FC = () => {
  const { salonId: paramSalonId } = useParams<{ salonId: string }>();
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  // Multi-step form state
  const [currentStep, setCurrentStep] = useState(1);
  const [formData, setFormData] = useState<BookingFormData>({
    salonId: paramSalonId || '',
    serviceId: searchParams.get('serviceId') || '',
    staffId: '',
    startTime: '',
    customerNotes: '',
  });

  // Data state
  const [salons, setSalons] = useState<SalonDto[]>([]);
  const [selectedSalon, setSelectedSalon] = useState<SalonDto | null>(null);
  const [services, setServices] = useState<ServiceDto[]>([]);
  const [selectedService, setSelectedService] = useState<ServiceDto | null>(null);
  const [staff, setStaff] = useState<StaffDto[]>([]);
  const [selectedStaff, setSelectedStaff] = useState<StaffDto | null>(null);
  const [selectedDate, setSelectedDate] = useState<string>('');
  const [availableSlots, setAvailableSlots] = useState<AvailableTimeSlotDto[]>([]);
  const [selectedSlot, setSelectedSlot] = useState<AvailableTimeSlotDto | null>(null);

  // UI state
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const [showSuccessModal, setShowSuccessModal] = useState(false);
  const [acceptedTerms, setAcceptedTerms] = useState(false);
  
  // Payment state
  const [showPaymentDialog, setShowPaymentDialog] = useState(false);
  const [createdAppointmentId, setCreatedAppointmentId] = useState<string>('');
  const [paymentLoading, setPaymentLoading] = useState(false);

  // Step 1: Load salons if not pre-selected
  useEffect(() => {
    if (!paramSalonId) {
      loadSalons();
    } else {
      loadSalonDetails(paramSalonId);
    }
  }, [paramSalonId]);

  // Load services when salon is selected
  useEffect(() => {
    if (formData.salonId) {
      loadServices(formData.salonId);
    }
  }, [formData.salonId]);

  // Load staff when service is selected
  useEffect(() => {
    if (formData.salonId && formData.serviceId) {
      loadStaff(formData.salonId);
    }
  }, [formData.salonId, formData.serviceId]);

  // Load available slots when date and staff/service are selected
  useEffect(() => {
    if (selectedDate && formData.serviceId && (formData.staffId || formData.salonId)) {
      loadAvailableSlots();
    }
  }, [selectedDate, formData.serviceId, formData.staffId, formData.salonId]);

  const loadSalons = async () => {
    try {
      setLoading(true);
      // Search all salons
      const response = await salonService.searchSalons({ searchTerm: '', city: '', pageSize: 100 });
      setSalons(response.salons);
    } catch (err) {
      setError('Salonlar yüklenemedi. Lütfen tekrar deneyin.');
      console.error('Error loading salons:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadSalonDetails = async (id: string) => {
    try {
      setLoading(true);
      const salon = await salonService.getSalonDetails(id);
      // Convert SalonDetailsDto to SalonDto
      const salonDto: SalonDto = {
        id: salon.id,
        tenantId: salon.tenantId,
        name: salon.name,
        address: salon.address,
        city: salon.city,
        phone: salon.phone,
        email: salon.email,
        state: salon.state,
        postalCode: salon.postalCode,
        latitude: salon.latitude,
        longitude: salon.longitude,
        businessHours: salon.businessHours,
        isActive: salon.isActive,
        averageRating: salon.averageRating,
        reviewCount: salon.reviewCount,
        createdAt: salon.createdAt,
      };
      setSelectedSalon(salonDto);
      setFormData((prev) => ({ ...prev, salonId: id }));
    } catch (err) {
      setError('Salon bilgileri yüklenemedi.');
      console.error('Error loading salon:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadServices = async (salonId: string) => {
    try {
      setLoading(true);
      const response = await salonService.getSalonServices(salonId);
      setServices(response);
    } catch (err) {
      setError('Hizmetler yüklenemedi.');
      console.error('Error loading services:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadStaff = async (salonId: string) => {
    try {
      setLoading(true);
      const salonDetails = await salonService.getSalonDetails(salonId);
      setStaff(salonDetails.staff);
    } catch (err) {
      setError('Personel listesi yüklenemedi.');
      console.error('Error loading staff:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadAvailableSlots = async () => {
    try {
      setLoading(true);
      setError('');

      if (formData.staffId) {
        // Load slots for specific staff
        const slots = await appointmentService.getAvailableTimeSlots(
          formData.staffId,
          selectedDate,
          selectedService?.durationMinutes || 60
        );
        setAvailableSlots(slots);
      } else if (formData.salonId && formData.serviceId) {
        // Load all available slots for the salon
        const salonSlots = await appointmentService.getSalonAvailability(
          formData.salonId,
          formData.serviceId,
          selectedDate
        );
        
        // Flatten all staff slots into a single array
        const allSlots: AvailableTimeSlotDto[] = [];
        Object.values(salonSlots).forEach((staffSlots) => {
          allSlots.push(...staffSlots);
        });
        
        // Remove duplicates based on startTime
        const uniqueSlots = allSlots.filter(
          (slot, index, self) =>
            index === self.findIndex((s) => s.startTime === slot.startTime)
        );
        
        setAvailableSlots(uniqueSlots);
      }
    } catch (err) {
      setError('Müsait saatler yüklenemedi.');
      console.error('Error loading slots:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSalonSelect = (salon: SalonDto) => {
    setSelectedSalon(salon);
    setFormData((prev) => ({ ...prev, salonId: salon.id }));
    setCurrentStep(2);
  };

  const handleServiceSelect = (service: ServiceDto) => {
    setSelectedService(service);
    setFormData((prev) => ({ ...prev, serviceId: service.id }));
    setCurrentStep(3);
  };

  const handleStaffSelect = (staffMember: StaffDto | null) => {
    setSelectedStaff(staffMember);
    setFormData((prev) => ({ ...prev, staffId: staffMember?.id || '' }));
    setCurrentStep(4);
  };

  const handleDateSelect = (date: string) => {
    setSelectedDate(date);
    setSelectedSlot(null);
  };

  const handleSlotSelect = (slot: AvailableTimeSlotDto) => {
    setSelectedSlot(slot);
    
    // Combine date and time to create ISO string
    const dateTime = new Date(`${selectedDate}T${slot.startTime}`);
    setFormData((prev) => ({ ...prev, startTime: dateTime.toISOString() }));
  };

  const handleNotesChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setFormData((prev) => ({ ...prev, customerNotes: e.target.value }));
  };

  const handleSubmit = async () => {
    if (!acceptedTerms) {
      setError('Lütfen randevu şartlarını kabul edin.');
      return;
    }

    try {
      setLoading(true);
      setError('');

      const appointmentData: CreateAppointmentDto = {
        salonId: formData.salonId,
        serviceId: formData.serviceId,
        staffId: formData.staffId,
        startTime: formData.startTime,
        customerNotes: formData.customerNotes,
      };

      const createdAppointment = await appointmentService.createAppointment(appointmentData);
      
      // Store appointment ID and show payment dialog
      setCreatedAppointmentId(createdAppointment.id);
      setShowPaymentDialog(true);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Randevu oluşturulamadı. Lütfen tekrar deneyin.');
      console.error('Error creating appointment:', err);
    } finally {
      setLoading(false);
    }
  };

  const handlePaymentSubmit = async (paymentData: CreatePaymentDto) => {
    try {
      setPaymentLoading(true);
      
      const result = await paymentService.createPayment(paymentData);
      
      if (result.status === 'Completed') {
        // Payment successful
        setShowPaymentDialog(false);
        setShowSuccessModal(true);
      } else if (result.paymentUrl) {
        // Redirect to payment gateway (for PayTR)
        window.location.href = result.paymentUrl;
      } else {
        // Payment failed
        setError(`Ödeme başarısız: ${result.errorMessage || 'Lütfen tekrar deneyin.'}`);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Ödeme işlemi başarısız oldu.');
      console.error('Error processing payment:', err);
    } finally {
      setPaymentLoading(false);
    }
  };

  const handlePaymentCancel = () => {
    setShowPaymentDialog(false);
    setError('Randevunuz oluşturuldu ancak ödeme yapılmadı. Randevularım sayfasından ödeme yapabilirsiniz.');
    // Still navigate to appointments page so user can complete payment later
    setTimeout(() => {
      navigate('/my-appointments');
    }, 3000);
  };

  const handleSuccessModalClose = () => {
    setShowSuccessModal(false);
    navigate('/my-appointments');
  };

  const handleBookAnother = () => {
    setShowSuccessModal(false);
    // Reset form
    setCurrentStep(1);
    setFormData({
      salonId: '',
      serviceId: '',
      staffId: '',
      startTime: '',
      customerNotes: '',
    });
    setSelectedSalon(null);
    setSelectedService(null);
    setSelectedStaff(null);
    setSelectedDate('');
    setSelectedSlot(null);
    setAcceptedTerms(false);
  };

  const goBack = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  };

  const goNext = () => {
    if (currentStep < 5) {
      setCurrentStep(currentStep + 1);
    }
  };

  // Get minimum date (today)
  const getMinDate = () => {
    return new Date().toISOString().split('T')[0];
  };

  // Get maximum date (3 months from now)
  const getMaxDate = () => {
    const maxDate = new Date();
    maxDate.setMonth(maxDate.getMonth() + 3);
    return maxDate.toISOString().split('T')[0];
  };

  // Format time from TimeSpan format (HH:mm:ss) to display format
  const formatTime = (timeString: string) => {
    const [hours, minutes] = timeString.split(':');
    return `${hours}:${minutes}`;
  };

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Progress Indicator */}
        <div className="mb-8">
          <div className="flex items-center justify-between">
            {[1, 2, 3, 4, 5].map((step) => (
              <div key={step} className="flex items-center">
                <div
                  className={`w-10 h-10 rounded-full flex items-center justify-center text-sm font-medium ${
                    step <= currentStep
                      ? 'bg-blue-600 text-white'
                      : 'bg-gray-200 text-gray-600'
                  }`}
                >
                  {step}
                </div>
                {step < 5 && (
                  <div
                    className={`w-16 h-1 mx-2 ${
                      step < currentStep ? 'bg-blue-600' : 'bg-gray-200'
                    }`}
                  />
                )}
              </div>
            ))}
          </div>
          <div className="flex justify-between mt-2 text-xs text-gray-600">
            <span>Salon</span>
            <span>Hizmet</span>
            <span>Personel</span>
            <span>Tarih/Saat</span>
            <span>Onay</span>
          </div>
        </div>

        {/* Error Display */}
        {error && (
          <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-800">{error}</p>
          </div>
        )}

        {/* Step Content */}
        <div className="bg-white rounded-lg shadow-md p-6">
          {/* Step 1: Select Salon */}
          {currentStep === 1 && (
            <div>
              <h2 className="text-2xl font-bold mb-6">Salon Seçin</h2>
              {loading ? (
                <div className="text-center py-8">
                  <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                  <p className="mt-4 text-gray-600">Yükleniyor...</p>
                </div>
              ) : paramSalonId && selectedSalon ? (
                <div className="border rounded-lg p-4 bg-blue-50">
                  <h3 className="font-semibold text-lg">{selectedSalon.name}</h3>
                  <p className="text-gray-600">{selectedSalon.address}, {selectedSalon.city}</p>
                  <p className="text-gray-600">{selectedSalon.phone}</p>
                  <button
                    onClick={() => setCurrentStep(2)}
                    className="mt-4 px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                  >
                    Devam Et
                  </button>
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {salons.map((salon) => (
                    <div
                      key={salon.id}
                      onClick={() => handleSalonSelect(salon)}
                      className="border rounded-lg p-4 hover:border-blue-500 hover:shadow-lg cursor-pointer transition"
                    >
                      <h3 className="font-semibold text-lg">{salon.name}</h3>
                      <p className="text-gray-600 text-sm">{salon.address}, {salon.city}</p>
                      <p className="text-gray-600 text-sm">{salon.phone}</p>
                      <div className="mt-2 flex items-center">
                        <span className="text-yellow-500">★</span>
                        <span className="ml-1 text-sm">{salon.averageRating.toFixed(1)}</span>
                        <span className="ml-2 text-sm text-gray-500">({salon.reviewCount} değerlendirme)</span>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}

          {/* Step 2: Select Service */}
          {currentStep === 2 && (
            <div>
              <h2 className="text-2xl font-bold mb-6">Hizmet Seçin</h2>
              {loading ? (
                <div className="text-center py-8">
                  <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                  <p className="mt-4 text-gray-600">Yükleniyor...</p>
                </div>
              ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {services.map((service) => (
                    <div
                      key={service.id}
                      onClick={() => handleServiceSelect(service)}
                      className="border rounded-lg p-4 hover:border-blue-500 hover:shadow-lg cursor-pointer transition"
                    >
                      {service.imageUrl && (
                        <img
                          src={service.imageUrl}
                          alt={service.name}
                          className="w-full h-32 object-cover rounded-md mb-3"
                        />
                      )}
                      <h3 className="font-semibold text-lg">{service.name}</h3>
                      {service.description && (
                        <p className="text-gray-600 text-sm mt-1">{service.description}</p>
                      )}
                      <div className="mt-3 flex justify-between items-center">
                        <span className="text-blue-600 font-bold">{service.price} ₺</span>
                        <span className="text-gray-500 text-sm">{service.durationMinutes} dk</span>
                      </div>
                      {service.categoryName && (
                        <span className="inline-block mt-2 px-2 py-1 bg-gray-100 rounded text-xs">
                          {service.categoryName}
                        </span>
                      )}
                    </div>
                  ))}
                </div>
              )}
              <div className="mt-6">
                <button
                  onClick={goBack}
                  className="px-6 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
                >
                  Geri
                </button>
              </div>
            </div>
          )}

          {/* Step 3: Select Staff */}
          {currentStep === 3 && (
            <div>
              <h2 className="text-2xl font-bold mb-6">Personel Seçin</h2>
              {loading ? (
                <div className="text-center py-8">
                  <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                  <p className="mt-4 text-gray-600">Yükleniyor...</p>
                </div>
              ) : (
                <>
                  {/* "Any Available" Option */}
                  <div
                    onClick={() => handleStaffSelect(null)}
                    className="border-2 border-dashed rounded-lg p-4 hover:border-blue-500 hover:bg-blue-50 cursor-pointer transition mb-4"
                  >
                    <h3 className="font-semibold text-lg">Müsait Personel</h3>
                    <p className="text-gray-600 text-sm">En yakın müsait personeli otomatik seç</p>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {staff.map((staffMember) => (
                      <div
                        key={staffMember.id}
                        onClick={() => handleStaffSelect(staffMember)}
                        className="border rounded-lg p-4 hover:border-blue-500 hover:shadow-lg cursor-pointer transition"
                      >
                        <div className="flex items-center">
                          {staffMember.profilePictureUrl ? (
                            <img
                              src={staffMember.profilePictureUrl}
                              alt={staffMember.fullName}
                              className="w-16 h-16 rounded-full object-cover"
                            />
                          ) : (
                            <div className="w-16 h-16 rounded-full bg-gray-200 flex items-center justify-center">
                              <span className="text-2xl text-gray-500">
                                {staffMember.firstName.charAt(0)}
                              </span>
                            </div>
                          )}
                          <div className="ml-4 flex-1">
                            <h3 className="font-semibold">{staffMember.fullName}</h3>
                            {staffMember.roleName && (
                              <p className="text-gray-600 text-sm">{staffMember.roleName}</p>
                            )}
                            <div className="flex items-center mt-1">
                              <span className="text-yellow-500 text-sm">★</span>
                              <span className="ml-1 text-sm">{staffMember.averageRating.toFixed(1)}</span>
                            </div>
                          </div>
                        </div>
                        {staffMember.bio && (
                          <p className="mt-3 text-gray-600 text-sm">{staffMember.bio}</p>
                        )}
                      </div>
                    ))}
                  </div>
                </>
              )}
              <div className="mt-6">
                <button
                  onClick={goBack}
                  className="px-6 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
                >
                  Geri
                </button>
              </div>
            </div>
          )}

          {/* Step 4: Select Date & Time */}
          {currentStep === 4 && (
            <div>
              <h2 className="text-2xl font-bold mb-6">Tarih ve Saat Seçin</h2>
              
              {/* Date Picker */}
              <div className="mb-6">
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Randevu Tarihi
                </label>
                <input
                  type="date"
                  value={selectedDate}
                  onChange={(e) => handleDateSelect(e.target.value)}
                  min={getMinDate()}
                  max={getMaxDate()}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>

              {/* Time Slots */}
              {selectedDate && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Müsait Saatler
                  </label>
                  {loading ? (
                    <div className="text-center py-8">
                      <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                      <p className="mt-4 text-gray-600">Müsait saatler yükleniyor...</p>
                    </div>
                  ) : availableSlots.length === 0 ? (
                    <div className="text-center py-8 text-gray-600">
                      <p>Bu tarih için müsait saat bulunamadı.</p>
                      <p className="text-sm mt-2">Lütfen başka bir tarih seçin.</p>
                    </div>
                  ) : (
                    <div className="grid grid-cols-3 md:grid-cols-4 gap-3">
                      {availableSlots.map((slot, index) => (
                        <button
                          key={index}
                          onClick={() => handleSlotSelect(slot)}
                          className={`px-4 py-3 border rounded-lg text-sm font-medium transition ${
                            selectedSlot?.startTime === slot.startTime
                              ? 'bg-blue-600 text-white border-blue-600'
                              : 'bg-white text-gray-700 border-gray-300 hover:border-blue-500 hover:bg-blue-50'
                          }`}
                        >
                          {formatTime(slot.startTime)}
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              )}

              <div className="mt-6 flex gap-4">
                <button
                  onClick={goBack}
                  className="px-6 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
                >
                  Geri
                </button>
                {selectedSlot && (
                  <button
                    onClick={goNext}
                    className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                  >
                    Devam Et
                  </button>
                )}
              </div>
            </div>
          )}

          {/* Step 5: Confirmation */}
          {currentStep === 5 && (
            <div>
              <h2 className="text-2xl font-bold mb-6">Randevu Özeti</h2>
              
              <div className="space-y-4 mb-6">
                {/* Salon Info */}
                <div className="border-b pb-4">
                  <h3 className="font-semibold text-gray-700">Salon</h3>
                  <p className="text-lg">{selectedSalon?.name}</p>
                  <p className="text-gray-600 text-sm">{selectedSalon?.address}, {selectedSalon?.city}</p>
                </div>

                {/* Service Info */}
                <div className="border-b pb-4">
                  <h3 className="font-semibold text-gray-700">Hizmet</h3>
                  <p className="text-lg">{selectedService?.name}</p>
                  <p className="text-gray-600 text-sm">
                    {selectedService?.durationMinutes} dakika - {selectedService?.price} ₺
                  </p>
                </div>

                {/* Staff Info */}
                <div className="border-b pb-4">
                  <h3 className="font-semibold text-gray-700">Personel</h3>
                  <p className="text-lg">{selectedStaff ? selectedStaff.fullName : 'Müsait Personel'}</p>
                </div>

                {/* Date & Time */}
                <div className="border-b pb-4">
                  <h3 className="font-semibold text-gray-700">Tarih ve Saat</h3>
                  <p className="text-lg">
                    {new Date(selectedDate).toLocaleDateString('tr-TR', {
                      weekday: 'long',
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric',
                    })}
                  </p>
                  <p className="text-gray-600">{selectedSlot && formatTime(selectedSlot.startTime)}</p>
                </div>

                {/* Customer Notes */}
                <div>
                  <label className="block font-semibold text-gray-700 mb-2">
                    Notlar (İsteğe Bağlı)
                  </label>
                  <textarea
                    value={formData.customerNotes}
                    onChange={handleNotesChange}
                    placeholder="Randevunuz ile ilgili özel bir notunuz var mı?"
                    rows={3}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>

                {/* Terms & Conditions */}
                <div className="flex items-start">
                  <input
                    type="checkbox"
                    id="terms"
                    checked={acceptedTerms}
                    onChange={(e) => setAcceptedTerms(e.target.checked)}
                    className="mt-1 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                  />
                  <label htmlFor="terms" className="ml-2 text-sm text-gray-700">
                    Randevu iptal koşullarını ve gizlilik politikasını okudum, kabul ediyorum.
                  </label>
                </div>

                {/* Price Summary */}
                <div className="bg-gray-50 p-4 rounded-lg">
                  <div className="flex justify-between items-center">
                    <span className="text-lg font-semibold">Toplam Tutar:</span>
                    <span className="text-2xl font-bold text-blue-600">{selectedService?.price} ₺</span>
                  </div>
                </div>
              </div>

              <div className="flex gap-4">
                <button
                  onClick={goBack}
                  className="px-6 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
                  disabled={loading}
                >
                  Geri
                </button>
                <button
                  onClick={handleSubmit}
                  disabled={!acceptedTerms || loading}
                  className="flex-1 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-300 disabled:cursor-not-allowed font-semibold"
                >
                  {loading ? 'Oluşturuluyor...' : 'Randevuyu Onayla'}
                </button>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Success Modal */}
      {showSuccessModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <div className="text-center">
              <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100 mb-4">
                <svg
                  className="h-6 w-6 text-green-600"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth="2"
                    d="M5 13l4 4L19 7"
                  />
                </svg>
              </div>
              <h3 className="text-lg font-medium text-gray-900 mb-2">Randevunuz Oluşturuldu!</h3>
              <p className="text-sm text-gray-500 mb-6">
                Ödemeniz başarıyla alındı. Randevu detayları e-posta adresinize gönderildi. 
                Randevularım sayfasından tüm randevularınızı görüntüleyebilirsiniz.
              </p>
              <div className="flex gap-3">
                <button
                  onClick={handleBookAnother}
                  className="flex-1 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
                >
                  Başka Randevu Al
                </button>
                <button
                  onClick={handleSuccessModalClose}
                  className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                >
                  Randevularım
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Payment Dialog */}
      <PaymentFormDialog
        open={showPaymentDialog}
        onClose={handlePaymentCancel}
        onSubmit={handlePaymentSubmit}
        amount={selectedService?.price || 0}
        currency="TRY"
        appointmentId={createdAppointmentId}
      />
    </div>
  );
};

export default BookAppointmentPage;
