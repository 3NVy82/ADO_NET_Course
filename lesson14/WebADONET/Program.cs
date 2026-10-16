using WebADONET.Data;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connectionString = app.Configuration.GetConnectionString("DefaultConnection")!;
var groupRepository = new GroupRepository(connectionString);
var studentRepository = new StudentRepository(connectionString);

// Раздача wwwroot/index.html по адресу /
app.UseDefaultFiles();
app.UseStaticFiles();

// GET /api/groups -> список групп (JSON)
app.MapGet("/api/groups", () => Results.Ok(groupRepository.GetAll()));

// GET /api/students -> все студенты (JSON)
app.MapGet("/api/students", () => Results.Ok(studentRepository.GetAll()));

// GET /api/students/5 -> один студент
app.MapGet("/api/students/{id:int}", (int id) =>
{
    var student = studentRepository.GetById(id);
    return student is null ? Results.NotFound() : Results.Ok(student);
});

// GET /api/groups/2/students -> студенты группы
app.MapGet("/api/groups/{groupId:int}/students", (int groupId) =>
    Results.Ok(studentRepository.GetByGroup(groupId)));

app.Run();
