# Опциональные задания
- ✔️ По желанию можно добавить выдачу и использование токена (JWT)
- ✔️ По желанию возвращать текст ошибки при невозможности выполнить действие
- ✔️ По желанию подключить любую базу данных (PostgreSQL)
> **Примечание:** ✔️ Желательно выложить код на гитхаб или в любую доступную систему контроля
> версий.

# Инструкция

## docker-compose.yml
```yml
version: '3.4'

services:
  postgredb:
    container_name: postgredb
    image: postgres
    ports:
      - "5432:5432"
    environment:
      POSTGRES_PASSWORD: Password1
      POSTGRES_DB: usersdb
      POSTGRES_USER: postgres
  webapi:
    container_name: webapi
    image: csharpenjoyer/testassignment2023may
    ports:
      - "8080:443"
      - "8081:80"
    build:
      context: .
      dockerfile: WebAPI/Dockerfile
    environment: 
      PG_CONNECTION_STRING: Host=postgredb;Port=5432;Database=usersdb;Username=postgres;Password=Password1
    depends_on:
      - postgredb
```

## Запуск
```shell
docker-compose up
```

## Доступ к swagger по адресу 

`http://localhost:8081/swagger/`

# Настройка
> **Примечание:** Используется база данных PostgreSQL 15  

В WebAPI/appsettings.json добавить содержимое следующего блока. Можно задать свои параметры
```json
  "JwtSettings": {
    "Key": "TestAssignmentSecurityKey2023",
    "Issuer": "TestAssignmentIssuer",
    "Audience": "TestAssignmentAudience"
  },
  "DefaultConnection": "Host=localhost;Port=5432;Database=usersdb;Username=postgres;Password=Password1"
```
По маршруту /login получаем JWT токен используя следующие данные:
```json
{
  "login": "Admin",
  "password": "StrongPassword2023"
}
```
JWT токен будет в ответе (Header запроса под ключом authorization). Далее с помощью интерфейса Swagger (кнопка Authorize) можно авторизоваться используя этот токен и пользоваться
API по назначению.
```
authorization: eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJiZTg0M2QwMS1iNDhlLTQ2MTktOGYzYy1lMjMzNzQ0MGZjMjUiLCJzdWIiOiI3MWU5N2I4Ny05NWFkLTRmMWEtODQyNi01OWNiZmY0YjBmYzciLCJuYW1lIjoiQWRtaW4iLCJJc0FkbWluIjoiVHJ1ZSIsIm5iZiI6MTY4NTU1MDE5MywiZXhwIjoxNjg1NTUzNzkzLCJpYXQiOjE2ODU1NTAxOTMsImlzcyI6IlRlc3RBc3NpZ25tZW50SXNzdWVyIiwiYXVkIjoiVGVzdEFzc2lnbm1lbnRBdWRpZW5jZSJ9.ozDrer4h9zSGN5wxDlsHXMWb0LCu1Z4A2SfUaGtroGTJxYrH8AZ_InxLmLDlJzgYEti55wYs8yzAczFKFYD1Ow
```

# Задание: 
> Написать Web API сервис на .NET Core 3.1 или выше, реализующий API методы CRUD над
> сущностью Users, доступ к API должен осуществляться через интерфейс Swagger.
> Все действия могут происходить в оперативной памяти и не сохраняться при остановке
> приложения (по желанию подключить любую базу данных).
> Предварительно должен быть создан пользователь Admin, от имени которого будут вначале
> происходить действия.

# CRUD методы контроллера UsersController
 > !!!Во всех запросах должны присутствовать параметры логин и пароль выполняющего запрос!!!
 > (по желанию возвращать текст ошибки при невозможности выполнить действие)
 > (по желанию можно добавить выдачу и использование токена)
 - Create
    - Создание пользователя по логину, паролю, имени, полу и дате рождения + указание будет ли
пользователь админом (Доступно Админам)
  - Update-1
    - Изменение имени, пола или даты рождения пользователя (Может менять Администратор, либо
    лично пользователь, если он активен (отсутствует RevokedOn))
    - Изменение пароля (Пароль может менять либо Администратор, либо лично пользователь, если
    он активен (отсутствует RevokedOn))
    - Изменение логина (Логин может менять либо Администратор, либо лично пользователь, если
    он активен (отсутствует RevokedOn), логин должен оставаться уникальным)
  - Read
    - Запрос списка всех активных (отсутствует RevokedOn) пользователей, список отсортирован по
    CreatedOn (Доступно Админам)
    - Запрос пользователя по логину, в списке долны быть имя, пол и дата рождения статус активный
    или нет (Доступно Админам)
    - Запрос пользователя по логину и паролю (Доступно только самому пользователю, если он
    активен (отсутствует RevokedOn))
    - Запрос всех пользователей старше определённого возраста (Доступно Админам)
  - Delete
    - Удаление пользователя по логину полное или мягкое (При мягком удалении должна
    происходить простановка RevokedOn и RevokedBy) (Доступно Админам)
  - Update-2
    - Восстановление пользователя - Очистка полей (RevokedOn, RevokedBy) (Доступно Админам)

# Поля сущности User
| Name         | Type         | Description                                                           |
|:------------:|:------------:|:----------------------------------------------------------------------|
| Guid         |`Guid`        | Уникальный идентификатор пользователя                                 |
| Login        |`string`      | Уникальный Логин (запрещены все символы кроме латинских букв и цифр), |
| Password     |`string`      | Пароль(запрещены все символы кроме латинских букв и цифр),            |
| Name         |`string`      | Имя (запрещены все символы кроме латинских и русских букв)            |
| Gender       |`int`         | Пол 0 - женщина, 1 - мужчина, 2 - неизвестно                          |
| Birthday     |`DateTime?`   | Поле даты рождения может быть Null                                    |
| Admin        |`bool`        | Указание является ли пользователь админом                             |
| CreatedOn    |`DateTime`    | Дата создания пользователя                                            |
| CreatedBy    |`string`      | Логин Пользователя, от имени которого этот пользователь создан        |
| ModifiedOn   |`DateTime`    | Дата изменения пользователя                                           |
| ModifiedBy   |`string`      | Логин Пользователя, от имени которого этот пользователь изменён       |
| RevokedOn    |`DateTime`    | Дата удаления пользователя                                            |
| RevokedBy    |`string`      | Логин Пользователя, от имени которого этот пользователь удалён        |
