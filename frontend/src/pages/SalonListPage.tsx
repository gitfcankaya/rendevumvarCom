import React, { useState, useEffect } from 'react';
import salonService, { type SalonDto, type SearchSalonDto } from '../services/salonService';
import SalonCard from '../components/SalonCard';

const SalonListPage: React.FC = () => {
  const [salons, setSalons] = useState<SalonDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  const [searchParams, setSearchParams] = useState<SearchSalonDto>({
    searchTerm: '',
    city: '',
    minRating: undefined,
    pageNumber: 1,
    pageSize: 12,
  });
  
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);

  useEffect(() => {
    fetchSalons();
  }, [searchParams.pageNumber]);

  const fetchSalons = async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await salonService.searchSalons(searchParams);
      setSalons(result.salons);
      setTotalCount(result.totalCount);
      setTotalPages(result.totalPages);
    } catch (err) {
      setError('Salonlar yüklenirken bir hata oluştu');
      console.error('Error fetching salons:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setSearchParams(prev => ({ ...prev, pageNumber: 1 }));
    fetchSalons();
  };

  const handlePrevPage = () => {
    if (searchParams.pageNumber && searchParams.pageNumber > 1) {
      setSearchParams(prev => ({ ...prev, pageNumber: (prev.pageNumber || 1) - 1 }));
    }
  };

  const handleNextPage = () => {
    if (searchParams.pageNumber && searchParams.pageNumber < totalPages) {
      setSearchParams(prev => ({ ...prev, pageNumber: (prev.pageNumber || 1) + 1 }));
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 py-8">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Salonları Keşfet</h1>
          <p className="mt-2 text-gray-600">Size en yakın berber ve kuaförü bulun</p>
        </div>

        {/* Search Bar */}
        <form onSubmit={handleSearch} className="bg-white p-4 rounded-lg shadow mb-8">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div>
              <label htmlFor="searchTerm" className="block text-sm font-medium text-gray-700 mb-1">
                Arama
              </label>
              <input
                type="text"
                id="searchTerm"
                value={searchParams.searchTerm}
                onChange={(e) => setSearchParams(prev => ({ ...prev, searchTerm: e.target.value }))}
                placeholder="Salon adı veya açıklama..."
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            
            <div>
              <label htmlFor="city" className="block text-sm font-medium text-gray-700 mb-1">
                Şehir
              </label>
              <input
                type="text"
                id="city"
                value={searchParams.city}
                onChange={(e) => setSearchParams(prev => ({ ...prev, city: e.target.value }))}
                placeholder="İstanbul, Ankara..."
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            
            <div>
              <label htmlFor="minRating" className="block text-sm font-medium text-gray-700 mb-1">
                Min. Puan
              </label>
              <select
                id="minRating"
                value={searchParams.minRating || ''}
                onChange={(e) => setSearchParams(prev => ({ 
                  ...prev, 
                  minRating: e.target.value ? parseFloat(e.target.value) : undefined 
                }))}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Tümü</option>
                <option value="3">3+ Puan</option>
                <option value="4">4+ Puan</option>
                <option value="4.5">4.5+ Puan</option>
              </select>
            </div>
            
            <div className="flex items-end">
              <button
                type="submit"
                className="w-full bg-blue-600 text-white px-6 py-2 rounded-md hover:bg-blue-700 transition-colors duration-200 flex items-center justify-center gap-2"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
                Ara
              </button>
            </div>
          </div>
        </form>

        {/* Results */}
        {loading ? (
          <div className="text-center py-12">
            <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            <p className="mt-4 text-gray-600">Salonlar yükleniyor...</p>
          </div>
        ) : error ? (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-center">
            <p className="text-red-600">{error}</p>
            <button
              onClick={fetchSalons}
              className="mt-2 text-red-600 hover:text-red-700 underline"
            >
              Tekrar Dene
            </button>
          </div>
        ) : salons.length === 0 ? (
          <div className="bg-white rounded-lg shadow p-12 text-center">
            <svg className="mx-auto h-16 w-16 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <h3 className="mt-4 text-lg font-medium text-gray-900">Salon bulunamadı</h3>
            <p className="mt-2 text-gray-500">Arama kriterlerinizi değiştirip tekrar deneyin.</p>
          </div>
        ) : (
          <>
            {/* Salon Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
              {salons.map(salon => (
                <SalonCard key={salon.id} salon={salon} />
              ))}
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="bg-white rounded-lg shadow p-4 flex items-center justify-between">
                <div className="text-sm text-gray-600">
                  {totalCount} salondan {((searchParams.pageNumber || 1) - 1) * (searchParams.pageSize || 12) + 1} 
                  - {Math.min((searchParams.pageNumber || 1) * (searchParams.pageSize || 12), totalCount)} arası gösteriliyor
                </div>
                
                <div className="flex gap-2">
                  <button
                    onClick={handlePrevPage}
                    disabled={searchParams.pageNumber === 1}
                    className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Önceki
                  </button>
                  
                  <span className="px-4 py-2 text-gray-700">
                    Sayfa {searchParams.pageNumber} / {totalPages}
                  </span>
                  
                  <button
                    onClick={handleNextPage}
                    disabled={searchParams.pageNumber === totalPages}
                    className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Sonraki
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default SalonListPage;
