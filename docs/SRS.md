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

## 7. Advanced Features - Detailed Requirements

### 7.1 Subscription Management Module

#### 7.1.1 Subscription Plans
**FR-SUB-001: Plan Definition**
- **Description:** System shall support multiple subscription tiers
- **Priority:** High
- **Requirements:**
  - System shall define 4 plan tiers: Free, Starter, Professional, Enterprise
  - Each plan shall have: name, price, billing cycle, feature limits
  - System shall support monthly and annual billing cycles
  - System shall offer discount for annual subscriptions (e.g., 2 months free)
  - System shall allow custom pricing for Enterprise plans

**Data Model:**
```csharp
public class SubscriptionPlan
{
    public int Id { get; set; }
    public string Name { get; set; } // Free, Starter, Professional, Enterprise
    public string Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public int TrialDays { get; set; } // 0-30 days
    public int MaxStaff { get; set; } // -1 for unlimited
    public int MaxAppointmentsPerMonth { get; set; } // -1 for unlimited
    public bool HasAdvancedAnalytics { get; set; }
    public bool HasSMSNotifications { get; set; }
    public bool HasCustomBranding { get; set; }
    public bool HasAPIAccess { get; set; }
    public bool HasMultiLocation { get; set; }
    public bool HasPackageManagement { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TenantSubscription
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int SubscriptionPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public SubscriptionStatus Status { get; set; } // Active, Trialing, Cancelled, Suspended, Expired
    public BillingCycle BillingCycle { get; set; } // Monthly, Annual
    public DateTime? NextBillingDate { get; set; }
    public string PaymentMethodId { get; set; }
    public bool AutoRenew { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum SubscriptionStatus
{
    Trialing,
    Active,
    PastDue,
    Cancelled,
    Suspended,
    Expired
}

public enum BillingCycle
{
    Monthly,
    Annual
}
```

**FR-SUB-002: Trial Management**
- **Priority:** High
- **Requirements:**
  - System shall automatically activate trial period on business registration
  - Trial duration shall be configurable per plan (7-30 days)
  - System shall send reminder emails: 7 days before, 3 days before, 1 day before trial end
  - System shall automatically downgrade to free plan if no payment method added
  - System shall allow one trial per tenant (email/phone validation)

**FR-SUB-003: Plan Upgrade/Downgrade**
- **Priority:** High
- **Requirements:**
  - System shall allow self-service plan changes
  - Upgrades shall be effective immediately
  - Downgrades shall be effective at next billing cycle
  - System shall validate feature limits before downgrade (e.g., staff count)
  - System shall prorate charges for mid-cycle upgrades
  - System shall send confirmation email after plan change

**API Endpoints:**
```
GET /api/subscriptions/plans - Get all available plans
GET /api/tenants/{tenantId}/subscription - Get current subscription
POST /api/tenants/{tenantId}/subscription - Create subscription
PUT /api/tenants/{tenantId}/subscription/upgrade - Upgrade plan
PUT /api/tenants/{tenantId}/subscription/downgrade - Downgrade plan
DELETE /api/tenants/{tenantId}/subscription - Cancel subscription
POST /api/subscriptions/webhooks/payment - Payment gateway webhook
```

#### 7.1.2 Billing and Invoicing
**FR-SUB-010: Automatic Billing**
- **Priority:** High
- **Requirements:**
  - System shall charge payment method on billing date
  - System shall retry failed payments: after 3 days, 5 days, 7 days
  - System shall send payment receipt via email
  - System shall suspend account after 3 failed payment attempts
  - System shall support dunning management

**FR-SUB-011: Invoice Generation**
- **Priority:** High
- **Requirements:**
  - System shall generate PDF invoices
  - Invoice shall include: invoice number, date, billing details, line items, tax
  - System shall store invoices in blob storage
  - System shall email invoice to tenant admin
  - System shall provide invoice history in dashboard

**Data Model:**
```csharp
public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } // INV-2025-0001
    public int TenantId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; }
    public string Currency { get; set; } // TRY
    public string PdfUrl { get; set; }
    public List<InvoiceLineItem> LineItems { get; set; }
    public DateTime? PaidAt { get; set; }
    public string PaymentTransactionId { get; set; }
}

public enum InvoiceStatus
{
    Draft,
    Sent,
    Paid,
    Overdue,
    Cancelled
}
```

### 7.2 Customer-Business Connection System

#### 7.2.1 Invitation Management
**FR-INV-001: QR Code Invitation**
- **Description:** Business generates QR code for customer onboarding
- **Priority:** High
- **Requirements:**
  - System shall generate unique QR code per tenant
  - QR code shall encode invitation URL with tenant ID and token
  - QR code shall be regeneratable (invalidates old codes)
  - QR code shall be downloadable as PNG/PDF
  - QR code shall include tenant logo/branding
  - Scanning QR code shall open invitation page in mobile browser

**Data Model:**
```csharp
public class InvitationCode
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Code { get; set; } // 6-8 character alphanumeric
    public InvitationType Type { get; set; } // QR, Link, SMS, Code
    public string Token { get; set; } // Unique secure token
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; } // Null = never expires
    public int? MaxUses { get; set; } // Null = unlimited
    public int UsedCount { get; set; }
    public bool IsActive { get; set; }
    public string CreatedByUserId { get; set; }
}

public enum InvitationType
{
    QR,
    Link,
    SMS,
    Code
}
```

**FR-INV-002: Invitation Link Generation**
- **Priority:** High
- **Requirements:**
  - System shall generate shareable invitation links
  - Link format: `https://rendevumvar.com/invite/{token}`
  - Link shall be copyable with one click
  - System shall track link usage analytics
  - Link shall work on all devices
  - Link shall have configurable expiry (7, 30, 90 days, never)

**FR-INV-003: SMS Invitation**
- **Priority:** High
- **Requirements:**
  - Business shall enter customer phone number
  - System shall validate phone number format
  - System shall send SMS with invitation link
  - SMS template: "[Salon Name] sizi davet ediyor! Randevu almak için: {link}"
  - System shall check SMS delivery status
  - System shall limit SMS invitations per day (anti-spam)
  - System shall charge SMS cost to tenant account

**FR-INV-004: Invitation Code System**
- **Priority:** Medium
- **Requirements:**
  - System shall generate 6-8 character alphanumeric codes
  - Codes shall be easy to type (no ambiguous characters: O/0, I/1)
  - Customer shall enter code during registration
  - System shall validate code and establish connection
  - Codes shall be displayable in salon (poster, business card)

**API Endpoints:**
```
POST /api/tenants/{tenantId}/invitations/qr - Generate QR code
POST /api/tenants/{tenantId}/invitations/link - Generate link
POST /api/tenants/{tenantId}/invitations/sms - Send SMS invitation
POST /api/tenants/{tenantId}/invitations/code - Generate invitation code
GET /api/tenants/{tenantId}/invitations - List all invitations
GET /api/invitations/{token}/validate - Validate invitation token
POST /api/invitations/{token}/accept - Accept invitation
DELETE /api/invitations/{id} - Deactivate invitation
```

#### 7.2.2 Mutual Approval System
**FR-CON-001: Connection Request Flow**
- **Description:** Establish connection between customer and business
- **Priority:** High
- **Requirements:**
  - System shall support bidirectional connection requests
  - System shall track connection status: Pending, Approved, Rejected, Blocked
  - System shall send notifications to both parties
  - System shall allow custom message with request
  - System shall store connection request timestamp

**Data Model:**
```csharp
public class CustomerBusinessConnection
{
    public int Id { get; set; }
    public string CustomerId { get; set; }
    public int TenantId { get; set; }
    public ConnectionStatus Status { get; set; }
    public ConnectionInitiator InitiatedBy { get; set; }
    public string RequestMessage { get; set; }
    public string RejectionReason { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? LastInteractionAt { get; set; }
    public int AppointmentCount { get; set; }
    public bool IsFavorite { get; set; } // Customer can mark business as favorite
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum ConnectionStatus
{
    PendingCustomerApproval, // Business invited customer
    PendingBusinessApproval, // Customer requested connection
    Approved,
    Rejected,
    Blocked,
    Disconnected
}

public enum ConnectionInitiator
{
    Customer,
    Business
}
```

**FR-CON-002: Approval Workflow - Customer Initiates**
- **Priority:** High
- **Requirements:**
  1. Customer scans QR/uses link/enters code
  2. If customer not registered: Complete registration first
  3. System creates connection request with status `PendingBusinessApproval`
  4. System sends notification to business owner/manager
  5. Business views customer profile (name, phone, previous bookings if any)
  6. Business approves or rejects with optional reason
  7. System updates connection status
  8. System notifies customer of decision
  9. If approved: Customer can now book appointments

**FR-CON-003: Approval Workflow - Business Initiates**
- **Priority:** High
- **Requirements:**
  1. Business enters customer phone number
  2. System checks if customer exists in system
  3. If exists: Send connection request notification
  4. If not exists: Send invitation SMS with registration link
  5. System creates connection request with status `PendingCustomerApproval`
  6. Customer receives notification
  7. Customer views business profile
  8. Customer accepts or declines
  9. System updates connection status
  10. System notifies business of decision

**FR-CON-004: Multi-Business Connections**
- **Priority:** High
- **Requirements:**
  - Customer can connect to multiple businesses
  - Customer can view all connected businesses in one list
  - Customer can favorite preferred businesses
  - Customer can disconnect from business anytime
  - Business can remove customer connection
  - System shall maintain connection history even after disconnect

**API Endpoints:**
```
GET /api/customers/{customerId}/connections - Get customer's connected businesses
GET /api/tenants/{tenantId}/connections - Get business's connected customers
POST /api/connections/request - Create connection request
PUT /api/connections/{id}/approve - Approve connection
PUT /api/connections/{id}/reject - Reject connection
DELETE /api/connections/{id} - Disconnect
PUT /api/connections/{id}/block - Block user
GET /api/connections/{id}/history - Get connection history
```

### 7.3 Multi-Staff Management

#### 7.3.1 Staff Roles and Permissions
**FR-STAFF-001: Role Definition**
- **Description:** System shall support granular role-based permissions
- **Priority:** High
- **Requirements:**
  - System shall define 4 staff roles: Owner, Manager, Staff, Receptionist
  - Each role shall have configurable permissions
  - System shall support custom role creation (Enterprise plan)
  - Permission changes shall take effect immediately

**Data Model:**
```csharp
public class StaffMember
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int TenantId { get; set; }
    public StaffRole Role { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string ProfilePictureUrl { get; set; }
    public string Title { get; set; } // "Senior Stylist", "Master Barber"
    public string Bio { get; set; }
    public List<string> Specialties { get; set; } // ["Haircut", "Coloring"]
    public List<int> ServiceIds { get; set; } // Services this staff can provide
    public decimal CommissionRate { get; set; } // Percentage
    public bool IsActive { get; set; }
    public bool IsVisibleToCustomers { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum StaffRole
{
    Owner,
    Manager,
    Staff,
    Receptionist
}

public class RolePermissions
{
    public StaffRole Role { get; set; }
    public bool CanManageStaff { get; set; }
    public bool CanManageServices { get; set; }
    public bool CanManageCustomers { get; set; }
    public bool CanViewAllAppointments { get; set; }
    public bool CanCreateAppointments { get; set; }
    public bool CanModifyAllAppointments { get; set; }
    public bool CanCancelAppointments { get; set; }
    public bool CanViewReports { get; set; }
    public bool CanViewFinancials { get; set; }
    public bool CanManageSettings { get; set; }
    public bool CanProcessPayments { get; set; }
    public bool CanManageSubscription { get; set; }
}
```

**Permission Matrix:**
| Permission | Owner | Manager | Staff | Receptionist |
|---|---|---|---|---|
| Manage Staff | ✓ | ✓ | ✗ | ✗ |
| Manage Services | ✓ | ✓ | ✗ | ✗ |
| Manage Customers | ✓ | ✓ | ✗ | ✓ |
| View All Appointments | ✓ | ✓ | ✗ | ✓ |
| Create Appointments | ✓ | ✓ | Own only | ✓ |
| Modify Appointments | ✓ | ✓ | Own only | ✓ |
| Cancel Appointments | ✓ | ✓ | Own only | ✓ |
| View Reports | ✓ | ✓ | Own only | ✗ |
| View Financials | ✓ | ✗ | ✗ | ✗ |
| Manage Settings | ✓ | ✗ | ✗ | ✗ |
| Process Payments | ✓ | ✓ | ✗ | ✓ |
| Manage Subscription | ✓ | ✗ | ✗ | ✗ |

#### 7.3.2 Staff Schedule Management
**FR-STAFF-010: Working Hours Configuration**
- **Description:** Define staff availability schedules
- **Priority:** High
- **Requirements:**
  - System shall allow per-staff working hours configuration
  - System shall support different hours per day of week
  - System shall support irregular schedules
  - System shall support multiple shifts per day
  - System shall validate time overlaps

**Data Model:**
```csharp
public class StaffWorkingHours
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsWorkingDay { get; set; }
    public List<BreakTime> Breaks { get; set; }
}

public class BreakTime
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Description { get; set; } // "Lunch", "Coffee Break"
}

public class StaffTimeOff
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeOffType Type { get; set; }
    public string Reason { get; set; }
    public TimeOffStatus Status { get; set; }
    public string ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum TimeOffType
{
    Vacation,
    SickLeave,
    PersonalLeave,
    Emergency,
    Training
}

public enum TimeOffStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public class StaffTimeBlock
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Reason { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrencePattern RecurrencePattern { get; set; }
}
```

**FR-STAFF-011: Calendar Visibility Settings**
- **Description:** Control staff calendar visibility to customers
- **Priority:** High
- **Requirements:**
  - Owner shall configure per-staff visibility settings
  - Options: Fully Visible, Hidden, Service-Specific
  - System shall respect visibility during booking flow
  - Hidden staff shall not appear in customer selection
  - System shall still allow manual assignment by owner/manager

#### 7.3.3 Staff Assignment Strategies
**FR-STAFF-020: Customer Booking Flow Modes**
- **Description:** Different ways customers can select staff
- **Priority:** High
- **Requirements:**
  - System shall support 4 booking modes (configurable per tenant):
    1. **Staff-First Mode:** Customer selects staff, then sees availability
    2. **Service-First Mode:** Customer selects service, system shows available staff
    3. **Auto-Assignment Mode:** System assigns staff automatically based on availability/load
    4. **Any Available Mode:** Customer requests appointment, first available staff assigned

**Business Rules:**
```csharp
public class TenantBookingSettings
{
    public int TenantId { get; set; }
    public BookingMode BookingMode { get; set; }
    public StaffAssignmentStrategy AssignmentStrategy { get; set; } // RoundRobin, LeastBusy, Random
    public bool AllowCustomerStaffPreference { get; set; }
    public bool AllowStaffChange { get; set; }
    public bool ShowStaffPriceVariations { get; set; }
}

public enum BookingMode
{
    StaffFirst,
    ServiceFirst,
    AutoAssignment,
    AnyAvailable
}

public enum StaffAssignmentStrategy
{
    RoundRobin, // Distribute evenly
    LeastBusy, // Assign to staff with fewest bookings
    Random,
    Priority // Based on staff seniority/ratings
}
```

**API Endpoints:**
```
GET /api/tenants/{tenantId}/staff - List all staff
POST /api/tenants/{tenantId}/staff - Add staff member
PUT /api/staff/{id} - Update staff details
DELETE /api/staff/{id} - Remove staff (soft delete)
GET /api/staff/{id}/schedule - Get staff schedule
PUT /api/staff/{id}/schedule - Update working hours
POST /api/staff/{id}/time-off - Request time off
GET /api/staff/{id}/availability - Check availability
POST /api/staff/{id}/time-blocks - Create time block
GET /api/staff/{id}/appointments - Get staff appointments
GET /api/staff/{id}/performance - Get performance metrics
```

### 7.4 Advanced Appointment Booking

#### 7.4.1 Booking Request System
**FR-BOOK-001: Request-Based Booking**
- **Description:** Customer sends booking request, business approves
- **Priority:** High
- **Requirements:**
  - Customer shall select service, preferred date/time, staff preference
  - System shall create appointment with status `PendingApproval`
  - System shall notify business (in-app + email + SMS)
  - Business shall review within 24 hours
  - Business can: Approve, Reject, or Propose Alternative
  - System shall notify customer of decision
  - Auto-reject if no response within 48 hours

**Data Model:**
```csharp
public class AppointmentRequest
{
    public int Id { get; set; }
    public string CustomerId { get; set; }
    public int TenantId { get; set; }
    public int? PreferredStaffId { get; set; }
    public List<int> ServiceIds { get; set; }
    public DateTime PreferredDateTime { get; set; }
    public DateTime? AlternativeDateTime1 { get; set; }
    public DateTime? AlternativeDateTime2 { get; set; }
    public string CustomerNotes { get; set; }
    public AppointmentRequestStatus Status { get; set; }
    public string ResponseNotes { get; set; }
    public DateTime? ProposedDateTime { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string RespondedByUserId { get; set; }
}

public enum AppointmentRequestStatus
{
    Pending,
    Approved,
    Rejected,
    Expired,
    CustomerCancelled
}
```

**FR-BOOK-002: Direct Booking with Slot Reservation**
- **Description:** Real-time availability booking with temporary hold
- **Priority:** High
- **Requirements:**
  - System shall display real-time available time slots
  - Customer shall select slot
  - System shall reserve slot for 10 minutes
  - Reserved slots shall show as "Being reserved" to other customers
  - System shall countdown remaining reservation time
  - Customer must complete booking within 10 minutes
  - System shall auto-release slot if not completed
  - Completed booking shall mark slot as booked

**Data Model:**
```csharp
public class SlotReservation
{
    public int Id { get; set; }
    public string CustomerId { get; set; }
    public int TenantId { get; set; }
    public int StaffId { get; set; }
    public DateTime SlotStartTime { get; set; }
    public DateTime SlotEndTime { get; set; }
    public DateTime ReservedAt { get; set; }
    public DateTime ExpiresAt { get; set; } // ReservedAt + 10 minutes
    public bool IsCompleted { get; set; }
    public int? AppointmentId { get; set; } // Set when booking completed
}
```

**Business Logic:**
```csharp
// Pseudo-code for availability check
public async Task<List<TimeSlot>> GetAvailableSlots(
    int tenantId,
    int serviceId,
    int? staffId,
    DateTime date)
{
    // 1. Get business working hours for date
    // 2. Get staff working hours (if specified)
    // 3. Get existing appointments
    // 4. Get active slot reservations
    // 5. Get time blocks
    // 6. Calculate service duration
    // 7. Generate available slots
    // 8. Filter out reserved slots
    // 9. Return available slots
}
```

#### 7.4.2 Booking Policies Engine
**FR-BOOK-010: Policy Configuration**
- **Description:** Configurable booking rules per tenant
- **Priority:** High
- **Requirements:**
  - System shall enforce booking policies
  - Policies shall be configurable per tenant
  - System shall validate bookings against policies
  - Policy violations shall prevent booking with clear error message

**Data Model:**
```csharp
public class BookingPolicy
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    
    // Advance booking rules
    public int MinimumAdvanceHours { get; set; } // e.g., 2
    public int MaximumAdvanceDays { get; set; } // e.g., 90
    public bool AllowSameDayBooking { get; set; }
    
    // Cancellation rules
    public int CancellationDeadlineHours { get; set; } // e.g., 24
    public bool ChargeCancellationFee { get; set; }
    public decimal CancellationFeeAmount { get; set; }
    public CancellationFeeType CancellationFeeType { get; set; } // Fixed, Percentage
    
    // Rescheduling rules
    public int ReschedulingDeadlineHours { get; set; }
    public int MaxReschedulesPerBooking { get; set; }
    public bool ChargeRescheduleFee { get; set; }
    public decimal RescheduleFeeAmount { get; set; }
    
    // No-show rules
    public int NoShowGracePeriodMinutes { get; set; } // e.g., 15
    public bool ChargeNoShowFee { get; set; }
    public decimal NoShowFeeAmount { get; set; }
    public int MaxNoShowsBeforeBlock { get; set; } // e.g., 3
    public bool RequireDepositAfterNoShow { get; set; }
    
    // Other rules
    public int BufferTimeBetweenAppointments { get; set; } // minutes
    public bool AllowDoubleBooking { get; set; }
    public bool RequirePhoneVerification { get; set; }
    public bool RequireDepositForFirstBooking { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}

public enum CancellationFeeType
{
    Fixed,
    Percentage
}
```

**FR-BOOK-011: Policy Validation**
- **Requirements:**
  - Validate minimum advance time: `BookingTime >= Now + MinimumAdvanceHours`
  - Validate maximum advance time: `BookingTime <= Now + MaximumAdvanceDays`
  - Validate cancellation deadline: `Now <= BookingTime - CancellationDeadlineHours`
  - Check customer no-show history before allowing booking
  - Enforce deposit requirement if policy requires

**Policy Display to Customers:**
```
Example Policy Display:
"Randevu Politikası:
- Randevu en az 2 saat önceden alınmalıdır
- En fazla 3 ay sonrası için randevu alınabilir
- İptal için randevudan en az 24 saat önce bildirim yapılmalıdır
- Geç iptal durumunda ₺50 ücret tahsil edilir
- 3 no-show durumunda hesap askıya alınır"
```

#### 7.4.3 Multi-Service Booking
**FR-BOOK-020: Combined Service Booking**
- **Description:** Book multiple services in single appointment
- **Priority:** Medium
- **Requirements:**
  - Customer shall select multiple services
  - System shall calculate total duration (sum of service durations + buffer time)
  - System shall check staff availability for entire duration
  - System shall support sequential services (Service A → Service B)
  - System shall calculate total price
  - System shall apply package discounts if applicable

**Data Model:**
```csharp
public class Appointment
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string CustomerId { get; set; }
    public int StaffId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public List<AppointmentService> Services { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public AppointmentStatus Status { get; set; }
    public string CustomerNotes { get; set; }
    public string StaffNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string CancelledByUserId { get; set; }
}

public class AppointmentService
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int ServiceId { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public int SequenceOrder { get; set; }
    public bool IsCompleted { get; set; }
}

public enum AppointmentStatus
{
    PendingApproval,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    NoShow,
    Rescheduled
}
```

#### 7.4.4 Group Booking
**FR-BOOK-030: Group Appointments**
- **Description:** Book for multiple people simultaneously
- **Priority:** Low
- **Requirements:**
  - Coordinator shall create group booking
  - Coordinator shall specify number of people
  - Each person can have different services
  - System shall check staff availability for all people
  - System shall assign different staff if needed
  - All appointments shall have same start time (or staggered)
  - Group discount shall apply
  - All members shall receive separate notifications
  - Coordinator can manage entire group booking

**Data Model:**
```csharp
public class GroupBooking
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string CoordinatorCustomerId { get; set; }
    public string GroupName { get; set; } // e.g., "Jane's Bridal Party"
    public int NumberOfPeople { get; set; }
    public DateTime PreferredDateTime { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal GroupDiscount { get; set; }
    public List<int> AppointmentIds { get; set; } // Individual appointments
    public GroupBookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum GroupBookingStatus
{
    Pending,
    Confirmed,
    PartiallyCompleted,
    Completed,
    Cancelled
}
```

**API Endpoints:**
```
POST /api/appointments/request - Create appointment request
POST /api/appointments/direct - Direct booking
POST /api/appointments/slots/reserve - Reserve time slot
GET /api/appointments/availability - Get available slots
PUT /api/appointments/{id}/approve - Approve request
PUT /api/appointments/{id}/reject - Reject request
PUT /api/appointments/{id}/propose - Propose alternative
POST /api/appointments/group - Create group booking
GET /api/appointments/{id} - Get appointment details
PUT /api/appointments/{id} - Update appointment
DELETE /api/appointments/{id}/cancel - Cancel appointment
PUT /api/appointments/{id}/reschedule - Reschedule appointment
PUT /api/appointments/{id}/check-in - Mark customer checked in
PUT /api/appointments/{id}/no-show - Mark as no-show
PUT /api/appointments/{id}/complete - Mark as completed
```

### 7.5 Package and Session Management

#### 7.5.1 Package Definition and Configuration
**FR-PKG-001: Package Creation**
- **Description:** Create service packages with multiple sessions
- **Priority:** High
- **Requirements:**
  - Business shall define package name and description
  - Business shall select services included (single or multiple)
  - Business shall specify number of sessions per service
  - Business shall set package price (discounted from individual prices)
  - Business shall set validity period (days)
  - Business shall configure usage restrictions
  - System shall calculate and display savings amount

**Data Model:**
```csharp
public class ServicePackage
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } // "10 Session Laser Package"
    public string Description { get; set; }
    public PackageType Type { get; set; }
    public List<PackageService> Services { get; set; }
    public decimal OriginalPrice { get; set; } // Sum of individual prices
    public decimal PackagePrice { get; set; } // Discounted price
    public decimal DiscountPercentage { get; set; }
    public int ValidityDays { get; set; } // 90, 180, 365
    public int MaxUsesPerWeek { get; set; } // 0 = unlimited
    public int MaxUsesPerMonth { get; set; } // 0 = unlimited
    public bool IsActive { get; set; }
    public bool IsRefundable { get; set; }
    public bool IsTransferable { get; set; }
    public string Terms { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum PackageType
{
    MultiSession, // Same service multiple times
    MixedService, // Different services bundled
    Unlimited, // Unlimited within time period
    Membership // Recurring subscription
}

public class PackageService
{
    public int Id { get; set; }
    public int ServicePackageId { get; set; }
    public int ServiceId { get; set; }
    public int Quantity { get; set; } // Number of sessions
    public decimal IndividualPrice { get; set; }
}
```

#### 7.5.2 Package Purchase and Payment
**FR-PKG-010: Package Purchase Flow**
- **Description:** Customer purchases package
- **Priority:** High
- **Requirements:**
  - Customer shall view available packages
  - System shall display package details, services, savings
  - Customer shall select payment option: Full, Installments, Deposit
  - System shall process payment
  - System shall create customer package record
  - System shall send confirmation email with package details
  - System shall activate package immediately after payment

**Data Model:**
```csharp
public class CustomerPackage
{
    public int Id { get; set; }
    public string CustomerId { get; set; }
    public int TenantId { get; set; }
    public int ServicePackageId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime ExpiryDate { get; set; } // PurchaseDate + ValidityDays
    public PackagePaymentType PaymentType { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public List<CustomerPackageService> Services { get; set; }
    public CustomerPackageStatus Status { get; set; }
    public bool IsExpired { get; set; }
    public int DaysUntilExpiry { get; set; }
    public string PurchaseInvoiceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum PackagePaymentType
{
    FullPayment,
    Installments,
    Deposit
}

public enum CustomerPackageStatus
{
    Active,
    Suspended,
    Expired,
    Cancelled,
    Completed
}

public class CustomerPackageService
{
    public int Id { get; set; }
    public int CustomerPackageId { get; set; }
    public int ServiceId { get; set; }
    public int TotalSessions { get; set; }
    public int UsedSessions { get; set; }
    public int RemainingSessions { get; set; }
    public List<PackageSessionUsage> UsageHistory { get; set; }
}

public class PackageSessionUsage
{
    public int Id { get; set; }
    public int CustomerPackageServiceId { get; set; }
    public int AppointmentId { get; set; }
    public DateTime UsedAt { get; set; }
    public int StaffId { get; set; }
    public string Notes { get; set; }
}
```

**FR-PKG-011: Installment Payment**
- **Description:** Pay for package in installments
- **Priority:** High
- **Requirements:**
  - Customer shall select installment plan (2, 3, 4, 6, 12 months)
  - System shall calculate monthly payment amount
  - First installment shall be due immediately
  - System shall auto-charge on due dates
  - System shall send payment reminders 3 days before due date
  - Late payment shall incur fee
  - System shall suspend package if payment 30 days overdue
  - Customer shall be able to pay remaining balance early

**Data Model:**
```csharp
public class PackageInstallmentPlan
{
    public int Id { get; set; }
    public int CustomerPackageId { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal InstallmentAmount { get; set; }
    public int DayOfMonthForPayment { get; set; }
    public List<Installment> Installments { get; set; }
    public InstallmentPlanStatus Status { get; set; }
}

public class Installment
{
    public int Id { get; set; }
    public int InstallmentPlanId { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string PaymentTransactionId { get; set; }
    public InstallmentStatus Status { get; set; }
}

public enum InstallmentStatus
{
    Pending,
    Paid,
    Overdue,
    Failed,
    Waived
}
```

#### 7.5.3 Session Tracking and Usage
**FR-PKG-020: Automatic Session Deduction**
- **Description:** Track package usage automatically
- **Priority:** High
- **Requirements:**
  - When appointment is completed, system shall check if customer has active package
  - If package covers the service, system shall deduct one session
  - System shall update remaining sessions count
  - System shall log usage with timestamp, staff, appointment details
  - Customer shall receive notification of remaining sessions
  - System shall notify when sessions are running low (e.g., 2 remaining)

**FR-PKG-021: Manual Session Adjustment**
- **Description:** Business can manually adjust sessions
- **Priority:** Medium
- **Requirements:**
  - Business owner/manager can add or remove sessions
  - Reason must be provided for adjustment
  - System shall log adjustment with user, timestamp, reason
  - Customer shall be notified of adjustment

**FR-PKG-022: Package Expiry Management**
- **Description:** Handle package expiration
- **Priority:** High
- **Requirements:**
  - System shall check package expiry daily (background job)
  - System shall send notifications: 30, 15, 7, 3, 1 days before expiry
  - Expired packages shall be marked as expired
  - Unused sessions in expired packages cannot be used
  - Customer can request extension (business approval required)
  - Business can set extension fee
  - System shall maintain history of expired packages

**API Endpoints:**
```
GET /api/packages - List available packages
POST /api/packages - Create package (business)
PUT /api/packages/{id} - Update package
DELETE /api/packages/{id} - Deactivate package
GET /api/customers/{customerId}/packages - Get customer's packages
POST /api/customers/{customerId}/packages/{packageId}/purchase - Purchase package
GET /api/customer-packages/{id} - Get package details
GET /api/customer-packages/{id}/usage-history - Get session usage history
POST /api/customer-packages/{id}/use-session - Manually deduct session
PUT /api/customer-packages/{id}/adjust-sessions - Adjust sessions
POST /api/customer-packages/{id}/extend - Request extension
PUT /api/customer-packages/{id}/cancel - Cancel package
GET /api/customer-packages/{id}/installments - Get installment schedule
POST /api/installments/{id}/pay - Make installment payment
```

### 7.6 Payment Integration

#### 7.6.1 Payment Provider Integration
**FR-PAY-001: PayTR Integration**
- **Description:** Integrate PayTR for payment processing
- **Priority:** High
- **Requirements:**
  - System shall integrate PayTR API
  - Support credit/debit card payments
  - Support 3D Secure authentication
  - Generate payment iframe
  - Handle payment callbacks/webhooks
  - Store transaction IDs
  - Handle refunds via API

**FR-PAY-002: QR Code Payment**
- **Description:** Generate QR codes for payment
- **Priority:** Medium
- **Requirements:**
  - System shall generate payment QR code
  - QR code shall encode payment amount, merchant info
  - Customer shall scan with banking app
  - System shall receive payment confirmation webhook
  - System shall update payment status in real-time

**Data Model:**
```csharp
public class Payment
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string CustomerId { get; set; }
    public int? AppointmentId { get; set; }
    public int? CustomerPackageId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } // TRY
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string TransactionId { get; set; } // From payment gateway
    public string PaymentGateway { get; set; } // PayTR, iyzico, etc.
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string FailureReason { get; set; }
    public string ReceiptUrl { get; set; }
    public int? RefundedPaymentId { get; set; }
}

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    BankTransfer,
    Cash,
    QRCode,
    Wallet,
    GiftCard
}

public enum PaymentStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded,
    PartiallyRefunded,
    Cancelled
}
```

### 7.7 Notification System

#### 7.7.1 Multi-Channel Notifications
**FR-NOT-001: Notification Delivery**
- **Description:** Send notifications via multiple channels
- **Priority:** High
- **Requirements:**
  - System shall support SMS, Email, Push, In-App notifications
  - User shall configure notification preferences
  - System shall respect user preferences
  - System shall queue notifications for async processing
  - System shall retry failed notifications (up to 3 attempts)
  - System shall log all notification attempts

**Data Model:**
```csharp
public class NotificationTemplate
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Subject { get; set; } // For email
    public string Body { get; set; } // Template with placeholders
    public string Language { get; set; } // tr, en
    public bool IsActive { get; set; }
}

public enum NotificationType
{
    BookingConfirmation,
    BookingReminder24h,
    BookingReminder2h,
    BookingCancelled,
    BookingRescheduled,
    BookingRequestReceived,
    BookingRequestApproved,
    BookingRequestRejected,
    PaymentReceived,
    PaymentFailed,
    PackagePurchased,
    PackageExpiring,
    PackageExpired,
    SessionUsed,
    InstallmentDue,
    InstallmentOverdue,
    NoShowRecorded,
    ReviewRequest,
    PromotionalMessage
}

public enum NotificationChannel
{
    SMS,
    Email,
    Push,
    InApp
}

public class NotificationQueue
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public NotificationChannel Channel { get; set; }
    public string RecipientId { get; set; }
    public string RecipientContact { get; set; } // Phone or email
    public string Subject { get; set; }
    public string Body { get; set; }
    public NotificationStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTime ScheduledFor { get; set; }
    public DateTime? SentAt { get; set; }
    public string FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum NotificationStatus
{
    Queued,
    Sending,
    Sent,
    Failed,
    Cancelled
}
```

**FR-NOT-002: Custom Notification Sounds**
- **Description:** Play custom sounds for notifications
- **Priority:** Low
- **Requirements:**
  - Business app shall play "scissors sound" for new appointment
  - Business app shall play "cash register sound" for payment
  - Sounds shall be configurable
  - Business can upload custom sounds
  - Customer app shall use standard notification sounds

**FR-NOT-003: Google Calendar Integration**
- **Description:** Sync appointments to Google Calendar
- **Priority:** Medium
- **Requirements:**
  - User shall connect Google account (OAuth)
  - System shall create calendar events for appointments
  - Two-way sync: changes in Google Calendar reflect in system
  - System shall handle event updates and deletions
  - System shall add calendar reminders
  - System shall color-code events by status

### 7.8 Cancellation and Modification

#### 7.8.1 Cancellation Rules Enforcement
**FR-CAN-001: Cancellation Validation**
- **Description:** Enforce cancellation policies
- **Priority:** High
- **Requirements:**
  - System shall check cancellation deadline before allowing cancellation
  - If within deadline: Free cancellation
  - If past deadline: Show warning and fee amount
  - Customer must confirm fee charge
  - System shall process cancellation fee payment
  - Customer must provide reason (minimum 25 characters)
  - System shall validate reason length
  - System shall save cancellation details

**FR-CAN-002: Cancellation Reason Validation**
- **Description:** Require meaningful cancellation reasons
- **Priority:** High
- **Requirements:**
  - Cancellation reason field must be at least 25 characters
  - System shall show character count
  - System shall prevent submission if too short
  - Reason categories: Personal, Emergency, Schedule Conflict, Dissatisfied, Other
  - For "Other", detailed explanation required

**FR-CAN-003: No-Show Tracking**
- **Description:** Track and manage no-shows
- **Priority:** High
- **Requirements:**
  - System shall mark appointment as no-show if customer doesn't arrive within grace period
  - System shall increment customer no-show count
  - After 3 no-shows: Customer flagged for review
  - System shall send warning notification
  - Further bookings may require deposit or approval
  - Customer can dispute no-show with evidence

**Data Model:**
```csharp
public class AppointmentCancellation
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public string CancelledByUserId { get; set; }
    public CancellationInitiator CancelledBy { get; set; }
    public DateTime CancelledAt { get; set; }
    public string Reason { get; set; } // Minimum 25 characters
    public CancellationReasonCategory Category { get; set; }
    public bool IsLateCancellation { get; set; }
    public decimal CancellationFee { get; set; }
    public bool FeeCharged { get; set; }
    public string CompensationOffered { get; set; } // If business cancels
}

public enum CancellationInitiator
{
    Customer,
    Business,
    System
}

public enum CancellationReasonCategory
{
    Personal,
    Emergency,
    ScheduleConflict,
    Dissatisfied,
    Other
}

public class CustomerNoShowHistory
{
    public int Id { get; set; }
    public string CustomerId { get; set; }
    public int TenantId { get; set; }
    public int NoShowCount { get; set; }
    public DateTime? LastNoShowDate { get; set; }
    public bool IsBlocked { get; set; }
    public bool RequiresDepositForBooking { get; set; }
    public DateTime? BlockedUntil { get; set; }
}
```

**API Endpoints:**
```
DELETE /api/appointments/{id}/cancel - Cancel appointment
POST /api/appointments/{id}/cancellation/reason - Submit cancellation reason
PUT /api/appointments/{id}/no-show - Mark as no-show
POST /api/appointments/{id}/no-show/dispute - Dispute no-show
GET /api/customers/{customerId}/no-show-history - Get no-show history
```

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
