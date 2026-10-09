using System;
using System.Text;
using AcademyApp.Data;
using AcademyApp.Models;

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
                Console.WriteLine("4 Добавить студента");
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

                        case "4":
                            // заполняем модель студента и передаём её в CreateStudent(Student)
                            Student newStudent = ReadStudentFromConsole(groupRepo);
                            int newId = studentRepo.CreateStudent(newStudent);

                            Console.WriteLine("Студент добавлен, id = " + newId);
                            Console.WriteLine(newStudent);
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

        // Метод заполняет модель Student данными из консоли
        static Student ReadStudentFromConsole(GroupRepository groupRepo)
        {
            var student = new Student();

            student.FirstName = ReadNotEmpty("Введите имя: ");
            student.LastName = ReadNotEmpty("Введите фамилию: ");
            student.Age = ReadAge();

            Console.WriteLine("Доступные группы:");
            foreach (var group in groupRepo.GetAll())
                Console.WriteLine(group.Id + " " + group);

            student.GroupId = ReadGroupId(groupRepo);

            return student;
        }

        static string ReadNotEmpty(string message)
        {
            while (true)
            {
                Console.Write(message);
                string value = (Console.ReadLine() ?? "").Trim();

                if (value.Length > 0)
                    return value;

                Console.WriteLine("Поле не может быть пустым");
            }
        }

        static int ReadAge()
        {
            while (true)
            {
                Console.Write("Введите возраст: ");
                string value = (Console.ReadLine() ?? "").Trim();

                if (int.TryParse(value, out int age) && age >= 10 && age <= 100)
                    return age;

                Console.WriteLine("Введите число от 10 до 100");
            }
        }

        static int? ReadGroupId(GroupRepository groupRepo)
        {
            while (true)
            {
                Console.Write("Введите номер группы (Enter - без группы): ");
                string value = (Console.ReadLine() ?? "").Trim();

                if (value.Length == 0)
                    return null;

                if (int.TryParse(value, out int id) && groupRepo.GetById(id) != null)
                    return id;

                Console.WriteLine("Такой группы нет");
            }
        }
    }
}
