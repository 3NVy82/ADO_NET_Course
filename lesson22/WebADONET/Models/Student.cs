using System.Text.Json.Serialization;

namespace WebADONET.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int? GroupId { get; set; }

        [JsonIgnore]
        public Group? Group { get; set; }

        public override string ToString() => $"{Id}. {FirstName} {LastName}";
    }

    // Что отдаём клиенту: студент вместе с названием группы
    public record StudentDto(int Id, string FirstName, string LastName, int? GroupId, string? GroupName);
}
