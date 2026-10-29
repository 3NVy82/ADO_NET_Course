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

// Студент -> StudentDto (с названием группы)
static StudentDto ToDto(Student s) =>
    new(s.Id, s.FirstName, s.LastName, s.GroupId, s.Group != null ? s.Group.Name : null);

// GET /groups/all -> все группы
app.MapGet("/groups/all", async (CollegeDbContext db) =>
    Results.Ok(await db.Groups.AsNoTracking().OrderBy(g => g.Id).ToListAsync()));

// GET /students/all -> все студенты
app.MapGet("/students/all", async (CollegeDbContext db) =>
{
    var students = await db.Students.AsNoTracking()
        .Include(s => s.Group)
        .OrderBy(s => s.Id)
        .ToListAsync();
    return Results.Ok(students.Select(ToDto));
});

// GET /students/{id} -> один студент
app.MapGet("/students/{id:int}", async (int id, CollegeDbContext db) =>
{
    var student = await db.Students.AsNoTracking()
        .Include(s => s.Group)
        .FirstOrDefaultAsync(s => s.Id == id);

    return student is null
        ? Results.NotFound(new { message = $"Студент с id={id} не найден" })
        : Results.Ok(ToDto(student));
});

// POST /students -> CREATE
app.MapPost("/students", async (Student input, CollegeDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.FirstName) || string.IsNullOrWhiteSpace(input.LastName))
        return Results.BadRequest(new { message = "Имя и фамилия обязательны" });

    var student = new Student
    {
        FirstName = input.FirstName.Trim(),
        LastName = input.LastName.Trim(),
        GroupId = input.GroupId
    };

    db.Students.Add(student);
    await db.SaveChangesAsync();
    await db.Entry(student).Reference(s => s.Group).LoadAsync();

    return Results.Created($"/students/{student.Id}", ToDto(student));
});

// PUT /students/{id} -> UPDATE (Entity Framework)
app.MapPut("/students/{id:int}", async (int id, Student input, CollegeDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.FirstName) || string.IsNullOrWhiteSpace(input.LastName))
        return Results.BadRequest(new { message = "Имя и фамилия обязательны" });

    var student = await db.Students.FindAsync(id);
    if (student is null)
        return Results.NotFound(new { message = $"Студент с id={id} не найден" });

    student.FirstName = input.FirstName.Trim();
    student.LastName = input.LastName.Trim();
    student.GroupId = input.GroupId;

    await db.SaveChangesAsync();
    await db.Entry(student).Reference(s => s.Group).LoadAsync();

    return Results.Ok(ToDto(student));
});

// DELETE /students/{id} -> DELETE (Entity Framework)
app.MapDelete("/students/{id:int}", async (int id, CollegeDbContext db) =>
{
    var student = await db.Students.FindAsync(id);
    if (student is null)
        return Results.NotFound(new { message = $"Студент с id={id} не найден" });

    try
    {
        db.Students.Remove(student);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
    {
        // нарушение внешнего ключа (например, у студента есть оценки)
        return Results.Conflict(new { message = "Нельзя удалить студента: на него ссылаются связанные записи (например, оценки)" });
    }
});

app.Run();
