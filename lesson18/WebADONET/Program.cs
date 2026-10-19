using Microsoft.Data.SqlClient;
using WebADONET.Data;
using WebADONET.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connectionString = app.Configuration.GetConnectionString("DefaultConnection")!;
var groupRepository = new GroupRepository(connectionString);
var studentRepository = new StudentRepository(connectionString);

// Раздача wwwroot/index.html по адресу /
app.UseDefaultFiles();
app.UseStaticFiles();

// GET /students/all -> все студенты (JSON)
app.MapGet("/students/all", () => Results.Ok(studentRepository.GetAll()));

// GET /student/{id} -> один студент по id (JSON), 404 если не найден
app.MapGet("/student/{id:int}", (int id) =>
{
    var student = studentRepository.GetById(id);
    return student is null
        ? Results.NotFound(new { message = $"Студент с id={id} не найден" })
        : Results.Ok(student);
});

// POST /student -> CREATE
app.MapPost("/student", (Student student) =>
{
    if (string.IsNullOrWhiteSpace(student.FirstName) || string.IsNullOrWhiteSpace(student.LastName))
        return Results.BadRequest(new { message = "Имя и фамилия обязательны" });

    student.FirstName = student.FirstName.Trim();
    student.LastName = student.LastName.Trim();
    student.Id = studentRepository.Create(student);

    return Results.Created($"/student/{student.Id}", studentRepository.GetById(student.Id));
});

// PUT /student/{id} -> UPDATE
app.MapPut("/student/{id:int}", (int id, Student student) =>
{
    if (string.IsNullOrWhiteSpace(student.FirstName) || string.IsNullOrWhiteSpace(student.LastName))
        return Results.BadRequest(new { message = "Имя и фамилия обязательны" });

    student.Id = id;
    student.FirstName = student.FirstName.Trim();
    student.LastName = student.LastName.Trim();

    if (!studentRepository.Update(student))
        return Results.NotFound(new { message = $"Студент с id={id} не найден" });

    return Results.Ok(studentRepository.GetById(id));
});

// DELETE /student/{id} -> DELETE
app.MapDelete("/student/{id:int}", (int id) =>
{
    try
    {
        return studentRepository.Delete(id)
            ? Results.NoContent()
            : Results.NotFound(new { message = $"Студент с id={id} не найден" });
    }
    catch (SqlException ex) when (ex.Number == 547)
    {
        // нарушение внешнего ключа (например, у студента есть оценки)
        return Results.Conflict(new { message = "Нельзя удалить студента: на него ссылаются связанные записи (например, оценки)" });
    }
});

// GET /groups/all -> все группы (JSON)
app.MapGet("/groups/all", () => Results.Ok(groupRepository.GetAll()));

app.Run();
