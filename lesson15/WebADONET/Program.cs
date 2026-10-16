using WebADONET.Data;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connectionString = app.Configuration.GetConnectionString("DefaultConnection")!;
var groupRepository = new GroupRepository(connectionString);
var studentRepository = new StudentRepository(connectionString);

// Раздача wwwroot/index.html по адресу /
app.UseDefaultFiles();
app.UseStaticFiles();

// GET /students/all -> все студенты (JSON), используется в index.html
app.MapGet("/students/all", () => Results.Ok(studentRepository.GetAll()));

// GET /students/{id} -> один студент
app.MapGet("/students/{id:int}", (int id) =>
{
    var student = studentRepository.GetById(id);
    return student is null ? Results.NotFound() : Results.Ok(student);
});

// GET /groups/all -> все группы (JSON)
app.MapGet("/groups/all", () => Results.Ok(groupRepository.GetAll()));

app.Run();
