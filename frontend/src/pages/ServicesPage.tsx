import React, { useState, useEffect } from 'react';
import { serviceService } from '../services/serviceService';
import type { ServiceListItem, Service, CreateServiceRequest, UpdateServiceRequest } from '../services/serviceTypes';
import { CategoryManagement } from '../components/CategoryManagement';
import '../styles/services.css';

interface ServiceFormData {
  name: string;
  description: string;
  price: number;
  durationMinutes: number;
  category: string;
  notes: string;
  imageUrl: string;
  isActive: boolean;
}

const ServicesPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState<'services' | 'categories'>('services');
  const [services, setServices] = useState<ServiceListItem[]>([]);
  const [categories, setCategories] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editingService, setEditingService] = useState<Service | null>(null);
  const [formData, setFormData] = useState<ServiceFormData>({
    name: '',
    description: '',
    price: 0,
    durationMinutes: 60,
    category: '',
    notes: '',
    imageUrl: '',
    isActive: true
  });
  const [filter, setFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const [categoryFilter, setCategoryFilter] = useState<string>('');

  useEffect(() => {
    loadServices();
    loadCategories();
  }, []);

  const loadServices = async () => {
    try {
      setLoading(true);
      const data = await serviceService.getAllServices();
      setServices(data);
    } catch (error) {
      console.error('Hizmetler yüklenirken hata:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadCategories = async () => {
    try {
      const data = await serviceService.getCategories();
      setCategories(data);
    } catch (error) {
      console.error('Kategoriler yüklenirken hata:', error);
    }
  };

  const filteredServices = services.filter(service => {
    const matchesStatus = filter === 'all' || 
                         (filter === 'active' && service.isActive) || 
                         (filter === 'inactive' && !service.isActive);
    const matchesCategory = !categoryFilter || service.category === categoryFilter;
    return matchesStatus && matchesCategory;
  });

  const handleFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      if (editingService) {
        const updateData: UpdateServiceRequest = { ...formData };
        await serviceService.updateService(editingService.id, updateData);
      } else {
        const createData: CreateServiceRequest = {
          name: formData.name,
          description: formData.description,
          price: formData.price,
          durationMinutes: formData.durationMinutes,
          category: formData.category,
          notes: formData.notes,
          imageUrl: formData.imageUrl
        };
        await serviceService.createService(createData);
      }
      
      await loadServices();
      await loadCategories();
      handleCancelForm();
    } catch (error) {
      console.error('Hizmet kaydedilirken hata:', error);
    }
  };

  const handleEdit = async (serviceId: string) => {
    try {
      const service = await serviceService.getServiceById(serviceId);
      if (service) {
        setEditingService(service);
        setFormData({
          name: service.name,
          description: service.description || '',
          price: service.price,
          durationMinutes: service.durationMinutes,
          category: service.category || '',
          notes: service.notes || '',
          imageUrl: service.imageUrl || '',
          isActive: service.isActive
        });
        setShowForm(true);
      }
    } catch (error) {
      console.error('Hizmet yüklenirken hata:', error);
    }
  };

  const handleDelete = async (serviceId: string) => {
    if (window.confirm('Bu hizmeti silmek istediğinizden emin misiniz?')) {
      try {
        await serviceService.deleteService(serviceId);
        await loadServices();
      } catch (error) {
        console.error('Hizmet silinirken hata:', error);
      }
    }
  };

  const handleRestore = async (serviceId: string) => {
    try {
      await serviceService.restoreService(serviceId);
      await loadServices();
    } catch (error) {
      console.error('Hizmet geri yüklenirken hata:', error);
    }
  };

  const handleCancelForm = () => {
    setShowForm(false);
    setEditingService(null);
    setFormData({
      name: '',
      description: '',
      price: 0,
      durationMinutes: 60,
      category: '',
      notes: '',
      imageUrl: '',
      isActive: true
    });
  };

  if (loading) {
    return (
      <div className="services-page">
        <div className="loading">Yükleniyor...</div>
      </div>
    );
  }

  return (
    <div className="services-page">
      <div className="page-header">
        <h1>Hizmet Yönetimi</h1>
        <p>Salon hizmetlerinizi ve kategorilerinizi yönetin</p>
      </div>

      <div className="tabs-container">
        <div className="tabs">
          <button
            className={`tab ${activeTab === 'services' ? 'active' : ''}`}
            onClick={() => setActiveTab('services')}
          >
            🛠️ Hizmetler
          </button>
          <button
            className={`tab ${activeTab === 'categories' ? 'active' : ''}`}
            onClick={() => setActiveTab('categories')}
          >
            📁 Kategoriler
          </button>
        </div>
      </div>

      {activeTab === 'services' ? (
        <>
          <div className="services-controls">
            <div className="filters">
          <select 
            value={filter} 
            onChange={(e) => setFilter(e.target.value as 'all' | 'active' | 'inactive')}
            className="filter-select"
          >
            <option value="all">Tüm Hizmetler</option>
            <option value="active">Aktif Hizmetler</option>
            <option value="inactive">Pasif Hizmetler</option>
          </select>

          <select 
            value={categoryFilter} 
            onChange={(e) => setCategoryFilter(e.target.value)}
            className="filter-select"
          >
            <option value="">Tüm Kategoriler</option>
            {categories.map(category => (
              <option key={category} value={category}>{category}</option>
            ))}
          </select>
            </div>

            <button 
              className="btn btn-primary"
              onClick={() => setShowForm(true)}
            >
              + Yeni Hizmet Ekle
            </button>
          </div>

          {showForm && (
            <div className="service-form-modal">
              <div className="service-form-content">
                <div className="form-header">
                  <h2>{editingService ? 'Hizmet Düzenle' : 'Yeni Hizmet Ekle'}</h2>
                  <button className="btn-close" onClick={handleCancelForm}>×</button>
                </div>

                <form onSubmit={handleFormSubmit} className="service-form">
                  <div className="form-row">
                    <div className="form-group">
                      <label>Hizmet Adı *</label>
                      <input
                        type="text"
                        value={formData.name}
                        onChange={(e) => setFormData({...formData, name: e.target.value})}
                        required
                        className="form-input"
                      />
                    </div>

                    <div className="form-group">
                      <label>Kategori</label>
                      <input
                        type="text"
                        value={formData.category}
                        onChange={(e) => setFormData({...formData, category: e.target.value})}
                        className="form-input"
                        list="categories"
                      />
                      <datalist id="categories">
                        {categories.map(category => (
                          <option key={category} value={category} />
                        ))}
                      </datalist>
                    </div>
                  </div>

                  <div className="form-row">
                    <div className="form-group">
                      <label>Fiyat (₺) *</label>
                      <input
                        type="number"
                        value={formData.price}
                        onChange={(e) => setFormData({...formData, price: Number(e.target.value)})}
                        required
                        min="0"
                        step="0.01"
                        className="form-input"
                      />
                    </div>

                    <div className="form-group">
                      <label>Süre (dakika) *</label>
                      <input
                        type="number"
                        value={formData.durationMinutes}
                        onChange={(e) => setFormData({...formData, durationMinutes: Number(e.target.value)})}
                        required
                        min="1"
                        max="1440"
                        className="form-input"
                      />
                    </div>
                  </div>

                  <div className="form-group">
                    <label>Açıklama</label>
                    <textarea
                      value={formData.description}
                      onChange={(e) => setFormData({...formData, description: e.target.value})}
                      className="form-textarea"
                      rows={3}
                    />
                  </div>

                  <div className="form-group">
                    <label>Notlar</label>
                    <textarea
                      value={formData.notes}
                      onChange={(e) => setFormData({...formData, notes: e.target.value})}
                      className="form-textarea"
                      rows={2}
                    />
                  </div>

                  <div className="form-group">
                    <label>Görsel URL</label>
                    <input
                      type="url"
                      value={formData.imageUrl}
                      onChange={(e) => setFormData({...formData, imageUrl: e.target.value})}
                      className="form-input"
                    />
                  </div>

                  {editingService && (
                    <div className="form-group">
                      <label className="checkbox-label">
                        <input
                          type="checkbox"
                          checked={formData.isActive}
                          onChange={(e) => setFormData({...formData, isActive: e.target.checked})}
                        />
                        Aktif
                      </label>
                    </div>
                  )}

                  <div className="form-actions">
                    <button type="button" className="btn btn-secondary" onClick={handleCancelForm}>
                      İptal
                    </button>
                    <button type="submit" className="btn btn-primary">
                      {editingService ? 'Güncelle' : 'Kaydet'}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          )}

          <div className="services-list">
            {filteredServices.length === 0 ? (
              <div className="empty-state">
                <h3>Hizmet bulunamadı</h3>
                <p>Seçilen filtrelere uygun hizmet bulunmuyor</p>
              </div>
            ) : (
              <div className="services-grid">
                {filteredServices.map(service => (
                  <div key={service.id} className={`service-card ${!service.isActive ? 'inactive' : ''}`}>
                    <div className="service-header">
                      <h3>{service.name}</h3>
                      <span className={`status-badge ${service.isActive ? 'active' : 'inactive'}`}>
                        {service.statusDisplay}
                      </span>
                    </div>

                    <div className="service-info">
                      {service.category && (
                        <div className="service-category">
                          📂 {service.category}
                        </div>
                      )}
                      
                      <div className="service-details">
                        <span className="price">💰 {service.priceDisplay}</span>
                        <span className="duration">⏱️ {service.durationDisplay}</span>
                      </div>

                      <div className="service-date">
                        📅 {new Date(service.createdAt).toLocaleDateString('tr-TR')}
                      </div>
                    </div>

                    <div className="service-actions">
                      <button 
                        className="btn btn-sm btn-secondary"
                        onClick={() => handleEdit(service.id)}
                      >
                        ✏️ Düzenle
                      </button>
                      
                      {service.isActive ? (
                        <button 
                          className="btn btn-sm btn-danger"
                          onClick={() => handleDelete(service.id)}
                        >
                          🗑️ Sil
                        </button>
                      ) : (
                        <button 
                          className="btn btn-sm btn-success"
                          onClick={() => handleRestore(service.id)}
                        >
                          ↩️ Geri Al
                        </button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </>
      ) : (
        <CategoryManagement />
      )}
    </div>
  );
};

export default ServicesPage;