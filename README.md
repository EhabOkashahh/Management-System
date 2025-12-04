#  Management System (SMS)

##  Overview
The **Management System (SMS)** is a secure and scalable web application built with **ASP.NET MVC**.  
It provides a flexible platform for managing entities such as **users**, **departments**, and **roles**, making it adaptable to many types of organizations or use cases.

The system features a **role-based access control** model, allowing administrators to manage users, assign roles, and access a dedicated dashboard, while regular users have access only to their allowed sections.

This project demonstrates clean architectural principles through the use of **N-Tier architecture**, **Repository**, and **Unit of Work** design patterns — ensuring maintainability, scalability, and code reusability.

---

##  Key Features
###  Authentication & Authorization
- **Google OAuth Login** for quick and secure access.
- **Role-Based Access Control (RBAC)** — only admins can manage users, roles, and departments.
- **Password Reset via Email**, allowing users to recover their accounts securely.
- **Phone Number Verification** using **Twilio API** for an extra layer of security.

###  User & Admin Features
- **CRUD Operations** for:
  - Employees
  - Departments
  - Roles
  - User Profiles
- **Admin Dashboard** Simple Dashboard can Modify on Users and Roles .
- **Profile Photos** for users and employees.
- **Responsive and clean UI** using Bootstrap and Razor views.

###  Architecture & Patterns
- **N-Tier Architecture** to separate presentation, business logic, and data access layers.
- **Repository Pattern** to abstract database logic and make it reusable.
- **Unit of Work Pattern** to coordinate and manage database transactions.
- **Dependency Injection** for clean service management.

---

##  Project Structure
The solution follows a clear **N-Tier architecture**, divided into:

| Layer | Description |
|-------|--------------|
| **Presentation Layer (UI)** | ASP.NET MVC web project handling views, controllers, and user interactions. |
| **Business Logic Layer (BLL)** | Contains services, business rules, and application logic. |
| **Data Access Layer (DAL)** | Handles Entity Framework operations using Repository and Unit of Work patterns. |

---

##  Technologies Used
| Category | Technology |
|-----------|-------------|
| **Frontend** | HTML5, CSS3, Bootstrap, JavaScript, Razor Views |
| **Backend** | ASP.NET MVC (C#), Entity Framework |
| **Database** | Microsoft SQL Server |
| **Authentication** | Google OAuth 2.0, Identity |
| **Communication** | Twilio API (Phone Verification), SMTP (Email) |
| **Architecture** | N-Tier with Repository & Unit of Work Patterns |

---

## Roles in the System
| Role | Description |
|------|--------------|
| **Admin** | Full access to the dashboard. Can manage users, departments, and roles. |
| **User/Employee** | Limited access. Can view and update personal data. |

---

##  Workflow
1. A user can **register or log in** (manually or using Google).
2. After login, **phone number verification** (via Twilio) ensures account security.
3. Admins can **add, edit, or delete** departments, employees, and roles.
4. Users can **update their profiles** (including profile pictures).
5. Password reset links are sent through **email** when requested.

---

##  Design Patterns Used
- **Repository Pattern** — abstracts data layer for cleaner and testable code.
- **Unit of Work Pattern** — ensures multiple database operations are handled within a single transaction.
- **Dependency Injection** — promotes flexibility and maintainability.

---

##  Security Highlights
- Role-based authorization using `[Authorize]` attribute.
- OAuth 2.0 for social login (Google).
- Encrypted password storage and secure token handling.
- Twilio verification and email-based password recovery.
