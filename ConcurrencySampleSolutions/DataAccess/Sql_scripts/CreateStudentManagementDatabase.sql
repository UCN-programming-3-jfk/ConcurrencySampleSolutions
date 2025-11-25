-- Create database
CREATE DATABASE StudentManagement;
GO

-- Switch to the new database
USE StudentManagement;
GO

-- Create Class table
CREATE TABLE SchoolClass (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(200) NOT NULL,
    RoomNumber  NVARCHAR(50) NOT NULL,
    MaxStudents INT NOT NULL,
    Timestamp   ROWVERSION                 -- auto-generated concurrency column
);
GO

-- Create Student table
CREATE TABLE Student (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    FirstName  NVARCHAR(150) NOT NULL,
    LastName   NVARCHAR(150) NOT NULL,
    Email      NVARCHAR(255) NOT NULL UNIQUE,
    SchoolClass_Id   INT NULL,

    CONSTRAINT FK_Student_Class
        FOREIGN KEY (Class_Id) REFERENCES SchoolClass(Id)
);
GO

-- Optional indexes
CREATE INDEX IX_Student_ClassId ON Student(Class_Id);
GO
