using System.Text.Json.Serialization;

namespace WebADONET.Models
{
    public class Grade
    {
        public int Id { get; set; }                 // столбец GradeId
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public int Score { get; set; }
        public DateTime GradeDate { get; set; }

        [JsonIgnore]
        public Student? Student { get; set; }

        [JsonIgnore]
        public Subject? Subject { get; set; }

        public override string ToString() => $"{Id}. Student {StudentId}, Subject {SubjectId}: {Score} ({GradeDate:yyyy-MM-dd})";
    }
}
