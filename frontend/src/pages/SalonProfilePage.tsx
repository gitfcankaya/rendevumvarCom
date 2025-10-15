import React, { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import salonService, { type SalonDetailsDto, type ServiceDto } from '../services/salonService';

const SalonProfilePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [salon, setSalon] = useState<SalonDetailsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedImageIndex, setSelectedImageIndex] = useState(0);

  useEffect(() => {
    if (id) {
      fetchSalonDetails();
    }
  }, [id]);

  const fetchSalonDetails = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await salonService.getSalonDetails(id!);
      setSalon(data);
    } catch (err) {
      setError('Salon bilgileri yüklenirken bir hata oluştu');
      console.error('Error fetching salon details:', err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
          <p className="mt-4 text-gray-600">Yükleniyor...</p>
        </div>
      </div>
    );
  }

  if (error || !salon) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="bg-white rounded-lg shadow p-8 max-w-md text-center">
          <p className="text-red-600 mb-4">{error || 'Salon bulunamadı'}</p>
          <Link to="/salons" className="text-blue-600 hover:text-blue-700">
            Salon Listesine Dön
          </Link>
        </div>
      </div>
    );
  }

  const images = salon.images.length > 0 ? salon.images : [{ imageUrl: 'https://via.placeholder.com/800x600?text=Salon', isPrimary: true }];
  const currentImage = images[selectedImageIndex] || images[0];

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Back Button */}
        <Link to="/salons" className="inline-flex items-center text-blue-600 hover:text-blue-700 mb-6">
          <svg className="w-5 h-5 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
          </svg>
          Geri
        </Link>

        <div className="bg-white rounded-lg shadow overflow-hidden">
          {/* Image Gallery */}
          <div className="relative">
            <div className="aspect-w-16 aspect-h-9 bg-gray-200">
              <img
                src={currentImage.imageUrl}
                alt={salon.name}
                className="w-full h-96 object-cover"
              />
            </div>
            
            {images.length > 1 && (
              <div className="absolute bottom-4 left-0 right-0 flex justify-center gap-2 px-4">
                {images.map((img, index) => (
                  <button
                    key={index}
                    onClick={() => setSelectedImageIndex(index)}
                    className={`w-16 h-16 rounded-lg overflow-hidden border-2 ${
                      index === selectedImageIndex ? 'border-white' : 'border-transparent'
                    }`}
                  >
                    <img src={img.imageUrl} alt="" className="w-full h-full object-cover" />
                  </button>
                ))}
              </div>
            )}
          </div>

          {/* Content */}
          <div className="p-6">
            {/* Header */}
            <div className="flex items-start justify-between mb-6">
              <div>
                <h1 className="text-3xl font-bold text-gray-900 mb-2">{salon.name}</h1>
                {salon.averageRating > 0 && (
                  <div className="flex items-center gap-2">
                    <div className="flex items-center">
                      {[...Array(5)].map((_, i) => (
                        <svg
                          key={i}
                          className={`w-5 h-5 ${
                            i < Math.floor(salon.averageRating) ? 'text-yellow-400' : 'text-gray-300'
                          } fill-current`}
                          viewBox="0 0 20 20"
                        >
                          <path d="M10 15l-5.878 3.09 1.123-6.545L.489 6.91l6.572-.955L10 0l2.939 5.955 6.572.955-4.756 4.635 1.123 6.545z" />
                        </svg>
                      ))}
                    </div>
                    <span className="text-sm text-gray-600">
                      {salon.averageRating.toFixed(1)} ({salon.reviewCount} değerlendirme)
                    </span>
                  </div>
                )}
              </div>
              
              <button className="bg-blue-600 text-white px-6 py-3 rounded-lg hover:bg-blue-700 transition-colors">
                Randevu Al
              </button>
            </div>

            {/* Description */}
            {salon.description && (
              <div className="mb-6">
                <h2 className="text-xl font-semibold text-gray-900 mb-2">Hakkında</h2>
                <p className="text-gray-600">{salon.description}</p>
              </div>
            )}

            {/* Contact Info */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6 pb-6 border-b">
              <div>
                <h3 className="font-semibold text-gray-900 mb-3">İletişim Bilgileri</h3>
                <div className="space-y-2">
                  <div className="flex items-start">
                    <svg className="w-5 h-5 text-gray-400 mr-2 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                    </svg>
                    <span className="text-gray-600">{salon.address}, {salon.city}</span>
                  </div>
                  
                  <div className="flex items-center">
                    <svg className="w-5 h-5 text-gray-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
                    </svg>
                    <a href={`tel:${salon.phone}`} className="text-blue-600 hover:text-blue-700">
                      {salon.phone}
                    </a>
                  </div>
                  
                  {salon.email && (
                    <div className="flex items-center">
                      <svg className="w-5 h-5 text-gray-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                      </svg>
                      <a href={`mailto:${salon.email}`} className="text-blue-600 hover:text-blue-700">
                        {salon.email}
                      </a>
                    </div>
                  )}
                  
                  {salon.website && (
                    <div className="flex items-center">
                      <svg className="w-5 h-5 text-gray-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9c1.657 0 3-4.03 3-9s-1.343-9-3-9m0 18c-1.657 0-3-4.03-3-9s1.343-9 3-9m-9 9a9 9 0 019-9" />
                      </svg>
                      <a href={salon.website} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:text-blue-700">
                        Website
                      </a>
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Services */}
            {salon.services.length > 0 && (
              <div className="mb-6">
                <h2 className="text-xl font-semibold text-gray-900 mb-4">Hizmetler</h2>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                  {salon.services.map(service => (
                    <div key={service.id} className="border border-gray-200 rounded-lg p-4">
                      <h3 className="font-semibold text-gray-900 mb-1">{service.name}</h3>
                      {service.description && (
                        <p className="text-sm text-gray-600 mb-2">{service.description}</p>
                      )}
                      <div className="flex items-center justify-between">
                        <span className="text-lg font-semibold text-blue-600">
                          ₺{service.price.toFixed(2)}
                        </span>
                        <span className="text-sm text-gray-500">
                          {service.durationMinutes} dk
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Staff */}
            {salon.staff.length > 0 && (
              <div>
                <h2 className="text-xl font-semibold text-gray-900 mb-4">Ekibimiz</h2>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                  {salon.staff.map(staff => (
                    <div key={staff.id} className="text-center">
                      <div className="w-24 h-24 mx-auto mb-3 rounded-full overflow-hidden bg-gray-200">
                        {staff.profilePictureUrl ? (
                          <img src={staff.profilePictureUrl} alt={`${staff.firstName} ${staff.lastName}`} className="w-full h-full object-cover" />
                        ) : (
                          <div className="w-full h-full flex items-center justify-center text-gray-400 text-2xl font-semibold">
                            {staff.firstName.charAt(0)}{staff.lastName.charAt(0)}
                          </div>
                        )}
                      </div>
                      <h3 className="font-semibold text-gray-900">
                        {staff.firstName} {staff.lastName}
                      </h3>
                      {staff.roleName && (
                        <p className="text-sm text-gray-500">{staff.roleName}</p>
                      )}
                      {staff.averageRating > 0 && (
                        <div className="flex items-center justify-center mt-1">
                          <svg className="w-4 h-4 text-yellow-400 fill-current" viewBox="0 0 20 20">
                            <path d="M10 15l-5.878 3.09 1.123-6.545L.489 6.91l6.572-.955L10 0l2.939 5.955 6.572.955-4.756 4.635 1.123 6.545z" />
                          </svg>
                          <span className="text-sm text-gray-600 ml-1">
                            {staff.averageRating.toFixed(1)}
                          </span>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default SalonProfilePage;
