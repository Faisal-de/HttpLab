
CREATE DATABASE StudentPortalDB;
GO

USE StudentPortalDB;
GO
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100),
    Username NVARCHAR(50) UNIQUE,
    Password NVARCHAR(100), 
    Role NVARCHAR(20) -- 'Admin' or 'Student'
);

INSERT INTO Users (FullName, Username, Password, Role) 
VALUES ('Faisal Farooq', 'faisal', 'pass123', 'Student'),
       ('Senior DevOps', 'admin', 'admin123', 'Admin');
GO