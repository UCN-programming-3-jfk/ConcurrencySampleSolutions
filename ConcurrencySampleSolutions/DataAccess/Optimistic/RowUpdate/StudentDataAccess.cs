using System.Data;
using ConcurrencySampleSolutions.DataAccess.Optimistic.RowUpdate.Model;
using Microsoft.Data.SqlClient;
namespace StudentManagement.DataAccess.Optimistic;

public class StudentDataAccess
{
    private readonly string _connectionString;

    public StudentDataAccess(string connectionString)
    {
        _connectionString = connectionString
            ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Updates a student row using optimistic concurrency.
    /// Update only succeeds if Id, FirstName, LastName, Email, and ClassId
    /// in the DB still match the values supplied in the Student object.
    /// </summary>
    public void UpdateStudent(Student student)
    {
        if (student == null) throw new ArgumentNullException(nameof(student));

        const string sql = @"
                UPDATE Student
                SET FirstName = @FirstName,
                    LastName  = @LastName,
                    Email     = @Email,
                    Class_Id  = @ClassId
                WHERE Id = @Id
                  AND FirstName = @FirstName
                  AND LastName  = @LastName
                  AND Email     = @Email
                  AND (
                        (Class_Id = @ClassId) 
                        OR (Class_Id IS NULL AND @ClassId IS NULL)
                      );";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(sql, connection))
        {
            command.Parameters.Add("@Id", SqlDbType.Int).Value = student.Id;
            command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 150).Value = student.FirstName;
            command.Parameters.Add("@LastName", SqlDbType.NVarChar, 150).Value = student.LastName;
            command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = student.Email;

            // Nullable parameter handling
            command.Parameters.Add("@ClassId", SqlDbType.Int)
                .Value = (object?)student.ClassId ?? DBNull.Value;

            connection.Open();

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    "Update failed due to optimistic concurrency conflict — " +
                    "the student record was modified by someone else.");
            }
        }
    }
}
