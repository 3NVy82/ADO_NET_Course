using WebADONET.Data;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

string connectionString = app.Configuration.GetConnectionString("DefaultConnection")!;
var groupRepository = new GroupRepository(connectionString);
var studentRepository = new StudentRepository(connectionString);

// GET /groups -> строка со всеми группами
app.MapGet("/groups", () =>
{
    var groups = groupRepository.GetAll();
    return string.Join(", ", groups.Select(g => g.ToString()));
});

// GET /students -> строка со всеми студентами
app.MapGet("/students", () =>
{
    var students = studentRepository.GetAll();
    return string.Join(", ", students.Select(s => s.ToString()));
});

app.Run();
