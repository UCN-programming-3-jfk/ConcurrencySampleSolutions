using System.Data;
using Microsoft.Data.SqlClient;
namespace InventoryManagement.DataAccess.Pessimistic;

public class InventoryDataAccess
{
    private readonly string _connectionString;

    public InventoryDataAccess(string connectionString)
    {
        _connectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Reduces the given quantity from the stock of the specified product.
    /// Uses pessimistic concurrency with a REPEATABLE READ transaction.
    /// 
    /// Throws InvalidOperationException if:
    /// - the product does not exist, or
    /// - there is not enough stock.
    /// </summary>
    public void ReduceStock(int productId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (var transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead))
            {
                try
                {
                    int currentStock;

                    // 1. Read the current stock with an update lock to prevent other writers
                    const string selectSql = @"
                            SELECT Stock
                            FROM Product
                            WHERE Id = @ProductId;";

                    using (var selectCommand = new SqlCommand(selectSql, connection, transaction))
                    {
                        selectCommand.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;

                        object result = selectCommand.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            throw new InvalidOperationException("Product not found.");
                        }

                        currentStock = (int)result;
                    }

                    // 2. Business rule: enough stock?
                    if (currentStock < quantity)
                    {
                        throw new InvalidOperationException("Not enough stock to fulfill the request.");
                    }

                    // 3. Reduce stock
                    const string updateSql = @"
                            UPDATE Product
                            SET Stock = Stock - @Quantity
                            WHERE Id = @ProductId;";

                    using (var updateCommand = new SqlCommand(updateSql, connection, transaction))
                    {
                        updateCommand.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;
                        updateCommand.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;

                        updateCommand.ExecuteNonQuery();
                    }

                    // 4. Commit
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

