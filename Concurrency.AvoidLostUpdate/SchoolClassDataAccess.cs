using System.Data;
using Microsoft.Data.SqlClient;
using StudentManagement.Models;
namespace StudentManagement.DataAccess;

public class ClassDataAccess
{
    private readonly string _connectionString;

    public ClassDataAccess(string connectionString)
    {
        _connectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Updates a SchoolClass row using optimistic concurrency.
    /// The update only succeeds if the Timestamp/ROWVERSION in the database
    /// matches the one in the passed SchoolClass object.
    /// </summary>
    public void UpdateSchoolClass(SchoolClass schoolClass)
    {
        if (schoolClass == null)
            throw new ArgumentNullException(nameof(schoolClass));

        const string sql = @"
                UPDATE SchoolClass
                SET Name        = @Name,
                    RoomNumber  = @RoomNumber,
                    MaxStudents = @MaxStudents
                WHERE Id = @Id
                  AND Timestamp = @Timestamp;";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);

        command.Parameters.Add("@Id", SqlDbType.Int)
            .Value = schoolClass.Id;

        command.Parameters.Add("@Name", SqlDbType.NVarChar, 200)
            .Value = schoolClass.Name;

        command.Parameters.Add("@RoomNumber", SqlDbType.NVarChar, 50)
            .Value = schoolClass.RoomNumber;

        command.Parameters.Add("@MaxStudents", SqlDbType.Int)
            .Value = schoolClass.MaxStudents;

        // ROWVERSION maps to binary(8)
        command.Parameters.Add("@Timestamp", SqlDbType.Timestamp)
            .Value = schoolClass.Timestamp;

        connection.Open();

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected == 0)
        {
            throw new InvalidOperationException(
                "Update failed due to optimistic concurrency conflict — " +
                "the class record was modified by someone else.");
        }
    }
}
