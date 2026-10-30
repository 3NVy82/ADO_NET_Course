using System.Text.Json.Serialization;

namespace WebADONET.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        [JsonIgnore]
        public List<Subject> Subjects { get; set; } = new();

        public override string ToString() => $"{Id}. {FirstName} {LastName}";
    }
}
