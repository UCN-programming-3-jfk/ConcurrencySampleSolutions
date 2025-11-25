Here's the improved `README.md` file that incorporates the new content while maintaining the existing structure and information:

# Concurrency Sample Solutions

This repository contains practical examples demonstrating various database concurrency control techniques using C# and SQL Server. Each project addresses specific concurrency problems that commonly occur in multi-user database applications.

## Projects Overview

### 1. [Concurrency.AvoidLostUpdate](Concurrency.AvoidLostUpdate)
**Problem**: Lost Update Problem
- **Scenario**: Multiple users editing the same record simultaneously, where one user's changes overwrite another's changes without warning.
- **Solutions Demonstrated**:
  - **Optimistic Concurrency with Row Versioning**: Uses SQL Server's `ROWVERSION`/`TIMESTAMP` column to detect concurrent modifications.
  - **Optimistic Concurrency with Original Values**: Stores original field values and compares them during updates.

**Key Files**:
- [`SchoolClassDataAccess.cs`](Concurrency.AvoidLostUpdate/SchoolClassDataAccess.cs): Demonstrates ROWVERSION-based optimistic concurrency.
- [`StudentDataAccess.cs`](Concurrency.AvoidLostUpdate/StudentDataAccess.cs): Shows original value comparison approach.
- [`Model/Student.cs`](Concurrency.AvoidLostUpdate/Model/Student.cs): Entity with original value tracking.

### 2. [Concurrency.CheckAndReduceStock](Concurrency.CheckAndReduceStock)
**Problem**: Race Conditions in Stock Management
- **Scenario**: Multiple concurrent requests trying to reduce inventory stock, potentially leading to negative stock levels.
- **Solutions Demonstrated**:
  - **Optimistic Approach**: Single atomic UPDATE statement with stock validation.
  - **Pessimistic Approach**: Explicit locking with `UPDLOCK` and `REPEATABLE READ` isolation level.

**Key Files**:
- [`Optimistic/InventoryDataAccess.cs`](Concurrency.CheckAndReduceStock/Optimistic/InventoryDataAccess.cs): Atomic stock reduction using conditional UPDATE.
- [`Pessimistic/InventoryDataAccess.cs`](Concurrency.CheckAndReduceStock/Pessimistic/InventoryDataAccess.cs): Lock-based approach with explicit transaction control.

### 3. [Concurrency.AvoidDuplicateBookings](Concurrency.AvoidDuplicateBookings)
**Problem**: Phantom Reads and Duplicate Reservations
- **Scenario**: Multiple users trying to book the same resource (trailer rental) for overlapping time periods.
- **Solution Demonstrated**:
  - **Pessimistic Concurrency with SERIALIZABLE Isolation**: Uses the highest isolation level to prevent phantom reads and ensure no overlapping bookings.

**Key Files**:
- [`Pessimistic/TrailerRentalDataAccess.cs`](Concurrency.AvoidDuplicateBookings/Pessimistic/TrailerRentalDataAccess.cs): Implements SERIALIZABLE transaction to prevent booking conflicts.

## Concurrency Control Techniques Explained

### Optimistic Concurrency Control
- **When to Use**: Low contention scenarios, read-heavy workloads.
- **How it Works**: Assumes conflicts are rare; detects conflicts at commit time.
- **Advantages**: Better performance, no locking overhead.
- **Disadvantages**: Requires conflict resolution logic.

### Pessimistic Concurrency Control
- **When to Use**: High contention scenarios, critical data consistency requirements.
- **How it Works**: Prevents conflicts by locking resources upfront.
- **Advantages**: Guarantees consistency, simpler conflict handling.
- **Disadvantages**: Potential for deadlocks, reduced concurrency.

### Isolation Levels Used
- **REPEATABLE READ**: Prevents dirty reads and non-repeatable reads.
- **SERIALIZABLE**: Highest isolation level, prevents phantom reads.

## Database Schema Requirements

Each project expects specific database tables. Common patterns include:
- [`Student`](Concurrency.AvoidLostUpdate/Model/Student.cs) table with fields: Id, FirstName, LastName, Email, Class_Id ([SQL Schema](Concurrency.AvoidLostUpdate/SQL_script/CreateStudentManagementDatabase.sql)).
- [`SchoolClass`](Concurrency.AvoidLostUpdate/Model/SchoolClass.cs) table with ROWVERSION column for optimistic concurrency ([SQL Schema](Concurrency.AvoidLostUpdate/SQL_script/CreateStudentManagementDatabase.sql)).
- `Product` table with Stock field for inventory management ([SQL Schema](Concurrency.CheckAndReduceStock/SQL_script/CreateInventoryManagementDatabase.sql)).
- `Rental` table for booking scenarios with date ranges ([SQL Schema](Concurrency.AvoidDuplicateBookings/SQL_script/CreateTrailerRentalDatabase.sql)).

## Technology Stack
- **.NET 8**: Target framework for all projects.
- **[Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/)**: For SQL Server database connectivity.
- **C# 12**: Latest language features enabled.

## Getting Started

1. Clone the repository.
2. Set up SQL Server database with appropriate schema.
3. Update connection strings in each project.
4. Run individual projects to see concurrency control in action.

## Learning Objectives

By studying these examples, you'll understand:
- When and how to implement different concurrency control strategies.
- Trade-offs between optimistic and pessimistic approaches.
- SQL Server isolation levels and locking mechanisms.
- Real-world scenarios where each technique applies.
- Best practices for handling concurrent database operations.

## Contributing

These examples are designed for educational purposes to demonstrate concurrency control concepts in database programming. Contributions are welcome to enhance the examples or add new scenarios.