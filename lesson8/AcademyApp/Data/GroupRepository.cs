using System.Collections.Generic;
using AcademyApp.Models;
using Microsoft.Data.SqlClient;

namespace AcademyApp.Data
{
    public class GroupRepository
    {
        private readonly string _connectionString;

        public GroupRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Group> GetAll()
        {
            var groups = new List<Group>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Name FROM Groups", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        groups.Add(new Group(reader.GetInt32(0), reader.GetString(1)));
                    }
                }
            }
            return groups;
        }

        public Group GetById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Name FROM Groups WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        return new Group(reader.GetInt32(0), reader.GetString(1));
                }
            }
            return null;
        }

        public void Add(Group group)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "INSERT INTO Groups (Name) OUTPUT INSERTED.Id VALUES (@Name)", connection);
                command.Parameters.AddWithValue("@Name", group.Name);

                group.Id = (int)command.ExecuteScalar();
            }
        }

        public void Update(Group group)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "UPDATE Groups SET Name = @Name WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Name", group.Name);
                command.Parameters.AddWithValue("@Id", group.Id);

                command.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Groups WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                command.ExecuteNonQuery();
            }
        }
    }
}
