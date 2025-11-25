# README.md

Чтобы запустить

Выполнить команды
```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Далее заполнить базу начальными данными, для этого:

```
- запустить Microsof SQL Management Studio
- подключиться к базе Valeeva_Zoomag
- найти слева в Object Explorer в папке Databases - Valeeva_Zoomag
- нажать правой кнопкой, и выбрать NewQuery
- в появившееся окно редактора запроса вставить текст из файла seed.sql
- нажать на клавиатруе ALT+x или на зеленый треугольник с надписью Execute
- теперь база Valeeva_Zoomag должна быть заполнена данными

Админ
 - логин:  admin
 - пароль: admin



Продавец-консультант
 - логин:  seller1
 - пароль: 123

```

Перед сдачей настоятельно рекомендуется удалить 
- все лишние комментарии в коде
- файл seed.sql (в корне)

