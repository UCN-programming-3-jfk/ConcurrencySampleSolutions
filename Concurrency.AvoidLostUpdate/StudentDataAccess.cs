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
    /// in the DB still match the original values from when the Student object was first created.
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

                WHERE Id = @OriginalId
                  AND FirstName = @OriginalFirstName
                  AND LastName  = @OriginalLastName
                  AND Email     = @OriginalEmail
                  AND (
                        (Class_Id = @OriginalClassId) 
                        OR (Class_Id IS NULL AND @OriginalClassId IS NULL)
                      );";

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand(sql, connection))
        {
            // Use original values for WHERE clause conditions
            command.Parameters.Add("@OriginalId", SqlDbType.Int).Value = student.OriginalId;
            command.Parameters.Add("@OriginalFirstName", SqlDbType.NVarChar, 150).Value = student.OriginalFirstName;
            command.Parameters.Add("@OriginalLastName", SqlDbType.NVarChar, 150).Value = student.OriginalLastName;
            command.Parameters.Add("@OriginalEmail", SqlDbType.NVarChar, 255).Value = student.OriginalEmail;
            command.Parameters.Add("@OriginalClassId", SqlDbType.Int)
                .Value = (object?)student.OriginalClassId ?? DBNull.Value;

            // Use current values for SET clause (the new values to update to)
            command.Parameters.Add("@FirstName", SqlDbType.NVarChar, 150).Value = student.FirstName;
            command.Parameters.Add("@LastName", SqlDbType.NVarChar, 150).Value = student.LastName;
            command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = student.Email;
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
