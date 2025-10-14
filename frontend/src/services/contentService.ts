// Content API types
export interface ContentPage {
  id: string;
  title: string;
  slug: string;
  content: string;
  metaDescription: string;
  metaKeywords: string;
  isActive: boolean;
  sortOrder: number;
  imageUrl?: string;
  buttonText?: string;
  buttonUrl?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateContentPage {
  title: string;
  slug: string;
  content: string;
  metaDescription: string;
  metaKeywords: string;
  isActive: boolean;
  sortOrder: number;
  imageUrl?: string;
  buttonText?: string;
  buttonUrl?: string;
}

// Content API service
const API_BASE_URL = 'http://localhost:5275/api';

export const contentService = {
  async getAll(): Promise<ContentPage[]> {
    const response = await fetch(`${API_BASE_URL}/content`);
    if (!response.ok) {
      throw new Error('Failed to fetch content pages');
    }
    return response.json();
  },

  async getBySlug(slug: string): Promise<ContentPage> {
    const response = await fetch(`${API_BASE_URL}/content/${slug}`);
    if (!response.ok) {
      throw new Error(`Failed to fetch content page: ${slug}`);
    }
    return response.json();
  },

  async getById(id: string): Promise<ContentPage> {
    const response = await fetch(`${API_BASE_URL}/content/by-id/${id}`);
    if (!response.ok) {
      throw new Error(`Failed to fetch content page with id: ${id}`);
    }
    return response.json();
  },

  async create(data: CreateContentPage): Promise<ContentPage> {
    const response = await fetch(`${API_BASE_URL}/content`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      throw new Error('Failed to create content page');
    }
    return response.json();
  },

  async update(id: string, data: CreateContentPage): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/content/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });
    if (!response.ok) {
      throw new Error('Failed to update content page');
    }
  },

  async delete(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/content/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) {
      throw new Error('Failed to delete content page');
    }
  }
};