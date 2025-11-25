using System.Data;
using Microsoft.Data.SqlClient;
namespace Concurrency.CheckAndReduceStock.Optimistic;

public class InventoryDataAccess
{
    private readonly string _connectionString;

    public InventoryDataAccess(string connectionString)
    {
        _connectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Deducts the given quantity from the stock of the specified product.
    /// Uses optimistic concurrency: a single UPDATE with a stock check.
    /// 
    /// Throws InvalidOperationException if:
    /// - the product does not exist, or
    /// - there is not enough stock (or a concurrent update "wins").
    /// </summary>
    public void ReduceStock(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        }

        const string updateSql = @"
                UPDATE Product
                SET Stock = Stock - @Quantity
                WHERE Id = @ProductId
                  AND Stock >= @Quantity;";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(updateSql, connection))
        {
            command.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
            command.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;

            connection.Open();

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                // Could be:
                // - product doesn't exist, or
                // - not enough stock (including due to a race condition)
                throw new InvalidOperationException(
                    "Could not deduct stock. Product may not exist or there is not enough stock.");
            }
        }
    }
}