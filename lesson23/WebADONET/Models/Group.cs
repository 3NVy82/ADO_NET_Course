using System.Text.Json.Serialization;

namespace WebADONET.Models
{
    public class Group
    {
        public int Id { get; set; }                 // столбец GroupId
        public string Name { get; set; } = string.Empty; // столбец GroupName

        [JsonIgnore]
        public List<Student> Students { get; set; } = new();

        public override string ToString() => $"{Id}. {Name}";
    }
}
