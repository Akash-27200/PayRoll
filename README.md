# Payroll Run Module — HR Portal

A complete full-stack payroll processing system built with ASP.NET Core 8, SQL Server, and vanilla HTML/JavaScript. This application allows HR teams to manage employees, track attendance, and process monthly payroll runs with automated salary calculations.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [System Architecture](#system-architecture)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Project Setup](#project-setup)
  - [Database Setup](#database-setup)
  - [Backend Setup](#backend-setup)
  - [Running the Application](#running-the-application)
- [Project Structure](#project-structure)
- [API Endpoints](#api-endpoints)
- [Payroll Calculation Logic](#payroll-calculation-logic)
- [Frontend Features](#frontend-features)
- [Key Implementation Details](#key-implementation-details)
- [Assumptions & Design Decisions](#assumptions--design-decisions)
- [Bonus Features Implemented](#bonus-features-implemented)
- [Known Limitations & Future Enhancements](#known-limitations--future-enhancements)
- [Testing](#testing)
- [Troubleshooting](#troubleshooting)

---

## Overview

This Payroll Run Module replaces manual Excel-based payroll processing with an automated, auditable system. HR staff can:

1. View all employees across departments
2. Run monthly payroll with a single click
3. View detailed payroll results in a professional table
4. Print individual employee payslips
5. Prevent accidental duplicate runs (409 Conflict handling)

The system enforces immutability — once a payroll run is finalized, it cannot be edited or deleted, ensuring audit compliance.

---

## Features

### Core Features (All Implemented ✓)

| Feature | Status | Details |
|---------|--------|---------|
| Employee Management | ✓ | CRUD operations, department-based organization |
| Attendance Tracking | ✓ | Monthly attendance records per employee |
| Payroll Calculation | ✓ | Automated gross pay, deductions, and net pay |
| Run History | ✓ | View finalized payroll runs by month/year |
| Individual Payslips | ✓ | Detailed earnings and deductions breakdown |
| REST API | ✓ | 4 core endpoints + health checks |
| Frontend Dashboard | ✓ | Responsive HTML/JS UI with dark theme |

### Bonus Features (All Implemented ✓)

| Feature | Status | Details |
|---------|--------|---------|
| Conflict Detection (409) | ✓ | Prevents duplicate payroll runs for same month/year |
| Unit Tests | ✓ | Comprehensive test suite for calculations and edge cases |
| Pagination | ✓ | Efficient data retrieval for large datasets |
| Payslip Printing | ✓ | Browser print-friendly payslip modal |
| Loading States | ✓ | Visual feedback during API calls |
| Error Handling | ✓ | User-friendly error messages and banners |
| CORS Support | ✓ | Allows cross-origin requests for frontend |
| Static File Serving | ✓ | Frontend served from ASP.NET Core app |

---

## System Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Frontend Layer                        │
│          HTML / CSS / JavaScript (index.html)            │
│                   (Responsive UI)                        │
└──────────────────────┬──────────────────────────────────┘
                       │ REST API Calls (HTTPS)
                       ↓
┌─────────────────────────────────────────────────────────┐
│              ASP.NET Core 8 API Layer                    │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Controllers (EmployeesController,               │   │
│  │               PayrollController)                 │   │
│  └──────────────┬───────────────────────────────────┘   │
│                 ↓                                         │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Service Layer (Business Logic)                  │   │
│  │  • IEmployeeService                              │   │
│  │  • IPayrollService (Calculations)                │   │
│  └──────────────┬───────────────────────────────────┘   │
│                 ↓                                         │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Repository Layer (Data Access)                  │   │
│  │  • IEmployeeRepository                           │   │
│  │  • IPayrollRepository                            │   │
│  └──────────────┬───────────────────────────────────┘   │
└──────────────────┼─────────────────────────────────────┘
                   │ ADO.NET + Dapper
                   ↓
┌─────────────────────────────────────────────────────────┐
│              SQL Server Database                         │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Stored Procedures                               │   │
│  │  • usp_RunPayroll (@Month, @Year)                │   │
│  │  • usp_GetPayrollRun                             │   │
│  │  • usp_GetPayrollDetails                         │   │
│  └──────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Tables                                          │   │
│  │  • tblEmployees                                  │   │
│  │  • tblDepartments                                │   │
│  │  • tblAttendance                                 │   │
│  │  • tblPayrollRuns                                │   │
│  │  • tblPayrollDetails                             │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

---

## Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Frontend | HTML5, CSS3, Vanilla JavaScript | ES6+ |
| Backend | ASP.NET Core | 8.0 |
| Language | C# | 12 |
| ORM/Database Access | Dapper + ADO.NET | Latest |
| Database | SQL Server | 2019+ / LocalDB |
| Testing | xUnit | Latest |
| Version Control | Git | N/A |

---


## Project Setup

### Step 1: Clone the Repository

```bash
git clone https://github.com/Akash-27200/PayRoll.git
cd Payroll
```

### Step 2: Database Setup

#### A. Using SQL Server Management Studio (SSMS)

1. Create the database:
   ```sql
   CREATE DATABASE PayrollDB;
   ```

2. Run the schema script (`DBQueries/01_Schema.sql`):
   - Open SSMS
   - Connect to your SQL Server instance
   - Open the file: `DBQueries/01_Schema.sql`
   - Execute (F5)

3. Run the seed data script (`DBQueries/02_SeedData.sql`):
   - Open the file: `DBQueries/02_SeedData.sql`
   - Execute (F5)
   - This creates 5 sample employees across 2 departments with attendance records

4. Verify the setup:
   ```sql
   SELECT * FROM tblEmployees;
   SELECT * FROM tblDepartments;
   SELECT * FROM tblAttendance;
   ```

#### B. Using Command Line (sqlcmd)

```bash
# Create database
sqlcmd -S .\SQLEXPRESS -i DBQueries/01_Schema.sql

# Seed data
sqlcmd -S .\SQLEXPRESS -d PayrollDB -i DBQueries/02_SeedData.sql

# Verify
sqlcmd -S .\SQLEXPRESS -d PayrollDB -Q "SELECT * FROM tblEmployees;"
```

#### C. Connection String Configuration

Edit `appsettings.json` in the `Payroll` project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PayrollDB;Trusted_Connection=true;"
  }
}
```

For SQL Server Express:
```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PayrollDB;Trusted_Connection=true;"
```

For SQL Server (named instance):
```json
"DefaultConnection": "Server=YOUR_SERVER_NAME;Database=PayrollDB;Trusted_Connection=true;Encrypt=false;"
```


### Step 3: Running the Application Using Visual Studio 2022

1. Open `Payroll.sln` in Visual Studio
2. Set `Payroll` as the startup project (right-click → Set as Startup Project)
3. Press F5 or click Start Debugging
4. The browser will automatically open to `https://localhost:7240/index.html`


## Project Structure

```
Payroll/
├── Payroll/                              # Main API Project
│   ├── Controllers/
│   │   ├── EmployeesController.cs        # Employee CRUD endpoints
│   │   └── PayrollController.cs          # Payroll run & retrieval endpoints
│   │
│   ├── Services/
│   │   ├── IEmployeeService.cs           # Interface for employee logic
│   │   ├── EmployeeService.cs            # Implementation
│   │   ├── IPayrollService.cs            # Interface for payroll logic
│   │   └── PayrollService.cs             # Calculation & execution logic
│   │
│   ├── Repositories/
│   │   ├── IEmployeeRepository.cs        # Interface for data access
│   │   ├── EmployeeRepository.cs         # Employee data operations
│   │   ├── IPayrollRepository.cs         # Interface for payroll data
│   │   └── PayrollRepository.cs          # Payroll data operations
│   │
│   ├── Models/
│   │   ├── Employee.cs                   # Employee entity
│   │   ├── PayrollRun.cs                 # Payroll run summary
│   │   └── PayrollDetail.cs              # Individual employee payroll
│   │
│   ├── DTO/
│   │   ├── PagedResult.cs                # Generic pagination wrapper
│   │   └── PayrollRunRequest.cs          # POST /payroll/run request
│   │
│   ├── Infrastructure/
│   │   ├── IDbConnectionFactory.cs       # Database connection interface
│   │   └── SqlConnectionFactory.cs       # SQL Server connection factory
│   │
│   ├── Frontend/
│   │   └── index.html                    # Single-page frontend application
│   │
│   ├── appsettings.json                  # Configuration (connection strings)
│   ├── appsettings.Development.json      # Dev-specific settings
│   ├── Program.cs                        # Startup & dependency injection
│   └── Payroll.csproj                    # Project file
│
├── PayrollTest/                          # Unit Test Project
│   ├── PayrollCalculationTests.cs        # Tests for salary calculations
│   ├── PayrollServiceTests.cs            # Service layer tests
│   └── PayrollTest.csproj                # Test project file
│
├── DBQueries/                            # Database Scripts (to be created)
│   ├── 01_Schema.sql                     # Table creation & relationships
│   ├── 02_SeedData.sql                   # Sample employees & attendance
│   └── 03_StoredProcedures.sql           # Payroll calculation procedures
│
├── .gitignore                            # Git ignore rules
├── Payroll.sln                           # Solution file
└── README.md                             # This file
```

---

## API Endpoints

All endpoints are prefixed with `https://localhost:7240/api`

### 1. Get All Employees

Request:
```http
GET /api/employees
```

Response (200 OK):
```json
[
  {
    "employeeId": 1,
    "employeeCode": "EMP001",
    "fullName": "Ravi Sharma",
    "basicSalary": 30000,
    "departmentId": 1,
    "departmentName": "Engineering"
  },
  {
    "employeeId": 2,
    "employeeCode": "EMP002",
    "fullName": "Priya Verma",
    "basicSalary": 25000,
    "departmentId": 2,
    "departmentName": "HR"
  }
]
```

---

### 2. Run Payroll (POST)

Request:
```http
POST /api/payroll/run
Content-Type: application/json

{
  "month": 12,
  "year": 2024
}
```

Success Response (201 Created):
```json
{
  "runId": 5,
  "month": 12,
  "year": 2024,
  "employeeCount": 5,
  "totalNetPay": 95000.00,
  "executedAt": "2024-12-15T10:30:00Z"
}
```

Conflict Response (409 Conflict): [BONUS FEATURE]
```json
{
  "runId": 4,
  "message": "Payroll run for December 2024 already exists (Run ID: 4)"
}
```

Error Response (400 Bad Request):
```json
{
  "message": "Invalid month (1-12) or year."
}
```

---

### 3. Get Payroll Run Details

Request:
```http
GET /api/payroll/{month}/{year}
```

Example:
```http
GET /api/payroll/12/2024
```

Success Response (200 OK):
```json
{
  "runId": 5,
  "month": 12,
  "year": 2024,
  "employeeCount": 5,
  "totalNetPay": 95000.00,
  "executedAt": "2024-12-15T10:30:00Z",
  "details": [
    {
      "employeeId": 1,
      "employeeCode": "EMP001",
      "fullName": "Ravi Sharma",
      "departmentName": "Engineering",
      "basicSalary": 30000,
      "workingDays": 26,
      "daysPresent": 24,
      "grossPay": 27692.31,
      "pfDeduction": 3600,
      "professionalTax": 200,
      "netPay": 23892.31
    }
  ]
}
```

Not Found Response (404):
```json
{
  "message": "Payroll run not found for December 2024"
}
```

---

### 4. Get Individual Payslip

Request:
```http
GET /api/payroll/{runId}/slip/{employeeId}
```

Example:
```http
GET /api/payroll/5/slip/1
```

Success Response (200 OK):
```json
{
  "runId": 5,
  "employeeId": 1,
  "employeeCode": "EMP001",
  "fullName": "Ravi Sharma",
  "departmentName": "Engineering",
  "basicSalary": 30000,
  "workingDays": 26,
  "daysPresent": 24,
  "grossPay": 27692.31,
  "pfDeduction": 3600,
  "professionalTax": 200,
  "netPay": 23892.31
}
```

---

## Payroll Calculation Logic

### Formula

| Component | Calculation | Notes |
|-----------|-------------|-------|
| Gross Pay | (Basic Salary ÷ Total Working Days) × Days Present | Prorated salary |
| PF Deduction | 12% of Basic Salary | Fixed percentage, not affected by attendance |
| Professional Tax | ₹200 (flat) | Fixed monthly deduction |
| Net Pay | Gross Pay − PF Deduction − Professional Tax | Final amount paid |

### Example Calculation

Employee: Ravi Sharma  
Basic Salary: ₹30,000  
Working Days: 26  
Days Present: 24

```
Gross Pay      = (30,000 ÷ 26) × 24 = ₹27,692.31
PF Deduction   = 30,000 × 12%        = ₹3,600.00
Professional Tax = Flat               = ₹200.00
─────────────────────────────────────────────────
Net Pay        = 27,692.31 − 3,600 − 200 = ₹23,892.31
```

### Edge Cases Handled

- Zero days present: Gross pay = 0, deductions still apply
- Missing attendance: Treated as 0 days present
- Division by zero: Prevented by validating working days > 0
- Negative values: Never allowed in final calculations
- Rounding: All amounts rounded to 2 decimal places

### Implementation Files

- Service Logic: `Services/PayrollService.cs` - Core calculation logic
- SQL Procedure: `DBQueries/StoredProcedures.sql` - `usp_RunPayroll`
- Tests: `PayrollTest/PayrollCalculationTests.cs` - Comprehensive test suite

---

## Frontend Features

### User Interface

The frontend is a responsive, single-page application with a modern dark theme (GitHub-inspired design).

#### Dashboard Components

1. Header
   - Logo and title: "Payroll Run Module"
   - Subtitle: "HR Portal — Monthly Payroll Processing"

2. Payroll Run Controls
   - Month selector (January–December)
   - Year input field (2000–2100)
   - "⚡ Run Payroll" button (primary action)
   - "🔍 Load Existing" button (view previous runs)

3. Status Banners
   - Success: Green banner confirming payroll execution
   - Warning: Yellow banner for duplicate runs (409) with option to load existing
   - Error: Red banner for validation or API errors
   - Info: Blue banner for loading states

4. Payroll Results
   - Summary Card: Shows period, employee count, total net pay, run ID, and status
   - Details Table: Sortable table with columns:
     - Employee name & code
     - Department
     - Basic salary, working days, days present
     - Gross pay, PF deduction, professional tax
     - Net pay (highlighted in red if negative)
     - "Slip" button to view individual payslip

5. Payslip Modal
   - Detailed earnings and deductions breakdown
   - Professional printing layout
   - Print-to-PDF support via browser
   - Close button and print button

### Interactive Features

- Loading States: Spinner icon during API calls
- Error Handling: User-friendly error messages with network diagnostics
- Month Selection: Pre-selects current month on page load
- Responsive Design: Works on desktop, tablet, and mobile
- Print-Friendly: Payslips optimized for printing (CSS media queries)
- CORS Handling: Allows secure cross-origin API calls

### Technologies Used

- HTML5: Semantic markup
- CSS3: CSS Grid, Flexbox, custom properties (dark theme tokens)
- JavaScript (ES6+):
  - Async/await for API calls
  - DOM manipulation
  - Event handling
  - Local state management
  - Number formatting (Indian locale)

---

## Key Implementation Details

### 1. Dependency Injection (Program.cs)

All services and repositories are registered in the DI container:

```csharp
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
```

### 2. Database Connection Management (Dapper)

- Interface: `IDbConnectionFactory`
- Implementation: `SqlConnectionFactory`
- Pattern: Factory pattern for consistent connection handling
- Benefits: Easy to mock in tests, supports multiple database backends

```csharp
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
```

### 3. Immutability Enforcement

Once a payroll run is finalized:
- No UPDATE or DELETE operations allowed
- Stored procedure creates new records only
- Database constraint: `UniqueConstraint(RunId, Month, Year)`

### 4. Pagination Support (Bonus Feature)

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
```

### 5. Static File Serving

The `Program.cs` configures ASP.NET Core to serve `index.html` from the `Frontend/` folder:

```csharp
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = { "index.html" }
});
app.UseStaticFiles();
```

This allows the frontend to be served alongside the API.

### 6. CORS Configuration

Enables the frontend to call the API:

```csharp
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()));
```

---

## Assumptions & Design Decisions

### 1. Flat Professional Tax
- Assumption: Professional tax is ₹200 per month (flat), regardless of salary or attendance.
- Rationale: Simplified calculation as per brief. In real HR systems, this is often salary-slab based.
- Could be changed: If needed, create a `tblTaxSlabs` table and update `PayrollService.cs`.

### 2. PF on Full Basic Salary
- Assumption: PF deduction (12%) is calculated on the full basic salary, not prorated by attendance.
- Rationale: Standard practice in Indian employment law. PF is a benefit regardless of attendance.
- Could be changed: Update line in `PayrollService.cs` if policy differs.

### 3. Zero Days Present Handling
- Assumption: If an employee has 0 days present:
  - Gross pay = ₹0
  - PF deduction = Still 12% of basic (mandatory)
  - Professional tax = Still ₹200 (monthly charge)
  - Net pay can be negative (LOP scenario)
- Rationale: Reflects real-world payroll where deductions apply even on leave of absence.
- Note: Payslip UI highlights negative net pay in red.

### 4. Month/Year Uniqueness
- Assumption: Only one payroll run is allowed per month/year combination.
- Rationale: Prevents accidental duplicate runs. If a run exists, the 409 response lets HR reload the existing run.
- Could be changed: Remove the unique constraint if multiple runs per month are needed.

### 5. Immutability
- Assumption: Once finalized, payroll runs cannot be modified or deleted.
- Rationale: Audit compliance and data integrity. Payroll is a sensitive operation.
- Alternative: Could add an "Unlock" feature for authorized users only.

### 6. Attendance Data Source
- Assumption: Attendance records (`tblAttendance`) are pre-populated by a separate system or manual entry.
- Rationale: Payroll module focuses on calculation, not attendance capture.
- Future: Could add an attendance import feature.

### 7. No Employee Deactivation
- Assumption: Deleted employees are removed from the system (soft delete not implemented).
- Rationale: Simplified model for the assessment. Production would use soft deletes.
- Could be added: Add `IsActive` boolean column to `tblEmployees`.

### 8. Single Department Per Employee
- Assumption: Each employee belongs to exactly one department.
- Rationale: Simplifies the model. Real systems might need multiple assignments.

### 9. Currency: Indian Rupees (₹)
- Assumption: All amounts are in INR; no currency conversion.
- Rationale: Brief mentions rupees in the example calculation.

### 10. API Response Codes
- 201 Created: Successful payroll run creation
- 200 OK: Successful data retrieval
- 404 Not Found: Payroll run doesn't exist
- 409 Conflict: Payroll run already exists (bonus feature)
- 400 Bad Request: Invalid month/year input

---

## Bonus Features Implemented

### ✓ 1. HTTP 409 Conflict on Duplicate Runs

When attempting to run payroll for a month/year that already exists:

```csharp
if (existingRun != null)
{
    return Conflict(new
    {
        runId = existingRun.RunId,
        message = $"Payroll run for {monthName} {year} already exists (Run ID: {existingRun.RunId})"
    });
}
```

Frontend Handling: Shows a warning banner and automatically loads the existing run.

---

### ✓ 2. Comprehensive Unit Tests

File: `PayrollTest/PayrollCalculationTests.cs`

Tests cover:
- ✓ Correct gross pay calculation
- ✓ PF deduction (12% of basic)
- ✓ Professional tax (flat ₹200)
- ✓ Net pay calculation
- ✓ Edge case: Zero days present
- ✓ Edge case: Full attendance
- ✓ Edge case: Partial attendance
- ✓ Rounding to 2 decimal places

Run tests:
```bash
dotnet test PayrollTest
```

---

### ✓ 3. Pagination Support

Generic `PagedResult<T>` class for paginated API responses:

```csharp
GET /api/employees?pageNumber=1&pageSize=10
```

Response:
```json
{
  "items": [...],
  "totalCount": 25,
  "pageNumber": 1,
  "pageSize": 10
}
```

---

### ✓ 4. Printable Payslip View

- Modal Display: Individual payslip in a centered modal
- Print-Friendly: CSS `@media print` rules hide non-essential UI
- Browser Print: Use Ctrl+P or the "Print" button
- PDF Export: Print to PDF directly from browser

Features:
- Employee details (name, code, department)
- Pay period
- Attendance info
- Earnings breakdown
- Deductions breakdown
- Net pay (highlighted)

---

## Testing

### Unit Tests

Tests are located in `PayrollTest/PayrollCalculationTests.cs`.
