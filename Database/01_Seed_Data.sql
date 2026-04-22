-- ============================================================
-- PECCI HRIS — Optional Manual Seed Data
-- Run this AFTER running EF migrations (Update-Database)
-- The EF seed already handles Roles, Admin User, Departments,
-- and Leave Types. This script adds sample employees for testing.
-- ============================================================

USE PECCI_HRIS_DB;
GO

-- ── Sample Positions ─────────────────────────────────────────────────────────
INSERT INTO Positions (PositionTitle, PositionCode, DepartmentID, BasicSalary, IsActive, CreatedAt)
VALUES
    ('HR Manager',          'HR-MGR',  1, 65000.00, 1, GETDATE()),
    ('HR Officer',          'HR-OFF',  1, 35000.00, 1, GETDATE()),
    ('Accountant',          'FIN-ACC', 2, 40000.00, 1, GETDATE()),
    ('Finance Manager',     'FIN-MGR', 2, 70000.00, 1, GETDATE()),
    ('IT Specialist',       'IT-SPEC', 3, 45000.00, 1, GETDATE()),
    ('Systems Administrator','IT-SA',  3, 55000.00, 1, GETDATE()),
    ('Operations Officer',  'OPS-OFF', 4, 32000.00, 1, GETDATE()),
    ('Marketing Officer',   'MKT-OFF', 5, 35000.00, 1, GETDATE());
GO

-- ── Sample Employees ─────────────────────────────────────────────────────────
INSERT INTO Employees (
    EmployeeNo, FirstName, MiddleName, LastName,
    DateOfBirth, Gender, CivilStatus, Nationality,
    Address, ContactNumber, CompanyEmail,
    SSSNumber, PhilHealthNumber, PagIbigNumber, TINNumber,
    DepartmentID, PositionID,
    DateHired, EmploymentStatus, Status, CreatedAt
)
VALUES
    ('EMP-0001', 'Maria',   'Santos',  'Reyes',    '1990-03-15', 'Female', 'Single',  'Filipino', 'Quezon City, Metro Manila', '09171234567', 'mreyes@pecci.com.ph',    '33-1234567-8', '12-345678901-2', '1234-5678-9', '123-456-789-000', 1, 1, '2020-01-06', 'Regular', 'Active', GETDATE()),
    ('EMP-0002', 'Juan',    'Cruz',    'Dela Cruz','1988-07-22', 'Male',   'Married', 'Filipino', 'Makati City, Metro Manila', '09181234567', 'jdelacruz@pecci.com.ph', '33-2345678-9', '12-456789012-3', '2345-6789-0', '234-567-890-000', 2, 3, '2019-06-01', 'Regular', 'Active', GETDATE()),
    ('EMP-0003', 'Ana',     'Lim',     'Garcia',   '1995-11-08', 'Female', 'Single',  'Filipino', 'Pasig City, Metro Manila',  '09191234567', 'agarcia@pecci.com.ph',   '33-3456789-0', '12-567890123-4', '3456-7890-1', '345-678-901-000', 3, 5, '2021-03-15', 'Regular', 'Active', GETDATE()),
    ('EMP-0004', 'Pedro',   'Bautista','Mendoza',  '1985-05-30', 'Male',   'Married', 'Filipino', 'Taguig City, Metro Manila', '09201234567', 'pmendoza@pecci.com.ph',  '33-4567890-1', '12-678901234-5', '4567-8901-2', '456-789-012-000', 4, 7, '2018-09-01', 'Regular', 'Active', GETDATE()),
    ('EMP-0005', 'Rosa',    'Aquino',  'Torres',   '1993-02-14', 'Female', 'Single',  'Filipino', 'Mandaluyong, Metro Manila', '09211234567', 'rtorres@pecci.com.ph',   '33-5678901-2', '12-789012345-6', '5678-9012-3', '567-890-123-000', 5, 8, '2022-01-10', 'Regular', 'Active', GETDATE());
GO

-- ── Leave Credits for current year ───────────────────────────────────────────
DECLARE @Year INT = YEAR(GETDATE());

INSERT INTO LeaveCredits (EmployeeID, LeaveTypeID, Year, TotalCredits, UsedCredits, PendingCredits, CreatedAt)
SELECT e.EmployeeID, lt.LeaveTypeID, @Year, lt.DefaultDaysPerYear, 0, 0, GETDATE()
FROM Employees e
CROSS JOIN LeaveTypes lt
WHERE e.Status = 'Active' AND lt.IsActive = 1;
GO

PRINT 'Sample data inserted successfully.';
GO
