namespace WebADONET.Models
{
    // Что отдаём клиенту: данные вместе с названиями связанных записей
    public record StudentDto(int Id, string FirstName, string LastName, int? Age, int? GroupId, string? GroupName);

    public record SubjectDto(int Id, string Name, int? TeacherId, string? TeacherName);

    public record GradeDto(int Id, int StudentId, string StudentName, int SubjectId, string SubjectName, int Score, string GradeDate);
}
