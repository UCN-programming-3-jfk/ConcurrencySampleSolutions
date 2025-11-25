-- Create database
CREATE DATABASE TrailerRentalDb;
GO

USE TrailerRentalDb;
GO

-- Customer table
CREATE TABLE Customer (
    Id     INT IDENTITY(1,1) PRIMARY KEY,
    Email  NVARCHAR(255) NOT NULL UNIQUE
);

-- Trailer table
CREATE TABLE Trailer (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(255) NOT NULL,
    MaxCarryWeight  DECIMAL(10,2) NOT NULL
);

-- Rental table with composite key
CREATE TABLE Rental (
    Customer_Id  INT NOT NULL,
    Trailer_Id   INT NOT NULL,
    RentalBegin  DATETIME2(0) NOT NULL,
    RentalEnd    DATETIME2(0) NOT NULL,

    CONSTRAINT FK_Rental_Customer
        FOREIGN KEY (Customer_Id) REFERENCES Customer(Id),

    CONSTRAINT FK_Rental_Trailer
        FOREIGN KEY (Trailer_Id) REFERENCES Trailer(Id),

    -- Composite primary key made of both FKs
    CONSTRAINT PK_Rental PRIMARY KEY (Customer_Id, Trailer_Id),

    CONSTRAINT CK_Rental_BeginBeforeEnd
        CHECK (RentalBegin < RentalEnd)
);

-- Optional index to improve availability/overlap checks
CREATE INDEX IX_Rental_Trailer_Range
    ON Rental (Trailer_Id, RentalBegin, RentalEnd);
GO
