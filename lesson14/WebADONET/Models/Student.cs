namespace WebADONET.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }

        public override string ToString() => $"{Id}. {FirstName} {LastName} ({GroupName})";
    }
}
