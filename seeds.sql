-- 1. ОЧИСТКА: сначала дочерние таблицы
DELETE FROM SupplyProducts;
DELETE FROM SalesProducts;
DELETE FROM Product;
DELETE FROM Supply;
DELETE FROM Sale;
DELETE FROM Category;
DELETE FROM Unit;
DELETE FROM User;

-- 2. СБРОС СЧЁТЧИКОВ и ВКЛЮЧЕНИЕ IDENTITY_INSERT
DBCC CHECKIDENT('Unit', RESEED, 0);
SET IDENTITY_INSERT Unit ON;
-- Вставляем с конкретными ID
INSERT INTO Unit (Id, Name) VALUES (1, N'шт'), (2, N'кг'), (3, N'л'), (4, N'упак'), (5, N'м');
SET IDENTITY_INSERT Unit OFF;

DBCC CHECKIDENT('Category', RESEED, 0);
SET IDENTITY_INSERT Category ON;
INSERT INTO Category (Id, Name) VALUES (1, N'Корма'), (2, N'Игрушки'), (3, N'Аксессуары'), (4, N'Гигиена'), (5, N'Витамины');
SET IDENTITY_INSERT Category OFF;

DBCC CHECKIDENT('Product', RESEED, 0);
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
SET IDENTITY_INSERT Product OFF;

DBCC CHECKIDENT('Supply', RESEED, 0);
SET IDENTITY_INSERT Supply ON;
INSERT INTO Supply (Id, Name, Date) VALUES
                                        (1, N'Поставка кормов от 01.11.2025', '2025-11-01'),
                                        (2, N'Поставка игрушек от 02.11.2025', '2025-11-02'),
                                        (3, N'Поставка аксессуаров от 03.11.2025', '2025-11-03'),
                                        (4, N'Поставка витаминов от 04.11.2025', '2025-11-04'),
                                        (5, N'Поставка гигиены от 05.11.2025', '2025-11-05');
SET IDENTITY_INSERT Supply OFF;

DBCC CHECKIDENT('Sale', RESEED, 0);
SET IDENTITY_INSERT Sale ON;
INSERT INTO Sale (Id, Name, Date, Amount, Price) VALUES
                                                     (1, N'Продажа от 06.11.2025 10:00', '2025-11-06 10:00:00', 3, 650),
                                                     (2, N'Продажа от 06.11.2025 11:30', '2025-11-06 11:30:00', 2, 480),
                                                     (3, N'Продажа от 07.11.2025 09:15', '2025-11-07 09:15:00', 4, 1700),
                                                     (4, N'Продажа от 07.11.2025 14:45', '2025-11-07 14:45:00', 1, 1200),
                                                     (5, N'Продажа от 08.11.2025 16:20', '2025-11-08 16:20:00', 3, 820);
SET IDENTITY_INSERT Sale OFF;

-- Теперь вставляем элементы поставок и продаж, используя правильные ID
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


INSERT INTO User (Id, Login, PasswordHash, Role) VALUES
    (1, 'admin', 'admin', 0), -- Admin
    (2, 'seller1', '123', 1); -- Seller
