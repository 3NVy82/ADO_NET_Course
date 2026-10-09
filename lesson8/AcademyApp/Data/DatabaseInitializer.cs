using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace AcademyApp.Data
{
    // Создаёт базу CollegeDB, таблицы и тестовые данные, если их ещё нет.
    public static class DatabaseInitializer
    {
        public static void Initialize(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            string dbName = builder.InitialCatalog;

            // 1. Создаём саму базу (подключаемся к master)
            builder.InitialCatalog = "master";
            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "IF DB_ID(@db) IS NULL EXEC('CREATE DATABASE [' + @db + ']')", connection);
                command.Parameters.AddWithValue("@db", dbName);
                command.ExecuteNonQuery();
            }

            // 2. Создаём таблицы и данные
            var statements = new List<string>
            {
                @"IF OBJECT_ID(N'dbo.Groups', N'U') IS NULL
                  CREATE TABLE dbo.Groups (
                      Id INT IDENTITY PRIMARY KEY,
                      Name NVARCHAR(50) NOT NULL)",

                @"IF OBJECT_ID(N'dbo.Teachers', N'U') IS NULL
                  CREATE TABLE dbo.Teachers (
                      Id INT IDENTITY PRIMARY KEY,
                      FullName NVARCHAR(100) NOT NULL)",

                @"IF OBJECT_ID(N'dbo.Subjects', N'U') IS NULL
                  CREATE TABLE dbo.Subjects (
                      Id INT IDENTITY PRIMARY KEY,
                      Name NVARCHAR(100) NOT NULL,
                      TeacherId INT NULL REFERENCES dbo.Teachers(Id))",

                @"IF OBJECT_ID(N'dbo.Students', N'U') IS NULL
                  CREATE TABLE dbo.Students (
                      Id INT IDENTITY PRIMARY KEY,
                      FirstName NVARCHAR(50) NOT NULL,
                      LastName NVARCHAR(50) NOT NULL,
                      Age INT NOT NULL,
                      GroupId INT NULL REFERENCES dbo.Groups(Id))",

                @"IF OBJECT_ID(N'dbo.Grades', N'U') IS NULL
                  CREATE TABLE dbo.Grades (
                      Id INT IDENTITY PRIMARY KEY,
                      StudentId INT NOT NULL REFERENCES dbo.Students(Id),
                      SubjectId INT NOT NULL REFERENCES dbo.Subjects(Id),
                      Value INT NOT NULL CHECK (Value BETWEEN 1 AND 5),
                      GradeDate DATE NOT NULL DEFAULT GETDATE())",

                @"IF NOT EXISTS (SELECT 1 FROM dbo.Groups)
                  INSERT INTO dbo.Groups (Name) VALUES (N'ИС-21'), (N'ПО-22')",

                @"IF NOT EXISTS (SELECT 1 FROM dbo.Teachers)
                  INSERT INTO dbo.Teachers (FullName) VALUES (N'Иванов И.И.')",

                @"IF NOT EXISTS (SELECT 1 FROM dbo.Subjects)
                  INSERT INTO dbo.Subjects (Name, TeacherId)
                  SELECT TOP 1 N'Программирование', Id FROM dbo.Teachers ORDER BY Id",

                @"IF NOT EXISTS (SELECT 1 FROM dbo.Students)
                  INSERT INTO dbo.Students (FirstName, LastName, Age, GroupId) VALUES
                      (N'Илья', N'Агапеев', 17, (SELECT TOP 1 Id FROM dbo.Groups WHERE Name = N'ИС-21')),
                      (N'Ксения', N'Адаменко', 20, (SELECT TOP 1 Id FROM dbo.Groups WHERE Name = N'ПО-22'))",

                @"IF NOT EXISTS (SELECT 1 FROM dbo.Grades)
                  INSERT INTO dbo.Grades (StudentId, SubjectId, Value)
                  SELECT s.Id, (SELECT TOP 1 Id FROM dbo.Subjects ORDER BY Id), 5
                  FROM dbo.Students s"
            };

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (var sql in statements)
                {
                    new SqlCommand(sql, connection).ExecuteNonQuery();
                }
            }
        }
    }
}
