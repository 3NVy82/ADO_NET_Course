using System;
using System.Text;
using AcademyApp.Data;

namespace AcademyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string connectionString =
                @"Server=(localdb)\MSSQLLocalDB;Database=CollegeDB;Trusted_Connection=True;TrustServerCertificate=True;";
            try
            {
                DatabaseInitializer.Initialize(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось подготовить базу данных: " + ex.Message);
                Console.WriteLine("Нажмите любую кнопку для выхода!");
                Console.ReadKey();
                return;
            }

            var studentRepo = new StudentRepository(connectionString);
            var groupRepo = new GroupRepository(connectionString);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("1 Просмотреть всех студентов");
                Console.WriteLine("2 Найти студента по имени");
                Console.WriteLine("3 Просмотреть все группы");
                Console.WriteLine("0 Выход");

                string choice = Console.ReadLine();

                if (choice == "0")
                    break;

                try
                {
                    switch (choice)
                    {
                        case "1":
                            foreach (var student in studentRepo.GetAll())
                                Console.WriteLine(student);
                            break;

                        case "2":
                            Console.Write("Введите имя: ");
                            string name = (Console.ReadLine() ?? "").Trim();

                            if (name.Length == 0)
                            {
                                Console.WriteLine("Имя не введено");
                                break;
                            }

                            var found = studentRepo.GetByName(name);

                            if (found.Count == 0)
                                Console.WriteLine("Студент не найден");
                            else
                                foreach (var student in found)
                                    Console.WriteLine(student);
                            break;

                        case "3":
                            foreach (var group in groupRepo.GetAll())
                                Console.WriteLine(group);
                            break;

                        default:
                            Console.WriteLine("Неверный пункт меню");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message);
                }

                Console.WriteLine("Нажмите любую кнопку для продолжения!");
                Console.ReadKey();
            }
        }
    }
}
