IF DB_ID(N'CollegeDB') IS NULL
    CREATE DATABASE CollegeDB;
GO
USE CollegeDB;
GO

CREATE TABLE Groups (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);

CREATE TABLE Teachers (
    Id INT IDENTITY PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL
);

CREATE TABLE Subjects (
    Id INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    TeacherId INT NULL REFERENCES Teachers(Id)
);

CREATE TABLE Students (
    Id INT IDENTITY PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Age INT NOT NULL,
    GroupId INT NULL REFERENCES Groups(Id)
);

CREATE TABLE Grades (
    Id INT IDENTITY PRIMARY KEY,
    StudentId INT NOT NULL REFERENCES Students(Id),
    SubjectId INT NOT NULL REFERENCES Subjects(Id),
    Value INT NOT NULL CHECK (Value BETWEEN 1 AND 5),
    GradeDate DATE NOT NULL DEFAULT GETDATE()
);
GO

INSERT INTO Groups (Name) VALUES (N'ИС-21'), (N'ПО-22');
INSERT INTO Teachers (FullName) VALUES (N'Иванов И.И.');
INSERT INTO Subjects (Name, TeacherId) VALUES (N'Программирование', 1);
INSERT INTO Students (FirstName, LastName, Age, GroupId) VALUES
    (N'Илья', N'Агапеев', 17, 1),
    (N'Ксения', N'Адаменко', 20, 2);
INSERT INTO Grades (StudentId, SubjectId, Value) VALUES (1, 1, 5), (2, 1, 4);
GO
