using System.Data.SqlClient;

// Строка подключения. Замените Server на имя вашего сервера из обозревателя объектов SQL Server:
//   (localdb)\MSSQLLocalDB  |  .\SQLEXPRESS  |  localhost
string connectionString =
    @"Server=(localdb)\MSSQLLocalDB;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";

try
{
    // 1. SqlConnection - соединение с сервером
    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();
        Console.WriteLine("Подключение открыто");
        Console.WriteLine($"Сервер: {connection.DataSource}, версия: {connection.ServerVersion}");
        Console.WriteLine();

        // 2. SqlCommand - команда с запросом SELECT
        string sql = "SELECT name, database_id FROM sys.databases";
        SqlCommand command = new SqlCommand(sql, connection);

        // 3. SqlDataReader - чтение результата
        using (SqlDataReader reader = command.ExecuteReader())
        {
            Console.WriteLine("ID\tИмя базы данных");
            Console.WriteLine("--\t---------------");

            while (reader.Read())
            {
                Console.WriteLine($"{reader["database_id"]}\t{reader["name"]}");
            }
        }
    }

    Console.WriteLine();
    Console.WriteLine("Подключение закрыто");
}
catch (SqlException ex)
{
    Console.WriteLine("Ошибка SQL: " + ex.Message);
}
catch (Exception ex)
{
    Console.WriteLine("Ошибка: " + ex.Message);
}

Console.WriteLine("Нажмите любую клавишу для выхода...");
Console.ReadKey();
