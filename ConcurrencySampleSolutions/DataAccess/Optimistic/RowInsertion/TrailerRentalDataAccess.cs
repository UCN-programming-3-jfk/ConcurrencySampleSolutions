using System.Data;
using Microsoft.Data.SqlClient;
namespace TrailerRental.DataAccess.Optimistic;

public class TrailerRentalDataAccess
{
    private readonly string _connectionString;

    public TrailerRentalDataAccess(string connectionString)
    {
        _connectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Creates a rental using optimistic concurrency:
    /// a single INSERT ... WHERE NOT EXISTS (...) statement.
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

        const string insertSql = @"
                INSERT INTO Rental (Customer_Id, Trailer_Id, RentalBegin, RentalEnd)
                SELECT @CustomerId, @TrailerId, @RentalBegin, @RentalEnd
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM Rental
                    WHERE Trailer_Id = @TrailerId
                      AND NOT (@RentalEnd <= RentalBegin OR @RentalBegin >= RentalEnd)
                );";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(insertSql, connection))
        {
            command.Parameters.Add("@CustomerId", SqlDbType.Int).Value = customerId;
            command.Parameters.Add("@TrailerId", SqlDbType.Int).Value = trailerId;
            command.Parameters.Add("@RentalBegin", SqlDbType.DateTime2).Value = rentalBegin;
            command.Parameters.Add("@RentalEnd", SqlDbType.DateTime2).Value = rentalEnd;

            connection.Open();

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                // The WHERE NOT EXISTS blocked the insert => overlap detected
                throw new InvalidOperationException("Trailer is not available in the requested period.");
            }
        }
    }
}
