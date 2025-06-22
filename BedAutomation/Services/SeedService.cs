using Microsoft.AspNetCore.Identity;
using BedAutomation.Data;
using BedAutomation.Models;
using Microsoft.EntityFrameworkCore;

namespace BedAutomation.Services
{
    public class SeedService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public SeedService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Create roles
            await CreateRoleAsync("Admin");
            await CreateRoleAsync("Patient");
            await CreateRoleAsync("Doctor");

            // Seed basic lookup data
            await SeedVitalSignTypesAsync();
            await SeedMedicationsAsync();
            await SeedLabTestsAsync();
            await SeedRoomsAndBedsAsync();

            // Create users and related entities
            await CreateUsersAsync();
            await SeedPatientsAsync();
            await SeedDoctorsAsync();

            // Seed transactional data
            await SeedReservationsAsync();
            await SeedMedicalRecordsAsync();
            await SeedVitalSignsAsync();
            await SeedPrescriptionsAsync();
            await SeedLabResultsAsync();
        }

        private async Task CreateRoleAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private async Task CreateUsersAsync()
        {
            // Admin user
            await CreateUserAsync("admin@hospital.com", "Admin123!", "Admin");

            // Doctor users
            await CreateUserAsync("dr.mehmet@hospital.com", "Doctor123!", "Doctor");
            await CreateUserAsync("dr.ayse@hospital.com", "Doctor123!", "Doctor");

            // Patient users
            await CreateUserAsync("patient1@email.com", "Patient123!", "Patient");
            await CreateUserAsync("patient2@email.com", "Patient123!", "Patient");
            await CreateUserAsync("patient3@email.com", "Patient123!", "Patient");
            await CreateUserAsync("patient4@email.com", "Patient123!", "Patient");
        }

        private async Task CreateUserAsync(string email, string password, string role)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
        }

        private async Task SeedRoomsAndBedsAsync()
        {
            if (!_context.Rooms.Any())
            {
                var rooms = new List<Room>
                {
                    new Room { RoomNumber = "101", RoomType = "Standard", Department = "Cardiology", BedCapacity = 2, Floor = 1, IsActive = true },
                    new Room { RoomNumber = "102", RoomType = "Standard", Department = "Cardiology", BedCapacity = 2, Floor = 1, IsActive = true },
                    new Room { RoomNumber = "103", RoomType = "VIP", Department = "Cardiology", BedCapacity = 1, Floor = 1, IsActive = true },
                    new Room { RoomNumber = "201", RoomType = "Standard", Department = "Neurology", BedCapacity = 2, Floor = 2, IsActive = true },
                    new Room { RoomNumber = "202", RoomType = "Standard", Department = "Neurology", BedCapacity = 2, Floor = 2, IsActive = true },
                    new Room { RoomNumber = "203", RoomType = "ICU", Department = "Emergency", BedCapacity = 1, Floor = 2, IsActive = true },
                    new Room { RoomNumber = "301", RoomType = "Standard", Department = "Orthopedics", BedCapacity = 2, Floor = 3, IsActive = true },
                    new Room { RoomNumber = "302", RoomType = "Standard", Department = "Orthopedics", BedCapacity = 2, Floor = 3, IsActive = true }
                };

                _context.Rooms.AddRange(rooms);
                await _context.SaveChangesAsync();

                // Create beds for each room
                var beds = new List<Bed>();
                foreach (var room in rooms)
                {
                    for (int i = 1; i <= room.BedCapacity; i++)
                    {
                        beds.Add(new Bed
                        {
                            RoomId = room.Id,
                            BedNumber = i.ToString(),
                            BedType = room.RoomType == "ICU" ? "ICU" : room.RoomType == "VIP" ? "VIP" : "Standard",
                            IsOccupied = false,
                            IsActive = true
                        });
                    }
                }

                _context.Beds.AddRange(beds);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedPatientsAsync()
        {
            if (!_context.Patients.Any())
            {
                var patient1User = await _userManager.FindByEmailAsync("patient1@email.com");
                var patient2User = await _userManager.FindByEmailAsync("patient2@email.com");
                var patient3User = await _userManager.FindByEmailAsync("patient3@email.com");
                var patient4User = await _userManager.FindByEmailAsync("patient4@email.com");

                var patients = new List<Patient>
                {
                    new Patient
                    {
                        UserId = patient1User?.Id,
                        FirstName = "Ahmet",
                        LastName = "Yılmaz",
                        DateOfBirth = DateTime.SpecifyKind(new DateTime(1985, 5, 15), DateTimeKind.Utc),
                        Gender = "Male",
                        IdentityNumber = "12345678901",
                        PhoneNumber = "+905321234567",
                        Email = "patient1@email.com",
                        Address = "Atatürk Cad. No:123 Kadıköy/İstanbul"
                    },
                    new Patient
                    {
                        UserId = patient2User?.Id,
                        FirstName = "Elif",
                        LastName = "Kaya",
                        DateOfBirth = DateTime.SpecifyKind(new DateTime(1992, 8, 22), DateTimeKind.Utc),
                        Gender = "Female",
                        IdentityNumber = "98765432109",
                        PhoneNumber = "+905339876543",
                        Email = "patient2@email.com",
                        Address = "İnönü Mah. Barış Sok. No:45 Beşiktaş/İstanbul"
                    },
                    new Patient
                    {
                        UserId = patient3User?.Id,
                        FirstName = "Mustafa",
                        LastName = "Demir",
                        DateOfBirth = DateTime.SpecifyKind(new DateTime(1978, 3, 10), DateTimeKind.Utc),
                        Gender = "Male",
                        IdentityNumber = "55443322110",
                        PhoneNumber = "+905345554433",
                        Email = "patient3@email.com",
                        Address = "Cumhuriyet Mah. Özgürlük Cad. No:78 Şişli/İstanbul"
                    },
                    new Patient
                    {
                        UserId = patient4User?.Id,
                        FirstName = "Zehra",
                        LastName = "Şahin",
                        DateOfBirth = DateTime.SpecifyKind(new DateTime(1996, 11, 5), DateTimeKind.Utc),
                        Gender = "Female",
                        IdentityNumber = "11223344556",
                        PhoneNumber = "+905351112233",
                        Email = "patient4@email.com",
                        Address = "Fenerbahçe Mah. Deniz Sok. No:12 Kadıköy/İstanbul"
                    }
                };

                _context.Patients.AddRange(patients);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedDoctorsAsync()
        {
            if (!_context.Doctors.Any())
            {
                var doctor1User = await _userManager.FindByEmailAsync("dr.mehmet@hospital.com");
                var doctor2User = await _userManager.FindByEmailAsync("dr.ayse@hospital.com");

                var doctors = new List<Doctor>
                {
                    new Doctor
                    {
                        UserId = doctor1User?.Id,
                        FirstName = "Mehmet",
                        LastName = "Özkan",
                        Email = "dr.mehmet@hospital.com",
                        PhoneNumber = "+905329998877",
                        IdentityNumber = "12312312312",
                        Specialization = "Cardiology",
                        Department = "Cardiology",
                        LicenseNumber = "DOC123456",
                        YearsOfExperience = 15,
                        Biography = "15 years of experience in interventional cardiology with expertise in cardiac catheterization and angioplasty.",
                        IsActive = true
                    },
                    new Doctor
                    {
                        UserId = doctor2User?.Id,
                        FirstName = "Ayşe",
                        LastName = "Yıldırım",
                        Email = "dr.ayse@hospital.com",
                        PhoneNumber = "+905337776655",
                        IdentityNumber = "32132132132",
                        Specialization = "Neurology",
                        Department = "Neurology",
                        LicenseNumber = "DOC789012",
                        YearsOfExperience = 12,
                        Biography = "12 years of experience in neurological disorders with special interest in epilepsy and movement disorders.",
                        IsActive = true
                    }
                };

                _context.Doctors.AddRange(doctors);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedVitalSignTypesAsync()
        {
            if (!_context.VitalSignTypes.Any())
            {
                var vitalSignTypes = new List<VitalSignType>
                {
                    new VitalSignType { Name = "Body Temperature", Unit = "°C", Category = "Basic", Description = "Core body temperature", MinNormalValue = 36.1m, MaxNormalValue = 37.2m, CriticalMinValue = 35.0m, CriticalMaxValue = 40.0m },
                    new VitalSignType { Name = "Heart Rate", Unit = "bpm", Category = "Cardiac", Description = "Heart beats per minute", MinNormalValue = 60, MaxNormalValue = 100, CriticalMinValue = 40, CriticalMaxValue = 150 },
                    new VitalSignType { Name = "Blood Pressure", Unit = "mmHg", Category = "Cardiac", Description = "Systolic pressure", MinNormalValue = 90, MaxNormalValue = 140 },
                    new VitalSignType { Name = "Respiratory Rate", Unit = "bpm", Category = "Respiratory", Description = "Breaths per minute", MinNormalValue = 12, MaxNormalValue = 20, CriticalMinValue = 8, CriticalMaxValue = 30 },
                    new VitalSignType { Name = "Oxygen Saturation", Unit = "%", Category = "Respiratory", Description = "Blood oxygen saturation", MinNormalValue = 95, MaxNormalValue = 100, CriticalMinValue = 85, CriticalMaxValue = 100 },
                    new VitalSignType { Name = "Weight", Unit = "kg", Category = "Basic", Description = "Body weight", MinNormalValue = 0, MaxNormalValue = 300 },
                    new VitalSignType { Name = "Height", Unit = "cm", Category = "Basic", Description = "Body height", MinNormalValue = 0, MaxNormalValue = 250 }
                };

                _context.VitalSignTypes.AddRange(vitalSignTypes);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedMedicationsAsync()
        {
            if (!_context.Medications.Any())
            {
                var medications = new List<Medication>
                {
                    new Medication { Name = "Paracetamol", GenericName = "Acetaminophen", Strength = "500mg", Form = "Tablet", Category = "Analgesic", Manufacturer = "Generic" },
                    new Medication { Name = "Ibuprofen", GenericName = "Ibuprofen", Strength = "400mg", Form = "Tablet", Category = "NSAID", Manufacturer = "Generic" },
                    new Medication { Name = "Amoxicillin", GenericName = "Amoxicillin", Strength = "500mg", Form = "Capsule", Category = "Antibiotic", Manufacturer = "Generic" },
                    new Medication { Name = "Aspirin", GenericName = "Acetylsalicylic Acid", Strength = "100mg", Form = "Tablet", Category = "Antiplatelet", Manufacturer = "Generic" },
                    new Medication { Name = "Metformin", GenericName = "Metformin HCl", Strength = "500mg", Form = "Tablet", Category = "Antidiabetic", Manufacturer = "Generic" },
                    new Medication { Name = "Atorvastatin", GenericName = "Atorvastatin", Strength = "20mg", Form = "Tablet", Category = "Statin", Manufacturer = "Generic" },
                    new Medication { Name = "Lisinopril", GenericName = "Lisinopril", Strength = "10mg", Form = "Tablet", Category = "ACE Inhibitor", Manufacturer = "Generic" },
                    new Medication { Name = "Omeprazole", GenericName = "Omeprazole", Strength = "20mg", Form = "Capsule", Category = "PPI", Manufacturer = "Generic" }
                };

                _context.Medications.AddRange(medications);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedLabTestsAsync()
        {
            if (!_context.LabTests.Any())
            {
                var labTests = new List<LabTest>
                {
                    new LabTest { Name = "Complete Blood Count", Code = "CBC", Description = "Full blood count analysis", Category = "Hematology", SampleType = "Blood", EstimatedTAT = 2 },
                    new LabTest { Name = "Basic Metabolic Panel", Code = "BMP", Description = "Basic chemistry panel", Category = "Chemistry", SampleType = "Blood", EstimatedTAT = 4 },
                    new LabTest { Name = "Lipid Panel", Code = "LIPID", Description = "Cholesterol and triglycerides", Category = "Chemistry", SampleType = "Blood", EstimatedTAT = 6 },
                    new LabTest { Name = "Liver Function Tests", Code = "LFT", Description = "Liver enzyme tests", Category = "Chemistry", SampleType = "Blood", EstimatedTAT = 4 },
                    new LabTest { Name = "Urinalysis", Code = "UA", Description = "Urine analysis", Category = "Urinalysis", SampleType = "Urine", EstimatedTAT = 2 },
                    new LabTest { Name = "Thyroid Function", Code = "TFT", Description = "TSH, T3, T4", Category = "Endocrinology", SampleType = "Blood", EstimatedTAT = 8 }
                };

                _context.LabTests.AddRange(labTests);
                await _context.SaveChangesAsync();

                await SeedLabParametersAsync();
            }
        }

        private async Task SeedLabParametersAsync()
        {
            if (!_context.LabParameters.Any())
            {
                var cbcTest = _context.LabTests.First(t => t.Code == "CBC");
                var bmpTest = _context.LabTests.First(t => t.Code == "BMP");
                var lipidTest = _context.LabTests.First(t => t.Code == "LIPID");

                var labParameters = new List<LabParameter>
                {
                    // CBC Parameters
                    new LabParameter { LabTestId = cbcTest.Id, Name = "White Blood Cells", Code = "WBC", Unit = "10³/µL", DataType = "Numeric" },
                    new LabParameter { LabTestId = cbcTest.Id, Name = "Red Blood Cells", Code = "RBC", Unit = "10⁶/µL", DataType = "Numeric" },
                    new LabParameter { LabTestId = cbcTest.Id, Name = "Hemoglobin", Code = "HGB", Unit = "g/dL", DataType = "Numeric" },
                    new LabParameter { LabTestId = cbcTest.Id, Name = "Platelets", Code = "PLT", Unit = "10³/µL", DataType = "Numeric" },

                    // BMP Parameters
                    new LabParameter { LabTestId = bmpTest.Id, Name = "Glucose", Code = "GLU", Unit = "mg/dL", DataType = "Numeric" },
                    new LabParameter { LabTestId = bmpTest.Id, Name = "Creatinine", Code = "CREA", Unit = "mg/dL", DataType = "Numeric" },
                    new LabParameter { LabTestId = bmpTest.Id, Name = "Sodium", Code = "NA", Unit = "mmol/L", DataType = "Numeric" },
                    new LabParameter { LabTestId = bmpTest.Id, Name = "Potassium", Code = "K", Unit = "mmol/L", DataType = "Numeric" },

                    // Lipid Parameters
                    new LabParameter { LabTestId = lipidTest.Id, Name = "Total Cholesterol", Code = "CHOL", Unit = "mg/dL", DataType = "Numeric" },
                    new LabParameter { LabTestId = lipidTest.Id, Name = "HDL Cholesterol", Code = "HDL", Unit = "mg/dL", DataType = "Numeric" },
                    new LabParameter { LabTestId = lipidTest.Id, Name = "LDL Cholesterol", Code = "LDL", Unit = "mg/dL", DataType = "Numeric" },
                    new LabParameter { LabTestId = lipidTest.Id, Name = "Triglycerides", Code = "TRIG", Unit = "mg/dL", DataType = "Numeric" }
                };

                _context.LabParameters.AddRange(labParameters);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedReservationsAsync()
        {
            if (!_context.Reservations.Any())
            {
                var patients = await _context.Patients.ToListAsync();
                var beds = await _context.Beds.ToListAsync();

                if (patients.Count >= 3 && beds.Count >= 6)
                {
                    var reservations = new List<Reservation>
                    {
                        new Reservation
                        {
                            PatientId = patients[0].Id,
                            BedId = beds[0].Id,
                            CheckInDate = DateTime.UtcNow.AddDays(-5),
                            CheckOutDate = DateTime.UtcNow.AddDays(2),
                            Status = "Active",
                            AdmissionType = "Scheduled",
                            MedicalNotes = "Chest pain evaluation and cardiac monitoring. Patient stable, monitoring vital signs regularly"
                        },
                        new Reservation
                        {
                            PatientId = patients[1].Id,
                            BedId = beds[3].Id,
                            CheckInDate = DateTime.UtcNow.AddDays(-3),
                            CheckOutDate = DateTime.UtcNow.AddDays(1),
                            Status = "Active",
                            AdmissionType = "Scheduled",
                            MedicalNotes = "Routine check-up and monitoring. Patient responding well to treatment"
                        },
                        new Reservation
                        {
                            PatientId = patients[2].Id,
                            BedId = beds[5].Id,
                            CheckInDate = DateTime.UtcNow.AddDays(-10),
                            CheckOutDate = DateTime.UtcNow.AddDays(-3),
                            Status = "Completed",
                            AdmissionType = "Emergency",
                            MedicalNotes = "Blood pressure management. Treatment completed successfully, patient discharged"
                        }
                    };

                    _context.Reservations.AddRange(reservations);
                    await _context.SaveChangesAsync();

                    // Update bed statuses
                    beds[0].IsOccupied = true;
                    beds[3].IsOccupied = true;
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task SeedMedicalRecordsAsync()
        {
            if (!_context.MedicalRecords.Any())
            {
                var patients = await _context.Patients.ToListAsync();
                var doctors = await _context.Doctors.ToListAsync();

                if (patients.Count >= 3 && doctors.Count >= 1)
                {
                    var medicalRecords = new List<MedicalRecord>
                    {
                        new MedicalRecord
                        {
                            PatientId = patients[0].Id,
                            DoctorId = doctors[0].Id,
                            RecordDate = DateTime.UtcNow.AddDays(-4),
                            RecordType = "Consultation",
                            ChiefComplaint = "Chest pain and shortness of breath",
                            HistoryOfPresentIllness = "Central chest pain, dyspnea on exertion, fatigue",
                            PhysicalExamination = "BP: 140/90, HR: 85, RR: 18, Temp: 36.8°C. Heart sounds normal, lungs clear.",
                            Assessment = "Possible angina pectoris, requires further cardiac evaluation",
                            Plan = "ECG, Cardiac enzymes, Echocardiogram, Start aspirin 81mg daily",
                            Status = "Active",
                            Department = "Cardiology"
                        },
                        new MedicalRecord
                        {
                            PatientId = patients[1].Id,
                            DoctorId = doctors.Count > 1 ? doctors[1].Id : doctors[0].Id,
                            RecordDate = DateTime.UtcNow.AddDays(-2),
                            RecordType = "Consultation",
                            ChiefComplaint = "Headache and dizziness",
                            HistoryOfPresentIllness = "Recurring headaches, episodes of dizziness, mild nausea",
                            PhysicalExamination = "BP: 120/80, HR: 72, Neurological exam normal, no focal deficits",
                            Assessment = "Tension headache, possible migraine",
                            Plan = "MRI brain, start preventive medication, lifestyle modifications",
                            Status = "Active",
                            Department = "Neurology"
                        }
                    };

                    _context.MedicalRecords.AddRange(medicalRecords);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task SeedVitalSignsAsync()
        {
            if (!_context.VitalSigns.Any())
            {
                var patients = await _context.Patients.ToListAsync();
                var vitalSignTypes = await _context.VitalSignTypes.ToListAsync();

                var vitalSigns = new List<VitalSign>();
                var random = new Random();

                foreach (var patient in patients.Take(3)) // First 3 patients
                {
                    for (int day = -7; day <= 0; day++)
                    {
                        var measurementDate = DateTime.UtcNow.AddDays(day);

                        // Temperature
                        var tempType = vitalSignTypes.FirstOrDefault(v => v.Name == "Body Temperature");
                        if (tempType != null)
                        {
                            vitalSigns.Add(new VitalSign
                            {
                                PatientId = patient.Id,
                                VitalSignTypeId = tempType.Id,
                                Value = Math.Round((decimal)(36.2 + random.NextDouble() * 1.5), 1),
                                MeasurementDate = measurementDate,
                                Status = "Normal",
                                Notes = "Regular monitoring"
                            });
                        }

                        // Heart Rate
                        var hrType = vitalSignTypes.FirstOrDefault(v => v.Name == "Heart Rate");
                        if (hrType != null)
                        {
                            var hrValue = 65 + random.Next(0, 30);
                            vitalSigns.Add(new VitalSign
                            {
                                PatientId = patient.Id,
                                VitalSignTypeId = hrType.Id,
                                Value = hrValue,
                                MeasurementDate = measurementDate,
                                Status = hrValue > 100 ? "Abnormal" : "Normal",
                                Notes = "Regular monitoring"
                            });
                        }
                    }
                }

                _context.VitalSigns.AddRange(vitalSigns);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedPrescriptionsAsync()
        {
            if (!_context.Prescriptions.Any())
            {
                var patients = await _context.Patients.ToListAsync();
                var doctors = await _context.Doctors.ToListAsync();
                var medications = await _context.Medications.ToListAsync();

                if (patients.Count >= 2 && doctors.Count >= 1 && medications.Count >= 4)
                {
                    var prescriptions = new List<Prescription>
                    {
                        new Prescription
                        {
                            PatientId = patients[0].Id,
                            DoctorId = doctors[0].Id,
                            PrescriptionDate = DateTime.UtcNow.AddDays(-3),
                            ExpiryDate = DateTime.UtcNow.AddDays(27),
                            Status = "Active",
                            Notes = "Take with food to avoid stomach upset",
                            Instructions = "Follow dosage instructions carefully"
                        },
                        new Prescription
                        {
                            PatientId = patients[1].Id,
                            DoctorId = doctors.Count > 1 ? doctors[1].Id : doctors[0].Id,
                            PrescriptionDate = DateTime.UtcNow.AddDays(-1),
                            ExpiryDate = DateTime.UtcNow.AddDays(29),
                            Status = "Active",
                            Notes = "Monitor for side effects",
                            Instructions = "Take at the same time each day"
                        }
                    };

                    _context.Prescriptions.AddRange(prescriptions);
                    await _context.SaveChangesAsync();

                    // Add prescription medications
                    var prescriptionMedications = new List<PrescriptionMedication>
                    {
                        new PrescriptionMedication
                        {
                            PrescriptionId = prescriptions[0].Id,
                            MedicationId = medications.First(m => m.Name == "Aspirin").Id,
                            Dosage = "81mg",
                            Frequency = "Once daily",
                            Duration = 30,
                            Instructions = "Take with breakfast",
                            Quantity = 30,
                            StartDate = DateTime.UtcNow.AddDays(-3),
                            EndDate = DateTime.UtcNow.AddDays(27)
                        },
                        new PrescriptionMedication
                        {
                            PrescriptionId = prescriptions[1].Id,
                            MedicationId = medications.First(m => m.Name == "Ibuprofen").Id,
                            Dosage = "400mg",
                            Frequency = "Twice daily",
                            Duration = 14,
                            Instructions = "Take with food",
                            Quantity = 28,
                            StartDate = DateTime.UtcNow.AddDays(-1),
                            EndDate = DateTime.UtcNow.AddDays(13)
                        }
                    };

                    _context.PrescriptionMedications.AddRange(prescriptionMedications);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private async Task SeedLabResultsAsync()
        {
            if (!_context.LabResults.Any())
            {
                var patients = await _context.Patients.ToListAsync();
                var labTests = await _context.LabTests.ToListAsync();
                var labParameters = await _context.LabParameters.ToListAsync();

                if (patients.Count >= 2 && labTests.Count >= 2 && labParameters.Count >= 4)
                {
                    var labResults = new List<LabResult>
                    {
                        new LabResult
                        {
                            PatientId = patients[0].Id,
                            LabTestId = labTests.First(t => t.Code == "CBC").Id,
                            LabParameterId = labParameters.First(p => p.Code == "WBC").Id,
                            TestDate = DateTime.UtcNow.AddDays(-2),
                            ResultDate = DateTime.UtcNow.AddDays(-2),
                            Status = "Normal",
                            NumericValue = 7.2m,
                            Notes = "Fasting sample collected",
                            TechnicianName = "Lab Tech 1",
                            DoctorName = "Dr. Mehmet Özkan",
                            IsVerified = true
                        },
                        new LabResult
                        {
                            PatientId = patients[1].Id,
                            LabTestId = labTests.First(t => t.Code == "BMP").Id,
                            LabParameterId = labParameters.First(p => p.Code == "GLU").Id,
                            TestDate = DateTime.UtcNow.AddDays(-1),
                            ResultDate = DateTime.UtcNow.AddDays(-1),
                            Status = "Normal",
                            NumericValue = 95m,
                            Notes = "Regular monitoring",
                            TechnicianName = "Lab Tech 2",
                            DoctorName = "Dr. Ayşe Yıldırım",
                            IsVerified = true
                        }
                    };

                    _context.LabResults.AddRange(labResults);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}