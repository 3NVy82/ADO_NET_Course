using System.Collections.Generic;
using System.Linq;
using AcademyApp.Models;
using Dapper;
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
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<Group>("SELECT Id, Name FROM Groups").ToList();
            }
        }

        public Group GetById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.QuerySingleOrDefault<Group>(
                    "SELECT Id, Name FROM Groups WHERE Id = @Id", new { Id = id });
            }
        }

        // Добавляет группу и возвращает id созданной строки
        public int CreateGroup(Group group)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Groups (Name) VALUES (@Name);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                int id = connection.QuerySingle<int>(sql, group);
                group.Id = id;
                return id;
            }
        }

        public void UpdateGroup(Group group)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute("UPDATE Groups SET Name = @Name WHERE Id = @Id", group);
            }
        }

        public void DeleteGroup(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute("DELETE FROM Groups WHERE Id = @Id", new { Id = id });
            }
        }
    }
}
