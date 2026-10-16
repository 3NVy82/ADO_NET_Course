using WebADONET.Data;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connectionString = app.Configuration.GetConnectionString("DefaultConnection")!;
var groupRepository = new GroupRepository(connectionString);

// GET / -> строка со всеми группами
app.MapGet("/", () =>
{
    var groups = groupRepository.GetAll();
    return string.Join(", ", groups.Select(g => g.ToString()));
});

app.Run();
