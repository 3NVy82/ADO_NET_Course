using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebADONET.Data;
using WebADONET.Models;

var builder = WebApplication.CreateBuilder(args);

// Подключаем Entity Framework DbContext
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddDbContext<CollegeDbContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

// Раздача wwwroot/index.html по адресу /
app.UseDefaultFiles();
app.UseStaticFiles();

// ---------- Вспомогательные методы ----------

static StudentDto StudentToDto(Student s) =>
    new(s.Id, s.FirstName, s.LastName, s.Age, s.GroupId, s.Group != null ? s.Group.Name : null);

static SubjectDto SubjectToDto(Subject s) =>
    new(s.Id, s.Name, s.TeacherId,
        s.Teacher != null ? s.Teacher.FirstName + " " + s.Teacher.LastName : null);

static GradeDto GradeToDto(Grade g) =>
    new(g.Id, g.StudentId,
        g.Student != null ? g.Student.FirstName + " " + g.Student.LastName : "",
        g.SubjectId,
        g.Subject != null ? g.Subject.Name : "",
        g.Score,
        g.GradeDate.ToString("yyyy-MM-dd"));

static Task<Student?> LoadStudent(CollegeDbContext db, int id) =>
    db.Students.AsNoTracking().Include(s => s.Group).FirstOrDefaultAsync(s => s.Id == id);

static Task<Subject?> LoadSubject(CollegeDbContext db, int id) =>
    db.Subjects.AsNoTracking().Include(s => s.Teacher).FirstOrDefaultAsync(s => s.Id == id);

static Task<Grade?> LoadGrade(CollegeDbContext db, int id) =>
    db.Grades.AsNoTracking().Include(g => g.Student).Include(g => g.Subject).FirstOrDefaultAsync(g => g.Id == id);

// Общее удаление: 404 если нет записи, 409 если на неё ссылаются другие таблицы
static async Task<IResult> DeleteEntity<T>(CollegeDbContext db, int id, string notFound, string conflict) where T : class
{
    var entity = await db.Set<T>().FindAsync(id);
    if (entity is null)
        return Results.NotFound(new { message = notFound });

    try
    {
        db.Set<T>().Remove(entity);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
    {
        return Results.Conflict(new { message = conflict });
    }
}

// ---------- GROUPS ----------

app.MapGet("/groups/all", async (CollegeDbContext db) =>
    Results.Ok(await db.Groups.AsNoTracking().OrderBy(g => g.Id).ToListAsync()));

app.MapGet("/groups/{id:int}", async (int id, CollegeDbContext db) =>
{
    var group = await db.Groups.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
    return group is null
        ? Results.NotFound(new { message = $"Группа с id={id} не найдена" })
        : Results.Ok(group);
});

app.MapPost("/groups", async (Group input, CollegeDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest(new { message = "Название группы обязательно" });

    var group = new Group { Name = input.Name.Trim() };
    db.Groups.Add(group);
    await db.SaveChangesAsync();
    return Results.Created($"/groups/{group.Id}", group);
});

app.MapPut("/groups/{id:int}", async (int id, Group input, CollegeDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest(new { message = "Название группы обязательно" });

    var group = await db.Groups.FindAsync(id);
    if (group is null)
        return Results.NotFound(new { message = $"Группа с id={id} не найдена" });

    group.Name = input.Name.Trim();
    await db.SaveChangesAsync();
    return Results.Ok(group);
});

app.MapDelete("/groups/{id:int}", (int id, CollegeDbContext db) =>
    DeleteEntity<Group>(db, id,
        $"Группа с id={id} не найдена",
        "Нельзя удалить группу: в ней есть студенты"));

// ---------- TEACHERS ----------

app.MapGet("/teachers/all", async (CollegeDbContext db) =>
    Results.Ok(await db.Teachers.AsNoTracking().OrderBy(t => t.Id).ToListAsync()));

app.MapGet("/teachers/{id:int}", async (int id, CollegeDbContext db) =>
{
    var teacher = await db.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
    return teacher is null
        ? Results.NotFound(new { message = $"Преподаватель с id={id} не найден" })
        : Results.Ok(teacher);
});

app.MapPost("/teachers", async (Teacher input, CollegeDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.FirstName) || string.IsNullOrWhiteSpace(input.LastName))
        return Results.BadRequest(new { message = "Имя и фамилия обязательны" });

    var teacher = new Teacher { FirstName = input.FirstName.Trim(), LastName = input.LastName.Trim() };
    db.Teachers.Add(teacher);
    await db.SaveChangesAsync();
    return Results.Created($"/teachers/{teacher.Id}", teacher);
});

app.MapPut("/teachers/{id:int}", async (int id, Teacher input, CollegeDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.FirstName) || string.IsNullOrWhiteSpace(input.LastName))
        return Results.BadRequest(new { message = "Имя и фамилия обязательны" });

    var teacher = await db.Teachers.FindAsync(id);
    if (teacher is null)
        return Results.NotFound(new { message = $"Преподаватель с id={id} не найден" });

    teacher.FirstName = input.FirstName.Trim();
    teacher.LastName = input.LastName.Trim();
    await db.SaveChangesAsync();
    return Results.Ok(teacher);
});

app.MapDelete("/teachers/{id:int}", (int id, CollegeDbContext db) =>
    DeleteEntity<Teacher>(db, id,
        $"Преподаватель с id={id} не найден",
        "Нельзя удалить преподавателя: за ним закреплены предметы"));

// ---------- SUBJECTS ----------

app.MapGet("/subjects/all", async (CollegeDbContext db) =>
{
    var subjects = await db.Subjects.AsNoTracking().Include(s => s.Teacher).OrderBy(s => s.Id).ToListAsync();
    return Results.Ok(subjects.Select(SubjectToDto));
});

app.MapGet("/subjects/{id:int}", async (int id, CollegeDbContext db) =>
{
    var subject = await LoadSubject(db, id);
    return subject is null
        ? Results.NotFound(new { message = $"Предмет с id={id} не найден" })
        : Results.Ok(SubjectToDto(subject));
});

app.MapPost("/subjects", async (Subject input, CollegeDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest(new { message = "Название предмета обязательно" });
    if (input.TeacherId != null && !await db.Teachers.AnyAsync(t => t.Id == input.TeacherId))
        return Results.BadRequest(new { message = "Указанного преподавателя не существует" });

    var subject = new Subject { Name = input.Name.Trim(), TeacherId = input.TeacherId };
    db.Subjects.Add(subject);
    await db.SaveChangesAsync();
    return Results.Created($"/subjects/{subject.Id}", SubjectToDto((await LoadSubject(db, subject.Id))!));
});

app.MapPut("/subjects/{id:int}", async (int id, Subject input, CollegeDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest(new { message = "Название предмета обязательно" });
    if (input.TeacherId != null && !await db.Teachers.AnyAsync(t => t.Id == input.TeacherId))
        return Results.BadRequest(new { message = "Указанного преподавателя не существует" });

    var subject = await db.Subjects.FindAsync(id);
    if (subject is null)
        return Results.NotFound(new { message = $"Предмет с id={id} не найден" });

    subject.Name = input.Name.Trim();
    subject.TeacherId = input.TeacherId;
    await db.SaveChangesAsync();
    return Results.Ok(SubjectToDto((await LoadSubject(db, id))!));
});

app.MapDelete("/subjects/{id:int}", (int id, CollegeDbContext db) =>
    DeleteEntity<Subject>(db, id,
        $"Предмет с id={id} не найден",
        "Нельзя удалить предмет: по нему есть оценки"));

// ---------- STUDENTS ----------

app.MapGet("/students/all", async (CollegeDbContext db) =>
{
    var students = await db.Students.AsNoTracking().Include(s => s.Group).OrderBy(s => s.Id).ToListAsync();
    return Results.Ok(students.Select(StudentToDto));
});

app.MapGet("/students/{id:int}", async (int id, CollegeDbContext db) =>
{
    var student = await LoadStudent(db, id);
    return student is null
        ? Results.NotFound(new { message = $"Студент с id={id} не найден" })
        : Results.Ok(StudentToDto(student));
});

app.MapPost("/students", async (Student input, CollegeDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.FirstName) || string.IsNullOrWhiteSpace(input.LastName))
        return Results.BadRequest(new { message = "Имя и фамилия обязательны" });
    if (input.GroupId != null && !await db.Groups.AnyAsync(g => g.Id == input.GroupId))
        return Results.BadRequest(new { message = "Указанной группы не существует" });

    var student = new Student
    {
        FirstName = input.FirstName.Trim(),
        LastName = input.LastName.Trim(),
        Age = input.Age,
        GroupId = input.GroupId
    };
    db.Students.Add(student);
    await db.SaveChangesAsync();
    return Results.Created($"/students/{student.Id}", StudentToDto((await LoadStudent(db, student.Id))!));
});

app.MapPut("/students/{id:int}", async (int id, Student input, CollegeDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.FirstName) || string.IsNullOrWhiteSpace(input.LastName))
        return Results.BadRequest(new { message = "Имя и фамилия обязательны" });
    if (input.GroupId != null && !await db.Groups.AnyAsync(g => g.Id == input.GroupId))
        return Results.BadRequest(new { message = "Указанной группы не существует" });

    var student = await db.Students.FindAsync(id);
    if (student is null)
        return Results.NotFound(new { message = $"Студент с id={id} не найден" });

    student.FirstName = input.FirstName.Trim();
    student.LastName = input.LastName.Trim();
    student.Age = input.Age;
    student.GroupId = input.GroupId;
    await db.SaveChangesAsync();
    return Results.Ok(StudentToDto((await LoadStudent(db, id))!));
});

app.MapDelete("/students/{id:int}", (int id, CollegeDbContext db) =>
    DeleteEntity<Student>(db, id,
        $"Студент с id={id} не найден",
        "Нельзя удалить студента: у него есть оценки"));

// ---------- GRADES ----------

app.MapGet("/grades/all", async (CollegeDbContext db) =>
{
    var grades = await db.Grades.AsNoTracking()
        .Include(g => g.Student).Include(g => g.Subject)
        .OrderBy(g => g.Id).ToListAsync();
    return Results.Ok(grades.Select(GradeToDto));
});

app.MapGet("/grades/{id:int}", async (int id, CollegeDbContext db) =>
{
    var grade = await LoadGrade(db, id);
    return grade is null
        ? Results.NotFound(new { message = $"Оценка с id={id} не найдена" })
        : Results.Ok(GradeToDto(grade));
});

app.MapPost("/grades", async (Grade input, CollegeDbContext db) =>
{
    if (!await db.Students.AnyAsync(s => s.Id == input.StudentId))
        return Results.BadRequest(new { message = "Указанного студента не существует" });
    if (!await db.Subjects.AnyAsync(s => s.Id == input.SubjectId))
        return Results.BadRequest(new { message = "Указанного предмета не существует" });
    if (input.Score < 0)
        return Results.BadRequest(new { message = "Оценка не может быть отрицательной" });

    var grade = new Grade
    {
        StudentId = input.StudentId,
        SubjectId = input.SubjectId,
        Score = input.Score,
        GradeDate = input.GradeDate.Date
    };
    db.Grades.Add(grade);
    await db.SaveChangesAsync();
    return Results.Created($"/grades/{grade.Id}", GradeToDto((await LoadGrade(db, grade.Id))!));
});

app.MapPut("/grades/{id:int}", async (int id, Grade input, CollegeDbContext db) =>
{
    if (!await db.Students.AnyAsync(s => s.Id == input.StudentId))
        return Results.BadRequest(new { message = "Указанного студента не существует" });
    if (!await db.Subjects.AnyAsync(s => s.Id == input.SubjectId))
        return Results.BadRequest(new { message = "Указанного предмета не существует" });
    if (input.Score < 0)
        return Results.BadRequest(new { message = "Оценка не может быть отрицательной" });

    var grade = await db.Grades.FindAsync(id);
    if (grade is null)
        return Results.NotFound(new { message = $"Оценка с id={id} не найдена" });

    grade.StudentId = input.StudentId;
    grade.SubjectId = input.SubjectId;
    grade.Score = input.Score;
    grade.GradeDate = input.GradeDate.Date;
    await db.SaveChangesAsync();
    return Results.Ok(GradeToDto((await LoadGrade(db, id))!));
});

app.MapDelete("/grades/{id:int}", (int id, CollegeDbContext db) =>
    DeleteEntity<Grade>(db, id,
        $"Оценка с id={id} не найдена",
        "Нельзя удалить оценку"));

app.Run();
