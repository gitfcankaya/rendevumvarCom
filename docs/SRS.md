# Software Requirements Specification (SRS)
# RendevumVar - SaaS Salon Appointment System

## Document Information
- **Version:** 1.0
- **Date:** 2024
- **Project:** RendevumVar
- **Status:** Draft

## Table of Contents
1. Introduction
2. Overall Description
3. System Features and Requirements
4. External Interface Requirements
5. Non-Functional Requirements
6. System Architecture

---

## 1. Introduction

### 1.1 Purpose
This Software Requirements Specification (SRS) document provides a comprehensive description of the RendevumVar salon appointment system. It describes the functional and non-functional requirements for developers, testers, and project stakeholders.

### 1.2 Scope
RendevumVar is a web-based, multi-tenant SaaS platform that enables:
- Customers to book appointments at salons and barbershops
- Business owners to manage their operations, staff, and services
- Staff members to view and manage their schedules

**System Boundaries:**
- IN SCOPE: Web application, REST API, database, notifications
- OUT OF SCOPE: Native mobile apps (Phase 1), POS hardware integration, payment processing (handled by third-party)

### 1.3 Definitions, Acronyms, and Abbreviations
- **SaaS:** Software as a Service
- **API:** Application Programming Interface
- **JWT:** JSON Web Token
- **RBAC:** Role-Based Access Control
- **SMS:** Short Message Service
- **CRUD:** Create, Read, Update, Delete
- **MFA:** Multi-Factor Authentication
- **PWA:** Progressive Web App
- **EF Core:** Entity Framework Core
- **MSSQL:** Microsoft SQL Server

### 1.4 References
- PRD (Product Requirements Document)
- SDD (Software Design Document)
- .NET Core 8.0 Documentation
- React 18 Documentation
- Entity Framework Core Documentation

### 1.5 Overview
The remainder of this document describes:
- System features and functional requirements
- External interfaces (user, hardware, software, communication)
- Non-functional requirements (performance, security, usability)
- System architecture and design constraints

---

## 2. Overall Description

### 2.1 Product Perspective
RendevumVar is a new, self-contained SaaS product that operates as a web application. It consists of:
- **Frontend:** React-based single-page application (SPA)
- **Backend:** .NET Core Web API
- **Database:** Microsoft SQL Server
- **External Services:** Email, SMS, payment gateway

### 2.2 Product Functions
The system provides the following high-level functions:

1. **User Management**
   - Registration and authentication
   - Profile management
   - Role-based access control

2. **Business Management**
   - Salon profile creation and management
   - Service catalog management
   - Staff management
   - Location management

3. **Appointment Management**
   - Booking creation
   - Appointment modification
   - Cancellation handling
   - Waitlist management

4. **Schedule Management**
   - Calendar views
   - Availability management
   - Time blocking

5. **Notification System**
   - Email notifications
   - SMS notifications
   - In-app notifications

6. **Payment Processing**
   - Online payment integration
   - Deposit handling
   - Transaction history

7. **Reporting and Analytics**
   - Business metrics
   - Revenue reports
   - Customer insights

### 2.3 User Classes and Characteristics

#### 2.3.1 Customer (End User)
- **Technical Expertise:** Basic to intermediate
- **Frequency of Use:** Occasional (monthly or bi-weekly)
- **Primary Functions:** Browse salons, book appointments, manage profile
- **Security Level:** Standard authentication

#### 2.3.2 Business Owner
- **Technical Expertise:** Intermediate
- **Frequency of Use:** Daily
- **Primary Functions:** Manage business operations, view analytics, configure settings
- **Security Level:** Enhanced authentication with sensitive data access

#### 2.3.3 Staff Member
- **Technical Expertise:** Basic to intermediate
- **Frequency of Use:** Daily
- **Primary Functions:** View schedule, manage appointments, update availability
- **Security Level:** Standard authentication with limited business data access

#### 2.3.4 System Administrator
- **Technical Expertise:** Advanced
- **Frequency of Use:** As needed
- **Primary Functions:** System configuration, tenant management, troubleshooting
- **Security Level:** Full system access with MFA

### 2.4 Operating Environment
- **Client-side:** Modern web browsers (Chrome, Firefox, Safari, Edge)
- **Server-side:** Windows Server or Linux with .NET Core runtime
- **Database:** Microsoft SQL Server 2019 or later
- **Cloud Platform:** Azure or AWS
- **Minimum Browser Versions:**
  - Chrome 90+
  - Firefox 88+
  - Safari 14+
  - Edge 90+

### 2.5 Design and Implementation Constraints

#### 2.5.1 Technical Constraints
- Must use .NET Core 8.0 for backend
- Must use React 18+ for frontend
- Must use Entity Framework Core for data access
- Must use Microsoft SQL Server for database
- Must support HTTPS only
- Must be mobile-responsive

#### 2.5.2 Regulatory Constraints
- GDPR compliance for EU users
- KVKK compliance for Turkish users
- PCI DSS for payment data (handled by payment gateway)
- Accessibility standards (WCAG 2.1 Level AA)

#### 2.5.3 Business Constraints
- Multi-tenant architecture for scalability
- Subdomain-based tenant isolation
- Support for 1000+ concurrent users

### 2.6 Assumptions and Dependencies

#### 2.6.1 Assumptions
- Users have reliable internet connectivity
- Users have modern web browsers
- Salons have valid business information
- Email and phone numbers are valid

#### 2.6.2 Dependencies
- Third-party email service (SendGrid or similar)
- Third-party SMS service (Twilio or similar)
- Third-party payment gateway
- Cloud hosting provider
- SSL certificate provider

---

## 3. System Features and Requirements

### 3.1 User Management

#### 3.1.1 User Registration
**FR-UM-001: Customer Registration**
- **Description:** Customers can create new accounts
- **Priority:** High
- **Requirements:**
  - System shall allow email-based registration
  - System shall validate email format
  - System shall require password with minimum 8 characters
  - System shall require password confirmation
  - System shall send verification email
  - System shall support social authentication (Google, Facebook)
  - System shall collect: first name, last name, email, phone number
  - System shall validate Turkish phone number format

**FR-UM-002: Business Registration**
- **Description:** Business owners can register their salons
- **Priority:** High
- **Requirements:**
  - System shall allow business owner registration
  - System shall collect business information: name, address, phone, tax ID
  - System shall require business verification
  - System shall create tenant account
  - System shall assign unique subdomain
  - System shall create admin user for business owner

**FR-UM-003: Staff Registration**
- **Description:** Business owners can add staff members
- **Priority:** High
- **Requirements:**
  - System shall allow business owners to create staff accounts
  - System shall send invitation email to staff
  - System shall allow staff to set password via invitation link
  - System shall collect staff information: name, email, phone, specialties

#### 3.1.2 Authentication
**FR-UM-010: User Login**
- **Description:** Users can authenticate to access the system
- **Priority:** High
- **Requirements:**
  - System shall support email and password authentication
  - System shall support social authentication
  - System shall generate JWT access token (15 minutes expiry)
  - System shall generate refresh token (7 days expiry)
  - System shall support "Remember Me" functionality
  - System shall implement rate limiting (5 failed attempts = 15-minute lockout)
  - System shall log authentication attempts

**FR-UM-011: Password Reset**
- **Description:** Users can reset forgotten passwords
- **Priority:** High
- **Requirements:**
  - System shall provide "Forgot Password" link
  - System shall send reset link via email
  - Reset link shall expire after 1 hour
  - System shall require password confirmation
  - System shall enforce password policy

**FR-UM-012: Logout**
- **Description:** Users can securely logout
- **Priority:** Medium
- **Requirements:**
  - System shall invalidate access token
  - System shall invalidate refresh token
  - System shall clear client-side session data

#### 3.1.3 Profile Management
**FR-UM-020: View Profile**
- **Description:** Users can view their profile information
- **Priority:** High
- **Requirements:**
  - System shall display user information
  - System shall display booking history (for customers)
  - System shall display business information (for business owners)

**FR-UM-021: Edit Profile**
- **Description:** Users can update their profile information
- **Priority:** High
- **Requirements:**
  - System shall allow editing: name, phone, preferences
  - System shall not allow email changes without verification
  - System shall validate all input fields
  - System shall save profile pictures to blob storage

**FR-UM-022: Change Password**
- **Description:** Users can change their password
- **Priority:** Medium
- **Requirements:**
  - System shall require current password
  - System shall require new password with confirmation
  - System shall enforce password policy
  - System shall invalidate all existing sessions

#### 3.1.4 Authorization
**FR-UM-030: Role-Based Access Control**
- **Description:** System enforces role-based permissions
- **Priority:** High
- **Requirements:**
  - System shall implement roles: Customer, Staff, BusinessOwner, Admin
  - System shall enforce permissions per role
  - System shall restrict access to tenant data
  - System shall log authorization failures

### 3.2 Business Management

#### 3.2.1 Salon Profile
**FR-BM-001: Create Salon Profile**
- **Description:** Business owners can create salon profiles
- **Priority:** High
- **Requirements:**
  - System shall collect: salon name, description, address, phone
  - System shall collect business hours for each day
  - System shall allow multiple photos upload
  - System shall validate required fields
  - System shall assign unique subdomain

**FR-BM-002: Edit Salon Profile**
- **Description:** Business owners can update salon information
- **Priority:** High
- **Requirements:**
  - System shall allow editing all profile fields
  - System shall maintain audit history
  - System shall validate changes

**FR-BM-003: Location Management**
- **Description:** Business owners can manage multiple locations
- **Priority:** Medium
- **Requirements:**
  - System shall support multiple locations per business
  - Each location shall have unique address
  - Each location shall have independent hours
  - Each location shall have independent staff assignment

#### 3.2.2 Service Management
**FR-BM-010: Create Service**
- **Description:** Business owners can add services
- **Priority:** High
- **Requirements:**
  - System shall collect: name, description, duration, price
  - System shall support service categories
  - System shall allow service photos
  - System shall validate duration (5-360 minutes)
  - System shall validate price (> 0)

**FR-BM-011: Edit Service**
- **Description:** Business owners can modify services
- **Priority:** High
- **Requirements:**
  - System shall allow editing all service fields
  - System shall maintain price history
  - System shall not allow deletion if service has future bookings

**FR-BM-012: Service Catalog**
- **Description:** Display all services for a salon
- **Priority:** High
- **Requirements:**
  - System shall list all active services
  - System shall support filtering by category
  - System shall display prices and durations
  - System shall show service images

#### 3.2.3 Staff Management
**FR-BM-020: Add Staff Member**
- **Description:** Business owners can add staff
- **Priority:** High
- **Requirements:**
  - System shall collect staff information
  - System shall assign services to staff
  - System shall define working hours
  - System shall send invitation email

**FR-BM-021: Edit Staff Member**
- **Description:** Business owners can update staff information
- **Priority:** High
- **Requirements:**
  - System shall allow editing staff details
  - System shall allow modifying service assignments
  - System shall allow updating working hours
  - System shall support staff deactivation

**FR-BM-022: Staff Schedule**
- **Description:** Define staff working hours
- **Priority:** High
- **Requirements:**
  - System shall support different hours per day
  - System shall support break times
  - System shall support time off requests
  - System shall support recurring schedules

### 3.3 Appointment Management

#### 3.3.1 Booking Creation
**FR-AM-001: Search Salons**
- **Description:** Customers can search for salons
- **Priority:** High
- **Requirements:**
  - System shall support search by name
  - System shall support search by location
  - System shall support filter by services
  - System shall display search results with ratings

**FR-AM-002: View Salon Details**
- **Description:** Customers can view salon information
- **Priority:** High
- **Requirements:**
  - System shall display salon profile
  - System shall display service catalog
  - System shall display staff profiles
  - System shall display reviews and ratings
  - System shall display business hours

**FR-AM-003: Select Service**
- **Description:** Customers can choose service(s)
- **Priority:** High
- **Requirements:**
  - System shall display available services
  - System shall support multiple service selection
  - System shall calculate total duration
  - System shall calculate total price
  - System shall validate service combinations

**FR-AM-004: Select Staff**
- **Description:** Customers can choose staff member
- **Priority:** High
- **Requirements:**
  - System shall display available staff for selected services
  - System shall support "No Preference" option
  - System shall display staff photos and ratings
  - System shall show staff availability

**FR-AM-005: Select Date and Time**
- **Description:** Customers can choose appointment date and time
- **Priority:** High
- **Requirements:**
  - System shall display calendar view
  - System shall show available time slots in 15-minute increments
  - System shall consider staff availability
  - System shall consider service duration
  - System shall consider business hours
  - System shall enforce minimum advance booking (2 hours)
  - System shall enforce maximum advance booking (90 days)
  - System shall prevent double bookings

**FR-AM-006: Confirm Booking**
- **Description:** Customers can confirm appointment
- **Priority:** High
- **Requirements:**
  - System shall display booking summary
  - System shall require confirmation
  - System shall create appointment record
  - System shall send confirmation email
  - System shall send confirmation SMS
  - System shall update staff calendar
  - System shall support guest checkout (no registration)

**FR-AM-007: Guest Booking**
- **Description:** Non-registered users can book appointments
- **Priority:** Medium
- **Requirements:**
  - System shall allow booking without registration
  - System shall collect: name, email, phone
  - System shall send confirmation to provided email
  - System shall provide booking reference number
  - System shall allow booking management via reference number

#### 3.3.2 Booking Modification
**FR-AM-010: View Appointments**
- **Description:** Users can view their appointments
- **Priority:** High
- **Requirements:**
  - Customers shall see their upcoming and past appointments
  - Staff shall see their assigned appointments
  - Business owners shall see all appointments
  - System shall display appointment details

**FR-AM-011: Reschedule Appointment**
- **Description:** Users can reschedule appointments
- **Priority:** High
- **Requirements:**
  - System shall enforce minimum notice period (24 hours)
  - System shall show available alternative slots
  - System shall update appointment record
  - System shall send rescheduling notification
  - System shall log change history

**FR-AM-012: Cancel Appointment**
- **Description:** Users can cancel appointments
- **Priority:** High
- **Requirements:**
  - System shall enforce cancellation policy
  - System shall update appointment status to "Cancelled"
  - System shall send cancellation notification
  - System shall free up time slot
  - System shall apply cancellation fee if applicable
  - System shall log cancellation reason

#### 3.3.3 Appointment Status
**FR-AM-020: Appointment States**
- **Description:** System manages appointment lifecycle
- **Priority:** High
- **Requirements:**
  - System shall support states: Pending, Confirmed, CheckedIn, InProgress, Completed, Cancelled, NoShow
  - Staff shall mark customer as "Checked In"
  - Staff shall mark appointment as "In Progress"
  - Staff shall mark appointment as "Completed"
  - Business owner shall mark as "No Show" if customer doesn't arrive
  - System shall send automated reminders based on state

### 3.4 Calendar and Schedule Management

#### 3.4.1 Calendar Views
**FR-CS-001: Daily View**
- **Description:** Display appointments for a single day
- **Priority:** High
- **Requirements:**
  - System shall show time slots from business opening to closing
  - System shall display appointments in chronological order
  - System shall color-code by status
  - System shall show appointment details on hover

**FR-CS-002: Weekly View**
- **Description:** Display appointments for a week
- **Priority:** Medium
- **Requirements:**
  - System shall show 7-day grid
  - System shall display appointments in respective time slots
  - System shall support scrolling for overflow

**FR-CS-003: Monthly View**
- **Description:** Display appointments for a month
- **Priority:** Medium
- **Requirements:**
  - System shall show calendar grid
  - System shall show booking count per day
  - System shall highlight days with bookings

**FR-CS-004: Staff-Specific View**
- **Description:** Filter calendar by staff member
- **Priority:** High
- **Requirements:**
  - System shall show individual staff schedules
  - System shall display staff availability
  - System shall show booked and free slots

#### 3.4.2 Availability Management
**FR-CS-010: Set Working Hours**
- **Description:** Define when staff are available
- **Priority:** High
- **Requirements:**
  - System shall allow setting hours per day of week
  - System shall support irregular schedules
  - System shall support breaks
  - System shall validate time ranges

**FR-CS-011: Block Time**
- **Description:** Reserve time slots for non-bookable activities
- **Priority:** Medium
- **Requirements:**
  - System shall allow blocking specific time slots
  - System shall support recurring blocks
  - System shall require reason for block
  - Blocked time shall not appear as available

**FR-CS-012: Time Off**
- **Description:** Staff can request time off
- **Priority:** Medium
- **Requirements:**
  - Staff shall submit time-off requests
  - Business owner shall approve/reject requests
  - System shall block availability for approved time off
  - System shall notify affected customers if appointments exist

### 3.5 Notification System

#### 3.5.1 Email Notifications
**FR-NS-001: Booking Confirmation Email**
- **Priority:** High
- **Requirements:**
  - System shall send email upon booking creation
  - Email shall include: salon details, service, date/time, staff, price
  - Email shall include calendar invite (ICS file)
  - Email shall include cancellation link

**FR-NS-002: Booking Reminder Email**
- **Priority:** High
- **Requirements:**
  - System shall send reminder 24 hours before appointment
  - Email shall include appointment details
  - Email shall include reschedule/cancel links

**FR-NS-003: Cancellation Email**
- **Priority:** High
- **Requirements:**
  - System shall send email upon cancellation
  - Email shall include cancellation details
  - Email shall include rebooking link

#### 3.5.2 SMS Notifications
**FR-NS-010: Booking Confirmation SMS**
- **Priority:** High
- **Requirements:**
  - System shall send SMS upon booking creation
  - SMS shall include: salon name, service, date/time
  - SMS shall be concise (< 160 characters)

**FR-NS-011: Booking Reminder SMS**
- **Priority:** High
- **Requirements:**
  - System shall send reminder 2 hours before appointment
  - SMS shall include essential details

#### 3.5.3 In-App Notifications
**FR-NS-020: Real-Time Notifications**
- **Priority:** Medium
- **Requirements:**
  - System shall show notifications in app header
  - System shall support notification badge count
  - Users shall receive notifications for: new bookings, cancellations, changes
  - Notifications shall be dismissible

### 3.6 Payment Processing

#### 3.6.1 Online Payments
**FR-PM-001: Payment Integration**
- **Description:** Process online payments securely
- **Priority:** Medium
- **Requirements:**
  - System shall integrate with payment gateway (Stripe or Iyzico for Turkey)
  - System shall support credit/debit card payments
  - System shall not store card details
  - System shall use PCI-compliant payment gateway
  - System shall support 3D Secure authentication

**FR-PM-002: Deposit Payment**
- **Description:** Require deposits for bookings
- **Priority:** Medium
- **Requirements:**
  - Business owner shall configure deposit amount (percentage or fixed)
  - System shall collect deposit during booking
  - System shall store payment transaction details
  - System shall support deposit refunds per cancellation policy

**FR-PM-003: Payment History**
- **Description:** Track payment transactions
- **Priority:** Medium
- **Requirements:**
  - System shall record all transactions
  - Users shall view payment history
  - System shall generate receipts
  - System shall support refund processing

### 3.7 Reviews and Ratings

#### 3.7.1 Review Submission
**FR-RR-001: Submit Review**
- **Description:** Customers can review completed appointments
- **Priority:** High
- **Requirements:**
  - System shall allow reviews only for completed appointments
  - System shall require 1-5 star rating
  - System shall allow optional written review
  - System shall support reviewing salon and/or specific staff
  - System shall prevent duplicate reviews for same appointment

**FR-RR-002: Edit Review**
- **Description:** Customers can modify their reviews
- **Priority:** Low
- **Requirements:**
  - System shall allow editing within 30 days
  - System shall maintain review history

#### 3.7.2 Review Display
**FR-RR-010: Display Reviews**
- **Description:** Show reviews on salon profile
- **Priority:** High
- **Requirements:**
  - System shall display average rating
  - System shall display review count
  - System shall show recent reviews
  - System shall support pagination
  - System shall allow filtering by rating

**FR-RR-011: Respond to Reviews**
- **Description:** Business owners can respond to reviews
- **Priority:** Medium
- **Requirements:**
  - System shall allow one response per review
  - Response shall appear below review
  - System shall notify customer of response

### 3.8 Reporting and Analytics

#### 3.8.1 Business Reports
**FR-RA-001: Dashboard Overview**
- **Description:** Display key business metrics
- **Priority:** Medium
- **Requirements:**
  - System shall show: total bookings, revenue, cancellation rate, average rating
  - System shall support date range filtering
  - System shall display trends (vs previous period)

**FR-RA-002: Revenue Report**
- **Description:** Track financial performance
- **Priority:** Medium
- **Requirements:**
  - System shall show total revenue by period
  - System shall break down by service
  - System shall break down by staff
  - System shall support export to CSV/PDF

**FR-RA-003: Booking Report**
- **Description:** Analyze booking patterns
- **Priority:** Medium
- **Requirements:**
  - System shall show booking trends over time
  - System shall identify peak hours/days
  - System shall show service popularity
  - System shall show customer retention metrics

**FR-RA-004: Staff Performance**
- **Description:** Track staff metrics
- **Priority:** Medium
- **Requirements:**
  - System shall show bookings per staff
  - System shall show revenue per staff
  - System shall show average rating per staff
  - System shall show utilization rate

---

## 4. External Interface Requirements

### 4.1 User Interfaces

#### 4.1.1 General UI Requirements
- **UI-001:** Interface shall be responsive for mobile, tablet, and desktop
- **UI-002:** Interface shall follow Material Design or similar modern design system
- **UI-003:** Interface shall be accessible (WCAG 2.1 Level AA)
- **UI-004:** Interface shall load within 2 seconds on 4G connection
- **UI-005:** Interface shall support keyboard navigation
- **UI-006:** Interface shall provide clear error messages
- **UI-007:** Interface shall support Turkish and English languages

#### 4.1.2 Customer Interface
- Home page with salon search
- Salon profile page
- Booking flow (multi-step wizard)
- User dashboard
- Appointment history
- Profile settings
- Review submission form

#### 4.1.3 Business Dashboard
- Dashboard overview with KPIs
- Calendar view (daily/weekly/monthly)
- Appointment management table
- Customer list
- Staff management
- Service management
- Reports section
- Settings pages

#### 4.1.4 Staff Portal
- Daily schedule view
- Appointment details
- Availability management
- Profile settings
- Performance metrics

### 4.2 Hardware Interfaces
- No direct hardware interfaces required
- System shall operate on standard web servers
- Database shall run on standard SQL Server instance

### 4.3 Software Interfaces

#### 4.3.1 Database Interface
- **Database:** Microsoft SQL Server 2019+
- **Access Method:** Entity Framework Core ORM
- **Connection:** Secure connection string with encrypted credentials
- **Pooling:** Connection pooling enabled

#### 4.3.2 Email Service Interface
- **Provider:** SendGrid, AWS SES, or similar
- **Protocol:** SMTP or REST API
- **Authentication:** API key
- **Rate Limits:** Must handle provider rate limits

#### 4.3.3 SMS Service Interface
- **Provider:** Twilio, Vonage, or Turkish provider
- **Protocol:** REST API
- **Authentication:** API key and secret
- **Rate Limits:** Must handle provider rate limits

#### 4.3.4 Payment Gateway Interface
- **Provider:** Stripe or Iyzico (for Turkey)
- **Protocol:** REST API with webhooks
- **Authentication:** API keys (public and secret)
- **PCI Compliance:** Gateway handles card data

#### 4.3.5 Storage Interface
- **Provider:** Azure Blob Storage or AWS S3
- **Purpose:** Store images and files
- **Access:** Via SDK with credentials
- **CDN:** Content delivery for public files

### 4.4 Communication Interfaces

#### 4.4.1 HTTP/HTTPS
- All communication shall use HTTPS (TLS 1.3)
- HTTP shall redirect to HTTPS
- API shall use RESTful architecture
- API shall return JSON responses

#### 4.4.2 WebSocket (Optional for future)
- Real-time notifications
- Live calendar updates

---

## 5. Non-Functional Requirements

### 5.1 Performance Requirements

#### 5.1.1 Response Time
- **NFR-P-001:** Web page load time shall be < 2 seconds (95th percentile)
- **NFR-P-002:** API response time shall be < 500ms (95th percentile)
- **NFR-P-003:** Database query time shall be < 200ms (95th percentile)
- **NFR-P-004:** Search results shall appear within 1 second

#### 5.1.2 Throughput
- **NFR-P-010:** System shall support 1000+ concurrent users
- **NFR-P-011:** System shall handle 100 bookings per minute
- **NFR-P-012:** System shall process 10,000 API requests per minute

#### 5.1.3 Capacity
- **NFR-P-020:** System shall support 10,000+ salons
- **NFR-P-021:** System shall support 1,000,000+ customers
- **NFR-P-022:** Database shall handle 100GB+ data
- **NFR-P-023:** File storage shall handle 1TB+ images

### 5.2 Security Requirements

#### 5.2.1 Authentication
- **NFR-S-001:** Passwords shall be hashed using bcrypt (cost factor 12)
- **NFR-S-002:** JWT tokens shall expire after 15 minutes
- **NFR-S-003:** Refresh tokens shall expire after 7 days
- **NFR-S-004:** Failed login attempts shall be rate-limited
- **NFR-S-005:** Account lockout after 5 failed attempts for 15 minutes

#### 5.2.2 Authorization
- **NFR-S-010:** All API endpoints shall require authentication (except public pages)
- **NFR-S-011:** Role-based access control shall be enforced
- **NFR-S-012:** Tenant data isolation shall be enforced at database level

#### 5.2.3 Data Protection
- **NFR-S-020:** All data in transit shall be encrypted (TLS 1.3)
- **NFR-S-021:** Sensitive data at rest shall be encrypted
- **NFR-S-022:** Personal data shall be anonymized in logs
- **NFR-S-023:** Payment card data shall never be stored
- **NFR-S-024:** Database backups shall be encrypted

#### 5.2.4 Application Security
- **NFR-S-030:** All inputs shall be validated and sanitized
- **NFR-S-031:** SQL injection prevention via parameterized queries
- **NFR-S-032:** XSS prevention via output encoding
- **NFR-S-033:** CSRF protection via tokens
- **NFR-S-034:** Security headers shall be implemented (CSP, HSTS, etc.)
- **NFR-S-035:** API rate limiting shall prevent abuse

### 5.3 Reliability Requirements

#### 5.3.1 Availability
- **NFR-R-001:** System uptime shall be 99.5% (monthly)
- **NFR-R-002:** Planned maintenance window: Sunday 2-4 AM
- **NFR-R-003:** System shall degrade gracefully if external services fail

#### 5.3.2 Fault Tolerance
- **NFR-R-010:** System shall retry failed email/SMS sends (3 attempts)
- **NFR-R-011:** Database connection failures shall be handled gracefully
- **NFR-R-012:** System shall log all errors for debugging

#### 5.3.3 Backup and Recovery
- **NFR-R-020:** Database backups daily at 3 AM
- **NFR-R-021:** Backup retention: 30 days
- **NFR-R-022:** Recovery Time Objective (RTO): 4 hours
- **NFR-R-023:** Recovery Point Objective (RPO): 24 hours

### 5.4 Scalability Requirements

#### 5.4.1 Horizontal Scaling
- **NFR-SC-001:** Web servers shall be stateless for load balancing
- **NFR-SC-002:** System shall support multiple application instances
- **NFR-SC-003:** Session state shall be stored externally (Redis)

#### 5.4.2 Database Scaling
- **NFR-SC-010:** Database shall support read replicas
- **NFR-SC-011:** Connection pooling shall be configured
- **NFR-SC-012:** Database partitioning shall be possible by tenant

### 5.5 Usability Requirements

#### 5.5.1 Ease of Use
- **NFR-U-001:** New users shall complete booking in < 3 minutes
- **NFR-U-002:** Interface shall require < 3 clicks for common tasks
- **NFR-U-003:** Help documentation shall be accessible
- **NFR-U-004:** Error messages shall be clear and actionable

#### 5.5.2 Accessibility
- **NFR-U-010:** Interface shall comply with WCAG 2.1 Level AA
- **NFR-U-011:** Interface shall be navigable by keyboard
- **NFR-U-012:** Interface shall support screen readers
- **NFR-U-013:** Color contrast ratio shall be at least 4.5:1
- **NFR-U-014:** Images shall have alt text

#### 5.5.3 Localization
- **NFR-U-020:** Interface shall support Turkish and English
- **NFR-U-021:** Date format shall follow locale conventions
- **NFR-U-022:** Currency shall display in Turkish Lira
- **NFR-U-023:** Phone numbers shall follow Turkish format

### 5.6 Maintainability Requirements

#### 5.6.1 Code Quality
- **NFR-M-001:** Code shall follow C# and TypeScript best practices
- **NFR-M-002:** Code shall be commented for complex logic
- **NFR-M-003:** Code shall have unit test coverage > 70%
- **NFR-M-004:** Code shall pass static analysis tools

#### 5.6.2 Logging and Monitoring
- **NFR-M-010:** System shall log all errors with stack traces
- **NFR-M-011:** System shall log all security events
- **NFR-M-012:** System shall track performance metrics
- **NFR-M-013:** Logs shall be centralized for analysis

#### 5.6.3 Documentation
- **NFR-M-020:** API shall be documented with OpenAPI/Swagger
- **NFR-M-021:** Database schema shall be documented
- **NFR-M-022:** Deployment process shall be documented
- **NFR-M-023:** User guides shall be provided

### 5.7 Portability Requirements

#### 5.7.1 Platform Independence
- **NFR-PO-001:** Backend shall run on Windows and Linux
- **NFR-PO-002:** Frontend shall work on all major browsers
- **NFR-PO-003:** System shall be deployable on Azure or AWS
- **NFR-PO-004:** System shall be containerized with Docker

---

## 6. System Architecture

### 6.1 Architecture Style
- **Pattern:** N-tier architecture with clean separation of concerns
- **Frontend:** Single Page Application (SPA)
- **Backend:** RESTful API
- **Database:** Relational database (MSSQL)

### 6.2 Technology Stack

#### 6.2.1 Backend
- **Framework:** .NET Core 8.0
- **Language:** C# 12
- **ORM:** Entity Framework Core 8.0
- **API Framework:** ASP.NET Core Web API
- **Authentication:** JWT with Microsoft.AspNetCore.Authentication.JwtBearer
- **Validation:** FluentValidation
- **Logging:** Serilog
- **Testing:** xUnit, Moq

#### 6.2.2 Frontend
- **Framework:** React 18
- **Language:** TypeScript 5
- **State Management:** Redux Toolkit or Zustand
- **Routing:** React Router 6
- **UI Library:** Material-UI (MUI) or Ant Design
- **HTTP Client:** Axios
- **Forms:** React Hook Form
- **Date Handling:** date-fns or Day.js
- **Build Tool:** Vite
- **Testing:** Jest, React Testing Library

#### 6.2.3 Database
- **RDBMS:** Microsoft SQL Server 2019+
- **Migrations:** Entity Framework Core Migrations
- **Indexing:** Strategic indexes on foreign keys and search fields

#### 6.2.4 Infrastructure
- **Hosting:** Azure App Service or AWS Elastic Beanstalk
- **Database:** Azure SQL Database or AWS RDS
- **File Storage:** Azure Blob Storage or AWS S3
- **Cache:** Redis (Azure Cache for Redis or AWS ElastiCache)
- **Email:** SendGrid or AWS SES
- **SMS:** Twilio or local provider
- **Monitoring:** Application Insights or CloudWatch
- **CI/CD:** GitHub Actions or Azure DevOps

### 6.3 Multi-Tenancy Architecture
- **Approach:** Shared database with tenant isolation via TenantId
- **Tenant Identification:** Subdomain (tenant.rendevumvar.com)
- **Data Isolation:** Row-level security enforced in queries
- **Tenant Context:** Middleware to resolve tenant from request

### 6.4 Security Architecture
- **Authentication Flow:**
  1. User submits credentials
  2. API validates credentials
  3. API generates JWT access token (15 min) and refresh token (7 days)
  4. Client stores tokens
  5. Client includes access token in Authorization header
  6. API validates token on each request
  7. Client uses refresh token to obtain new access token

- **Authorization Flow:**
  1. API validates JWT token
  2. API extracts user claims (userId, role, tenantId)
  3. API checks permission for requested resource
  4. API filters data by tenantId

### 6.5 Data Architecture

#### 6.5.1 Core Entities
- **User:** Id, Email, PasswordHash, FirstName, LastName, Phone, Role, TenantId
- **Tenant:** Id, Name, Subdomain, SubscriptionPlan, Status
- **Salon:** Id, TenantId, Name, Description, Address, Phone, BusinessHours
- **Service:** Id, TenantId, SalonId, Name, Description, Duration, Price, CategoryId
- **Staff:** Id, TenantId, SalonId, UserId, Specialties, WorkingHours
- **Appointment:** Id, TenantId, SalonId, CustomerId, StaffId, ServiceId, StartTime, EndTime, Status, Notes
- **Review:** Id, AppointmentId, CustomerId, Rating, Comment, Response, CreatedAt
- **Payment:** Id, AppointmentId, Amount, Method, Status, TransactionId

#### 6.5.2 Relationships
- Tenant has many Salons
- Salon has many Services
- Salon has many Staff
- Staff belongs to User
- Appointment belongs to Customer (User), Staff, Service
- Review belongs to Appointment, Customer

### 6.6 API Design

#### 6.6.1 RESTful Endpoints
```
Authentication:
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
POST /api/auth/forgot-password
POST /api/auth/reset-password

Salons:
GET /api/salons (public - search)
GET /api/salons/{id} (public)
POST /api/salons (business owner)
PUT /api/salons/{id} (business owner)
DELETE /api/salons/{id} (business owner)

Services:
GET /api/salons/{salonId}/services (public)
GET /api/services/{id} (public)
POST /api/services (business owner)
PUT /api/services/{id} (business owner)
DELETE /api/services/{id} (business owner)

Staff:
GET /api/salons/{salonId}/staff (public)
GET /api/staff/{id} (public)
POST /api/staff (business owner)
PUT /api/staff/{id} (business owner/staff)
DELETE /api/staff/{id} (business owner)
GET /api/staff/{id}/availability

Appointments:
GET /api/appointments (authenticated)
GET /api/appointments/{id}
POST /api/appointments
PUT /api/appointments/{id}
DELETE /api/appointments/{id}
GET /api/availability (check available slots)

Reviews:
GET /api/salons/{salonId}/reviews (public)
POST /api/reviews (customer)
PUT /api/reviews/{id} (customer)
POST /api/reviews/{id}/response (business owner)

Reports:
GET /api/reports/dashboard (business owner)
GET /api/reports/revenue (business owner)
GET /api/reports/bookings (business owner)
GET /api/reports/staff-performance (business owner)
```

#### 6.6.2 Response Format
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful",
  "errors": []
}
```

#### 6.6.3 Error Format
```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": [
    {
      "field": "email",
      "message": "Email is required"
    }
  ]
}
```

### 6.7 Deployment Architecture
- **Production Environment:**
  - Load Balancer → Multiple Web Servers
  - Web Servers → SQL Database (primary + replica)
  - Web Servers → Redis Cache
  - Web Servers → Blob Storage
  - Background Jobs → Message Queue (Azure Service Bus or AWS SQS)

- **Development Environment:**
  - Single web server
  - Local SQL Server
  - No load balancer

### 6.8 System Constraints
- .NET Core 8.0 required
- Entity Framework Core 8.0
- React 18+
- TypeScript 5+
- Microsoft SQL Server 2019+
- HTTPS only
- Modern browser support (last 2 versions)

---

## Appendices

### Appendix A: Glossary
- **Tenant:** A salon or business that uses the platform
- **Multi-tenancy:** Architecture supporting multiple tenants in one system
- **JWT:** JSON Web Token for authentication
- **SPA:** Single Page Application
- **RBAC:** Role-Based Access Control
- **ORM:** Object-Relational Mapping

### Appendix B: Revision History
| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2024 | System | Initial draft |

### Appendix C: Approval
This document requires approval from:
- Product Owner
- Technical Lead
- Business Stakeholders

---

**End of Software Requirements Specification**
