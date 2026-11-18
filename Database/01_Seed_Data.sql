-- ============================================================
-- PECCI HRIS — Sample / Test Seed Data
-- Run this AFTER: dotnet ef database update
-- EF migrations already handle: Roles, Admin User, Departments (1–5),
-- Leave Types, and System Settings.
-- This script adds: Auditing department (if missing), Positions, Sample Employees,
-- User Accounts, and Leave Credits for testing across all departments.
--
-- All test account passwords: Test@1234
-- Admin password: Admin@123
-- ============================================================



-- ── Auditing department (not in EF seed — insert if missing) ─────────────────
IF NOT EXISTS (SELECT 1 FROM Departments WHERE DepartmentCode = 'AUD')
    INSERT INTO Departments (DepartmentName, DepartmentCode, IsActive, CreatedAt)
    VALUES ('Auditing', 'AUD', 1, '2026-01-01');
GO

-- ── Positions (one manager + one staff per department) ────────────────────────
-- Skip if already exist
IF NOT EXISTS (SELECT 1 FROM Positions WHERE PositionCode = 'HR-MGR')
INSERT INTO Positions (PositionTitle, PositionCode, DepartmentID, BasicSalary, IsActive, CreatedAt)
VALUES
  ('HR Manager',          'HR-MGR',  1, 65000.00, 1, '2026-01-01'),
  ('HR Staff',            'HR-STF',  1, 35000.00, 1, '2026-01-01'),
  ('Finance Manager',     'FIN-MGR', 2, 70000.00, 1, '2026-01-01'),
  ('Accountant',          'FIN-ACC', 2, 40000.00, 1, '2026-01-01'),
  ('IT Manager',          'IT-MGR',  3, 75000.00, 1, '2026-01-01'),
  ('IT Staff',            'IT-STF',  3, 38000.00, 1, '2026-01-01'),
  ('Operations Manager',  'OPS-MGR', 4, 68000.00, 1, '2026-01-01'),
  ('Operations Staff',    'OPS-STF', 4, 32000.00, 1, '2026-01-01'),
  ('Marketing Manager',   'MKT-MGR', 5, 62000.00, 1, '2026-01-01'),
  ('Marketing Staff',     'MKT-STF', 5, 33000.00, 1, '2026-01-01'),
  ('Audit Manager',       'AUD-MGR', 6, 67000.00, 1, '2026-01-01'),
  ('Auditor',             'AUD-STF', 6, 38000.00, 1, '2026-01-01');
GO

-- ── Sample Employees (2 per department, 12 total) ─────────────────────────────
-- Uses MERGE to avoid duplicate key errors on re-run
MERGE Employees AS target
USING (VALUES
  -- HR (DeptID 1)
  ('EMP-0003','Maria', 'Santos',  'Reyes',     '1990-03-15','Female','Single',  'Filipino','1 Test St, Sample City',       '09000000001','mreyes@example.com',      'HR', 'HR-MGR', '2022-01-10','Regular',     'Active'),
  ('EMP-0004','Jose',  'Cruz',    'Dela Cruz', '1993-07-22','Male',  'Single',  'Filipino','2 Test St, Sample City',       '09000000002','jdelacruz@example.com',   'HR', 'HR-STF', '2023-03-01','Regular',     'Active'),
  -- Finance (DeptID 2)
  ('EMP-0005','Ana',   'Lopez',   'Garcia',    '1988-11-05','Female','Married', 'Filipino','3 Test St, Sample City',       '09000000003','agarcia@example.com',     'FIN','FIN-MGR','2021-06-15','Regular',     'Active'),
  ('EMP-0006','Pedro', 'Ramos',   'Santos',    '1995-02-18','Male',  'Single',  'Filipino','4 Test St, Sample City',       '09000000004','psantos@example.com',     'FIN','FIN-ACC','2023-08-01','Probationary','Active'),
  -- IT (DeptID 3)
  ('EMP-0007','Carlo', 'Bautista','Mendoza',   '1991-09-30','Male',  'Single',  'Filipino','5 Test St, Sample City',       '09000000005','cmendoza@example.com',    'IT', 'IT-MGR', '2020-04-20','Regular',     'Active'),
  ('EMP-0008','Liza',  'Torres',  'Villanueva','1996-05-12','Female','Single',  'Filipino','6 Test St, Sample City',       '09000000006','lvillanueva@example.com', 'IT', 'IT-STF', '2024-01-15','Probationary','Active'),
  -- Operations (DeptID 4)
  ('EMP-0009','Ramon', 'Flores',  'Castillo',  '1987-12-01','Male',  'Married', 'Filipino','7 Test St, Sample City',       '09000000007','rcastillo@example.com',   'OPS','OPS-MGR','2019-09-01','Regular',     'Active'),
  ('EMP-0010','Grace', 'Aquino',  'Fernandez', '1994-04-25','Female','Single',  'Filipino','8 Test St, Sample City',       '09000000008','gfernandez@example.com',  'OPS','OPS-STF','2022-11-01','Regular',     'Active'),
  -- Marketing (DeptID 5)
  ('EMP-0011','Mark',  'Diaz',    'Pascual',   '1992-08-14','Male',  'Single',  'Filipino','9 Test St, Sample City',       '09000000009','mpascual@example.com',    'MKT','MKT-MGR','2021-02-01','Regular',     'Active'),
  ('EMP-0012','Nina',  'Reyes',   'Soriano',   '1997-01-08','Female','Single',  'Filipino','10 Test St, Sample City',      '09000000010','nsoriano@example.com',    'MKT','MKT-STF','2023-05-15','Regular',     'Active'),
  -- Auditing (DeptID 6)
  ('EMP-0013','Victor','Lim',     'Tan',       '1985-06-20','Male',  'Married', 'Filipino','11 Test St, Sample City',      '09000000011','vtan@example.com',        'AUD','AUD-MGR','2018-07-01','Regular',     'Active'),
  ('EMP-0014','Claire','Ong',     'Navarro',   '1993-10-03','Female','Single',  'Filipino','12 Test St, Sample City',      '09000000012','cnavarro@example.com',    'AUD','AUD-STF','2022-09-01','Regular',     'Active')
) AS source (EmployeeNo, FirstName, MiddleName, LastName, DateOfBirth, Gender, CivilStatus, Nationality, Address, ContactNumber, CompanyEmail, DeptCode, PosCode, DateHired, EmploymentStatus, Status)
ON target.EmployeeNo = source.EmployeeNo
WHEN NOT MATCHED THEN
  INSERT (EmployeeNo, FirstName, MiddleName, LastName, DateOfBirth, Gender, CivilStatus, Nationality, Address, ContactNumber, CompanyEmail, DepartmentID, PositionID, DateHired, EmploymentStatus, Status, CreatedAt)
  VALUES (
    source.EmployeeNo, source.FirstName, source.MiddleName, source.LastName,
    source.DateOfBirth, source.Gender, source.CivilStatus, source.Nationality,
    source.Address, source.ContactNumber, source.CompanyEmail,
    (SELECT DepartmentID FROM Departments WHERE DepartmentCode = source.DeptCode),
    (SELECT PositionID   FROM Positions   WHERE PositionCode  = source.PosCode),
    source.DateHired, source.EmploymentStatus, source.Status, GETDATE()
  );
GO

-- ── User Accounts for test employees ─────────────────────────────────────────
-- Password for all: Test@1234
-- BCrypt hash (cost 11) for "Test@1234"
DECLARE @hash NVARCHAR(MAX) = N'$2a$11$D9ldhzgheBjmOc3r1stdYe3SsG.ck.K/Z08Kc2kJSuz/lANU0MfL.'

-- RoleID: 1=HR Admin, 2=HR Staff, 3=Manager, 4=Employee
MERGE Users AS target
USING (VALUES
  (N'mreyes',     @hash, N'mreyes@example.com',      2, N'EMP-0003'),  -- HR Staff
  (N'jdelacruz',  @hash, N'jdelacruz@example.com',   4, N'EMP-0004'),  -- Employee
  (N'agarcia',    @hash, N'agarcia@example.com',      3, N'EMP-0005'),  -- Manager
  (N'psantos',    @hash, N'psantos@example.com',      4, N'EMP-0006'),  -- Employee
  (N'cmendoza',   @hash, N'cmendoza@example.com',     3, N'EMP-0007'),  -- Manager (IT)
  (N'lvillanueva',@hash, N'lvillanueva@example.com',  4, N'EMP-0008'),  -- Employee
  (N'rcastillo',  @hash, N'rcastillo@example.com',    3, N'EMP-0009'),  -- Manager
  (N'gfernandez', @hash, N'gfernandez@example.com',   4, N'EMP-0010'),  -- Employee
  (N'mpascual',   @hash, N'mpascual@example.com',     3, N'EMP-0011'),  -- Manager
  (N'nsoriano',   @hash, N'nsoriano@example.com',     4, N'EMP-0012'),  -- Employee
  (N'vtan',       @hash, N'vtan@example.com',         3, N'EMP-0013'),  -- Manager
  (N'cnavarro',   @hash, N'cnavarro@example.com',     4, N'EMP-0014')   -- Employee
) AS source (Username, PasswordHash, Email, RoleID, EmployeeNo)
ON target.Username = source.Username
WHEN NOT MATCHED THEN
  INSERT (Username, PasswordHash, Email, RoleID, EmployeeID, IsActive, CreatedAt)
  VALUES (
    source.Username, source.PasswordHash, source.Email, source.RoleID,
    (SELECT EmployeeID FROM Employees WHERE EmployeeNo = source.EmployeeNo),
    1, GETDATE()
  );
GO

-- ── Leave Credits for all active employees (current year) ────────────────────
DECLARE @Year INT = YEAR(GETDATE());

INSERT INTO LeaveCredits (EmployeeID, LeaveTypeID, Year, TotalCredits, UsedCredits, PendingCredits, CreatedAt)
SELECT e.EmployeeID, lt.LeaveTypeID, @Year, lt.DefaultDaysPerYear, 0, 0, GETDATE()
FROM Employees e
CROSS JOIN LeaveTypes lt
WHERE e.Status = 'Active'
  AND lt.IsActive = 1
  AND NOT EXISTS (
    SELECT 1 FROM LeaveCredits lc
    WHERE lc.EmployeeID = e.EmployeeID
      AND lc.LeaveTypeID = lt.LeaveTypeID
      AND lc.Year = @Year
  );
GO

PRINT '============================================';
PRINT 'Seed data inserted successfully.';
PRINT 'Test accounts (password: Test@1234):';
PRINT '  mreyes     - HR Staff      - Human Resources';
PRINT '  jdelacruz  - Employee      - Human Resources';
PRINT '  agarcia    - Manager       - Finance & Accounting';
PRINT '  psantos    - Employee      - Finance & Accounting';
PRINT '  cmendoza   - Manager       - Information Technology';
PRINT '  lvillanueva- Employee      - Information Technology';
PRINT '  rcastillo  - Manager       - Operations';
PRINT '  gfernandez - Employee      - Operations';
PRINT '  mpascual   - Manager       - Marketing';
PRINT '  nsoriano   - Employee      - Marketing';
PRINT '  vtan       - Manager       - Auditing';
PRINT '  cnavarro   - Employee      - Auditing';
PRINT '============================================';
GO
