
-- Сбрасываем счетчики ID (если используется IDENTITY)
DBCC CHECKIDENT('Unit', RESEED, 0);
DBCC CHECKIDENT('Category', RESEED, 0);
DBCC CHECKIDENT('Product', RESEED, 0);
DBCC CHECKIDENT('Supply', RESEED, 0);
DBCC CHECKIDENT('Sale', RESEED, 0);

-- 1. Инструкция очистки базы данных
-- -------------------------------
-- Удаляем дочерние таблицы (с внешними ключами) первыми
DELETE FROM SupplyProducts;
DELETE FROM SalesProducts;
DELETE FROM Product;
-- Удаляем родительские таблицы
DELETE FROM Supply;
DELETE FROM Sale;
DELETE FROM Category;
DELETE FROM Unit;


-- 2. Сиды данных
-- --------------

-- Единицы измерения (Id: 1-5)
INSERT INTO Unit (Name) VALUES (N'шт'), (N'кг'), (N'л'), (N'упак'), (N'м');

-- Категории (Id: 1-5)
INSERT INTO Category (Name) VALUES (N'Корма'), (N'Игрушки'), (N'Аксессуары'), (N'Гигиена'), (N'Витамины');

-- Товары (включая 3 с нулевым остатком)
-- Вставляем с учетом Id Unit и Category
INSERT INTO Product (Name, UnitId, CategoryId, Price, Amount) VALUES
                                                                  (N'Корм для кошек Whiskas', 1, 1, 150, 50), -- UnitId=1, CategoryId=1
                                                                  (N'Корм для собак Pedigree', 1, 1, 200, 30), -- UnitId=1, CategoryId=1
                                                                  (N'Корм для рыбок Tetra', 4, 1, 180, 20), -- UnitId=4, CategoryId=1
                                                                  (N'Игрушка-мышь для кошек', 1, 2, 250, 100), -- UnitId=1, CategoryId=2
                                                                  (N'Мячик для собак', 1, 2, 300, 15), -- UnitId=1, CategoryId=2
                                                                  (N'Шлейка для кота', 1, 3, 450, 200), -- UnitId=1, CategoryId=3
                                                                  (N'Ошейник для собаки', 1, 3, 350, 80), -- UnitId=1, CategoryId=3
                                                                  (N'Витамины для попугаев', 4, 5, 600, 5), -- UnitId=4, CategoryId=5
                                                                  (N'Витамины для кошек', 4, 5, 750, 12), -- UnitId=4, CategoryId=5
                                                                  (N'Когтеточка', 1, 3, 1200, 35), -- UnitId=1, CategoryId=3
                                                                  (N'Лежак для собаки', 1, 3, 2500, 18), -- UnitId=1, CategoryId=3
                                                                  (N'Корм для хомяков', 4, 1, 100, 0), -- UnitId=4, CategoryId=1 -- Нулевой остаток
                                                                  (N'Косточка для собак', 1, 2, 150, 0), -- UnitId=1, CategoryId=2 -- Нулевой остаток
                                                                  (N'Шампунь для животных', 3, 4, 400, 0), -- UnitId=3, CategoryId=4 -- Нулевой остаток
                                                                  (N'Корм для попугаев', 4, 1, 220, 150), -- UnitId=4, CategoryId=1
                                                                  (N'Игрушка-косточка', 1, 2, 180, 25), -- UnitId=1, CategoryId=2
                                                                  (N'Переноска для кота', 1, 3, 3000, 7), -- UnitId=1, CategoryId=3
                                                                  (N'Корм для черепах', 4, 1, 300, 120), -- UnitId=4, CategoryId=1
                                                                  (N'Песок для кошек', 2, 4, 250, 40), -- UnitId=2, CategoryId=4
                                                                  (N'Игрушка-вертушка для грызунов', 1, 2, 120, 300); -- UnitId=1, CategoryId=2

-- Поставки (Id: 1-5)
INSERT INTO Supply (Name, Date) VALUES
                                    (N'Поставка кормов от 01.11.2025', '2025-11-01'),
                                    (N'Поставка игрушек от 02.11.2025', '2025-11-02'),
                                    (N'Поставка аксессуаров от 03.11.2025', '2025-11-03'),
                                    (N'Поставка витаминов от 04.11.2025', '2025-11-04'),
                                    (N'Поставка гигиены от 05.11.2025', '2025-11-05');

-- Элементы поставок (SupplyProducts) - ссылается на Id из Supply и Product
-- Убедимся, что SupplyId и ProductId существуют
INSERT INTO SupplyProducts (SupplyId, ProductId, Quantity, Price, Total) VALUES
                                                                             (1, 1, 100, 140, 14000), -- SupplyId=1, ProductId=1
                                                                             (1, 2, 50, 190, 9500),   -- SupplyId=1, ProductId=2
                                                                             (1, 3, 60, 170, 10200),  -- SupplyId=1, ProductId=3
                                                                             (2, 4, 200, 240, 48000), -- SupplyId=2, ProductId=4
                                                                             (2, 5, 30, 290, 8700),   -- SupplyId=2, ProductId=5
                                                                             (3, 6, 500, 440, 220000),-- SupplyId=3, ProductId=6
                                                                             (3, 7, 150, 340, 51000), -- SupplyId=3, ProductId=7
                                                                             (4, 8, 10, 590, 5900),   -- SupplyId=4, ProductId=8
                                                                             (4, 9, 20, 740, 14800),  -- SupplyId=4, ProductId=9
                                                                             (5, 10, 50, 1190, 59500),-- SupplyId=5, ProductId=10
                                                                             (5, 11, 25, 2490, 62250);-- SupplyId=5, ProductId=11

-- Продажи (Id: 1-5)
INSERT INTO Sale (Name, Date, Amount, Price) VALUES
                                                 (N'Продажа от 06.11.2025 10:00', N'06.11.2025 10:00', 3, 650),
                                                 (N'Продажа от 06.11.2025 11:30', N'06.11.2025 11:30', 2, 480),
                                                 (N'Продажа от 07.11.2025 09:15', N'07.11.2025 09:15', 4, 1700),
                                                 (N'Продажа от 07.11.2025 14:45', N'07.11.2025 14:45', 1, 1200),
                                                 (N'Продажа от 08.11.2025 16:20', N'08.11.2025 16:20', 3, 820);

-- Элементы продаж (SalesProducts) - ссылается на Id из Sale и Product
-- Убедимся, что SaleId и ProductId существуют
INSERT INTO SalesProducts (SaleId, ProductId) VALUES
                                                  (1, 1), -- Корм для кошек
                                                  (1, 4), -- Игрушка-мышь
                                                  (1, 15), -- Корм для попугаев
                                                  (2, 2), -- Корм для собак
                                                  (2, 5), -- Мячик для собак
                                                  (3, 10), -- Когтеточка (SaleId=3, ProductId=10) - УДАЛИЛА ВТОРУЮ СТРОКУ
                                                  (3, 16), -- Игрушка-косточка
                                                  (4, 8), -- Витамины для попугаев
                                                  (5, 3), -- Корм для рыбок
                                                  (5, 19), -- Песок для кошек
                                                  (5, 20); -- Игрушка-вертушка
