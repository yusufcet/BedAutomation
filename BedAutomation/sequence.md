# BedAutomation - Sequence Diagramları

Bu dosya BedAutomation hastane otomasyon sistemi için detaylı sequence diagramlarını içerir.

## 1. Kimlik Doğrulama ve Rol Atama

```mermaid
sequenceDiagram
    participant U as "User"
    participant B as "Browser"
    participant S as "ASP.NET Core Server"
    participant A as "Auth Middleware"
    participant R as "AutoRoleAssignment<br/>Middleware"
    participant HC as "HomeController"
    participant PC as "PatientController"
    participant RC as "ReservationController"
    participant DC as "DoctorController"
    participant DB as "PostgreSQL Database"
    participant ID as "Identity System"
    
    Note over U,DB: 1. USER AUTHENTICATION & ROLE ASSIGNMENT
    
    U->>B: Access application
    B->>S: GET /Home/Index
    S->>A: Check authentication
    A->>ID: Validate user session
    
    alt User not authenticated
        ID-->>A: No valid session
        A-->>B: Redirect to /Identity/Account/Login
        B->>U: Show login page
        U->>B: Submit credentials
        B->>S: POST /Identity/Account/Login
        S->>ID: Authenticate user
        ID->>DB: Validate credentials
        DB-->>ID: User data
        ID-->>S: Authentication success
        S->>R: Auto-assign role
        R->>ID: Check user roles
        
        alt No roles assigned
            R->>ID: Assign default "Patient" role
            ID->>DB: Insert user role
        end
        
        R-->>S: Role assignment complete
        S-->>B: Redirect to /Home/Index
    else User authenticated
        ID-->>A: Valid session
        A-->>S: Continue to controller
    end
    
    Note over U,DB: 2. DASHBOARD ROUTING BY ROLE
    
    S->>HC: HomeController.Index()
    HC->>ID: GetUserAsync()
    ID-->>HC: Current user
    HC->>ID: GetRolesAsync()
    ID-->>HC: User roles
    
    alt Patient Role
        HC-->>B: Redirect to /Patient/MyReservations
        B->>S: GET /Patient/MyReservations
        S->>PC: PatientController.MyReservations()
        PC->>ID: GetUserAsync()
        PC->>DB: Get patient profile & reservations
        DB-->>PC: Patient data with reservations
        PC-->>B: Render MyReservations view
        B->>U: Show patient dashboard
        
    else Doctor Role
        HC-->>B: Redirect to /Doctor/MyProfile
        B->>S: GET /Doctor/MyProfile
        S->>DC: DoctorController.MyProfile()
        DC->>ID: GetUserAsync()
        DC->>DB: Get doctor profile
        DB-->>DC: Doctor data
        DC-->>B: Render MyProfile view
        B->>U: Show doctor dashboard
        
    else Admin Role
        HC->>DB: Get statistics (patients, doctors, rooms, beds)
        HC->>DB: Get reservation data
        HC->>DB: Sync bed occupancy status
        DB-->>HC: Dashboard statistics
        HC-->>B: Render admin dashboard
        B->>U: Show admin dashboard with analytics
    end
```

## 2. Hasta Profil Oluşturma

```mermaid
sequenceDiagram
    participant P as "Patient User"
    participant B as "Browser"
    participant S as "ASP.NET Core Server"
    participant A as "Authorization"
    participant PC as "PatientController"
    participant DB as "PostgreSQL Database"
    
    Note over P,DB: PATIENT PROFILE CREATION
    
    P->>B: First login as Patient
    B->>S: GET /Patient/MyReservations
    S->>A: Check Patient role
    A-->>S: Authorized
    S->>PC: PatientController.MyReservations()
    PC->>DB: Find patient by UserId
    
    alt Patient profile doesn't exist
        DB-->>PC: No patient found
        PC-->>B: Redirect to /Patient/CreateProfile
        B->>S: GET /Patient/CreateProfile
        S->>PC: PatientController.CreateProfile()
        PC-->>B: Render CreateProfile form
        B->>P: Show profile creation form
        
        P->>B: Submit profile data
        B->>S: POST /Patient/CreateProfile
        S->>PC: PatientController.CreateProfile(patient)
        PC->>PC: Set UserId = currentUser.Id
        PC->>PC: Convert DateTime to UTC
        PC->>DB: Insert new patient record
        DB-->>PC: Patient created
        PC-->>B: Redirect to /Patient/MyReservations
        
    else Patient profile exists
        DB-->>PC: Patient data with reservations
        PC-->>B: Render MyReservations view
    end
    
    B->>P: Show patient reservations page
```

## 3. Rezervasyon Oluşturma

```mermaid
sequenceDiagram
    participant P as "Patient User"
    participant B as "Browser"
    participant S as "ASP.NET Core Server"
    participant A as "Authorization"
    participant RC as "ReservationController"
    participant DB as "PostgreSQL Database"
    
    Note over P,DB: RESERVATION CREATION FLOW
    
    P->>B: Click "Create Reservation"
    B->>S: GET /Reservation/Create
    S->>A: Check authorization
    A-->>S: Patient role authorized
    S->>RC: ReservationController.Create()
    
    RC->>DB: Check if patient has active reservation
    
    alt Patient has active reservation
        DB-->>RC: Active reservation found
        RC-->>B: Redirect with error message
        B->>P: Show error message
        
    else No active reservation
        DB-->>RC: No active reservation
        RC->>DB: Get available departments
        RC->>DB: Get active doctors
        DB-->>RC: Departments and doctors list
        RC-->>B: Render Create form with dropdowns
        B->>P: Show reservation form
        
        P->>B: Select department, room type, room, bed
        B->>S: AJAX calls for cascade dropdowns
        S->>RC: GetRoomTypes(department)
        RC->>DB: Query rooms by department
        DB-->>RC: Room types
        RC-->>B: JSON response
        
        P->>B: Continue selections
        B->>S: GetRooms(department, roomType)
        S->>RC: Get rooms
        RC->>DB: Query specific rooms
        DB-->>RC: Available rooms
        RC-->>B: JSON response
        
        P->>B: Final bed selection
        B->>S: GetBeds(roomId)
        S->>RC: Get available beds
        RC->>DB: Query beds for room
        DB-->>RC: Available beds
        RC-->>B: JSON response
        
        P->>B: Submit reservation form
        B->>S: POST /Reservation/Create
        S->>RC: ReservationController.Create(reservation)
        
        RC->>RC: Set PatientId from current user
        RC->>RC: Validate form data
        RC->>DB: Check bed availability
        
        alt Bed is available
            DB-->>RC: Bed available
            RC->>DB: Insert reservation
            RC->>DB: Update bed occupancy status
            DB-->>RC: Reservation created
            RC-->>B: Redirect to success page
            B->>P: Show success message
            
        else Bed not available
            DB-->>RC: Bed occupied
            RC-->>B: Return form with error
            B->>P: Show error message
        end
    end
```

## 4. Admin Yönetim İşlemleri

```mermaid
sequenceDiagram
    participant A as "Admin User"
    participant B as "Browser"
    participant S as "ASP.NET Core Server"
    participant Auth as "Authorization"
    participant RC as "ReservationController"
    participant PC as "PatientController"
    participant DC as "DoctorController"
    participant DB as "PostgreSQL Database"
    
    Note over A,DB: ADMIN MANAGEMENT OPERATIONS
    
    A->>B: Access admin dashboard
    B->>S: GET /Home/Index
    S->>Auth: Check Admin role
    Auth-->>S: Admin authorized
    
    par Parallel Statistics Gathering
        S->>DB: Count active reservations
        S->>DB: Count total patients
        S->>DB: Count total doctors
        S->>DB: Count rooms and beds
        S->>DB: Calculate occupancy rate
        S->>DB: Get monthly trends
    end
    
    DB-->>S: All statistics data
    S-->>B: Render admin dashboard
    B->>A: Show comprehensive analytics
    
    A->>B: Navigate to patient management
    B->>S: GET /Patient/Index
    S->>PC: PatientController.Index()
    PC->>DB: Get all patients with users and reservations
    DB-->>PC: Patient list with relationships
    PC-->>B: Render patient index
    B->>A: Show patient list
    
    A->>B: Create new patient
    B->>S: POST /Patient/Create
    S->>PC: PatientController.Create(patient)
    PC->>DB: Insert patient
    DB-->>PC: Patient created
    PC-->>B: Redirect to patient list
    B->>A: Show success message
```

## 5. Doktor İşlemleri

```mermaid
sequenceDiagram
    participant D as "Doctor User"
    participant B as "Browser"
    participant S as "ASP.NET Core Server"
    participant Auth as "Authorization"
    participant DC as "DoctorController"
    participant MRC as "MedicalRecordController"
    participant PRC as "PrescriptionController"
    participant DB as "PostgreSQL Database"
    
    Note over D,DB: DOCTOR OPERATIONS
    
    D->>B: First login as Doctor
    B->>S: GET /Doctor/MyProfile
    S->>DC: DoctorController.MyProfile()
    DC->>DB: Find doctor by UserId
    
    alt Doctor profile doesn't exist
        DB-->>DC: No doctor found
        DC-->>B: Redirect to /Doctor/CreateProfile
        B->>D: Show doctor profile creation form
        D->>B: Submit profile
        B->>S: POST /Doctor/CreateProfile
        S->>DC: DoctorController.CreateProfile(doctor)
        DC->>DB: Insert new doctor record
        DB-->>DC: Doctor profile created
        
    else Doctor profile exists
        DB-->>DC: Doctor data
        DC-->>B: Render MyProfile view
    end
    
    D->>B: Create medical record
    B->>S: GET /MedicalRecord/Create
    S->>MRC: MedicalRecordController.Create()
    MRC->>DB: Get patients and doctors list
    DB-->>MRC: Dropdown data
    MRC-->>B: Render create form
    
    D->>B: Submit medical record
    B->>S: POST /MedicalRecord/Create
    S->>MRC: MedicalRecordController.Create(record)
    MRC->>DB: Insert medical record
    DB-->>MRC: Record created
    
    D->>B: Create prescription
    B->>S: GET /Prescription/Create
    S->>PRC: PrescriptionController.Create()
    PRC->>DB: Get patients and medications
    DB-->>PRC: Data for prescription
    PRC-->>B: Render prescription form
    
    D->>B: Submit prescription
    B->>S: POST /Prescription/Create
    S->>PRC: PrescriptionController.Create(prescription)
    PRC->>DB: Insert prescription with medications
    DB-->>PRC: Prescription created
```

## 6. Hasta Self-Servis

```mermaid
sequenceDiagram
    participant P as "Patient User"
    participant B as "Browser"
    participant S as "ASP.NET Core Server"
    participant Auth as "Authorization"
    participant PC as "PatientController"
    participant RC as "ReservationController"
    participant DB as "PostgreSQL Database"
    
    Note over P,DB: PATIENT SELF-SERVICE
    
    P->>B: View my medical records
    B->>S: GET /Patient/MyMedicalRecords
    S->>PC: PatientController.MyMedicalRecords()
    PC->>DB: Get patient medical records by UserId
    DB-->>PC: Patient's medical records
    PC-->>B: Render patient medical records
    B->>P: Show my medical records
    
    P->>B: View my lab results
    B->>S: GET /Patient/MyLabResults
    S->>PC: PatientController.MyLabResults()
    PC->>DB: Get patient lab results by UserId
    DB-->>PC: Patient's lab results
    PC-->>B: Render patient lab results
    B->>P: Show my lab results
    
    P->>B: View my prescriptions
    B->>S: GET /Patient/MyPrescriptions
    S->>PC: PatientController.MyPrescriptions()
    PC->>DB: Get patient prescriptions by UserId
    DB-->>PC: Patient's prescriptions
    PC-->>B: Render patient prescriptions
    B->>P: Show my prescriptions
    
    P->>B: Cancel reservation
    B->>S: POST /Reservation/Cancel
    S->>RC: ReservationController.Cancel(id, reason)
    RC->>DB: Update reservation status to "Cancelled"
    RC->>DB: Update bed occupancy status
    DB-->>RC: Reservation cancelled
    RC-->>B: Redirect with success message
    B->>P: Show cancellation confirmation
    
    P->>B: Edit my profile
    B->>S: GET /Patient/EditProfile
    S->>PC: PatientController.EditProfile()
    PC->>DB: Get current patient data by UserId
    DB-->>PC: Patient profile data
    PC-->>B: Render edit profile form
    
    P->>B: Submit profile changes
    B->>S: POST /Patient/EditProfile
    S->>PC: PatientController.EditProfile(patient)
    PC->>DB: Update patient profile
    DB-->>PC: Profile updated
    PC-->>B: Redirect to /Patient/MyProfile
    B->>P: Show updated profile
```

---

## Sistem Mimarisi Özeti

### Teknoloji Stack:
- **Framework**: ASP.NET Core 9 MVC
- **Database**: PostgreSQL + Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **UI**: Bootstrap 5 + Modern CSS

### Ana Roller:
- **Admin**: Tam sistem yönetimi
- **Doctor**: Tıbbi işlemler ve hasta yönetimi
- **Patient**: Self-servis ve rezervasyon yönetimi

### Güvenlik:
- Role-based authorization
- User-specific data access
- Authentication middleware
- Data validation

Bu diagramlar projenin tüm ana akışlarını göstermektedir. 