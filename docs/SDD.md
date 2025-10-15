# Software Design Document (SDD)
# RendevumVar - SaaS Salon Appointment System

## Document Information
- **Version:** 1.0
- **Date:** 2024
- **Project:** RendevumVar
- **Status:** Draft

## Table of Contents
1. Introduction
2. System Architecture
3. Database Design
4. API Design
5. Frontend Design
6. Security Design
7. Integration Design
8. Deployment Design

---

## 1. Introduction

### 1.1 Purpose
This Software Design Document describes the detailed technical design of the RendevumVar salon appointment system. It serves as a blueprint for developers implementing the system.

### 1.2 Scope
This document covers:
- System architecture and component design
- Database schema and relationships
- API endpoints and contracts
- Frontend components and state management
- Security mechanisms
- Third-party integrations
- Deployment strategy

### 1.3 Design Goals
- **Modularity:** Clear separation of concerns
- **Scalability:** Support for growth in users and tenants
- **Maintainability:** Clean code with comprehensive tests
- **Security:** Industry-standard security practices
- **Performance:** Fast response times and efficient queries
- **Usability:** Intuitive user interface

---

## 2. System Architecture

### 2.1 High-Level Architecture

```
┌─────────────┐
│   Client    │ (React SPA)
│   Browser   │
└──────┬──────┘
       │ HTTPS
       │
┌──────▼──────────────────────────────────┐
│     Load Balancer / CDN                  │
└──────┬──────────────────────────────────┘
       │
┌──────▼──────────────────────────────────┐
│     Web Application Layer                │
│   (ASP.NET Core Web API)                 │
│   ┌────────────────────────────────┐    │
│   │  Controllers                   │    │
│   │  Middleware (Auth, Tenant)     │    │
│   └────────────┬───────────────────┘    │
│                │                         │
│   ┌────────────▼───────────────────┐    │
│   │  Application Services          │    │
│   │  (Business Logic Layer)        │    │
│   └────────────┬───────────────────┘    │
│                │                         │
│   ┌────────────▼───────────────────┐    │
│   │  Domain Layer                  │    │
│   │  (Entities, Interfaces)        │    │
│   └────────────┬───────────────────┘    │
│                │                         │
│   ┌────────────▼───────────────────┐    │
│   │  Data Access Layer             │    │
│   │  (EF Core, Repositories)       │    │
│   └────────────┬───────────────────┘    │
└────────────────┼───────────────────────┘
                 │
    ┌────────────┴────────────┬─────────────────┐
    │                         │                 │
┌───▼─────┐         ┌─────────▼──────┐   ┌─────▼─────┐
│  MSSQL  │         │  Redis Cache   │   │   Blob    │
│Database │         │                │   │  Storage  │
└─────────┘         └────────────────┘   └───────────┘
    │
    │
┌───▼──────────────────────────┐
│  External Services:          │
│  - Email (SendGrid)          │
│  - SMS (Twilio)              │
│  - Payment (Stripe/Iyzico)   │
└──────────────────────────────┘
```

### 2.2 Component Architecture

#### 2.2.1 Backend Components

**API Layer (Presentation)**
- Controllers: Handle HTTP requests
- DTOs: Data Transfer Objects for API contracts
- Filters: Exception handling, validation
- Middleware: Authentication, tenant resolution, logging

**Application Layer (Business Logic)**
- Services: Business logic implementation
- Validators: FluentValidation for input validation
- Mappers: AutoMapper for object mapping
- Commands/Queries: CQRS pattern (optional)

**Domain Layer**
- Entities: Core business objects
- Value Objects: Immutable domain concepts
- Interfaces: Contracts for repositories and services
- Domain Services: Complex business rules

**Infrastructure Layer**
- DbContext: Entity Framework Core configuration
- Repositories: Data access implementation
- External Service Clients: Email, SMS, Payment
- Background Jobs: Scheduled tasks

#### 2.2.2 Frontend Components

**Pages/Views**
- Home: Landing page with search
- Salon Profile: Salon details and booking
- Booking Flow: Multi-step booking wizard
- Dashboard: User/Business dashboard
- Admin Panel: Business management

**Components**
- Layout: Header, footer, sidebar
- Forms: Reusable form components
- Calendar: Booking calendar
- Cards: Service cards, staff cards
- Modals: Dialogs and popups

**Services**
- API Service: HTTP client wrapper
- Auth Service: Authentication logic
- Storage Service: Local/session storage
- Notification Service: Toast messages

**State Management**
- Auth State: User authentication
- Booking State: Booking flow data
- UI State: Loading, modals, etc.

### 2.3 Layer Responsibilities

**Presentation Layer**
- Receive HTTP requests
- Validate request format
- Serialize/deserialize JSON
- Return HTTP responses

**Application Layer**
- Orchestrate business operations
- Validate business rules
- Coordinate between domain and infrastructure
- Transaction management

**Domain Layer**
- Define business entities
- Encapsulate business logic
- Define interfaces
- No external dependencies

**Infrastructure Layer**
- Implement data persistence
- Call external services
- Handle file storage
- Implement caching

---

## 3. Database Design

### 3.1 Entity Relationship Diagram (ERD)

```
┌─────────────┐         ┌──────────────┐
│   Tenant    │────┬────│    Salon     │
└─────────────┘    │    └──────────────┘
                   │            │
                   │    ┌───────┴──────────┐
                   │    │                  │
                   │    ▼                  ▼
                   │ ┌─────────┐    ┌──────────┐
                   │ │ Service │    │  Staff   │
                   │ └─────────┘    └──────────┘
                   │       │              │
                   │       └──────┬───────┘
                   │              │
                   │              ▼
                   │      ┌──────────────┐      ┌──────────┐
                   └──────│ Appointment  │──────│   User   │
                          └──────────────┘      └──────────┘
                                  │                    │
                                  ▼                    │
                            ┌──────────┐              │
                            │  Review  │──────────────┘
                            └──────────┘
                                  │
                                  ▼
                            ┌──────────┐
                            │ Payment  │
                            └──────────┘
```

### 3.2 Table Schemas

#### 3.2.1 Tenant Table
```sql
CREATE TABLE Tenants (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Subdomain NVARCHAR(100) UNIQUE NOT NULL,
    SubscriptionPlan NVARCHAR(50) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0
);

CREATE INDEX IX_Tenants_Subdomain ON Tenants(Subdomain);
```

#### 3.2.2 User Table
```sql
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NULL,
    Email NVARCHAR(256) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Role NVARCHAR(50) NOT NULL, -- Customer, Staff, BusinessOwner, Admin
    ProfilePictureUrl NVARCHAR(500) NULL,
    EmailConfirmed BIT DEFAULT 0,
    PhoneConfirmed BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    LastLoginAt DATETIME2 NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);

CREATE INDEX IX_Users_TenantId ON Users(TenantId);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Role ON Users(Role);
```

#### 3.2.3 RefreshToken Table
```sql
CREATE TABLE RefreshTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(500) UNIQUE NOT NULL,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    RevokedAt DATETIME2 NULL,
    IsRevoked BIT DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
```

#### 3.2.4 Salon Table
```sql
CREATE TABLE Salons (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Phone NVARCHAR(20) NOT NULL,
    Email NVARCHAR(256) NULL,
    Website NVARCHAR(200) NULL,
    Address NVARCHAR(500) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    State NVARCHAR(100) NULL,
    PostalCode NVARCHAR(20) NULL,
    Latitude DECIMAL(9,6) NULL,
    Longitude DECIMAL(9,6) NULL,
    BusinessHours NVARCHAR(MAX) NULL, -- JSON: {"Monday": {"open": "09:00", "close": "18:00"}}
    AverageRating DECIMAL(3,2) DEFAULT 0,
    ReviewCount INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);

CREATE INDEX IX_Salons_TenantId ON Salons(TenantId);
CREATE INDEX IX_Salons_City ON Salons(City);
CREATE INDEX IX_Salons_IsActive ON Salons(IsActive);
```

#### 3.2.5 SalonImage Table
```sql
CREATE TABLE SalonImages (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    SalonId UNIQUEIDENTIFIER NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    IsPrimary BIT DEFAULT 0,
    DisplayOrder INT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (SalonId) REFERENCES Salons(Id) ON DELETE CASCADE
);

CREATE INDEX IX_SalonImages_SalonId ON SalonImages(SalonId);
```

#### 3.2.6 ServiceCategory Table
```sql
CREATE TABLE ServiceCategories (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    DisplayOrder INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);

CREATE INDEX IX_ServiceCategories_TenantId ON ServiceCategories(TenantId);
```

#### 3.2.7 Service Table
```sql
CREATE TABLE Services (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    SalonId UNIQUEIDENTIFIER NOT NULL,
    CategoryId UNIQUEIDENTIFIER NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    DurationMinutes INT NOT NULL, -- Duration in minutes
    Price DECIMAL(10,2) NOT NULL,
    ImageUrl NVARCHAR(500) NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    FOREIGN KEY (SalonId) REFERENCES Salons(Id) ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES ServiceCategories(Id)
);

CREATE INDEX IX_Services_TenantId ON Services(TenantId);
CREATE INDEX IX_Services_SalonId ON Services(SalonId);
CREATE INDEX IX_Services_CategoryId ON Services(CategoryId);
```

#### 3.2.8 Staff Table
```sql
CREATE TABLE Staff (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    SalonId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Specialties NVARCHAR(MAX) NULL, -- JSON array
    Bio NVARCHAR(1000) NULL,
    WorkingHours NVARCHAR(MAX) NULL, -- JSON: {"Monday": {"start": "09:00", "end": "17:00", "breaks": []}}
    AverageRating DECIMAL(3,2) DEFAULT 0,
    ReviewCount INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    FOREIGN KEY (SalonId) REFERENCES Salons(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IX_Staff_TenantId ON Staff(TenantId);
CREATE INDEX IX_Staff_SalonId ON Staff(SalonId);
CREATE INDEX IX_Staff_UserId ON Staff(UserId);
```

#### 3.2.9 StaffService Table (Many-to-Many)
```sql
CREATE TABLE StaffServices (
    StaffId UNIQUEIDENTIFIER NOT NULL,
    ServiceId UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (StaffId, ServiceId),
    FOREIGN KEY (StaffId) REFERENCES Staff(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE NO ACTION
);
```

#### 3.2.10 Appointment Table
```sql
CREATE TABLE Appointments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    SalonId UNIQUEIDENTIFIER NOT NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    StaffId UNIQUEIDENTIFIER NOT NULL,
    ServiceId UNIQUEIDENTIFIER NOT NULL,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    Status NVARCHAR(20) NOT NULL, -- Pending, Confirmed, CheckedIn, InProgress, Completed, Cancelled, NoShow
    Notes NVARCHAR(MAX) NULL,
    CustomerNotes NVARCHAR(500) NULL,
    CancellationReason NVARCHAR(500) NULL,
    CancelledAt DATETIME2 NULL,
    TotalPrice DECIMAL(10,2) NOT NULL,
    DepositPaid DECIMAL(10,2) DEFAULT 0,
    ReminderSent BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    FOREIGN KEY (SalonId) REFERENCES Salons(Id),
    FOREIGN KEY (CustomerId) REFERENCES Users(Id),
    FOREIGN KEY (StaffId) REFERENCES Staff(Id),
    FOREIGN KEY (ServiceId) REFERENCES Services(Id)
);

CREATE INDEX IX_Appointments_TenantId ON Appointments(TenantId);
CREATE INDEX IX_Appointments_SalonId ON Appointments(SalonId);
CREATE INDEX IX_Appointments_CustomerId ON Appointments(CustomerId);
CREATE INDEX IX_Appointments_StaffId ON Appointments(StaffId);
CREATE INDEX IX_Appointments_StartTime ON Appointments(StartTime);
CREATE INDEX IX_Appointments_Status ON Appointments(Status);
```

#### 3.2.11 Review Table
```sql
CREATE TABLE Reviews (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AppointmentId UNIQUEIDENTIFIER NOT NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    SalonId UNIQUEIDENTIFIER NOT NULL,
    StaffId UNIQUEIDENTIFIER NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment NVARCHAR(1000) NULL,
    Response NVARCHAR(1000) NULL,
    ResponseBy UNIQUEIDENTIFIER NULL,
    ResponseAt DATETIME2 NULL,
    IsPublished BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id),
    FOREIGN KEY (CustomerId) REFERENCES Users(Id),
    FOREIGN KEY (SalonId) REFERENCES Salons(Id),
    FOREIGN KEY (StaffId) REFERENCES Staff(Id),
    FOREIGN KEY (ResponseBy) REFERENCES Users(Id)
);

CREATE INDEX IX_Reviews_AppointmentId ON Reviews(AppointmentId);
CREATE INDEX IX_Reviews_SalonId ON Reviews(SalonId);
CREATE INDEX IX_Reviews_StaffId ON Reviews(StaffId);
CREATE INDEX IX_Reviews_CustomerId ON Reviews(CustomerId);
```

#### 3.2.12 Payment Table
```sql
CREATE TABLE Payments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AppointmentId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Currency NVARCHAR(3) DEFAULT 'TRY',
    PaymentMethod NVARCHAR(50) NOT NULL, -- CreditCard, Cash, BankTransfer
    Status NVARCHAR(20) NOT NULL, -- Pending, Completed, Failed, Refunded
    TransactionId NVARCHAR(200) NULL,
    PaymentGateway NVARCHAR(50) NULL, -- Stripe, Iyzico
    PaymentDate DATETIME2 NULL,
    RefundDate DATETIME2 NULL,
    RefundAmount DECIMAL(10,2) NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id)
);

CREATE INDEX IX_Payments_AppointmentId ON Payments(AppointmentId);
CREATE INDEX IX_Payments_TransactionId ON Payments(TransactionId);
CREATE INDEX IX_Payments_Status ON Payments(Status);
```

#### 3.2.13 Notification Table
```sql
CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- BookingConfirmation, Reminder, Cancellation, etc.
    Channel NVARCHAR(20) NOT NULL, -- Email, SMS, InApp
    Subject NVARCHAR(200) NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Status NVARCHAR(20) NOT NULL, -- Pending, Sent, Failed
    SentAt DATETIME2 NULL,
    ReadAt DATETIME2 NULL,
    ErrorMessage NVARCHAR(500) NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
CREATE INDEX IX_Notifications_Status ON Notifications(Status);
CREATE INDEX IX_Notifications_CreatedAt ON Notifications(CreatedAt);
```

#### 3.2.14 TimeBlock Table
```sql
CREATE TABLE TimeBlocks (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    StaffId UNIQUEIDENTIFIER NOT NULL,
    StartTime DATETIME2 NOT NULL,
    EndTime DATETIME2 NOT NULL,
    Reason NVARCHAR(200) NULL,
    IsRecurring BIT DEFAULT 0,
    RecurrencePattern NVARCHAR(100) NULL, -- Weekly, Monthly, etc.
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    FOREIGN KEY (StaffId) REFERENCES Staff(Id) ON DELETE CASCADE
);

CREATE INDEX IX_TimeBlocks_StaffId ON TimeBlocks(StaffId);
CREATE INDEX IX_TimeBlocks_StartTime ON TimeBlocks(StartTime);
```

### 3.3 Indexing Strategy

**Primary Indexes:**
- Primary keys on all tables (clustered)

**Foreign Key Indexes:**
- All foreign keys have non-clustered indexes

**Query Optimization Indexes:**
- Composite index on (TenantId, SalonId, StartTime) for Appointments
- Composite index on (SalonId, IsActive) for Services
- Composite index on (StaffId, StartTime, EndTime) for availability queries
- Full-text index on Salon.Name and Salon.Description for search

### 3.4 Database Constraints

**Unique Constraints:**
- Tenants.Subdomain
- Users.Email
- RefreshTokens.Token

**Check Constraints:**
- Reviews.Rating BETWEEN 1 AND 5
- Services.DurationMinutes > 0
- Services.Price >= 0
- Appointments.EndTime > StartTime

**Default Values:**
- CreatedAt, UpdatedAt: GETUTCDATE()
- IsDeleted, IsActive: 0/1
- GUIDs: NEWID()

### 3.5 Data Retention and Archival

**Soft Delete:**
- Most tables use IsDeleted flag
- Actual deletion happens via scheduled cleanup

**Archival:**
- Appointments older than 2 years moved to archive table
- Reviews retained indefinitely
- Audit logs retained for 1 year

---

## 4. API Design

### 4.1 API Architecture

**Style:** RESTful API
**Protocol:** HTTPS only
**Format:** JSON
**Versioning:** URL-based (e.g., /api/v1/)
**Authentication:** JWT Bearer tokens

### 4.2 Authentication Endpoints

#### POST /api/auth/register
**Description:** Register a new customer account
**Request:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phone": "+905551234567",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!"
}
```
**Response (201):**
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe"
  },
  "message": "Registration successful. Please check your email to verify your account."
}
```

#### POST /api/auth/login
**Description:** Authenticate user
**Request:**
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePass123!",
  "rememberMe": true
}
```
**Response (200):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "guid",
    "expiresIn": 900,
    "user": {
      "id": "guid",
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "role": "Customer"
    }
  }
}
```

#### POST /api/auth/refresh
**Description:** Refresh access token
**Request:**
```json
{
  "refreshToken": "guid"
}
```
**Response (200):**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "guid",
    "expiresIn": 900
  }
}
```

### 4.3 Salon Endpoints

#### GET /api/salons
**Description:** Search salons (public)
**Query Parameters:**
- `search`: string (salon name or description)
- `city`: string
- `latitude`: decimal
- `longitude`: decimal
- `radius`: int (in km)
- `page`: int (default 1)
- `pageSize`: int (default 20)

**Response (200):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "name": "Salon Name",
        "description": "...",
        "address": "...",
        "city": "Istanbul",
        "phone": "+905551234567",
        "averageRating": 4.5,
        "reviewCount": 120,
        "imageUrl": "https://...",
        "distance": 2.5
      }
    ],
    "totalCount": 150,
    "page": 1,
    "pageSize": 20,
    "totalPages": 8
  }
}
```

#### GET /api/salons/{id}
**Description:** Get salon details (public)
**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "name": "Salon Name",
    "description": "...",
    "phone": "+905551234567",
    "email": "info@salon.com",
    "address": "...",
    "city": "Istanbul",
    "businessHours": {
      "Monday": {"open": "09:00", "close": "18:00"},
      "Tuesday": {"open": "09:00", "close": "18:00"}
    },
    "images": ["url1", "url2"],
    "averageRating": 4.5,
    "reviewCount": 120,
    "services": [...],
    "staff": [...]
  }
}
```

#### POST /api/salons
**Description:** Create new salon (business owner only)
**Auth:** Required (BusinessOwner role)
**Request:**
```json
{
  "name": "My Salon",
  "description": "Best salon in town",
  "phone": "+905551234567",
  "email": "info@salon.com",
  "address": "Street Address",
  "city": "Istanbul",
  "postalCode": "34000",
  "businessHours": {
    "Monday": {"open": "09:00", "close": "18:00"}
  }
}
```
**Response (201):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "name": "My Salon",
    "subdomain": "my-salon"
  }
}
```

### 4.4 Service Endpoints

#### GET /api/salons/{salonId}/services
**Description:** Get all services for a salon (public)
**Response (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Haircut",
      "description": "...",
      "durationMinutes": 30,
      "price": 150.00,
      "categoryName": "Hair Services",
      "imageUrl": "https://..."
    }
  ]
}
```

#### POST /api/services
**Description:** Create new service (business owner only)
**Auth:** Required (BusinessOwner role)
**Request:**
```json
{
  "salonId": "guid",
  "categoryId": "guid",
  "name": "Haircut",
  "description": "Professional haircut",
  "durationMinutes": 30,
  "price": 150.00
}
```
**Response (201):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "name": "Haircut",
    "price": 150.00
  }
}
```

### 4.5 Appointment Endpoints

#### GET /api/appointments/availability
**Description:** Check available time slots
**Query Parameters:**
- `salonId`: guid (required)
- `serviceId`: guid (required)
- `staffId`: guid (optional, "any" for no preference)
- `date`: date (YYYY-MM-DD, required)

**Response (200):**
```json
{
  "success": true,
  "data": {
    "date": "2024-01-15",
    "availableSlots": [
      {
        "startTime": "09:00",
        "endTime": "09:30",
        "staffId": "guid",
        "staffName": "Jane Smith"
      },
      {
        "startTime": "09:30",
        "endTime": "10:00",
        "staffId": "guid",
        "staffName": "Jane Smith"
      }
    ]
  }
}
```

#### POST /api/appointments
**Description:** Create new appointment
**Auth:** Optional (can be guest)
**Request:**
```json
{
  "salonId": "guid",
  "serviceId": "guid",
  "staffId": "guid",
  "startTime": "2024-01-15T10:00:00Z",
  "customerInfo": {
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "phone": "+905551234567"
  },
  "notes": "Please call when you arrive"
}
```
**Response (201):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "confirmationCode": "ABC123",
    "startTime": "2024-01-15T10:00:00Z",
    "endTime": "2024-01-15T10:30:00Z",
    "totalPrice": 150.00,
    "status": "Confirmed"
  },
  "message": "Appointment booked successfully. Confirmation sent to your email."
}
```

#### GET /api/appointments
**Description:** Get user's appointments
**Auth:** Required
**Query Parameters:**
- `status`: string (optional: upcoming, past, cancelled)
- `page`: int
- `pageSize`: int

**Response (200):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "salonName": "Salon Name",
        "serviceName": "Haircut",
        "staffName": "Jane Smith",
        "startTime": "2024-01-15T10:00:00Z",
        "endTime": "2024-01-15T10:30:00Z",
        "status": "Confirmed",
        "totalPrice": 150.00,
        "canCancel": true,
        "canReschedule": true
      }
    ],
    "totalCount": 25
  }
}
```

#### PUT /api/appointments/{id}/cancel
**Description:** Cancel appointment
**Auth:** Required
**Request:**
```json
{
  "reason": "Schedule conflict"
}
```
**Response (200):**
```json
{
  "success": true,
  "message": "Appointment cancelled successfully."
}
```

### 4.6 Review Endpoints

#### GET /api/salons/{salonId}/reviews
**Description:** Get salon reviews (public)
**Query Parameters:**
- `rating`: int (filter by rating)
- `page`: int
- `pageSize`: int

**Response (200):**
```json
{
  "success": true,
  "data": {
    "averageRating": 4.5,
    "totalCount": 120,
    "ratingDistribution": {
      "5": 80,
      "4": 25,
      "3": 10,
      "2": 3,
      "1": 2
    },
    "reviews": [
      {
        "id": "guid",
        "customerName": "John D.",
        "rating": 5,
        "comment": "Great service!",
        "staffName": "Jane Smith",
        "createdAt": "2024-01-10T15:30:00Z",
        "response": "Thank you!",
        "responseAt": "2024-01-11T09:00:00Z"
      }
    ]
  }
}
```

#### POST /api/reviews
**Description:** Submit review
**Auth:** Required
**Request:**
```json
{
  "appointmentId": "guid",
  "rating": 5,
  "comment": "Great service!"
}
```
**Response (201):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "rating": 5
  },
  "message": "Thank you for your review!"
}
```

### 4.7 Error Responses

**400 Bad Request:**
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "message": "Email is required"
    }
  ]
}
```

**401 Unauthorized:**
```json
{
  "success": false,
  "message": "Authentication required"
}
```

**403 Forbidden:**
```json
{
  "success": false,
  "message": "You don't have permission to access this resource"
}
```

**404 Not Found:**
```json
{
  "success": false,
  "message": "Resource not found"
}
```

**500 Internal Server Error:**
```json
{
  "success": false,
  "message": "An unexpected error occurred. Please try again later.",
  "errorId": "guid"
}
```

### 4.8 API Rate Limiting

**Limits:**
- Unauthenticated: 60 requests per minute
- Authenticated: 300 requests per minute
- Business owners: 600 requests per minute

**Headers:**
```
X-RateLimit-Limit: 300
X-RateLimit-Remaining: 250
X-RateLimit-Reset: 1640000000
```

---

## 5. Frontend Design

### 5.1 Component Structure

```
src/
├── app/
│   ├── App.tsx
│   ├── App.css
│   └── routes.tsx
├── components/
│   ├── common/
│   │   ├── Button/
│   │   ├── Input/
│   │   ├── Card/
│   │   ├── Modal/
│   │   └── Loading/
│   ├── layout/
│   │   ├── Header/
│   │   ├── Footer/
│   │   ├── Sidebar/
│   │   └── Layout/
│   ├── booking/
│   │   ├── SearchBar/
│   │   ├── SalonCard/
│   │   ├── ServiceList/
│   │   ├── StaffList/
│   │   ├── Calendar/
│   │   └── BookingWizard/
│   └── dashboard/
│       ├── DashboardCard/
│       ├── AppointmentTable/
│       ├── StaffSchedule/
│       └── RevenueChart/
├── pages/
│   ├── Home/
│   ├── SalonProfile/
│   ├── Booking/
│   ├── MyAppointments/
│   ├── Dashboard/
│   └── Auth/
│       ├── Login/
│       ├── Register/
│       └── ForgotPassword/
├── services/
│   ├── api.service.ts
│   ├── auth.service.ts
│   ├── salon.service.ts
│   ├── appointment.service.ts
│   └── storage.service.ts
├── store/
│   ├── slices/
│   │   ├── authSlice.ts
│   │   ├── bookingSlice.ts
│   │   └── uiSlice.ts
│   └── store.ts
├── hooks/
│   ├── useAuth.ts
│   ├── useBooking.ts
│   └── useToast.ts
├── utils/
│   ├── dateUtils.ts
│   ├── validators.ts
│   └── formatters.ts
└── types/
    ├── user.types.ts
    ├── salon.types.ts
    └── appointment.types.ts
```

### 5.2 State Management

**Redux Store Structure:**
```typescript
{
  auth: {
    user: User | null,
    accessToken: string | null,
    isAuthenticated: boolean,
    loading: boolean
  },
  booking: {
    selectedSalon: Salon | null,
    selectedService: Service | null,
    selectedStaff: Staff | null,
    selectedDateTime: Date | null,
    step: number
  },
  ui: {
    loading: boolean,
    notifications: Notification[],
    modals: {
      login: boolean,
      booking: boolean
    }
  }
}
```

### 5.3 Routing

**Public Routes:**
- `/` - Home page
- `/salons` - Salon search
- `/salons/:id` - Salon profile
- `/booking/:salonId` - Booking flow
- `/login` - Login page
- `/register` - Registration page

**Protected Routes (Customer):**
- `/appointments` - My appointments
- `/profile` - User profile

**Protected Routes (Business Owner):**
- `/dashboard` - Business dashboard
- `/dashboard/appointments` - Appointment management
- `/dashboard/services` - Service management
- `/dashboard/staff` - Staff management
- `/dashboard/reports` - Reports

**Protected Routes (Staff):**
- `/schedule` - Staff schedule
- `/appointments/:id` - Appointment details

### 5.4 Key Components

#### SearchBar Component
```typescript
interface SearchBarProps {
  onSearch: (query: string, location: string) => void;
  loading?: boolean;
}

// Features:
// - Location autocomplete
// - Service type filter
// - Real-time search
```

#### BookingWizard Component
```typescript
interface BookingWizardProps {
  salonId: string;
  onComplete: (booking: Booking) => void;
}

// Steps:
// 1. Select service
// 2. Select staff (optional)
// 3. Select date and time
// 4. Enter customer info
// 5. Confirmation
```

#### Calendar Component
```typescript
interface CalendarProps {
  availableSlots: TimeSlot[];
  selectedDate: Date;
  onDateSelect: (date: Date) => void;
  onTimeSelect: (time: string) => void;
}

// Features:
// - Month/week/day view
// - Available slot highlighting
// - Disabled dates for unavailability
```

### 5.5 Responsive Design

**Breakpoints:**
- Mobile: < 768px
- Tablet: 768px - 1024px
- Desktop: > 1024px

**Mobile-First Approach:**
- Start with mobile layout
- Add complexity for larger screens
- Touch-friendly UI elements
- Hamburger menu for mobile

### 5.6 Theme and Styling

**Color Palette:**
```css
:root {
  --primary-color: #2563eb; /* Blue */
  --secondary-color: #10b981; /* Green */
  --accent-color: #f59e0b; /* Amber */
  --text-primary: #1f2937;
  --text-secondary: #6b7280;
  --background: #ffffff;
  --background-secondary: #f9fafb;
  --border: #e5e7eb;
  --error: #ef4444;
  --success: #10b981;
}
```

**Typography:**
```css
--font-family: 'Inter', 'Segoe UI', sans-serif;
--font-size-xs: 0.75rem;
--font-size-sm: 0.875rem;
--font-size-base: 1rem;
--font-size-lg: 1.125rem;
--font-size-xl: 1.25rem;
--font-size-2xl: 1.5rem;
```

---

## 6. Security Design

### 6.1 Authentication Flow

```
1. User submits credentials (email + password)
2. API validates credentials
3. API generates:
   - Access Token (JWT, 15 min expiry)
   - Refresh Token (GUID, 7 days expiry, stored in DB)
4. Client stores tokens:
   - Access Token in memory
   - Refresh Token in httpOnly cookie (preferred) or localStorage
5. Client includes Access Token in Authorization header
6. When Access Token expires:
   - Client sends Refresh Token
   - API validates Refresh Token
   - API issues new Access Token and Refresh Token
7. On logout:
   - API revokes Refresh Token
   - Client clears tokens
```

### 6.2 JWT Token Structure

**Claims:**
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "role": "Customer",
  "tenantId": "tenant-id",
  "iat": 1640000000,
  "exp": 1640000900
}
```

### 6.3 Authorization Matrix

| Role | Permissions |
|------|-------------|
| Customer | - View salons<br>- Book appointments<br>- View own appointments<br>- Submit reviews<br>- Manage own profile |
| Staff | - View assigned appointments<br>- Update appointment status<br>- Manage availability<br>- View customer info<br>- Manage own profile |
| BusinessOwner | - All Staff permissions<br>- Manage salon profile<br>- Manage services<br>- Manage staff<br>- View reports<br>- Manage appointments |
| Admin | - All permissions<br>- Manage tenants<br>- System configuration |

### 6.4 Data Protection

**Encryption:**
- Passwords: bcrypt with cost factor 12
- Data in transit: TLS 1.3
- Sensitive data at rest: AES-256
- Database connections: encrypted

**PII Handling:**
- Minimal data collection
- Consent management
- Data anonymization in logs
- GDPR-compliant data export/deletion

### 6.5 Security Headers

```
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Content-Security-Policy: default-src 'self'; script-src 'self' 'unsafe-inline'
Referrer-Policy: no-referrer-when-downgrade
```

### 6.6 Input Validation

**Backend:**
- FluentValidation for all DTOs
- Whitelist allowed characters
- Maximum length enforcement
- Type validation
- Parameterized queries (EF Core)

**Frontend:**
- React Hook Form validation
- Real-time validation feedback
- Sanitize user input
- Prevent XSS via React's built-in escaping

---

## 7. Integration Design

### 7.1 Email Service Integration

**Provider:** SendGrid
**Authentication:** API Key
**Configuration:**
```csharp
public class EmailSettings
{
    public string ApiKey { get; set; }
    public string FromEmail { get; set; }
    public string FromName { get; set; }
}
```

**Templates:**
- Booking confirmation
- Booking reminder (24h, 2h)
- Cancellation confirmation
- Password reset
- Welcome email

### 7.2 SMS Service Integration

**Provider:** Twilio
**Authentication:** Account SID + Auth Token
**Configuration:**
```csharp
public class SmsSettings
{
    public string AccountSid { get; set; }
    public string AuthToken { get; set; }
    public string FromNumber { get; set; }
}
```

**Message Types:**
- Booking confirmation
- Reminder (2h before)
- Cancellation confirmation
- Verification codes

### 7.3 Payment Gateway Integration

**Provider:** Stripe (primary), Iyzico (Turkey)
**Integration Method:** Stripe Checkout / Payment Intents
**Webhook Events:**
- payment_intent.succeeded
- payment_intent.payment_failed
- charge.refunded

**Flow:**
1. Customer initiates booking
2. Backend creates payment intent
3. Frontend displays Stripe Checkout
4. Customer completes payment
5. Stripe sends webhook
6. Backend confirms appointment

### 7.4 File Storage Integration

**Provider:** Azure Blob Storage
**Containers:**
- `profile-pictures`: User and staff photos
- `salon-images`: Salon photos
- `service-images`: Service photos

**CDN:** Azure CDN for fast delivery
**Upload Process:**
1. Generate SAS token
2. Upload directly from client
3. Store URL in database

---

## 8. Deployment Design

### 8.1 Infrastructure Architecture

**Azure Deployment:**
```
Internet
    │
    ▼
Azure Application Gateway (WAF + SSL)
    │
    ▼
Azure App Service (Web App)
    ├── Auto-scaling: 2-10 instances
    ├── Always On: Enabled
    └── Health Check: /api/health
    │
    ├──▶ Azure SQL Database
    │     ├── Tier: Standard S3
    │     ├── Backups: Automated daily
    │     └── Geo-replication: Enabled
    │
    ├──▶ Azure Cache for Redis
    │     └── Tier: Standard C1
    │
    └──▶ Azure Blob Storage
          └── CDN: Enabled
```

### 8.2 CI/CD Pipeline

**GitHub Actions Workflow:**
```yaml
name: Deploy to Azure

on:
  push:
    branches: [main]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Build Backend
        run: dotnet build
      
      - name: Test Backend
        run: dotnet test
      
      - name: Setup Node
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Build Frontend
        run: |
          cd frontend
          npm install
          npm run build
      
      - name: Deploy to Azure
        uses: azure/webapps-deploy@v2
        with:
          app-name: rendevumvar-api
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
```

### 8.3 Environment Configuration

**Development:**
- Local SQL Server
- Local file storage
- Mock email/SMS services
- Debug logging

**Staging:**
- Azure SQL Database (Basic tier)
- Azure Blob Storage
- SendGrid sandbox
- Info logging

**Production:**
- Azure SQL Database (Standard tier)
- Azure Blob Storage with CDN
- SendGrid production
- Warning/Error logging
- Application Insights

### 8.4 Monitoring and Logging

**Application Insights:**
- Request tracking
- Dependency tracking
- Exception tracking
- Custom events
- Performance metrics

**Alerts:**
- High error rate
- Slow response time
- Database connection failures
- High CPU/memory usage

**Log Levels:**
- Debug: Development only
- Info: Staging and production
- Warning: Anomalies
- Error: Failures
- Critical: System down

### 8.5 Backup and Recovery

**Database Backups:**
- Automated daily backups
- Retention: 30 days
- Point-in-time restore: 7 days
- Geo-redundant backups

**File Backups:**
- Blob storage: Geo-redundant
- Versioning enabled
- Soft delete: 30 days

**Disaster Recovery Plan:**
- RTO: 4 hours
- RPO: 24 hours
- Documented recovery procedures
- Regular DR testing

---

## 9. Testing Strategy

### 9.1 Unit Testing

**Backend:**
- Framework: xUnit
- Mocking: Moq
- Coverage Target: > 70%
- Test: Services, validators, mappers

**Frontend:**
- Framework: Jest + React Testing Library
- Coverage Target: > 60%
- Test: Components, hooks, utilities

### 9.2 Integration Testing

**API Testing:**
- Framework: xUnit with WebApplicationFactory
- Test database: In-memory or Docker SQL Server
- Test: API endpoints, authentication, authorization

### 9.3 End-to-End Testing

**Framework:** Playwright or Cypress
**Scenarios:**
- Complete booking flow
- User registration and login
- Business owner dashboard
- Staff schedule management

### 9.4 Performance Testing

**Tools:** JMeter or k6
**Tests:**
- Load test: 1000 concurrent users
- Stress test: Find breaking point
- Endurance test: 24-hour sustained load

### 9.5 Security Testing

**OWASP Top 10:**
- SQL injection testing
- XSS testing
- CSRF testing
- Authentication bypass attempts
- Authorization bypass attempts

---

## 10. Advanced Features Architecture

### 10.1 Multi-Tenant Architecture Design

#### 10.1.1 Tenant Isolation Strategy

**Approach: Shared Database with Row-Level Security**

```
Benefits:
✓ Cost-effective (single database)
✓ Easy maintenance and upgrades
✓ Simplified backup/recovery
✓ Resource pooling

Challenges:
× Must ensure data isolation
× Query performance with large datasets
× Tenant-specific customization complexity
```

**Implementation Pattern:**
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

// Global Query Filter in DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply tenant filter to all entities
    modelBuilder.Entity<Salon>()
        .HasQueryFilter(s => s.TenantId == _currentTenantId);
    
    modelBuilder.Entity<Service>()
        .HasQueryFilter(s => s.TenantId == _currentTenantId);
    
    // Repeat for all tenant-scoped entities
}
```

**Tenant Resolution Middleware:**
```csharp
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        // Method 1: Subdomain-based
        var host = context.Request.Host.Host;
        var subdomain = ExtractSubdomain(host); // e.g., "salonname" from "salonname.rendevumvar.com"
        
        // Method 2: Header-based (for API clients)
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantId))
        {
            await tenantService.SetCurrentTenant(tenantId);
        }
        else
        {
            var tenant = await tenantService.GetTenantBySubdomain(subdomain);
            if (tenant != null)
            {
                await tenantService.SetCurrentTenant(tenant.Id);
            }
        }
        
        await _next(context);
    }
}
```

#### 10.1.2 Subscription Management Architecture

**Subscription State Machine:**
```
┌──────────┐
│ Trialing │
└─────┬────┘
      │ Trial expires
      │ Payment added
      ▼
┌──────────┐
│  Active  │◄──── Payment successful
└─────┬────┘
      │
      ├──► PastDue ──► Suspended ──► Cancelled
      │     (Payment failed)  (Grace period)
      │
      └──► Cancelled (User cancels)
```

**Subscription Enforcement:**
```csharp
public class SubscriptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, ISubscriptionService subscriptionService)
    {
        var tenantId = context.GetTenantId();
        var subscription = await subscriptionService.GetCurrentSubscription(tenantId);
        
        // Check subscription status
        if (subscription.Status == SubscriptionStatus.Suspended)
        {
            context.Response.StatusCode = 402; // Payment Required
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "Subscription suspended. Please update payment method." 
            });
            return;
        }
        
        // Check feature limits
        if (context.Request.Path.StartsWithSegments("/api/staff"))
        {
            var staffCount = await subscriptionService.GetStaffCount(tenantId);
            var maxStaff = subscription.Plan.MaxStaff;
            
            if (staffCount >= maxStaff && context.Request.Method == "POST")
            {
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsJsonAsync(new 
                { 
                    error = $"Your plan allows maximum {maxStaff} staff members. Please upgrade." 
                });
                return;
            }
        }
        
        await _next(context);
    }
}
```

**Subscription Database Tables:**
```sql
-- Subscription Plans
CREATE TABLE SubscriptionPlans (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500),
    MonthlyPrice DECIMAL(10,2) NOT NULL,
    AnnualPrice DECIMAL(10,2) NOT NULL,
    TrialDays INT DEFAULT 0,
    MaxStaff INT DEFAULT -1, -- -1 = unlimited
    MaxAppointmentsPerMonth INT DEFAULT -1,
    HasAdvancedAnalytics BIT DEFAULT 0,
    HasSMSNotifications BIT DEFAULT 0,
    HasCustomBranding BIT DEFAULT 0,
    HasAPIAccess BIT DEFAULT 0,
    HasMultiLocation BIT DEFAULT 0,
    HasPackageManagement BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Tenant Subscriptions
CREATE TABLE TenantSubscriptions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    SubscriptionPlanId INT NOT NULL,
    Status NVARCHAR(20) NOT NULL, -- Trialing, Active, PastDue, Suspended, Cancelled, Expired
    BillingCycle NVARCHAR(10) NOT NULL, -- Monthly, Annual
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2 NULL,
    TrialEndDate DATETIME2 NULL,
    NextBillingDate DATETIME2 NULL,
    PaymentMethodId NVARCHAR(100) NULL,
    AutoRenew BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlans(Id)
);

CREATE INDEX IX_TenantSubscriptions_TenantId ON TenantSubscriptions(TenantId);
CREATE INDEX IX_TenantSubscriptions_Status ON TenantSubscriptions(Status);

-- Invoices
CREATE TABLE Invoices (
    Id INT PRIMARY KEY IDENTITY(1,1),
    InvoiceNumber NVARCHAR(20) UNIQUE NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    InvoiceDate DATETIME2 NOT NULL,
    DueDate DATETIME2 NOT NULL,
    SubTotal DECIMAL(10,2) NOT NULL,
    TaxAmount DECIMAL(10,2) NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL, -- Draft, Sent, Paid, Overdue, Cancelled
    Currency NVARCHAR(3) DEFAULT 'TRY',
    PdfUrl NVARCHAR(500) NULL,
    PaidAt DATETIME2 NULL,
    PaymentTransactionId NVARCHAR(100) NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);

CREATE INDEX IX_Invoices_TenantId ON Invoices(TenantId);
CREATE INDEX IX_Invoices_Status ON Invoices(Status);

-- Invoice Line Items
CREATE TABLE InvoiceLineItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    InvoiceId INT NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    LineTotal DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
);
```

### 10.2 Invitation and Connection System Architecture

#### 10.2.1 Invitation Flow Diagram

```
┌─────────────────────────────────────────────────────┐
│            INVITATION GENERATION                    │
└─────────────────────────────────────────────────────┘

Business Dashboard
      │
      ├──► Generate QR Code ──► Create InvitationCode (Type=QR)
      │                         └──► Return QR Image + URL
      │
      ├──► Generate Link ──────► Create InvitationCode (Type=Link)
      │                         └──► Return Shareable URL
      │
      ├──► Send SMS ───────────► Create InvitationCode (Type=SMS)
      │                         └──► Send SMS via Twilio
      │                         └──► Track SMS delivery
      │
      └──► Generate Code ──────► Create InvitationCode (Type=Code)
                                └──► Return 6-8 char code


┌─────────────────────────────────────────────────────┐
│         CUSTOMER ACCEPTANCE FLOW                    │
└─────────────────────────────────────────────────────┘

Customer scans QR / clicks link / enters code
      │
      ├──► Validate invitation token
      │    ├──► Invalid → Error
      │    └──► Valid → Continue
      │
      ├──► Check if customer registered
      │    ├──► No → Registration Form
      │    └──► Yes → Connection Request
      │
      ├──► Create CustomerBusinessConnection
      │    └──► Status: PendingBusinessApproval
      │
      ├──► Notify Business Owner
      │    └──► In-App + Email + SMS
      │
      └──► Await Business Response
           ├──► Approved → Connection Active
           └──► Rejected → Notify Customer
```

**Invitation Database Schema:**
```sql
-- Invitation Codes
CREATE TABLE InvitationCodes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Code NVARCHAR(10) NOT NULL, -- 6-8 character code
    Token NVARCHAR(100) UNIQUE NOT NULL, -- Secure token for URL
    Type NVARCHAR(20) NOT NULL, -- QR, Link, SMS, Code
    CreatedByUserId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NULL,
    MaxUses INT NULL,
    UsedCount INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
);

CREATE INDEX IX_InvitationCodes_Token ON InvitationCodes(Token);
CREATE INDEX IX_InvitationCodes_TenantId ON InvitationCodes(TenantId);

-- Customer-Business Connections
CREATE TABLE CustomerBusinessConnections (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(30) NOT NULL, -- PendingCustomerApproval, PendingBusinessApproval, Approved, Rejected, Blocked, Disconnected
    InitiatedBy NVARCHAR(20) NOT NULL, -- Customer, Business
    RequestMessage NVARCHAR(500) NULL,
    RejectionReason NVARCHAR(500) NULL,
    RequestedAt DATETIME2 DEFAULT GETUTCDATE(),
    RespondedAt DATETIME2 NULL,
    LastInteractionAt DATETIME2 NULL,
    AppointmentCount INT DEFAULT 0,
    IsFavorite BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Users(Id),
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);

CREATE INDEX IX_Connections_CustomerId ON CustomerBusinessConnections(CustomerId);
CREATE INDEX IX_Connections_TenantId ON CustomerBusinessConnections(TenantId);
CREATE INDEX IX_Connections_Status ON CustomerBusinessConnections(Status);

-- Connection History (Audit Trail)
CREATE TABLE ConnectionHistory (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ConnectionId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- Created, Approved, Rejected, Disconnected, Blocked
    PerformedByUserId UNIQUEIDENTIFIER NOT NULL,
    Notes NVARCHAR(500) NULL,
    PerformedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (ConnectionId) REFERENCES CustomerBusinessConnections(Id)
);
```

**Invitation Service Implementation:**
```csharp
public class InvitationService : IInvitationService
{
    public async Task<QRCodeResult> GenerateQRCodeAsync(Guid tenantId, Guid userId)
    {
        // Generate secure token
        var token = GenerateSecureToken();
        
        // Create invitation code
        var invitation = new InvitationCode
        {
            TenantId = tenantId,
            Code = GenerateShortCode(),
            Token = token,
            Type = InvitationType.QR,
            CreatedByUserId = userId,
            IsActive = true
        };
        
        await _context.InvitationCodes.AddAsync(invitation);
        await _context.SaveChangesAsync();
        
        // Generate QR code image
        var invitationUrl = $"https://rendevumvar.com/invite/{token}";
        var qrCodeImage = _qrCodeGenerator.Generate(invitationUrl);
        
        // Upload to blob storage
        var imageUrl = await _blobStorage.UploadAsync(qrCodeImage, $"qr/{token}.png");
        
        return new QRCodeResult
        {
            ImageUrl = imageUrl,
            InvitationUrl = invitationUrl,
            Token = token
        };
    }
    
    public async Task<InvitationValidationResult> ValidateInvitationAsync(string token)
    {
        var invitation = await _context.InvitationCodes
            .Include(i => i.Tenant)
            .FirstOrDefaultAsync(i => i.Token == token && i.IsActive);
        
        if (invitation == null)
            return new InvitationValidationResult { IsValid = false, Error = "Invalid invitation" };
        
        // Check expiry
        if (invitation.ExpiresAt.HasValue && invitation.ExpiresAt.Value < DateTime.UtcNow)
            return new InvitationValidationResult { IsValid = false, Error = "Invitation expired" };
        
        // Check max uses
        if (invitation.MaxUses.HasValue && invitation.UsedCount >= invitation.MaxUses.Value)
            return new InvitationValidationResult { IsValid = false, Error = "Invitation limit reached" };
        
        return new InvitationValidationResult 
        { 
            IsValid = true, 
            TenantId = invitation.TenantId,
            TenantName = invitation.Tenant.Name
        };
    }
    
    private string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, 32);
    }
    
    private string GenerateShortCode()
    {
        // Generate 6-character code without ambiguous characters (O, 0, I, 1)
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 6)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }
}
```

### 10.3 Package and Session Management Architecture

#### 10.3.1 Package Purchase Flow

```
Customer views packages
        │
        ├──► Selects package
        │
        ├──► Choose payment type
        │    ├──► Full Payment ──────► Process payment ──► Activate package
        │    ├──► Installments ──────► Create InstallmentPlan ──► First payment ──► Activate
        │    └──► Deposit ───────────► Partial payment ──► Activate with balance due
        │
        ├──► Create CustomerPackage
        │    └──► Status: Active
        │
        ├──► Create CustomerPackageServices
        │    └──► TotalSessions, UsedSessions = 0, RemainingSessions = TotalSessions
        │
        ├──► Generate Invoice
        │
        └──► Send Confirmation Email
             └──► Package details, Expiry date, Terms
```

**Package Database Schema:**
```sql
-- Service Packages
CREATE TABLE ServicePackages (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Type NVARCHAR(20) NOT NULL, -- MultiSession, MixedService, Unlimited, Membership
    OriginalPrice DECIMAL(10,2) NOT NULL,
    PackagePrice DECIMAL(10,2) NOT NULL,
    DiscountPercentage DECIMAL(5,2) NOT NULL,
    ValidityDays INT NOT NULL,
    MaxUsesPerWeek INT DEFAULT 0,
    MaxUsesPerMonth INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    IsRefundable BIT DEFAULT 0,
    IsTransferable BIT DEFAULT 0,
    Terms NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);

-- Package Services (What's included in package)
CREATE TABLE PackageServices (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ServicePackageId INT NOT NULL,
    ServiceId UNIQUEIDENTIFIER NOT NULL,
    Quantity INT NOT NULL, -- Number of sessions
    IndividualPrice DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (ServicePackageId) REFERENCES ServicePackages(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id)
);

-- Customer Packages (Purchased packages)
CREATE TABLE CustomerPackages (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    ServicePackageId INT NOT NULL,
    PurchaseDate DATETIME2 NOT NULL,
    ExpiryDate DATETIME2 NOT NULL,
    PaymentType NVARCHAR(20) NOT NULL, -- FullPayment, Installments, Deposit
    TotalAmount DECIMAL(10,2) NOT NULL,
    AmountPaid DECIMAL(10,2) NOT NULL,
    AmountDue DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL, -- Active, Suspended, Expired, Cancelled, Completed
    IsExpired BIT DEFAULT 0,
    PurchaseInvoiceId INT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Users(Id),
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    FOREIGN KEY (ServicePackageId) REFERENCES ServicePackages(Id),
    FOREIGN KEY (PurchaseInvoiceId) REFERENCES Invoices(Id)
);

CREATE INDEX IX_CustomerPackages_CustomerId ON CustomerPackages(CustomerId);
CREATE INDEX IX_CustomerPackages_Status ON CustomerPackages(Status);

-- Customer Package Services (Session tracking)
CREATE TABLE CustomerPackageServices (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerPackageId INT NOT NULL,
    ServiceId UNIQUEIDENTIFIER NOT NULL,
    TotalSessions INT NOT NULL,
    UsedSessions INT DEFAULT 0,
    RemainingSessions AS (TotalSessions - UsedSessions) PERSISTED,
    FOREIGN KEY (CustomerPackageId) REFERENCES CustomerPackages(Id) ON DELETE CASCADE,
    FOREIGN KEY (ServiceId) REFERENCES Services(Id)
);

-- Package Session Usage (Audit trail)
CREATE TABLE PackageSessionUsages (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerPackageServiceId INT NOT NULL,
    AppointmentId UNIQUEIDENTIFIER NOT NULL,
    UsedAt DATETIME2 DEFAULT GETUTCDATE(),
    StaffId UNIQUEIDENTIFIER NOT NULL,
    Notes NVARCHAR(500) NULL,
    FOREIGN KEY (CustomerPackageServiceId) REFERENCES CustomerPackageServices(Id),
    FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id),
    FOREIGN KEY (StaffId) REFERENCES Staff(Id)
);

-- Installment Plans
CREATE TABLE PackageInstallmentPlans (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerPackageId INT NOT NULL,
    NumberOfInstallments INT NOT NULL,
    InstallmentAmount DECIMAL(10,2) NOT NULL,
    DayOfMonthForPayment INT NOT NULL,
    Status NVARCHAR(20) NOT NULL, -- Active, Completed, Suspended, Cancelled
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (CustomerPackageId) REFERENCES CustomerPackages(Id)
);

-- Installments
CREATE TABLE Installments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    InstallmentPlanId INT NOT NULL,
    InstallmentNumber INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    DueDate DATETIME2 NOT NULL,
    PaidDate DATETIME2 NULL,
    Status NVARCHAR(20) NOT NULL, -- Pending, Paid, Overdue, Failed, Waived
    PaymentTransactionId NVARCHAR(100) NULL,
    FOREIGN KEY (InstallmentPlanId) REFERENCES PackageInstallmentPlans(Id)
);

CREATE INDEX IX_Installments_Status ON Installments(Status);
CREATE INDEX IX_Installments_DueDate ON Installments(DueDate);
```

**Session Deduction Logic:**
```csharp
public class PackageSessionService : IPackageSessionService
{
    public async Task<bool> DeductSessionAsync(Guid appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Services)
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);
        
        if (appointment == null || appointment.Status != AppointmentStatus.Completed)
            return false;
        
        // Find active packages for customer
        var activePackages = await _context.CustomerPackages
            .Where(cp => cp.CustomerId == appointment.CustomerId 
                      && cp.TenantId == appointment.TenantId
                      && cp.Status == CustomerPackageStatus.Active
                      && cp.ExpiryDate > DateTime.UtcNow)
            .Include(cp => cp.Services)
            .ToListAsync();
        
        foreach (var appointmentService in appointment.Services)
        {
            // Find package that covers this service
            var packageService = activePackages
                .SelectMany(p => p.Services)
                .FirstOrDefault(ps => ps.ServiceId == appointmentService.ServiceId 
                                   && ps.RemainingSessions > 0);
            
            if (packageService != null)
            {
                // Deduct session
                packageService.UsedSessions++;
                
                // Log usage
                var usage = new PackageSessionUsage
                {
                    CustomerPackageServiceId = packageService.Id,
                    AppointmentId = appointmentId,
                    StaffId = appointment.StaffId,
                    UsedAt = DateTime.UtcNow,
                    Notes = $"Session used for {appointmentService.Service.Name}"
                };
                
                await _context.PackageSessionUsages.AddAsync(usage);
                
                // Send notification
                await _notificationService.SendAsync(new Notification
                {
                    UserId = appointment.CustomerId,
                    Type = NotificationType.SessionUsed,
                    Title = "Package session used",
                    Body = $"You have {packageService.RemainingSessions - 1} sessions remaining"
                });
                
                // Check if package is completed
                var allServices = await _context.CustomerPackageServices
                    .Where(cps => cps.CustomerPackageId == packageService.CustomerPackageId)
                    .ToListAsync();
                
                if (allServices.All(s => s.RemainingSessions == 0))
                {
                    var package = await _context.CustomerPackages
                        .FindAsync(packageService.CustomerPackageId);
                    package.Status = CustomerPackageStatus.Completed;
                    
                    await _notificationService.SendAsync(new Notification
                    {
                        UserId = appointment.CustomerId,
                        Type = NotificationType.PackageCompleted,
                        Title = "Package completed",
                        Body = "All sessions in your package have been used"
                    });
                }
            }
        }
        
        await _context.SaveChangesAsync();
        return true;
    }
}
```

**Package Expiry Background Job:**
```csharp
public class PackageExpiryJob : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IServiceProvider _serviceProvider;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Run daily at 2 AM
        _timer = new Timer(CheckExpiredPackages, null, TimeSpan.Zero, TimeSpan.FromHours(24));
        return Task.CompletedTask;
    }

    private async void CheckExpiredPackages(object state)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        
        var today = DateTime.UtcNow.Date;
        
        // Find expiring packages (30, 15, 7, 3, 1 days before)
        var expiringPackages = await context.CustomerPackages
            .Where(cp => cp.Status == CustomerPackageStatus.Active 
                      && cp.ExpiryDate >= today 
                      && cp.ExpiryDate <= today.AddDays(30))
            .Include(cp => cp.Customer)
            .Include(cp => cp.ServicePackage)
            .ToListAsync();
        
        foreach (var package in expiringPackages)
        {
            var daysUntilExpiry = (package.ExpiryDate - today).Days;
            
            // Send notifications at specific intervals
            if (new[] { 30, 15, 7, 3, 1 }.Contains(daysUntilExpiry))
            {
                await notificationService.SendAsync(new Notification
                {
                    UserId = package.CustomerId,
                    Type = NotificationType.PackageExpiring,
                    Title = "Package expiring soon",
                    Body = $"Your '{package.ServicePackage.Name}' package expires in {daysUntilExpiry} days"
                });
            }
        }
        
        // Mark expired packages
        var expiredPackages = await context.CustomerPackages
            .Where(cp => cp.Status == CustomerPackageStatus.Active 
                      && cp.ExpiryDate < today)
            .ToListAsync();
        
        foreach (var package in expiredPackages)
        {
            package.Status = CustomerPackageStatus.Expired;
            package.IsExpired = true;
            
            await notificationService.SendAsync(new Notification
            {
                UserId = package.CustomerId,
                Type = NotificationType.PackageExpired,
                Title = "Package expired",
                Body = $"Your '{package.ServicePackage.Name}' package has expired"
            });
        }
        
        await context.SaveChangesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
```

### 10.4 Notification System Architecture

#### 10.4.1 Multi-Channel Notification Flow

```
┌─────────────────────────────────────────────────┐
│         NOTIFICATION TRIGGER                    │
└─────────────────────────────────────────────────┘
                    │
                    ├──► Booking Created
                    ├──► Appointment Reminder
                    ├──► Payment Received
                    ├──► Package Expiring
                    └──► Custom Event
                    │
                    ▼
┌─────────────────────────────────────────────────┐
│      NOTIFICATION SERVICE                       │
│  - Determine recipient                          │
│  - Get user notification preferences            │
│  - Load notification template                   │
│  - Merge template with data                     │
└─────────────┬───────────────────────────────────┘
              │
              ├──► Queue for SMS ──────► SMS Provider (Twilio)
              │                          └──► Delivery Status
              │
              ├──► Queue for Email ─────► Email Provider (SendGrid)
              │                          └──► Open/Click Tracking
              │
              ├──► Queue for Push ──────► Push Service (Firebase)
              │                          └──► Delivery Receipt
              │
              └──► Store In-App ────────► Database
                                         └──► Mark as unread
```

**Notification Service Implementation:**
```csharp
public class NotificationService : INotificationService
{
    public async Task SendBookingConfirmationAsync(Guid appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.Services)
            .ThenInclude(s => s.Service)
            .Include(a => a.Tenant)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);
        
        if (appointment == null) return;
        
        // Get user notification preferences
        var preferences = await GetUserPreferencesAsync(appointment.CustomerId);
        
        // Prepare notification data
        var data = new Dictionary<string, string>
        {
            ["CustomerName"] = appointment.Customer.FirstName,
            ["SalonName"] = appointment.Tenant.Name,
            ["DateTime"] = appointment.StartTime.ToString("dd MMMM yyyy HH:mm"),
            ["Services"] = string.Join(", ", appointment.Services.Select(s => s.Service.Name)),
            ["StaffName"] = $"{appointment.Staff.FirstName} {appointment.Staff.LastName}",
            ["TotalPrice"] = appointment.TotalPrice.ToString("C"),
            ["CancellationUrl"] = $"https://rendevumvar.com/appointments/{appointmentId}/cancel"
        };
        
        // Send via enabled channels
        if (preferences.EmailEnabled)
        {
            await QueueEmailNotificationAsync(
                NotificationType.BookingConfirmation,
                appointment.Customer.Email,
                data);
        }
        
        if (preferences.SMSEnabled)
        {
            await QueueSMSNotificationAsync(
                NotificationType.BookingConfirmation,
                appointment.Customer.PhoneNumber,
                data);
        }
        
        if (preferences.PushEnabled)
        {
            await QueuePushNotificationAsync(
                NotificationType.BookingConfirmation,
                appointment.CustomerId,
                data);
        }
        
        // Always create in-app notification
        await CreateInAppNotificationAsync(
            NotificationType.BookingConfirmation,
            appointment.CustomerId,
            data);
    }
    
    private async Task QueueEmailNotificationAsync(
        NotificationType type, 
        string email, 
        Dictionary<string, string> data)
    {
        var template = await GetTemplateAsync(type, NotificationChannel.Email);
        var subject = MergeTemplate(template.Subject, data);
        var body = MergeTemplate(template.Body, data);
        
        var notification = new NotificationQueue
        {
            Type = type,
            Channel = NotificationChannel.Email,
            RecipientContact = email,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.Queued,
            ScheduledFor = DateTime.UtcNow,
            AttemptCount = 0
        };
        
        await _context.NotificationQueue.AddAsync(notification);
        await _context.SaveChangesAsync();
    }
}
```

**Notification Background Worker:**
```csharp
public class NotificationWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessNotificationQueueAsync();
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
    
    private async Task ProcessNotificationQueueAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Get pending notifications
        var notifications = await context.NotificationQueue
            .Where(n => n.Status == NotificationStatus.Queued 
                     && n.ScheduledFor <= DateTime.UtcNow
                     && n.AttemptCount < 3)
            .Take(50)
            .ToListAsync();
        
        foreach (var notification in notifications)
        {
            try
            {
                notification.Status = NotificationStatus.Sending;
                notification.AttemptCount++;
                await context.SaveChangesAsync();
                
                switch (notification.Channel)
                {
                    case NotificationChannel.Email:
                        await _emailService.SendAsync(
                            notification.RecipientContact,
                            notification.Subject,
                            notification.Body);
                        break;
                    
                    case NotificationChannel.SMS:
                        await _smsService.SendAsync(
                            notification.RecipientContact,
                            notification.Body);
                        break;
                    
                    case NotificationChannel.Push:
                        await _pushService.SendAsync(
                            notification.RecipientId,
                            notification.Subject,
                            notification.Body);
                        break;
                }
                
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                notification.Status = notification.AttemptCount >= 3 
                    ? NotificationStatus.Failed 
                    : NotificationStatus.Queued;
                notification.FailureReason = ex.Message;
            }
            
            await context.SaveChangesAsync();
        }
    }
}
```

### 10.5 Payment Integration Architecture

**Payment Flow Diagram:**
```
Customer initiates payment
        │
        ├──► PayTR Integration
        │    ├──► Generate payment iframe
        │    ├──► Customer enters card details
        │    ├──► 3D Secure authentication
        │    ├──► Payment processed
        │    └──► Webhook callback
        │
        ├──► iyzico Integration
        │    └──► Similar flow
        │
        └──► QR Code Payment
             ├──► Generate QR with payment details
             ├──► Customer scans with bank app
             ├──► Payment confirmation
             └──► Webhook callback

Payment Gateway Webhook
        │
        ├──► Validate signature
        ├──► Update Payment record
        ├──► Update Appointment/Package
        ├──► Send receipt email
        └──► Trigger success notification
```

**Payment Service:**
```csharp
public class PaymentService : IPaymentService
{
    public async Task<PaymentInitiationResult> InitiatePaymentAsync(PaymentRequest request)
    {
        // Create payment record
        var payment = new Payment
        {
            TenantId = request.TenantId,
            CustomerId = request.CustomerId,
            AppointmentId = request.AppointmentId,
            CustomerPackageId = request.CustomerPackageId,
            Amount = request.Amount,
            Currency = "TRY",
            Method = request.Method,
            Status = PaymentStatus.Pending,
            PaymentGateway = request.Gateway
        };
        
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
        
        // Call payment gateway
        switch (request.Gateway)
        {
            case "PayTR":
                return await InitiatePayTRPaymentAsync(payment, request);
            case "iyzico":
                return await InitiateiyzPaymentAsync(payment, request);
            default:
                throw new NotSupportedException($"Gateway {request.Gateway} not supported");
        }
    }
    
    public async Task<bool> HandleWebhookAsync(string gateway, string payload, string signature)
    {
        // Validate webhook signature
        if (!ValidateSignature(gateway, payload, signature))
        {
            _logger.LogWarning("Invalid webhook signature from {Gateway}", gateway);
            return false;
        }
        
        // Parse webhook data
        var webhookData = ParseWebhookData(gateway, payload);
        
        // Find payment record
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.TransactionId == webhookData.TransactionId);
        
        if (payment == null)
        {
            _logger.LogWarning("Payment not found for transaction {TransactionId}", webhookData.TransactionId);
            return false;
        }
        
        // Update payment status
        payment.Status = webhookData.Success ? PaymentStatus.Completed : PaymentStatus.Failed;
        payment.CompletedAt = DateTime.UtcNow;
        payment.FailureReason = webhookData.ErrorMessage;
        
        if (webhookData.Success)
        {
            // Update related entity (appointment or package)
            if (payment.AppointmentId.HasValue)
            {
                var appointment = await _context.Appointments.FindAsync(payment.AppointmentId.Value);
                appointment.PaymentStatus = PaymentStatus.Completed;
            }
            else if (payment.CustomerPackageId.HasValue)
            {
                var package = await _context.CustomerPackages.FindAsync(payment.CustomerPackageId.Value);
                package.AmountPaid += payment.Amount;
                package.AmountDue -= payment.Amount;
            }
            
            // Send receipt
            await _notificationService.SendPaymentReceiptAsync(payment.Id);
        }
        
        await _context.SaveChangesAsync();
        return true;
    }
}
```

---

## 11. Appendices

### Appendix A: Technology Versions
- .NET Core: 8.0
- Entity Framework Core: 8.0
- React: 18.2
- TypeScript: 5.0
- SQL Server: 2019+

### Appendix B: Third-Party Libraries

**Backend:**
- AutoMapper
- FluentValidation
- Serilog
- Swashbuckle (Swagger)
- xUnit
- Moq

**Frontend:**
- React Router
- Redux Toolkit
- Material-UI
- Axios
- React Hook Form
- date-fns

### Appendix C: Naming Conventions

**C# (Backend):**
- PascalCase for classes, methods, properties
- camelCase for local variables, parameters
- IInterface for interfaces
- Async suffix for async methods

**TypeScript (Frontend):**
- PascalCase for components, types, interfaces
- camelCase for variables, functions
- UPPER_SNAKE_CASE for constants
- kebab-case for file names

### Appendix D: Git Workflow

**Branches:**
- `main`: Production-ready code
- `develop`: Integration branch
- `feature/feature-name`: Feature development
- `bugfix/bug-name`: Bug fixes
- `hotfix/issue-name`: Production hotfixes

**Commit Message Format:**
```
<type>(<scope>): <subject>

<body>

<footer>
```

Types: feat, fix, docs, style, refactor, test, chore

---

**End of Software Design Document**
