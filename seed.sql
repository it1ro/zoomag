-- 1. Очистка дочерних таблиц (связи)
DELETE FROM SupplyProducts;
DELETE FROM SalesProducts;

-- 2. Очистка основных таблиц
DELETE FROM Product;
DELETE FROM Supply;
DELETE FROM Sale;
DELETE FROM Category;
DELETE FROM Unit;
DELETE FROM [User];

-- ✅ Проверка: Убедись, что таблицы пусты
SELECT COUNT(*) FROM Product;
SELECT COUNT(*) FROM Supply;
SELECT COUNT(*) FROM Sale;
SELECT COUNT(*) FROM Category;
SELECT COUNT(*) FROM Unit;
SELECT COUNT(*) FROM [User];


-- Включаем вставку ID для Unit
SET IDENTITY_INSERT Unit ON;

-- Вставляем единицы измерения с конкретными ID (как в твоем скрипте)
INSERT INTO Unit (Id, Name) VALUES
                                (1, N'шт'),
                                (2, N'кг'),
                                (3, N'л'),
                                (4, N'упак'),
                                (5, N'м');

-- Выключаем вставку ID для Unit
SET IDENTITY_INSERT Unit OFF;

-- Аналогично для Category
SET IDENTITY_INSERT Category ON;
INSERT INTO Category (Id, Name) VALUES
                                    (1, N'Корма'),
                                    (2, N'Игрушки'),
                                    (3, N'Аксессуары'),
                                    (4, N'Гигиена'),
                                    (5, N'Витамины');
SET IDENTITY_INSERT Category OFF;

-- Для User (если хочешь сохранить Admin/Seller с ID 1 и 2)
SET IDENTITY_INSERT [User] ON;
INSERT INTO [User] (Id, Login, Password, Role) VALUES
    (1, 'admin', 'admin', 0),
    (2, 'seller1', '123', 1);
    (3, 'seller2', '123', 1);
SET IDENTITY_INSERT [User] OFF;

-- Для Supply
SET IDENTITY_INSERT Supply ON;
INSERT INTO Supply (Id, Name, Date) VALUES
                                        (1, N'Поставка кормов от 01.11.2025', '2025-11-01'),
                                        (2, N'Поставка игрушек от 02.11.2025', '2025-11-02'),
                                        (3, N'Поставка аксессуаров от 03.11.2025', '2025-11-03'),
                                        (4, N'Поставка витаминов от 04.11.2025', '2025-11-04'),
                                        (5, N'Поставка гигиены от 05.11.2025', '2025-11-05');
SET IDENTITY_INSERT Supply OFF;

-- Для Sale
SET IDENTITY_INSERT Sale ON;
INSERT INTO Sale (Id, Name, Date, Amount, Price) VALUES
                                                     (1, N'Продажа от 06.11.2025 10:00', '2025-11-06 10:00:00', 3, 650),
                                                     (2, N'Продажа от 06.11.2025 11:30', '2025-11-06 11:30:00', 2, 480),
                                                     (3, N'Продажа от 07.11.2025 09:15', '2025-11-07 09:15:00', 4, 1700),
                                                     (4, N'Продажа от 07.11.2025 14:45', '2025-11-07 14:45:00', 1, 1200),
                                                     (5, N'Продажа от 08.11.2025 16:20', '2025-11-08 16:20:00', 3, 820);
SET IDENTITY_INSERT Sale OFF;


-- Включаем для Product
SET IDENTITY_INSERT Product ON;

INSERT INTO Product (Id, Name, UnitId, CategoryId, Price, Amount) VALUES
                                                                      (1, N'Корм для кошек Whiskas', 1, 1, 150, 50),
                                                                      (2, N'Корм для собак Pedigree', 1, 1, 200, 30),
                                                                      (3, N'Корм для рыбок Tetra', 4, 1, 180, 20),
                                                                      (4, N'Игрушка-мышь для кошек', 1, 2, 250, 100),
                                                                      (5, N'Мячик для собак', 1, 2, 300, 15),
                                                                      (6, N'Шлейка для кота', 1, 3, 450, 200),
                                                                      (7, N'Ошейник для собаки', 1, 3, 350, 80),
                                                                      (8, N'Витамины для попугаев', 4, 5, 600, 5),
                                                                      (9, N'Витамины для кошек', 4, 5, 750, 12),
                                                                      (10, N'Когтеточка', 1, 3, 1200, 35),
                                                                      (11, N'Лежак для собаки', 1, 3, 2500, 18),
                                                                      (12, N'Корм для хомяков', 4, 1, 100, 0),
                                                                      (13, N'Косточка для собак', 1, 2, 150, 0),
                                                                      (14, N'Шампунь для животных', 3, 4, 400, 0),
                                                                      (15, N'Корм для попугаев', 4, 1, 220, 150),
                                                                      (16, N'Игрушка-косточка', 1, 2, 180, 25),
                                                                      (17, N'Переноска для кота', 1, 3, 3000, 7),
                                                                      (18, N'Корм для черепах', 4, 1, 300, 120),
                                                                      (19, N'Песок для кошек', 2, 4, 250, 40),
                                                                      (20, N'Игрушка-вертушка для грызунов', 1, 2, 120, 300);

-- Выключаем для Product
SET IDENTITY_INSERT Product OFF;


-- Вставляем элементы поставок
INSERT INTO SupplyProducts (SupplyId, ProductId, Quantity, Price, Total) VALUES
                                                                             (1, 1, 100, 140, 14000),
                                                                             (1, 2, 50, 190, 9500),
                                                                             (1, 3, 60, 170, 10200),
                                                                             (2, 4, 200, 240, 48000),
                                                                             (2, 5, 30, 290, 8700),
                                                                             (3, 6, 500, 440, 220000),
                                                                             (3, 7, 150, 340, 51000),
                                                                             (4, 8, 10, 590, 5900),
                                                                             (4, 9, 20, 740, 14800),
                                                                             (5, 10, 50, 1190, 59500),
                                                                             (5, 11, 25, 2490, 62250);

-- Вставляем элементы продаж
INSERT INTO SalesProducts (SaleId, ProductId) VALUES
                                                  (1, 1),
                                                  (1, 4),
                                                  (1, 15),
                                                  (2, 2),
                                                  (2, 5),
                                                  (3, 10),
                                                  (3, 16),
                                                  (4, 8),
                                                  (5, 3),
                                                  (5, 19),
                                                  (5, 20);


-- Проверка количества записей
SELECT 'Unit' AS Table_Name, COUNT(*) AS Count FROM Unit
UNION ALL
SELECT 'Category', COUNT(*) FROM Category
UNION ALL
SELECT 'Product', COUNT(*) FROM Product
UNION ALL
SELECT 'Supply', COUNT(*) FROM Supply
UNION ALL
SELECT 'Sale', COUNT(*) FROM Sale
UNION ALL
SELECT 'SupplyProducts', COUNT(*) FROM SupplyProducts
UNION ALL
SELECT 'SalesProducts', COUNT(*) FROM SalesProducts
UNION ALL
SELECT 'User', COUNT(*) FROM [User];


DBCC CHECKIDENT('Product', RESEED, 20);




