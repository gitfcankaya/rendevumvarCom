# RendevumVar - SaaS Salon Appointment System

A comprehensive, multi-tenant SaaS platform designed for salon, barbershop, and beauty business appointment management. Built with modern technologies to provide a scalable, secure, and user-friendly solution.

## 🚀 Features

- **Multi-tenant Architecture**: Support for multiple salons with complete data isolation
- **User Management**: Customer, Staff, Business Owner, and Admin roles
- **Salon Management**: Comprehensive salon profile management with multiple locations
- **Service Catalog**: Flexible service management with categories and pricing
- **Staff Scheduling**: Advanced staff management with working hours and availability
- **Appointment Booking**: Real-time availability checking and booking system
- **Reviews and Ratings**: Customer feedback system with business responses
- **Payment Integration**: Support for online payments (Stripe/Iyzico for Turkey)
- **Notifications**: Email and SMS notifications for bookings and reminders
- **Analytics**: Business intelligence dashboard with reports and metrics
- **Mobile Responsive**: Fully responsive design for all devices
- **Localization**: Support for Turkish and English languages

## 🏗️ Tech Stack

### Backend
- **.NET Core 9.0**: Modern, cross-platform framework
- **Entity Framework Core**: ORM for database operations
- **Microsoft SQL Server**: Relational database
- **JWT Authentication**: Secure token-based authentication
- **AutoMapper**: Object-object mapping
- **FluentValidation**: Input validation
- **Swagger/OpenAPI**: API documentation

### Frontend
- **React 18**: Modern JavaScript library
- **TypeScript**: Type-safe development
- **Vite**: Fast build tool
- **Redux Toolkit**: State management
- **Material-UI**: UI component library
- **React Router**: Client-side routing
- **Axios**: HTTP client
- **React Hook Form**: Form management
- **date-fns**: Date utilities

## 📁 Project Structure

```
rendevumvarCom/
├── docs/                           # Documentation
│   ├── PRD.md                     # Product Requirements Document
│   ├── SRS.md                     # Software Requirements Specification
│   ├── SDD.md                     # Software Design Document
│   └── TODO.md                    # Implementation checklist
├── src/                           # Backend source code
│   ├── RendevumVar.API/           # Web API layer
│   ├── RendevumVar.Core/          # Domain entities and interfaces
│   │   ├── Entities/              # Domain models
│   │   ├── Enums/                 # Enumerations
│   │   └── Interfaces/            # Interfaces
│   ├── RendevumVar.Application/   # Business logic layer
│   └── RendevumVar.Infrastructure/# Data access layer
│       └── Data/                  # DbContext and configurations
├── frontend/                      # React frontend application
│   ├── src/
│   │   ├── components/            # Reusable components
│   │   ├── pages/                 # Page components
│   │   ├── services/              # API services
│   │   ├── store/                 # Redux store
│   │   ├── hooks/                 # Custom hooks
│   │   ├── utils/                 # Utility functions
│   │   └── types/                 # TypeScript types
│   └── public/                    # Static assets
└── tests/                         # Test projects (to be added)
```

## 🎯 Architecture

The project follows **Clean Architecture** principles with clear separation of concerns:

1. **API Layer**: HTTP endpoints, controllers, middleware
2. **Application Layer**: Business logic, services, validators
3. **Domain Layer**: Core entities, interfaces, business rules
4. **Infrastructure Layer**: Data access, external services, repositories

## 🗄️ Database Schema

### Core Entities
- **Tenant**: Multi-tenant isolation
- **User**: Authentication and user management
- **RefreshToken**: JWT refresh token storage
- **Salon**: Business profile and location
- **SalonImage**: Salon photos
- **ServiceCategory**: Service categorization
- **Service**: Service catalog
- **Staff**: Staff members and schedules
- **Appointment**: Booking records
- **TimeBlock**: Staff availability blocking
- **Review**: Customer feedback
- **Payment**: Transaction records

### Relationships
- Multi-tenant with row-level security (TenantId on all entities)
- Many-to-many relationship between Staff and Services
- One-to-one relationship between Appointment and Review
- Foreign key constraints with proper cascade behaviors

## 🚦 Getting Started

### Prerequisites
- .NET 9.0 SDK
- Node.js 18+ and npm
- SQL Server 2019+ (or SQL Server Express)
- Git

### Backend Setup

1. Clone the repository:
```bash
git clone https://github.com/gitfcankaya/rendevumvarCom.git
cd rendevumvarCom
```

2. Update the connection string in `src/RendevumVar.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RendevumVar;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

3. Run database migrations:
```bash
cd src/RendevumVar.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../RendevumVar.API
dotnet ef database update --startup-project ../RendevumVar.API
```

4. Run the API:
```bash
cd ../RendevumVar.API
dotnet run
```

The API will be available at `https://localhost:7000` and `http://localhost:5000`

### Frontend Setup

1. Navigate to the frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Start the development server:
```bash
npm run dev
```

The frontend will be available at `http://localhost:5173`

## 📚 API Documentation

Once the API is running, access the Swagger documentation at:
- `https://localhost:7000/swagger`

### Testing Authentication

1. **Register a new user:**
   - Open Swagger UI at `https://localhost:7000/swagger`
   - Find `POST /api/auth/register` endpoint
   - Click "Try it out"
   - Fill in the request body:
   ```json
   {
     "email": "test@example.com",
     "password": "Test123!",
     "confirmPassword": "Test123!",
     "firstName": "Test",
     "lastName": "User",
     "phone": "+905551234567",
     "role": "Customer"
   }
   ```
   - Click "Execute"
   - Copy the `accessToken` from the response

2. **Authorize in Swagger:**
   - Click the "Authorize" button (🔒) at the top of Swagger UI
   - Enter: `Bearer your-copied-access-token`
   - Click "Authorize"
   - Now you can test protected endpoints!

For detailed authentication documentation, see [docs/AUTHENTICATION.md](docs/AUTHENTICATION.md)

## ✅ Current Implementation Status

### Phase 1: Project Setup (100% Complete) ✅
- Backend solution with Clean Architecture
- Frontend scaffold with React + TypeScript + Vite
- Docker configuration
- Database schema designed

### Phase 2: Domain Entities (90% Complete) ✅
- All 13 core entities implemented
- DbContext configured with relationships
- Initial migration created
- Awaiting database setup for testing

### Phase 3: Authentication (85% Complete) ✅
- JWT token generation with refresh tokens
- User registration and login
- Password hashing with BCrypt
- Role-based authorization
- Complete AuthController with 6 endpoints
- Swagger UI with JWT authentication support

### Phase 4: Core API Endpoints (Next Priority) ⏳
- Repository pattern implementation
- Salon, Service, Staff controllers
- AutoMapper DTOs
- FluentValidation

### Phase 5: Booking System (Upcoming) 📅
- Availability calculation
- Appointment booking flow
- Calendar integration

For a complete task list, see [docs/TODO.md](docs/TODO.md)

## 🧪 Testing

### Backend Tests
```bash
cd src/RendevumVar.Tests
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm test
```

## 📋 Development Roadmap

See [TODO.md](docs/TODO.md) for the detailed implementation plan.

### Phase 1 - MVP (Months 1-3)
- ✅ Project setup and documentation
- ✅ Backend infrastructure
- ✅ Domain entities and DbContext
- 🔄 Authentication and authorization
- 🔄 Core API endpoints
- 🔄 Frontend setup and routing
- ⏳ Booking flow implementation

### Phase 2 - Core Features (Months 4-6)
- ⏳ Notifications (Email/SMS)
- ⏳ Payment integration
- ⏳ Reviews and ratings
- ⏳ Staff management
- ⏳ Advanced calendar features

### Phase 3 - Advanced Features (Months 7-9)
- ⏳ Analytics and reporting
- ⏳ Marketing tools
- ⏳ Multi-location support
- ⏳ Mobile optimization
- ⏳ API for integrations

## 🔐 Security

- JWT-based authentication with refresh tokens
- Role-based access control (RBAC)
- Password hashing with bcrypt
- HTTPS/TLS encryption
- SQL injection prevention via parameterized queries
- XSS protection
- CSRF protection
- Rate limiting
- Multi-tenant data isolation

## 🌍 Localization

The application supports:
- Turkish (Türkçe) - Primary language
- English - Secondary language

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Team

- Product Owner: TBD
- Technical Lead: TBD
- Backend Developer: TBD
- Frontend Developer: TBD
- DevOps Engineer: TBD

## 📞 Support

For support, email support@rendevumvar.com or open an issue in the GitHub repository.

## 🙏 Acknowledgments

This project was inspired by leading salon booking platforms:
- salonrandevu.app
- sfera.ai
- hizliappy.com
- salonrandevu.com
- kolayrandevu.com
- enrandevu.com
- salonappy.com
- kuaforumyanimda.com
- notet.net

## 📊 Project Status

**Status**: In Development

**Current Version**: 0.1.0 (Alpha)

**Last Updated**: October 2024
