import type { 
  Service, 
  ServiceListItem, 
  CreateServiceRequest, 
  UpdateServiceRequest,
  ServiceCategory,
  ServiceCategoryListItem,
  CreateCategoryRequest,
  UpdateCategoryRequest
} from './serviceTypes';

// Mock data for services
const mockServices: ServiceListItem[] = [
  {
    id: '1',
    name: 'Saç Kesimi',
    category: 'Saç Bakımı',
    price: 150,
    durationMinutes: 45,
    isActive: true,
    createdAt: '2024-01-15T10:00:00Z',
    priceDisplay: '150 ₺',
    durationDisplay: '45 dk',
    statusDisplay: 'Aktif'
  },
  {
    id: '2', 
    name: 'Saç Boyama',
    category: 'Saç Bakımı',
    price: 350,
    durationMinutes: 120,
    isActive: true,
    createdAt: '2024-01-10T14:30:00Z',
    priceDisplay: '350 ₺',
    durationDisplay: '120 dk',
    statusDisplay: 'Aktif'
  },
  {
    id: '3',
    name: 'Manikür',
    category: 'Tırnak Bakımı',
    price: 100,
    durationMinutes: 60,
    isActive: true,
    createdAt: '2024-01-05T09:15:00Z',
    priceDisplay: '100 ₺',
    durationDisplay: '60 dk',
    statusDisplay: 'Aktif'
  },
  {
    id: '4',
    name: 'Pedikür',
    category: 'Tırnak Bakımı',
    price: 120,
    durationMinutes: 75,
    isActive: false,
    createdAt: '2024-01-08T16:45:00Z',
    priceDisplay: '120 ₺',
    durationDisplay: '75 dk',
    statusDisplay: 'Pasif'
  },
  {
    id: '5',
    name: 'Cilt Bakımı',
    category: 'Estetik',
    price: 250,
    durationMinutes: 90,
    isActive: true,
    createdAt: '2024-01-12T11:20:00Z',
    priceDisplay: '250 ₺',
    durationDisplay: '90 dk',
    statusDisplay: 'Aktif'
  }
];

const mockService: Service = {
  id: '1',
  name: 'Saç Kesimi',
  description: 'Profesyonel saç kesimi ve şekillendirme hizmeti',
  price: 150,
  durationMinutes: 45,
  category: 'Saç Bakımı',
  isActive: true,
  notes: 'Randevu öncesi saç yıkama dahil',
  imageUrl: '',
  createdAt: '2024-01-15T10:00:00Z',
  createdBy: 'admin@example.com',
  updatedAt: '2024-01-15T10:00:00Z',
  updatedBy: 'admin@example.com'
};

class ServiceService {
  private services: ServiceListItem[] = [...mockServices];

  async getAllServices(): Promise<ServiceListItem[]> {
    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 500));
    return [...this.services];
  }

  async getActiveServices(): Promise<ServiceListItem[]> {
    await new Promise(resolve => setTimeout(resolve, 300));
    return this.services.filter(s => s.isActive);
  }

  async getServiceById(id: string): Promise<Service | null> {
    await new Promise(resolve => setTimeout(resolve, 300));
    
    if (id === '1') {
      return { ...mockService };
    }
    
    // Convert list item to full service for other IDs
    const listItem = this.services.find(s => s.id === id);
    if (!listItem) return null;

    return {
      ...listItem,
      description: `${listItem.name} hizmeti detayları`,
      notes: 'Hizmet notları',
      imageUrl: '',
      createdBy: 'admin@example.com',
      updatedAt: listItem.createdAt,
      updatedBy: 'admin@example.com'
    };
  }

  async createService(data: CreateServiceRequest): Promise<Service> {
    await new Promise(resolve => setTimeout(resolve, 800));
    
    const newId = (this.services.length + 1).toString();
    const now = new Date().toISOString();
    
    const newService: ServiceListItem = {
      id: newId,
      name: data.name,
      category: data.category,
      price: data.price,
      durationMinutes: data.durationMinutes,
      isActive: true,
      createdAt: now,
      priceDisplay: `${data.price} ₺`,
      durationDisplay: `${data.durationMinutes} dk`,
      statusDisplay: 'Aktif'
    };

    this.services.push(newService);

    return {
      ...newService,
      description: data.description,
      notes: data.notes,
      imageUrl: data.imageUrl,
      createdBy: 'current-user@example.com',
      updatedAt: now,
      updatedBy: 'current-user@example.com'
    };
  }

  async updateService(id: string, data: UpdateServiceRequest): Promise<Service | null> {
    await new Promise(resolve => setTimeout(resolve, 700));
    
    const index = this.services.findIndex(s => s.id === id);
    if (index === -1) return null;

    const now = new Date().toISOString();
    
    const updatedService: ServiceListItem = {
      ...this.services[index],
      name: data.name,
      category: data.category,
      price: data.price,
      durationMinutes: data.durationMinutes,
      isActive: data.isActive,
      priceDisplay: `${data.price} ₺`,
      durationDisplay: `${data.durationMinutes} dk`,
      statusDisplay: data.isActive ? 'Aktif' : 'Pasif'
    };

    this.services[index] = updatedService;

    return {
      ...updatedService,
      description: data.description,
      notes: data.notes,
      imageUrl: data.imageUrl,
      createdBy: 'admin@example.com',
      updatedAt: now,
      updatedBy: 'current-user@example.com'
    };
  }

  async deleteService(id: string): Promise<boolean> {
    await new Promise(resolve => setTimeout(resolve, 600));
    
    const index = this.services.findIndex(s => s.id === id);
    if (index === -1) return false;

    // Soft delete - mark as inactive
    this.services[index] = {
      ...this.services[index],
      isActive: false,
      statusDisplay: 'Silindi'
    };

    return true;
  }

  async restoreService(id: string): Promise<boolean> {
    await new Promise(resolve => setTimeout(resolve, 400));
    
    const index = this.services.findIndex(s => s.id === id);
    if (index === -1) return false;

    this.services[index] = {
      ...this.services[index],
      isActive: true,
      statusDisplay: 'Aktif'
    };

    return true;
  }

  async getServicesByCategory(category: string): Promise<ServiceListItem[]> {
    await new Promise(resolve => setTimeout(resolve, 300));
    return this.services.filter(s => s.category === category && s.isActive);
  }

  async getCategories(): Promise<string[]> {
    await new Promise(resolve => setTimeout(resolve, 200));
    const categories = [...new Set(this.services.map(s => s.category).filter(Boolean))];
    return categories as string[];
  }
}

// Category Service
class CategoryService {
  private categories: ServiceCategoryListItem[] = [
    {
      id: '1',
      name: 'Saç Bakımı',
      description: 'Saç kesimi, boyama ve bakım hizmetleri',
      displayOrder: 1,
      isActive: true,
      serviceCount: 2,
      statusDisplay: 'Aktif'
    },
    {
      id: '2',
      name: 'Tırnak Bakımı',
      description: 'Manikür, pedikür ve tırnak tasarımı',
      displayOrder: 2,
      isActive: true,
      serviceCount: 2,
      statusDisplay: 'Aktif'
    },
    {
      id: '3',
      name: 'Cilt Bakımı',
      description: 'Cilt temizliği ve bakım işlemleri',
      displayOrder: 3,
      isActive: true,
      serviceCount: 1,
      statusDisplay: 'Aktif'
    }
  ];

  async getAllCategories(): Promise<ServiceCategoryListItem[]> {
    await new Promise(resolve => setTimeout(resolve, 300));
    return [...this.categories].sort((a, b) => a.displayOrder - b.displayOrder);
  }

  async getActiveCategories(): Promise<ServiceCategoryListItem[]> {
    await new Promise(resolve => setTimeout(resolve, 300));
    return this.categories
      .filter(c => c.isActive)
      .sort((a, b) => a.displayOrder - b.displayOrder);
  }

  async getCategoryById(id: string): Promise<ServiceCategory | null> {
    await new Promise(resolve => setTimeout(resolve, 250));
    const category = this.categories.find(c => c.id === id);
    if (!category) return null;

    return {
      ...category,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };
  }

  async createCategory(data: CreateCategoryRequest): Promise<ServiceCategory> {
    await new Promise(resolve => setTimeout(resolve, 500));

    const newCategory: ServiceCategoryListItem = {
      id: Math.random().toString(36).substr(2, 9),
      name: data.name,
      description: data.description,
      displayOrder: data.displayOrder ?? this.categories.length + 1,
      isActive: true,
      serviceCount: 0,
      statusDisplay: 'Aktif'
    };

    this.categories.push(newCategory);

    return {
      ...newCategory,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };
  }

  async updateCategory(id: string, data: UpdateCategoryRequest): Promise<ServiceCategory | null> {
    await new Promise(resolve => setTimeout(resolve, 500));

    const index = this.categories.findIndex(c => c.id === id);
    if (index === -1) return null;

    this.categories[index] = {
      ...this.categories[index],
      name: data.name,
      description: data.description,
      displayOrder: data.displayOrder ?? this.categories[index].displayOrder,
      isActive: data.isActive,
      statusDisplay: data.isActive ? 'Aktif' : 'Pasif'
    };

    return {
      ...this.categories[index],
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    };
  }

  async deleteCategory(id: string): Promise<boolean> {
    await new Promise(resolve => setTimeout(resolve, 400));

    const category = this.categories.find(c => c.id === id);
    if (!category) return false;

    // Eğer kategoriye ait hizmet varsa silinemez
    if (category.serviceCount > 0) {
      throw new Error('Bu kategoriye ait hizmetler bulunmaktadır. Önce hizmetleri silmelisiniz.');
    }

    // Soft delete
    const index = this.categories.findIndex(c => c.id === id);
    this.categories[index] = {
      ...this.categories[index],
      isActive: false,
      statusDisplay: 'Silindi'
    };

    return true;
  }

  async restoreCategory(id: string): Promise<boolean> {
    await new Promise(resolve => setTimeout(resolve, 400));

    const index = this.categories.findIndex(c => c.id === id);
    if (index === -1) return false;

    this.categories[index] = {
      ...this.categories[index],
      isActive: true,
      statusDisplay: 'Aktif'
    };

    return true;
  }

  async reorderCategories(orderMap: Record<string, number>): Promise<void> {
    await new Promise(resolve => setTimeout(resolve, 500));

    Object.entries(orderMap).forEach(([id, order]) => {
      const index = this.categories.findIndex(c => c.id === id);
      if (index !== -1) {
        this.categories[index].displayOrder = order;
      }
    });
  }
}

export const serviceService = new ServiceService();
export const categoryService = new CategoryService();