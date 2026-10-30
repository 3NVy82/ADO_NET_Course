using System.Text.Json.Serialization;

namespace WebADONET.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public int Value { get; set; }

        [JsonIgnore]
        public Student? Student { get; set; }

        [JsonIgnore]
        public Subject? Subject { get; set; }

        public override string ToString() => $"{Id}. Student {StudentId}, Subject {SubjectId}: {Value}";
    }
}
