using System.Collections.Generic;
using System.Linq;
using AcademyApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AcademyApp.Data
{
    public class StudentRepository
    {
        private readonly string _connectionString;

        private const string SelectQuery =
            "SELECT Id, FirstName, LastName, Age, GroupId FROM Students";

        public StudentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Student> GetAll()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<Student>(SelectQuery).ToList();
            }
        }

        public Student GetById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.QuerySingleOrDefault<Student>(
                    SelectQuery + " WHERE Id = @Id", new { Id = id });
            }
        }

        public List<Student> GetByName(string name)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<Student>(
                    SelectQuery + @" WHERE FirstName = @Name
                                       OR LastName = @Name
                                       OR FirstName + N' ' + LastName = @Name",
                    new { Name = name }).ToList();
            }
        }

        // Добавляет студента и возвращает id созданной строки
        public int CreateStudent(Student student)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Students (FirstName, LastName, Age, GroupId)
                               VALUES (@FirstName, @LastName, @Age, @GroupId);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int id = connection.QuerySingle<int>(sql, student);
                student.Id = id;
                return id;
            }
        }

        public void UpdateStudent(Student student)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute(
                    @"UPDATE Students
                      SET FirstName = @FirstName, LastName = @LastName,
                          Age = @Age, GroupId = @GroupId
                      WHERE Id = @Id", student);
            }
        }

        public void DeleteStudent(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute("DELETE FROM Students WHERE Id = @Id", new { Id = id });
            }
        }
    }
}
