using Dapper;
using Microsoft.Data.SqlClient;
using WebADONET.Models;

namespace WebADONET.Data
{
    public class StudentRepository
    {
        private readonly string _connectionString;

        public StudentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private const string BaseSql = @"
            SELECT s.Id, s.FirstName, s.LastName, s.GroupId, g.Name AS GroupName
            FROM Students s
            LEFT JOIN Groups g ON g.Id = s.GroupId";

        public IEnumerable<Student> GetAll()
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.Query<Student>(BaseSql).ToList();
        }

        public Student? GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.QueryFirstOrDefault<Student>(BaseSql + " WHERE s.Id = @Id", new { Id = id });
        }

        public IEnumerable<Student> GetByGroup(int groupId)
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.Query<Student>(BaseSql + " WHERE s.GroupId = @GroupId", new { GroupId = groupId }).ToList();
        }
        // CREATE: добавляет студента и возвращает его новый Id
        public int Create(Student student)
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.QuerySingle<int>(@"
                INSERT INTO Students (FirstName, LastName, GroupId)
                VALUES (@FirstName, @LastName, @GroupId);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", student);
        }
    }
}
