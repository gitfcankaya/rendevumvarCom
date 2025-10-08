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

## 10. Appendices

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
