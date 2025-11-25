using Microsoft.Data.SqlClient;
using System.Data;

namespace Concurrency.AvoidDuplicateBookings.Pessimistic;

public class RentalDataAccess
{
    private readonly string _connectionString;

    public RentalDataAccess(string connectionString)
    {
        _connectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Creates a rental for the given customer and trailer in the specified period.
    /// Uses a SERIALIZABLE transaction to avoid concurrent overlap issues.
    /// Throws InvalidOperationException if the trailer is not available.
    /// </summary>
    public void CreateRental(
        int customerId,
        int trailerId,
        DateTime rentalBegin,
        DateTime rentalEnd)
    {
        if (rentalBegin >= rentalEnd)
        {
            throw new ArgumentException("RentalBegin must be before RentalEnd.");
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Begin a transaction with isolation level SERIALIZABLE
            using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    // 1. Check for overlaps
                    // Overlap condition:
                    // Existing rental overlaps if NOT (newEnd <= existingBegin OR newBegin >= existingEnd)
                    const string overlapQuery = @"
                            SELECT COUNT(*) 
                            FROM Rental
                            WHERE Trailer_Id = @TrailerId
                              AND NOT (@RentalEnd <= RentalBegin OR @RentalBegin >= RentalEnd);";

                    using (var checkCmd = new SqlCommand(overlapQuery, connection, transaction))
                    {
                        checkCmd.Parameters.Add("@TrailerId", SqlDbType.Int).Value = trailerId;
                        checkCmd.Parameters.Add("@RentalBegin", SqlDbType.DateTime2).Value = rentalBegin;
                        checkCmd.Parameters.Add("@RentalEnd", SqlDbType.DateTime2).Value = rentalEnd;

                        int overlappingCount = (int)checkCmd.ExecuteScalar();

                        if (overlappingCount > 0)
                        {
                            // Trailer is already rented in the requested period
                            throw new InvalidOperationException("Trailer is not available in the requested period.");
                        }
                    }

                    // 2. Insert rental
                    const string insertCommandText = @"
                            INSERT INTO Rental (Customer_Id, Trailer_Id, RentalBegin, RentalEnd)
                            VALUES (@CustomerId, @TrailerId, @RentalBegin, @RentalEnd);";

                    using (var insertCmd = new SqlCommand(insertCommandText, connection, transaction))
                    {
                        insertCmd.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
                        insertCmd.Parameters.Add("@TrailerId", SqlDbType.Int).Value = trailerId;
                        insertCmd.Parameters.Add("@RentalBegin", SqlDbType.DateTime2).Value = rentalBegin;
                        insertCmd.Parameters.Add("@RentalEnd", SqlDbType.DateTime2).Value = rentalEnd;

                        insertCmd.ExecuteNonQuery();
                    }

                    // 3. Commit
                    transaction.Commit();
                }
                catch
                {
                    // Roll back on any error
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
