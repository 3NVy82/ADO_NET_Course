namespace AcademyApp.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public int? GroupId { get; set; }

        public Student() { }

        public Student(int id, string firstName, string lastName, int age, int? groupId)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            GroupId = groupId;
        }

        public override string ToString()
        {
            return "студент " + FirstName + " " + LastName + " : " + Age + " лет";
        }
    }
}
