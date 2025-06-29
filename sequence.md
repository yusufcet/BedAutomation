# BedAutomation - Sequence Diagramları

Bu dosya BedAutomation hastane otomasyon sistemi için detaylı sequence diagramlarını içerir.

## 📋 İçindekiler

1. [Kimlik Doğrulama ve Rol Atama](#1-kimlik-doğrulama-ve-rol-atama)
2. [Hasta Profil Oluşturma](#2-hasta-profil-oluşturma)
3. [Rezervasyon Oluşturma](#3-rezervasyon-oluşturma)
4. [Admin Yönetim İşlemleri](#4-admin-yönetim-işlemleri)
5. [Doktor İşlemleri](#5-doktor-işlemleri)
6. [Hasta Self-Servis](#6-hasta-self-servis)

---

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

---

## 2. Hasta Profil Oluşturma

```mermaid
sequenceDiagram
    participant P as "Patient User"
    participant B as "Browser"
    participant S as "ASP.NET Core Server"
    participant A as "Authorization"
    participant PC as "PatientController"
    participant RC as "ReservationController"
    participant DB as "PostgreSQL Database"
    
    Note over P,DB: 3. PATIENT PROFILE CREATION
    
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
        
        P->>B: Submit profile data (name, TC, phone, etc.)
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

---

## 3. Rezervasyon Oluşturma

```mermaid
sequenceDiagram
    participant P as "Patient User"
    participant B as "Browser"
    participant S as "ASP.NET Core Server"
    participant A as "Authorization"
    participant RC as "ReservationController"
    participant DB as "PostgreSQL Database"
    
    Note over P,DB: 4. RESERVATION CREATION FLOW
    
    P->>B: Click "Create Reservation"
    B->>S: GET /Reservation/Create
    S->>A: Check authorization
    A-->>S: Patient role authorized
    S->>RC: ReservationController.Create()
    
    RC->>DB: Check if patient has active reservation
    
    alt Patient has active reservation
        DB-->>RC: Active reservation found
        RC-->>B: Redirect with error "You already have an active reservation"
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
            B->>P: Show "Bed no longer available" error
        end
    end
```

---

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
    participant MRC as "MedicalRecordController"
    participant DB as "PostgreSQL Database"
    
    Note over A,DB: 5. ADMIN MANAGEMENT OPERATIONS
    
    A->>B: Access admin dashboard
    B->>S: GET /Home/Index
    S->>Auth: Check Admin role
    Auth-->>S: Admin authorized
    S->>RC: Get reservation statistics
    S->>PC: Get patient count
    S->>DC: Get doctor count
    S->>DB: Get room/bed data
    S->>DB: Sync bed occupancy with reservations
    
    par Parallel Statistics Gathering
        RC->>DB: Count active reservations
        PC->>DB: Count total patients
        DC->>DB: Count total doctors
        S->>DB: Count rooms and beds
        S->>DB: Calculate occupancy rate
        S->>DB: Get monthly trends
    end
    
    DB-->>S: All statistics data
    S-->>B: Render admin dashboard
    B->>A: Show comprehensive analytics
    
    Note over A,DB: 6. PATIENT MANAGEMENT BY ADMIN
    
    A->>B: Navigate to patient management
    B->>S: GET /Patient/Index
    S->>Auth: Check Admin role
    Auth-->>S: Admin authorized
    S->>PC: PatientController.Index()
    PC->>DB: Get all patients with users and reservations
    DB-->>PC: Patient list with relationships
    PC-->>B: Render patient index
    B->>A: Show patient list
    
    A->>B: Create new patient
    B->>S: GET /Patient/Create
    S->>PC: PatientController.Create()
    PC-->>B: Render create form
    B->>A: Show patient form
    
    A->>B: Submit patient data
    B->>S: POST /Patient/Create
    S->>PC: PatientController.Create(patient)
    PC->>PC: Convert DateTime to UTC
    PC->>DB: Insert patient
    DB-->>PC: Patient created
    PC-->>B: Redirect to patient list
    B->>A: Show success message
    
    Note over A,DB: 7. RESERVATION MANAGEMENT BY ADMIN
    
    A->>B: View all reservations
    B->>S: GET /Reservation/Index
    S->>Auth: Check Admin role
    Auth-->>S: Admin authorized
    S->>RC: ReservationController.Index()
    RC->>DB: Get all reservations with patients, beds, rooms
    DB-->>RC: Complete reservation data
    RC-->>B: Render reservation index
    B->>A: Show all reservations
    
    A->>B: Edit reservation
    B->>S: GET /Reservation/Edit/id
    S->>RC: ReservationController.Edit(id)
    RC->>DB: Get reservation details
    DB-->>RC: Reservation data
    RC-->>B: Render edit form
    B->>A: Show edit form
    
    A->>B: Update reservation
    B->>S: POST /Reservation/Edit
    S->>RC: ReservationController.Edit(reservation)
    RC->>DB: Update reservation
    RC->>DB: Update bed occupancy if needed
    DB-->>RC: Update complete
    RC-->>B: Redirect to reservation list
    B->>A: Show updated reservation
```

---

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
    participant LRC as "LabResultController"
    participant VSC as "VitalSignController"
    participant DB as "PostgreSQL Database"
    
    Note over D,DB: 8. DOCTOR PROFILE & MEDICAL OPERATIONS
    
    D->>B: First login as Doctor
    B->>S: GET /Doctor/MyProfile
    S->>Auth: Check Doctor role
    Auth-->>S: Doctor authorized
    S->>DC: DoctorController.MyProfile()
    DC->>DB: Find doctor by UserId
    
    alt Doctor profile doesn't exist
        DB-->>DC: No doctor found
        DC-->>B: Redirect to /Doctor/CreateProfile
        B->>S: GET /Doctor/CreateProfile
        S->>DC: DoctorController.CreateProfile()
        DC-->>B: Render CreateProfile form
        B->>D: Show doctor profile creation form
        
        D->>B: Submit profile (license, specialization, etc.)
        B->>S: POST /Doctor/CreateProfile
        S->>DC: DoctorController.CreateProfile(doctor)
        DC->>DB: Check for duplicate license number
        DC->>DB: Insert new doctor record
        DB-->>DC: Doctor profile created
        DC-->>B: Redirect to /Doctor/MyProfile
        
    else Doctor profile exists
        DB-->>DC: Doctor data
        DC-->>B: Render MyProfile view
    end
    
    B->>D: Show doctor dashboard
    
    Note over D,DB: 9. MEDICAL RECORD MANAGEMENT
    
    D->>B: View patient medical records
    B->>S: GET /MedicalRecord/Index
    S->>Auth: Check Doctor/Admin role
    Auth-->>S: Authorized
    S->>MRC: MedicalRecordController.Index()
    MRC->>DB: Get medical records with patients and doctors
    DB-->>MRC: Medical records data
    MRC-->>B: Render medical records list
    B->>D: Show medical records
    
    D->>B: Create new medical record
    B->>S: GET /MedicalRecord/Create
    S->>MRC: MedicalRecordController.Create()
    MRC->>DB: Get patients list
    MRC->>DB: Get doctors list
    DB-->>MRC: Dropdown data
    MRC-->>B: Render create form
    B->>D: Show medical record form
    
    D->>B: Submit medical record
    B->>S: POST /MedicalRecord/Create
    S->>MRC: MedicalRecordController.Create(record)
    MRC->>MRC: Convert DateTime to UTC
    MRC->>DB: Insert medical record
    DB-->>MRC: Record created
    MRC-->>B: Redirect to records list
    B->>D: Show success message
    
    Note over D,DB: 10. PRESCRIPTION MANAGEMENT
    
    D->>B: Create prescription
    B->>S: GET /Prescription/Create
    S->>Auth: Check Doctor/Admin role
    Auth-->>S: Authorized
    S->>PRC: PrescriptionController.Create()
    PRC->>DB: Get patients and medications
    DB-->>PRC: Patients and medications list
    PRC-->>B: Render prescription form
    B->>D: Show prescription form
    
    D->>B: Submit prescription with medications
    B->>S: POST /Prescription/Create
    S->>PRC: PrescriptionController.Create(prescription)
    
    PRC->>DB: Start transaction
    PRC->>DB: Insert prescription
    PRC->>DB: Insert prescription medications
    DB-->>PRC: Prescription created with medications
    PRC-->>B: Redirect to prescriptions list
    B->>D: Show prescription created
    
    Note over D,DB: 11. LAB RESULTS & VITAL SIGNS
    
    D->>B: Add lab results
    B->>S: GET /LabResult/Create
    S->>LRC: LabResultController.Create()
    LRC->>DB: Get patients, lab tests, parameters
    DB-->>LRC: Lab data
    LRC-->>B: Render lab result form
    B->>D: Show lab form
    
    D->>B: Submit lab results
    B->>S: POST /LabResult/Create
    S->>LRC: LabResultController.Create(labResult)
    LRC->>DB: Insert lab result
    DB-->>LRC: Lab result saved
    LRC-->>B: Redirect to lab results
    B->>D: Show lab result created
    
    D->>B: Add vital signs
    B->>S: GET /VitalSign/Create
    S->>VSC: VitalSignController.Create()
    VSC->>DB: Get patients and vital sign types
    DB-->>VSC: Vital signs data
    VSC-->>B: Render vital signs form
    B->>D: Show vital signs form
    
    D->>B: Submit vital signs
    B->>S: POST /VitalSign/Create
    S->>VSC: VitalSignController.Create(vitalSign)
    VSC->>DB: Insert vital sign
    DB-->>VSC: Vital sign saved
    VSC-->>B: Redirect to vital signs list
    B->>D: Show vital sign created
```

---

## 6. Hasta Self-Servis

```mermaid
sequenceDiagram
    participant P as "Patient User"
    participant B as "Browser"
    participant S as "ASP.NET Core Server"
    participant Auth as "Authorization"
    participant PC as "PatientController"
    participant RC as "ReservationController"
    participant MRC as "MedicalRecordController"
    participant LRC as "LabResultController"
    participant VSC as "VitalSignController"
    participant PRC as "PrescriptionController"
    participant DB as "PostgreSQL Database"
    
    Note over P,DB: 12. PATIENT SELF-SERVICE OPERATIONS
    
    P->>B: View my medical records
    B->>S: GET /Patient/MyMedicalRecords
    S->>Auth: Check Patient role
    Auth-->>S: Patient authorized
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
    
    P->>B: View my vital signs
    B->>S: GET /Patient/MyVitalSigns
    S->>PC: PatientController.MyVitalSigns()
    PC->>DB: Get patient vital signs by UserId
    DB-->>PC: Patient's vital signs
    PC-->>B: Render vital signs with charts
    B->>P: Show my vital signs dashboard
    
    P->>B: View my prescriptions
    B->>S: GET /Patient/MyPrescriptions
    S->>PC: PatientController.MyPrescriptions()
    PC->>DB: Get patient prescriptions by UserId
    DB-->>PC: Patient's prescriptions with medications
    PC-->>B: Render patient prescriptions
    B->>P: Show my prescriptions
    
    Note over P,DB: 13. RESERVATION MANAGEMENT BY PATIENT
    
    P->>B: Check reservation status
    B->>S: GET /Patient/MyReservations
    S->>PC: PatientController.MyReservations()
    PC->>DB: Get patient reservations with bed/room info
    DB-->>PC: Patient's reservations
    PC-->>B: Render reservations with status
    B->>P: Show my reservations
    
    P->>B: Cancel reservation
    B->>S: POST /Reservation/Cancel
    S->>Auth: Check Patient owns reservation
    Auth-->>S: Authorization check passed
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
    B->>P: Show editable profile form
    
    P->>B: Submit profile changes
    B->>S: POST /Patient/EditProfile
    S->>PC: PatientController.EditProfile(patient)
    PC->>PC: Validate patient owns this profile
    PC->>DB: Update patient profile
    DB-->>PC: Profile updated
    PC-->>B: Redirect to /Patient/MyProfile
    B->>P: Show updated profile
```

---

## 📊 Sistem Mimarisi Özeti

### **Teknoloji Stack:**
- **Framework**: ASP.NET Core 9 MVC
- **Database**: PostgreSQL + Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **UI**: Bootstrap 5 + Modern CSS + Bootstrap Icons
- **Pattern**: MVC (Model-View-Controller)

### **Güvenlik Katmanları:**
- **Authentication**: Identity middleware
- **Authorization**: Role-based access control (Admin/Patient/Doctor)
- **Data Protection**: User-specific data access kontrolü
- **Validation**: Model validation ve business rules

### **Ana Akışlar:**
1. **Kimlik Doğrulama**: Otomatik rol atama ile
2. **Profil Yönetimi**: Role-specific profile creation
3. **Rezervasyon Sistemi**: Cascading dropdown ve availability check
4. **Admin Dashboard**: Comprehensive analytics ve management
5. **Tıbbi İşlemler**: Medical records, prescriptions, lab results
6. **Self-Service**: Patient portal functionalities

### **Veritabanı İlişkileri:**
- Patient ↔ Reservations (One-to-Many)
- Reservation ↔ Bed (Many-to-One)
- Bed ↔ Room (Many-to-One)
- Doctor ↔ Reservations (One-to-Many)
- Patient ↔ MedicalRecords (One-to-Many)
- MedicalRecord ↔ Diagnoses (One-to-Many)

Bu sequence diagramları projenizin tam işleyişini gösteriyor ve PNG'ye çevirmek için kullanabilirsiniz.
