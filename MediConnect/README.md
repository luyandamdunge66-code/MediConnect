
# 🏥 MediConnect — Modern Clinic Management & EMR System

MediConnect is a full-stack Clinic Management and Electronic Medical Record (EMR) web application built to automate medical appointment scheduling, enforce clinical administrative governance, and provide patients with digital prescriptions and verified doctor reviews.

Built with **ASP.NET Core MVC (C#)**, powered by an enterprise relational **MySQL** database via **Entity Framework Core**.

---

## 🌟 Key Features

### 👤 Patient Portal
- **Self-Service Registration & Cookie Authentication:** Secure account creation with role claims.
- **Verified Doctor Directory:** Browse certified medical specialists, filter specialties, view medical license numbers, and inspect live patient ratings.
- **Conflict-Free Scheduling:** Real-time appointment booking engine with slot collision prevention (no double-booking) and past-date validation.
- **Digital Consultations & Prescription Slips:** Access completed visit records, medical diagnoses, doctor instructions, and printable medical slips.
- **Verified Review Engine:** Rate doctors (1 to 5 stars) and write consultation reviews exclusively after completed visits.

### 👨‍⚕️ Doctor Portal
- **Credentialed Registration:** Doctors register with license numbers and medical specialties (including custom specialties).
- **Administrative Verification Gate:** Accounts remain in `Pending` status until reviewed and approved by clinic administrators.
- **Consultation Queue & Daily Schedule:** Manage incoming patient queues in real time (Accept, Decline, or Complete visits).
- **EMR Diagnosis & Prescriptions:** Complete consultations by recording medical diagnoses and dosage instructions directly into MySQL.
- **Profile Customization:** Upload professional profile photos displayed across the directory.
- **Private Patient Feedback Feed:** Dedicated portal section displaying patient star ratings and written comments.

### 🛡️ Administration Command Center
- **Live Clinic Analytics:** Real-time KPI stat cards tracking registered patients, active doctors, pending applications, and clinic-wide appointments.
- **Medical Credential Verification:** Review doctor licenses, approve genuine medical personnel, or reject fraudulent applications.
- **Staff Lifecycle Management:** Permanent removal and cascade deletion of doctor accounts.
- **Clinic-Wide Audit Feed:** Live table showing all consultation activity across all departments.

---

## 🛠️ Technology Stack

- **Backend Framework:** ASP.NET Core MVC (C# / .NET)
- **Database & ORM:** MySQL Server, Entity Framework Core (Pomelo MySQL Provider)
- **Database Administration:** MySQL Workbench
- **Frontend Architecture:** HTML5, CSS3, Bootstrap 5, Bootstrap Icons
- **Authentication & Security:** Cookie-Based Authentication & Role-Based Access Control (RBAC)

---

## 🗄️ Relational Database Architecture

```text
       ┌──────────────┐
       │    Users     │
       └──────┬───────┘
              │ 1:1
       ┌──────┴───────┐
       │              │
┌──────▼─────┐ ┌──────▼─────┐
│  Patients  │ │  Doctors   │
└──────┬─────┘ └──────┬─────┘
       │ 1:N          │ 1:N
       └──────┬───────┘
              │
       ┌──────▼───────┐
       │ Appointments │
       └──────┬───────┘
              │ 1:1
       ┌──────▼───────┐
       │   Reviews    │
       └──────────────┘