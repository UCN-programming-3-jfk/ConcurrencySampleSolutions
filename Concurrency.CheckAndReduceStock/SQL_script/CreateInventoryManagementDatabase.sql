-- Create the database
CREATE DATABASE InventoryManagement;
GO

-- Switch to the new database
USE InventoryManagement;
GO

-- Create Product table
CREATE TABLE Product (
    Id     INT IDENTITY(1,1) PRIMARY KEY,
    Name   NVARCHAR(200) NOT NULL,
    Price  DECIMAL(10,2) NOT NULL,
    Stock  INT NOT NULL DEFAULT 0
);

-- Optional: index for quick name lookups
CREATE INDEX IX_Product_Name ON Product(Name);
GO
