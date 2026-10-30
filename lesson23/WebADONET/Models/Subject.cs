using System.Text.Json.Serialization;

namespace WebADONET.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? TeacherId { get; set; }

        [JsonIgnore]
        public Teacher? Teacher { get; set; }

        [JsonIgnore]
        public List<Grade> Grades { get; set; } = new();

        public override string ToString() => $"{Id}. {Name}";
    }
}
