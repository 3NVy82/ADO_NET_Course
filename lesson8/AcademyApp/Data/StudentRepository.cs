using System;
using System.Collections.Generic;
using AcademyApp.Models;
using Microsoft.Data.SqlClient;
using System.Data;

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

        private static Student Map(SqlDataReader reader)
        {
            return new Student(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4));
        }

        public List<Student> GetAll()
        {
            var students = new List<Student>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(SelectQuery, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        students.Add(Map(reader));
                }
            }
            return students;
        }

        public Student GetById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(SelectQuery + " WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        return Map(reader);
                }
            }
            return null;
        }

        public List<Student> GetByName(string name)
        {
            var students = new List<Student>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    SelectQuery + @" WHERE FirstName = @Name
                               OR LastName = @Name
                               OR FirstName + N' ' + LastName = @Name", connection);
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        students.Add(Map(reader));
                }
            }
            return students;
        }

        public void Add(Student student)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    @"INSERT INTO Students (FirstName, LastName, Age, GroupId)
                      OUTPUT INSERTED.Id
                      VALUES (@FirstName, @LastName, @Age, @GroupId)", connection);
                command.Parameters.AddWithValue("@FirstName", student.FirstName);
                command.Parameters.AddWithValue("@LastName", student.LastName);
                command.Parameters.AddWithValue("@Age", student.Age);
                command.Parameters.AddWithValue("@GroupId", (object)student.GroupId ?? DBNull.Value);

                student.Id = (int)command.ExecuteScalar();
            }
        }

        public void Update(Student student)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    @"UPDATE Students
                      SET FirstName = @FirstName, LastName = @LastName,
                          Age = @Age, GroupId = @GroupId
                      WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@FirstName", student.FirstName);
                command.Parameters.AddWithValue("@LastName", student.LastName);
                command.Parameters.AddWithValue("@Age", student.Age);
                command.Parameters.AddWithValue("@GroupId", (object)student.GroupId ?? DBNull.Value);
                command.Parameters.AddWithValue("@Id", student.Id);

                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Students WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                command.ExecuteNonQuery();
            }
        }
    }
}
