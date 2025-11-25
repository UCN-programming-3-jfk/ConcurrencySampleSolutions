namespace StudentManagement.Models;

public class SchoolClass
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string RoomNumber { get; set; } = "";

    public int MaxStudents { get; set; }

    // ROWVERSION/TIMESTAMP columns map to byte[]
    public byte[] Timestamp { get; set; } = Array.Empty<byte>();
}
