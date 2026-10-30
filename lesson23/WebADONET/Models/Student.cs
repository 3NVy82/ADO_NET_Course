using System.Text.Json.Serialization;

namespace WebADONET.Models
{
    public class Student
    {
        public int Id { get; set; }                 // столбец StudentId
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int? Age { get; set; }
        public int? GroupId { get; set; }

        [JsonIgnore]
        public Group? Group { get; set; }

        [JsonIgnore]
        public List<Grade> Grades { get; set; } = new();

        public override string ToString() => $"{Id}. {FirstName} {LastName}";
    }
}
