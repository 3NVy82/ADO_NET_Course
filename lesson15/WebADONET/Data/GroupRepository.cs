using Dapper;
using Microsoft.Data.SqlClient;
using WebADONET.Models;

namespace WebADONET.Data
{
    public class GroupRepository
    {
        private readonly string _connectionString;

        public GroupRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Group> GetAll()
        {
            using var connection = new SqlConnection(_connectionString);
            return connection.Query<Group>("SELECT Id, Name FROM Groups").ToList();
        }
    }
}
