// Mock appointment service - gerçek API gelince replace edilecek
export interface Appointment {
  id: string;
  customerName: string;
  customerPhone: string;
  customerEmail: string;
  serviceName: string;
  servicePrice: number;
  staffName: string;
  appointmentDate: string;
  appointmentTime: string;
  status: 'scheduled' | 'completed' | 'cancelled' | 'in-progress';
  duration: number; // dakika
  notes?: string;
}

export interface Service {
  id: string;
  name: string;
  description: string;
  price: number;
  duration: number; // dakika
  category: string;
  isActive: boolean;
}

export interface Staff {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  specialties: string[];
  isActive: boolean;
  avatar?: string;
}

export interface Customer {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  lastVisit?: string;
  totalAppointments: number;
  totalSpent: number;
}

export interface DashboardStats {
  todayAppointments: number;
  totalCustomers: number;
  monthlyRevenue: number;
  averageRating: number;
  appointmentGrowth: number;
  customerGrowth: number;
  revenueGrowth: number;
  ratingGrowth: number;
}

// Mock data
const mockAppointments: Appointment[] = [
  {
    id: '1',
    customerName: 'Ahmet Yılmaz',
    customerPhone: '0532 123 45 67',
    customerEmail: 'ahmet@example.com',
    serviceName: 'Saç Kesimi',
    servicePrice: 150,
    staffName: 'Mehmet Usta',
    appointmentDate: '2025-10-12',
    appointmentTime: '10:00',
    status: 'scheduled',
    duration: 45,
    notes: 'Kısa saç kesimi istiyor'
  },
  {
    id: '2',
    customerName: 'Ayşe Demir',
    customerPhone: '0533 987 65 43',
    customerEmail: 'ayse@example.com',
    serviceName: 'Saç Boyama',
    servicePrice: 300,
    staffName: 'Fatma Hanım',
    appointmentDate: '2025-10-12',
    appointmentTime: '14:30',
    status: 'in-progress',
    duration: 120
  },
  {
    id: '3',
    customerName: 'Can Özkan',
    customerPhone: '0534 456 78 90',
    customerEmail: 'can@example.com',
    serviceName: 'Sakal Traşı',
    servicePrice: 75,
    staffName: 'Ali Usta',
    appointmentDate: '2025-10-12',
    appointmentTime: '16:00',
    status: 'scheduled',
    duration: 30
  },
  {
    id: '4',
    customerName: 'Zeynep Kaya',
    customerPhone: '0535 789 01 23',
    customerEmail: 'zeynep@example.com',
    serviceName: 'Makyaj',
    servicePrice: 200,
    staffName: 'Elif Hanım',
    appointmentDate: '2025-10-13',
    appointmentTime: '11:00',
    status: 'scheduled',
    duration: 60
  }
];

const mockServices: Service[] = [
  {
    id: '1',
    name: 'Saç Kesimi',
    description: 'Profesyonel erkek saç kesimi',
    price: 150,
    duration: 45,
    category: 'Saç',
    isActive: true
  },
  {
    id: '2',
    name: 'Saç Boyama',
    description: 'Doğal saç boyama hizmeti',
    price: 300,
    duration: 120,
    category: 'Saç',
    isActive: true
  },
  {
    id: '3',
    name: 'Sakal Traşı',
    description: 'Geleneksel sakal traşı',
    price: 75,
    duration: 30,
    category: 'Sakal',
    isActive: true
  },
  {
    id: '4',
    name: 'Makyaj',
    description: 'Özel gün makyajı',
    price: 200,
    duration: 60,
    category: 'Makyaj',
    isActive: true
  },
  {
    id: '5',
    name: 'Kaş Alma',
    description: 'Profesyonel kaş alma',
    price: 50,
    duration: 20,
    category: 'Kaş',
    isActive: true
  }
];

const mockStaff: Staff[] = [
  {
    id: '1',
    firstName: 'Mehmet',
    lastName: 'Usta',
    email: 'mehmet@salon.com',
    phone: '0532 111 22 33',
    specialties: ['Saç Kesimi', 'Sakal Traşı'],
    isActive: true
  },
  {
    id: '2',
    firstName: 'Fatma',
    lastName: 'Hanım',
    email: 'fatma@salon.com',
    phone: '0533 444 55 66',
    specialties: ['Saç Boyama', 'Saç Kesimi'],
    isActive: true
  },
  {
    id: '3',
    firstName: 'Ali',
    lastName: 'Usta',
    email: 'ali@salon.com',
    phone: '0534 777 88 99',
    specialties: ['Sakal Traşı', 'Saç Kesimi'],
    isActive: true
  },
  {
    id: '4',
    firstName: 'Elif',
    lastName: 'Hanım',
    email: 'elif@salon.com',
    phone: '0535 222 33 44',
    specialties: ['Makyaj', 'Kaş Alma'],
    isActive: true
  }
];

const mockCustomers: Customer[] = [
  {
    id: '1',
    firstName: 'Ahmet',
    lastName: 'Yılmaz',
    email: 'ahmet@example.com',
    phone: '0532 123 45 67',
    lastVisit: '2025-10-10',
    totalAppointments: 5,
    totalSpent: 750
  },
  {
    id: '2',
    firstName: 'Ayşe',
    lastName: 'Demir',
    email: 'ayse@example.com',
    phone: '0533 987 65 43',
    lastVisit: '2025-10-08',
    totalAppointments: 12,
    totalSpent: 2400
  },
  {
    id: '3',
    firstName: 'Can',
    lastName: 'Özkan',
    email: 'can@example.com',
    phone: '0534 456 78 90',
    lastVisit: '2025-10-05',
    totalAppointments: 3,
    totalSpent: 225
  },
  {
    id: '4',
    firstName: 'Zeynep',
    lastName: 'Kaya',
    email: 'zeynep@example.com',
    phone: '0535 789 01 23',
    lastVisit: '2025-09-28',
    totalAppointments: 8,
    totalSpent: 1600
  }
];

export const appointmentService = {
  // Appointments
  getAllAppointments: async (): Promise<Appointment[]> => {
    return new Promise((resolve) => {
      setTimeout(() => resolve(mockAppointments), 300);
    });
  },

  getTodayAppointments: async (): Promise<Appointment[]> => {
    return new Promise((resolve) => {
      const today = new Date().toISOString().split('T')[0];
      const todayAppointments = mockAppointments.filter(
        apt => apt.appointmentDate === today
      );
      setTimeout(() => resolve(todayAppointments), 300);
    });
  },

  createAppointment: async (appointment: Omit<Appointment, 'id'>): Promise<Appointment> => {
    return new Promise((resolve) => {
      const newAppointment = {
        ...appointment,
        id: Math.random().toString(36).substr(2, 9)
      };
      mockAppointments.push(newAppointment);
      setTimeout(() => resolve(newAppointment), 300);
    });
  },

  updateAppointmentStatus: async (id: string, status: Appointment['status']): Promise<void> => {
    return new Promise((resolve) => {
      const appointment = mockAppointments.find(apt => apt.id === id);
      if (appointment) {
        appointment.status = status;
      }
      setTimeout(() => resolve(), 300);
    });
  },

  // Services
  getAllServices: async (): Promise<Service[]> => {
    return new Promise((resolve) => {
      setTimeout(() => resolve(mockServices), 300);
    });
  },

  createService: async (service: Omit<Service, 'id'>): Promise<Service> => {
    return new Promise((resolve) => {
      const newService = {
        ...service,
        id: Math.random().toString(36).substr(2, 9)
      };
      mockServices.push(newService);
      setTimeout(() => resolve(newService), 300);
    });
  },

  // Staff
  getAllStaff: async (): Promise<Staff[]> => {
    return new Promise((resolve) => {
      setTimeout(() => resolve(mockStaff), 300);
    });
  },

  createStaff: async (staff: Omit<Staff, 'id'>): Promise<Staff> => {
    return new Promise((resolve) => {
      const newStaff = {
        ...staff,
        id: Math.random().toString(36).substr(2, 9)
      };
      mockStaff.push(newStaff);
      setTimeout(() => resolve(newStaff), 300);
    });
  },

  // Customers
  getAllCustomers: async (): Promise<Customer[]> => {
    return new Promise((resolve) => {
      setTimeout(() => resolve(mockCustomers), 300);
    });
  },

  // Dashboard Stats
  getDashboardStats: async (): Promise<DashboardStats> => {
    return new Promise((resolve) => {
      const today = new Date().toISOString().split('T')[0];
      const todayAppointments = mockAppointments.filter(
        apt => apt.appointmentDate === today
      ).length;

      const thisMonthRevenue = mockAppointments
        .filter(apt => apt.status === 'completed')
        .reduce((total, apt) => total + apt.servicePrice, 0);

      setTimeout(() => resolve({
        todayAppointments: todayAppointments,
        totalCustomers: mockCustomers.length,
        monthlyRevenue: thisMonthRevenue + 2450, // Mock monthly revenue
        averageRating: 4.8,
        appointmentGrowth: 15,
        customerGrowth: 8,
        revenueGrowth: -3,
        ratingGrowth: 0.2
      }), 300);
    });
  },

  // Daily Appointments
  getDailyAppointments: async (date: string): Promise<Appointment[]> => {
    return new Promise((resolve) => {
      const dailyAppointments = mockAppointments.filter(
        apt => apt.appointmentDate === date
      ).sort((a, b) => a.appointmentTime.localeCompare(b.appointmentTime));
      
      setTimeout(() => resolve(dailyAppointments), 300);
    });
  },

  // Weekly Appointments
  getWeeklyAppointments: async (startDate: string): Promise<Appointment[]> => {
    return new Promise((resolve) => {
      const start = new Date(startDate);
      const end = new Date(start);
      end.setDate(start.getDate() + 6);

      const weeklyAppointments = mockAppointments.filter(apt => {
        const aptDate = new Date(apt.appointmentDate);
        return aptDate >= start && aptDate <= end;
      }).sort((a, b) => {
        if (a.appointmentDate === b.appointmentDate) {
          return a.appointmentTime.localeCompare(b.appointmentTime);
        }
        return a.appointmentDate.localeCompare(b.appointmentDate);
      });

      setTimeout(() => resolve(weeklyAppointments), 300);
    });
  },

  // Monthly Appointments
  getMonthlyAppointments: async (year: number, month: number): Promise<Appointment[]> => {
    return new Promise((resolve) => {
      const monthlyAppointments = mockAppointments.filter(apt => {
        const aptDate = new Date(apt.appointmentDate);
        return aptDate.getFullYear() === year && aptDate.getMonth() === month;
      }).sort((a, b) => {
        if (a.appointmentDate === b.appointmentDate) {
          return a.appointmentTime.localeCompare(b.appointmentTime);
        }
        return a.appointmentDate.localeCompare(b.appointmentDate);
      });

      setTimeout(() => resolve(monthlyAppointments), 300);
    });
  },

  // Get appointments by date range
  getAppointmentsByDateRange: async (startDate: string, endDate: string): Promise<Appointment[]> => {
    return new Promise((resolve) => {
      const start = new Date(startDate);
      const end = new Date(endDate);

      const rangeAppointments = mockAppointments.filter(apt => {
        const aptDate = new Date(apt.appointmentDate);
        return aptDate >= start && aptDate <= end;
      }).sort((a, b) => {
        if (a.appointmentDate === b.appointmentDate) {
          return a.appointmentTime.localeCompare(b.appointmentTime);
        }
        return a.appointmentDate.localeCompare(b.appointmentDate);
      });

      setTimeout(() => resolve(rangeAppointments), 300);
    });
  }
};