import { useState, useEffect } from 'react';
import type { 
  ServiceCategoryListItem, 
  CreateCategoryRequest, 
  UpdateCategoryRequest 
} from '../services/serviceTypes';
import { categoryService } from '../services/serviceService';

interface CategoryFormData {
  name: string;
  description: string;
  displayOrder: number;
  isActive: boolean;
}

export function CategoryManagement() {
  const [categories, setCategories] = useState<ServiceCategoryListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [formData, setFormData] = useState<CategoryFormData>({
    name: '',
    description: '',
    displayOrder: 0,
    isActive: true
  });

  useEffect(() => {
    loadCategories();
  }, []);

  const loadCategories = async () => {
    try {
      setLoading(true);
      const data = await categoryService.getAllCategories();
      setCategories(data);
    } catch (error) {
      console.error('Kategoriler yüklenirken hata:', error);
      alert('Kategoriler yüklenemedi');
    } finally {
      setLoading(false);
    }
  };

  const handleOpenModal = (category?: ServiceCategoryListItem) => {
    if (category) {
      setEditingId(category.id);
      setFormData({
        name: category.name,
        description: category.description || '',
        displayOrder: category.displayOrder,
        isActive: category.isActive
      });
    } else {
      setEditingId(null);
      setFormData({
        name: '',
        description: '',
        displayOrder: categories.length + 1,
        isActive: true
      });
    }
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingId(null);
    setFormData({
      name: '',
      description: '',
      displayOrder: 0,
      isActive: true
    });
  };

  const handleFormSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.name.trim()) {
      alert('Kategori adı zorunludur');
      return;
    }

    try {
      if (editingId) {
        const updateData: UpdateCategoryRequest = {
          name: formData.name,
          description: formData.description || undefined,
          displayOrder: formData.displayOrder,
          isActive: formData.isActive
        };
        await categoryService.updateCategory(editingId, updateData);
      } else {
        const createData: CreateCategoryRequest = {
          name: formData.name,
          description: formData.description || undefined,
          displayOrder: formData.displayOrder
        };
        await categoryService.createCategory(createData);
      }

      await loadCategories();
      handleCloseModal();
    } catch (error) {
      console.error('Kategori kaydedilirken hata:', error);
      alert('İşlem başarısız oldu');
    }
  };

  const handleDelete = async (id: string) => {
    const category = categories.find(c => c.id === id);
    if (!category) return;

    if (category.serviceCount > 0) {
      alert('Bu kategoriye ait hizmetler bulunmaktadır. Önce hizmetleri silmelisiniz.');
      return;
    }

    if (!confirm(`"${category.name}" kategorisini silmek istediğinizden emin misiniz?`)) {
      return;
    }

    try {
      await categoryService.deleteCategory(id);
      await loadCategories();
    } catch (error: any) {
      console.error('Kategori silinirken hata:', error);
      alert(error.message || 'Kategori silinemedi');
    }
  };

  const handleRestore = async (id: string) => {
    try {
      await categoryService.restoreCategory(id);
      await loadCategories();
    } catch (error) {
      console.error('Kategori geri yüklenirken hata:', error);
      alert('Kategori geri yüklenemedi');
    }
  };

  const handleMoveUp = async (category: ServiceCategoryListItem) => {
    const currentIndex = categories.findIndex(c => c.id === category.id);
    if (currentIndex <= 0) return;

    const previousCategory = categories[currentIndex - 1];
    const orderMap = {
      [category.id]: previousCategory.displayOrder,
      [previousCategory.id]: category.displayOrder
    };

    try {
      await categoryService.reorderCategories(orderMap);
      await loadCategories();
    } catch (error) {
      console.error('Sıralama güncellenirken hata:', error);
      alert('Sıralama güncellenemedi');
    }
  };

  const handleMoveDown = async (category: ServiceCategoryListItem) => {
    const currentIndex = categories.findIndex(c => c.id === category.id);
    if (currentIndex >= categories.length - 1) return;

    const nextCategory = categories[currentIndex + 1];
    const orderMap = {
      [category.id]: nextCategory.displayOrder,
      [nextCategory.id]: category.displayOrder
    };

    try {
      await categoryService.reorderCategories(orderMap);
      await loadCategories();
    } catch (error) {
      console.error('Sıralama güncellenirken hata:', error);
      alert('Sıralama güncellenemedi');
    }
  };

  if (loading) {
    return <div className="loading-state">Kategoriler yükleniyor...</div>;
  }

  return (
    <div className="category-management">
      <div className="category-header">
        <h2>Kategori Yönetimi</h2>
        <button className="btn-primary" onClick={() => handleOpenModal()}>
          + Yeni Kategori
        </button>
      </div>

      <div className="category-list">
        {categories.length === 0 ? (
          <div className="empty-state">
            <p>Henüz kategori eklenmemiş</p>
            <button className="btn-primary" onClick={() => handleOpenModal()}>
              İlk Kategoriyi Ekle
            </button>
          </div>
        ) : (
          <div className="category-cards">
            {categories.map((category, index) => (
              <div 
                key={category.id} 
                className={`category-card ${!category.isActive ? 'inactive' : ''}`}
              >
                <div className="category-card-header">
                  <div className="category-order">
                    <button
                      className="btn-icon"
                      onClick={() => handleMoveUp(category)}
                      disabled={index === 0 || !category.isActive}
                      title="Yukarı Taşı"
                    >
                      ↑
                    </button>
                    <span className="order-number">#{category.displayOrder}</span>
                    <button
                      className="btn-icon"
                      onClick={() => handleMoveDown(category)}
                      disabled={index === categories.length - 1 || !category.isActive}
                      title="Aşağı Taşı"
                    >
                      ↓
                    </button>
                  </div>
                  <span className={`status-badge ${category.isActive ? 'active' : 'inactive'}`}>
                    {category.statusDisplay}
                  </span>
                </div>

                <div className="category-card-content">
                  <h3>{category.name}</h3>
                  {category.description && (
                    <p className="category-description">{category.description}</p>
                  )}
                  <div className="category-stats">
                    <span className="service-count">
                      {category.serviceCount} Hizmet
                    </span>
                  </div>
                </div>

                <div className="category-card-actions">
                  {category.isActive ? (
                    <>
                      <button
                        className="btn-secondary"
                        onClick={() => handleOpenModal(category)}
                      >
                        Düzenle
                      </button>
                      <button
                        className="btn-danger"
                        onClick={() => handleDelete(category.id)}
                        disabled={category.serviceCount > 0}
                        title={category.serviceCount > 0 ? 'Kategoriye ait hizmetler var' : 'Sil'}
                      >
                        Sil
                      </button>
                    </>
                  ) : (
                    <button
                      className="btn-success"
                      onClick={() => handleRestore(category.id)}
                    >
                      Geri Yükle
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {showModal && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h3>{editingId ? 'Kategori Düzenle' : 'Yeni Kategori'}</h3>
              <button className="modal-close" onClick={handleCloseModal}>
                ×
              </button>
            </div>

            <form onSubmit={handleFormSubmit} className="modal-form">
              <div className="form-group">
                <label htmlFor="name">
                  Kategori Adı <span className="required">*</span>
                </label>
                <input
                  id="name"
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  placeholder="Örn: Saç Bakımı"
                  required
                  maxLength={100}
                />
              </div>

              <div className="form-group">
                <label htmlFor="description">Açıklama</label>
                <textarea
                  id="description"
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Kategori açıklaması..."
                  rows={3}
                  maxLength={500}
                />
              </div>

              <div className="form-group">
                <label htmlFor="displayOrder">Sıra No</label>
                <input
                  id="displayOrder"
                  type="number"
                  value={formData.displayOrder}
                  onChange={(e) => setFormData({ ...formData, displayOrder: parseInt(e.target.value) || 0 })}
                  min={0}
                  max={9999}
                />
              </div>

              {editingId && (
                <div className="form-group">
                  <label className="checkbox-label">
                    <input
                      type="checkbox"
                      checked={formData.isActive}
                      onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                    />
                    <span>Aktif</span>
                  </label>
                </div>
              )}

              <div className="modal-actions">
                <button type="button" className="btn-secondary" onClick={handleCloseModal}>
                  İptal
                </button>
                <button type="submit" className="btn-primary">
                  {editingId ? 'Güncelle' : 'Ekle'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
