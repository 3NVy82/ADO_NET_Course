using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace AdoNetLesson4
{
    internal class Program
    {
        // Замените Server на имя вашего сервера из обозревателя объектов SQL Server:
        //   (localdb)\MSSQLLocalDB  |  .\SQLEXPRESS  |  localhost
        // База данных ShopDb создаётся скриптом setup.sql
        private const string ConnectionString =
            @"Server=(localdb)\MSSQLLocalDB;Database=ShopDb;Trusted_Connection=True;TrustServerCertificate=True;";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            bool exit = false;

            // Повторяющийся цикл: меню показывается, пока пользователь не выберет выход
            while (!exit)
            {
                ShowMenu();
                string? choice = Console.ReadLine()?.Trim();
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            ShowAllProducts();
                            break;
                        case "2":
                            ShowProductById();
                            break;
                        case "3":
                            AddProduct();
                            break;
                        case "0":
                            exit = true;
                            Console.WriteLine("Выход из программы.");
                            break;
                        default:
                            Console.WriteLine("Неверный пункт меню, попробуйте ещё раз.");
                            break;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Ошибка работы с базой данных: " + ex.Message);
                }

                if (!exit)
                {
                    Console.WriteLine();
                    Console.WriteLine("Нажмите Enter, чтобы продолжить...");
                    Console.ReadLine();
                }
            }
        }

        private static void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine("=== Магазин: управление товарами ===");
            Console.WriteLine("1 - Показать все товары");
            Console.WriteLine("2 - Показать товар по id");
            Console.WriteLine("3 - Добавить товар");
            Console.WriteLine("0 - Выход");
            Console.Write("Ваш выбор: ");
        }

        // Логика из прошлого урока (SqlConnection -> SqlCommand -> SqlDataReader)
        private static void ShowAllProducts()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string sql = "SELECT Id, Name, Price, Quantity FROM Products ORDER BY Id";
                SqlCommand command = new SqlCommand(sql, connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        Console.WriteLine("Таблица Products пуста.");
                        return;
                    }

                    PrintHeader();
                    while (reader.Read())
                    {
                        PrintProduct(reader);
                    }
                }
            }
        }

        // Показ товара по id (запрос с параметром)
        private static void ShowProductById()
        {
            int id = ReadInt("Введите id товара: ");

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string sql = "SELECT Id, Name, Price, Quantity FROM Products WHERE Id = @Id";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        Console.WriteLine($"Товар с id = {id} не найден.");
                        return;
                    }

                    PrintHeader();
                    PrintProduct(reader);
                }
            }
        }

        // Добавление товара (INSERT с параметрами - защита от SQL-инъекций)
        private static void AddProduct()
        {
            Console.Write("Название товара: ");
            string? name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Название не может быть пустым.");
                return;
            }

            decimal price = ReadDecimal("Цена: ");
            int quantity = ReadInt("Количество: ");

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                string sql = "INSERT INTO Products (Name, Price, Quantity) VALUES (@Name, @Price, @Quantity)";
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
                command.Parameters.Add("@Price", SqlDbType.Decimal).Value = price;
                command.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;

                int rows = command.ExecuteNonQuery();
                Console.WriteLine($"Добавлено строк: {rows}");
            }
        }

        // ---------- Вспомогательные методы ----------

        private static void PrintHeader()
        {
            Console.WriteLine($"{"Id",-5}{"Название",-30}{"Цена",-12}{"Кол-во",-8}");
            Console.WriteLine(new string('-', 55));
        }

        private static void PrintProduct(SqlDataReader reader)
        {
            int id = reader.GetInt32(0);
            string name = reader.GetString(1);
            decimal price = reader.GetDecimal(2);
            int quantity = reader.GetInt32(3);

            Console.WriteLine($"{id,-5}{name,-30}{price,-12:F2}{quantity,-8}");
        }

        private static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int value) && value >= 0)
                    return value;

                Console.WriteLine("Введите целое неотрицательное число.");
            }
        }

        private static decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine() ?? "";

                if ((decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal value) ||
                     decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out value))
                    && value >= 0)
                    return value;

                Console.WriteLine("Введите число (например, 199.99).");
            }
        }
    }
}
