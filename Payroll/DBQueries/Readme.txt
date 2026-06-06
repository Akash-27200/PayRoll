# Payroll Run Module — Technical Assessment

## Stack

| Layer       | Technology                             |
|-------------|----------------------------------------|
| Backend     | ASP.NET Core 8 Web API (C#)            |
| ORM / DB    | Dapper + ADO.NET, SQL Server (LocalDB) |
| Frontend    | Single-file HTML + Vanilla JS          |
| Tests       | xUnit (unit tests, pure math — no DB)  |
| Version     | Git                                    |

---

## Project Structure

```
PayrollModule/
├── Database/
│   ├── 01_Schema.sql           ← Run first: all tables, constraints, indexes
│   ├── 02_SeedData.sql         ← Run second: 5 employees, 2 depts, attendance
│   └── 03_StoredProcedures.sql ← Run third: usp_RunPayroll
│
├── PayrollModule.API/
│   ├── Controllers/            ← EmployeesController, PayrollController
│   ├── Services/               ← Business logic + input validation
│   ├── Repositories/           ← Dapper queries + SP calls
│   ├── Models/                 ← POCO models + DTOs
│   ├── Infrastructure/         ← IDbConnectionFactory, SqlConnectionFactory
│   ├── Program.cs
│   └── appsettings.json
│
├── PayrollModule.Tests/
│   └── PayrollCalculationTests.cs  ← xUnit tests for net-pay formula
│
└── Frontend/
    └── index.html              ← Self-contained HTML/JS HR page
```

---

## Local Setup — Step by Step

### Prerequisites
- .NET 8 SDK  →  https://dotnet.microsoft.com/download
- SQL Server LocalDB (ships with Visual Studio) OR SQL Server Express
- A browser (for the frontend)

---

### 1. Database Setup

Open SQL Server Management Studio (SSMS) or **sqlcmd** and run the three scripts
**in order**:

```bash
# Using sqlcmd (LocalDB)
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "Database/01_Schema.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "Database/02_SeedData.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -i "Database/03_StoredProcedures.sql"
```

Or run each file directly inside SSMS.

The scripts are idempotent-safe:
- `01_Schema.sql` creates the `PayrollDB` database if it doesn't exist.
- `02_SeedData.sql` skips silently if data is already present.

---

### 2. Connection String

The default connection string in `appsettings.json` targets SQL Server LocalDB:

```
Server=(localdb)\mssqllocaldb;Database=PayrollDB;Trusted_Connection=True;TrustServerCertificate=True;
```

If you are using a full SQL Server instance, update it to:

```
Server=YOUR_SERVER;Database=PayrollDB;User Id=sa;Password=YOUR_PASS;TrustServerCertificate=True;
```

---

### 3. Run the API

```bash
cd PayrollModule.API
dotnet run
```

The API starts at `https://localhost:7001` (or `http://localhost:5001`).
Swagger UI is available at: **https://localhost:7001/swagger**

---

### 4. Run the Frontend

Open `Frontend/index.html` directly in a browser. No npm/build step needed.

> If the port differs from `7001`, update the `API` constant at the top of
> `index.html` to match.

---

### 5. Run Tests

```bash
cd PayrollModule.Tests
dotnet test
```

Tests cover: the brief's example (Ravi Sharma), full attendance, 0 days present
(LOP edge case), rounding to 2 decimal places, and parameterised checks for all
five seed employees.

---

## API Reference

| Method | Endpoint                              | Status | Description                 |
|--------|---------------------------------------|--------|-----------------------------|
| GET    | /api/employees                        | 200    | All active employees        |
| POST   | /api/payroll/run                      | 201 / 409 / 400 | Trigger payroll run  |
| GET    | /api/payroll/{month}/{year}           | 200 / 404 | Full run + details    |
| GET    | /api/payroll/{runId}/slip/{employeeId}| 200 / 404 | Individual payslip   |

Bonus pagination: `GET /api/payroll/{month}/{year}?page=1&pageSize=10`

---

## Assumptions

1. **PF on LOP (0 days present):** The formula `Gross - PF - PT` is applied as-is.
   When an employee is on full Loss-of-Pay, GrossPay = 0 but PF and PT are still
   calculated on BasicSalary, yielding a negative NetPay. This matches Indian
   payroll practice (recovery in next month). I noted this as an edge case in
   tests and the SP comment block.

2. **Working days per month:** Hardcoded in seed data as 26. In a production
   system this would be derived from a calendar table or a config.

3. **Professional Tax:** Fixed at ₹200 as stated in the brief. In reality PT slabs
   vary by state and salary band.

4. **PF basis:** PF is 12% of BasicSalary (not capped at EPFO ₹15,000 limit).
   The brief specifies a flat 12% so I followed it literally. I'd cap it at
   ₹1,800 in a production system and note it in business rules.

5. **Attendance must exist:** If an active employee has no attendance record for
   the selected month, they are silently excluded from the payroll run. The API
   returns a 201 with only the employees who have records. This is noted in the
   run summary (`employeeCount`).

6. **Immutability:** `IsFinalized` is always set to `1` on insert. There are no
   UPDATE or DELETE routes. The UNIQUE constraint on `(Month, Year)` in
   `PayrollRuns` enforces this at the database level. Any re-run attempt returns
   HTTP 409 Conflict.

---

## What I Would Add With More Time

- **Authentication / RBAC** — JWT bearer tokens; only HR Manager role can trigger
  payroll runs.
- **Working-days calendar table** — Replace the hardcoded value with a proper
  `WorkCalendar` table per department.
- **Soft-deletes / audit trail** — `CreatedBy`, `UpdatedAt` columns on all tables.
- **Integration tests** — Use `WebApplicationFactory` + a real test database with
  `Respawn` for teardown between runs.
- **Docker Compose** — One `docker-compose up` to run API + SQL Server together.
- **Email payslip** — Send each employee their payslip PDF on run finalization.
- **Negative NetPay handling** — Surface a warning when NetPay < 0 and require
  an explicit HR override flag before finalizing.
- **Concurrency** — Use optimistic locking or a DB-level `sp_getapplock` to
  prevent two processes from simultaneously submitting the same month's payroll.