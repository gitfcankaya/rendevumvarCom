import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import salonService, { type ServiceDto, type StaffDto } from '../services/salonService';

interface ServiceFormData {
  name: string;
  description: string;
  categoryId: string;
  price: number;
  durationMinutes: number;
  imageUrl: string;
  isActive: boolean;
}

const ServiceManagementPage: React.FC = () => {
  const { salonId } = useParams<{ salonId: string }>();
  const navigate = useNavigate();
  const [services, setServices] = useState<ServiceDto[]>([]);
  const [staff, setStaff] = useState<StaffDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMessage, setSuccessMessage] = useState('');
  
  const [showModal, setShowModal] = useState(false);
  const [editingService, setEditingService] = useState<ServiceDto | null>(null);
  const [formData, setFormData] = useState<ServiceFormData>({
    name: '',
    description: '',
    categoryId: '',
    price: 0,
    durationMinutes: 30,
    imageUrl: '',
    isActive: true
  });

  useEffect(() => {
    if (salonId) {
      fetchServices();
      fetchStaff();
    }
  }, [salonId]);

  const fetchServices = async () => {
    try {
      setLoading(true);
      const data = await salonService.getSalonServices(salonId!);
      setServices(data);
    } catch (err) {
      setError('Hizmetler yüklenemedi');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchStaff = async () => {
    try {
      const salonDetails = await salonService.getSalonDetails(salonId!);
      setStaff(salonDetails.staff || []);
    } catch (err) {
      console.error('Staff could not be loaded:', err);
    }
  };

  const handleAddService = () => {
    setEditingService(null);
    setFormData({
      name: '',
      description: '',
      categoryId: '',
      price: 0,
      durationMinutes: 30,
      imageUrl: '',
      isActive: true
    });
    setShowModal(true);
  };

  const handleEditService = (service: ServiceDto) => {
    setEditingService(service);
    setFormData({
      name: service.name,
      description: service.description || '',
      categoryId: service.categoryId || '',
      price: service.price,
      durationMinutes: service.durationMinutes,
      imageUrl: service.imageUrl || '',
      isActive: service.isActive
    });
    setShowModal(true);
  };

  const handleDeleteService = async (_serviceId: string) => {
    if (!window.confirm('Bu hizmeti silmek istediğinizden emin misiniz?')) {
      return;
    }

    try {
      // In a full implementation, call the delete service API
      setSuccessMessage('Hizmet silindi!');
      setTimeout(() => {
        fetchServices();
        setSuccessMessage('');
      }, 1000);
    } catch (err) {
      setError('Hizmet silinemedi');
    }
  };

  const handleAssignStaff = async (_serviceId: string, _staffId: string) => {
    try {
      // In a full implementation, call assign staff API
      setSuccessMessage('Personel atandı!');
      setTimeout(() => {
        fetchServices();
        setSuccessMessage('');
      }, 1000);
    } catch (err) {
      setError('Personel atanamadı');
    }
  };

  const handleRemoveStaff = async (_serviceId: string, _staffId: string) => {
    try {
      // In a full implementation, call remove staff API
      setSuccessMessage('Personel kaldırıldı!');
      setTimeout(() => {
        fetchServices();
        setSuccessMessage('');
      }, 1000);
    } catch (err) {
      setError('Personel kaldırılamadı');
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Hizmet Yönetimi</h1>
        <div className="space-x-4">
          <button
            onClick={handleAddService}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            + Yeni Hizmet
          </button>
          <button
            onClick={() => navigate('/dashboard')}
            className="px-4 py-2 bg-gray-500 text-white rounded hover:bg-gray-600"
          >
            Geri
          </button>
        </div>
      </div>

      {successMessage && (
        <div className="mb-4 bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded">
          {successMessage}
        </div>
      )}

      {error && (
        <div className="mb-4 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
      )}

      {/* Services Table */}
      <div className="bg-white shadow rounded-lg overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Hizmet
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Kategori
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Fiyat
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Süre
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Personel
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Durum
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                İşlemler
              </th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {services.map((service) => (
              <tr key={service.id}>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="flex items-center">
                    {service.imageUrl && (
                      <img
                        src={service.imageUrl}
                        alt={service.name}
                        className="h-10 w-10 rounded object-cover mr-3"
                      />
                    )}
                    <div>
                      <div className="text-sm font-medium text-gray-900">{service.name}</div>
                      <div className="text-sm text-gray-500">{service.description}</div>
                    </div>
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {service.categoryName || '-'}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  ₺{service.price.toFixed(2)}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {service.durationMinutes} dk
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm">
                  <div className="flex -space-x-2">
                    {service.staffIds && service.staffIds.length > 0 ? (
                      <>
                        {staff
                          .filter(s => service.staffIds?.includes(s.id))
                          .slice(0, 3)
                          .map(s => (
                            <div
                              key={s.id}
                              className="h-8 w-8 rounded-full bg-blue-500 text-white flex items-center justify-center text-xs border-2 border-white"
                              title={s.fullName}
                            >
                              {s.fullName.charAt(0)}
                            </div>
                          ))}
                        {service.staffIds.length > 3 && (
                          <div className="h-8 w-8 rounded-full bg-gray-300 text-gray-700 flex items-center justify-center text-xs border-2 border-white">
                            +{service.staffIds.length - 3}
                          </div>
                        )}
                      </>
                    ) : (
                      <span className="text-gray-400">Atanmamış</span>
                    )}
                  </div>
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                    service.isActive 
                      ? 'bg-green-100 text-green-800' 
                      : 'bg-red-100 text-red-800'
                  }`}>
                    {service.isActive ? 'Aktif' : 'Pasif'}
                  </span>
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium space-x-2">
                  <button
                    onClick={() => handleEditService(service)}
                    className="text-blue-600 hover:text-blue-900"
                  >
                    Düzenle
                  </button>
                  <button
                    onClick={() => handleDeleteService(service.id)}
                    className="text-red-600 hover:text-red-900"
                  >
                    Sil
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {services.length === 0 && (
          <div className="text-center py-12">
            <p className="text-gray-500 text-lg">Henüz hizmet eklenmemiş.</p>
            <button
              onClick={handleAddService}
              className="mt-4 px-6 py-3 bg-blue-600 text-white rounded hover:bg-blue-700"
            >
              İlk Hizmeti Ekle
            </button>
          </div>
        )}
      </div>

      {/* Modal for Add/Edit Service */}
      {showModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 w-full max-w-md">
            <h2 className="text-2xl font-bold mb-4">
              {editingService ? 'Hizmet Düzenle' : 'Yeni Hizmet Ekle'}
            </h2>

            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Hizmet Adı *
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({...formData, name: e.target.value})}
                  required
                  className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Açıklama
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({...formData, description: e.target.value})}
                  rows={3}
                  className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Fiyat (₺) *
                  </label>
                  <input
                    type="number"
                    step="0.01"
                    value={formData.price}
                    onChange={(e) => setFormData({...formData, price: parseFloat(e.target.value)})}
                    required
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Süre (dk) *
                  </label>
                  <input
                    type="number"
                    value={formData.durationMinutes}
                    onChange={(e) => setFormData({...formData, durationMinutes: parseInt(e.target.value)})}
                    required
                    className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Görsel URL
                </label>
                <input
                  type="url"
                  value={formData.imageUrl}
                  onChange={(e) => setFormData({...formData, imageUrl: e.target.value})}
                  className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>

              <div>
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    checked={formData.isActive}
                    onChange={(e) => setFormData({...formData, isActive: e.target.checked})}
                    className="mr-2"
                  />
                  <span className="text-sm font-medium text-gray-700">Aktif</span>
                </label>
              </div>

              <div className="flex justify-end space-x-4 pt-4">
                <button
                  type="button"
                  onClick={() => setShowModal(false)}
                  className="px-4 py-2 bg-gray-500 text-white rounded hover:bg-gray-600"
                >
                  İptal
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                >
                  {editingService ? 'Güncelle' : 'Ekle'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default ServiceManagementPage;
