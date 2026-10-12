# ADO.NET Course - Урок 4 (SqlClient + Parameters)

## Что сделано
- Логика вывода вынесена в метод `ShowAllProducts` и вызывается из `Main`.
- Меню с выбором через `switch-case` в повторяющемся цикле.
- Новые методы: `ShowProductById` (SELECT с параметром `@Id`) и `AddProduct` (INSERT с параметрами).

## Как запустить
1. Выполните `AdoNetLesson4/setup.sql` на вашем сервере (создаст базу `ShopDb` и таблицу `Products`).
2. Откройте `AdoNetLesson4/AdoNetLesson4.csproj` в Visual Studio.
3. В `Program.cs` замените `Server=...` в `ConnectionString` на имя вашего сервера.
4. Запустите (F5).

## Выкладка на GitHub (папка с номером урока)
```
git init
git add .
git commit -m "Lesson 4: SqlClient + Parameters"
git branch -M main
git remote add origin https://github.com/<ваш-логин>/<репозиторий>.git
git push -u origin main
```
Папка `Lesson4` уже лежит в корне, так что в репозитории будет директория с номером урока.
