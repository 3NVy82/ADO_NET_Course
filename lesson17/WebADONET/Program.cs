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

// POST /student -> CREATE: создаёт студента из JSON в теле запроса
app.MapPost("/student", (Student student) =>
{
    if (string.IsNullOrWhiteSpace(student.FirstName) || string.IsNullOrWhiteSpace(student.LastName))
        return Results.BadRequest(new { message = "Имя и фамилия обязательны" });

    student.FirstName = student.FirstName.Trim();
    student.LastName = student.LastName.Trim();
    student.Id = studentRepository.Create(student);

    var created = studentRepository.GetById(student.Id);
    return Results.Created($"/student/{student.Id}", created);
});

// GET /groups/all -> все группы (JSON)
app.MapGet("/groups/all", () => Results.Ok(groupRepository.GetAll()));

app.Run();
