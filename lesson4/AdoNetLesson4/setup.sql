-- Скрипт создания базы данных и таблицы для урока 4.
-- Выполните его в SSMS или в обозревателе объектов SQL Server (Новый запрос).

IF DB_ID('ShopDb') IS NULL
    CREATE DATABASE ShopDb;
GO

USE ShopDb;
GO

IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id       INT IDENTITY(1,1) PRIMARY KEY,
        Name     NVARCHAR(100)  NOT NULL,
        Price    DECIMAL(10, 2) NOT NULL,
        Quantity INT            NOT NULL
    );

    INSERT INTO dbo.Products (Name, Price, Quantity) VALUES
        (N'Ноутбук',      54990.00, 5),
        (N'Мышь',           799.00, 40),
        (N'Клавиатура',    2490.00, 25),
        (N'Монитор 24"',  12990.00, 10),
        (N'Наушники',      3290.00, 18);
END
GO
