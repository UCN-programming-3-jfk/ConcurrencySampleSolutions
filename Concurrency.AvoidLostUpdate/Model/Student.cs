namespace ConcurrencySampleSolutions.DataAccess.Optimistic.RowUpdate.Model;
public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public int? ClassId { get; set; }

    // Read-only originals captured when the instance is constructed
    public int OriginalId { get; }
    public string OriginalFirstName { get; }
    public string OriginalLastName { get; }
    public string OriginalEmail { get; }
    public int? OriginalClassId { get; }

    // Parameterless constructor (preserves implicit behavior and captures defaults)
    public Student()
    {
        OriginalId = Id;
        OriginalFirstName = FirstName;
        OriginalLastName = LastName;
        OriginalEmail = Email;
        OriginalClassId = ClassId;
    }

    // Constructor that accepts all properties and captures their original values
    public Student(int id, string firstName, string lastName, string email, int? classId)
    {
        OriginalId = Id = id;
        OriginalFirstName = FirstName = firstName ?? "";
        OriginalLastName = LastName = lastName ?? "";
        OriginalEmail = Email = email ?? "";
        OriginalClassId = ClassId = classId;
    }
}