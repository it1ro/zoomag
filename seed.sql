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
-- (Закомментировано для production, можно раскомментировать при отладке)
-- SELECT COUNT(*) FROM Product;
-- SELECT COUNT(*) FROM Supply;
-- SELECT COUNT(*) FROM Sale;
-- SELECT COUNT(*) FROM Category;
-- SELECT COUNT(*) FROM Unit;
-- SELECT COUNT(*) FROM [User];

-- Включаем вставку ID для Unit
SET IDENTITY_INSERT Unit ON;
INSERT INTO Unit (Id, Name) VALUES
    (1, N'шт'),
    (2, N'кг'),
    (3, N'л'),
    (4, N'упак'),
    (5, N'м');
SET IDENTITY_INSERT Unit OFF;

-- Для Category
SET IDENTITY_INSERT Category ON;
INSERT INTO Category (Id, Name) VALUES
    (1, N'Корма'),
    (2, N'Игрушки'),
    (3, N'Аксессуары'),
    (4, N'Гигиена'),
    (5, N'Витамины');
SET IDENTITY_INSERT Category OFF;

-- Для User (открытые пароли — временно)
SET IDENTITY_INSERT [User] ON;
INSERT INTO [User] (Id, Login, Password, Role) VALUES
    (1, 'admin', 'admin', 0),
    (2, 'seller1', '123', 1),
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

-- Для Sale — без Amount и Price!
SET IDENTITY_INSERT Sale ON;
INSERT INTO Sale (Id, Name, Date) VALUES
    (1, N'Продажа от 06.11.2025 10:00', '2025-11-06 10:00:00'),
    (2, N'Продажа от 06.11.2025 11:30', '2025-11-06 11:30:00'),
    (3, N'Продажа от 07.11.2025 09:15', '2025-11-07 09:15:00'),
    (4, N'Продажа от 07.11.2025 14:45', '2025-11-07 14:45:00'),
    (5, N'Продажа от 08.11.2025 16:20', '2025-11-08 16:20:00');
SET IDENTITY_INSERT Sale OFF;

-- Для Product — без Price и Amount!
SET IDENTITY_INSERT Product ON;
INSERT INTO Product (Id, Name, UnitId, CategoryId) VALUES
    (1,  N'Корм для кошек Whiskas',         1, 1),
    (2,  N'Корм для собак Pedigree',        1, 1),
    (3,  N'Корм для рыбок Tetra',           4, 1),
    (4,  N'Игрушка-мышь для кошек',        1, 2),
    (5,  N'Мячик для собак',               1, 2),
    (6,  N'Шлейка для кота',               1, 3),
    (7,  N'Ошейник для собаки',            1, 3),
    (8,  N'Витамины для попугаев',         4, 5),
    (9,  N'Витамины для кошек',            4, 5),
    (10, N'Когтеточка',                    1, 3),
    (11, N'Лежак для собаки',              1, 3),
    (12, N'Корм для хомяков',              4, 1),
    (13, N'Косточка для собак',            1, 2),
    (14, N'Шампунь для животных',          3, 4),
    (15, N'Корм для попугаев',             4, 1),
    (16, N'Игрушка-косточка',              1, 2),
    (17, N'Переноска для кота',            1, 3),
    (18, N'Корм для черепах',              4, 1),
    (19, N'Песок для кошек',               2, 4),
    (20, N'Игрушка-вертушка для грызунов',1, 2);
SET IDENTITY_INSERT Product OFF;

-- Поставки (SupplyProducts) — ВСЕ товары получают поставку
INSERT INTO SupplyProducts (SupplyId, ProductId, Quantity, Price, Total) VALUES
    -- Поставка 1: Корма
    (1, 1,  100, 140, 14000),  -- Whiskas
    (1, 2,  50,  190, 9500),   -- Pedigree
    (1, 3,  60,  170, 10200),  -- Tetra
    (1, 12, 40,  160, 6400),   -- Хомяки
    (1, 15, 30,  220, 6600),   -- Попугаи ← было пропущено!
    (1, 18, 50,  220, 11000),  -- Черепахи

    -- Поставка 2: Игрушки
    (2, 4,  200, 240, 48000),  -- Мышь
    (2, 5,  30,  290, 8700),   -- Мячик
    (2, 13, 80,  130, 10400),  -- Косточка
    (2, 16, 100, 180, 18000),  -- Косточка-игрушка ← было пропущено!
    (2, 20, 60,  120, 7200),   -- Вертушка ← было пропущено!

    -- Поставка 3: Аксессуары
    (3, 6,  500, 440, 220000), -- Шлейка
    (3, 7,  150, 340, 51000),  -- Ошейник
    (3, 10, 50,  1190, 59500), -- Когтеточка
    (3, 11, 25,  2490, 62250), -- Лежак
    (3, 17, 20,  1800, 36000), -- Переноска

    -- Поставка 4: Витамины
    (4, 8,  10,  590, 5900),   -- Попугаи
    (4, 9,  20,  740, 14800),  -- Кошки

    -- Поставка 5: Гигиена
    (5, 14, 30,  210, 6300),   -- Шампунь ← было пропущено!
    (5, 19, 25,  250, 6250);   -- Песок ← было пропущено!

-- Продажи (SalesProducts) — только после поставки!
INSERT INTO SalesProducts (SaleId, ProductId, Quantity, Price) VALUES
    (1, 1,  1, 150),
    (1, 4,  1, 250),
    (1, 15, 1, 220),  -- Теперь есть поставка!
    (2, 2,  1, 200),
    (2, 5,  1, 300),
    (3, 10, 1, 1200),
    (3, 16, 1, 180),  -- Теперь есть поставка!
    (4, 8,  1, 600),
    (5, 3,  1, 180),
    (5, 19, 1, 250),  -- Теперь есть поставка!
    (5, 20, 1, 120);  -- Теперь есть поставка!

-- ✅ Финальная проверка: нет ли отрицательных остатков
-- Запрос можно оставить закомментированным или использовать при отладке
/*
SELECT
    p.Id,
    p.Name,
    ISNULL(sup.Qty, 0) AS Supplied,
    ISNULL(sal.Qty, 0) AS Sold,
    ISNULL(sup.Qty, 0) - ISNULL(sal.Qty, 0) AS Balance
FROM Product p
LEFT JOIN (SELECT ProductId, SUM(Quantity) AS Qty FROM SupplyProducts GROUP BY ProductId) sup
    ON p.Id = sup.ProductId
LEFT JOIN (SELECT ProductId, SUM(Quantity) AS Qty FROM SalesProducts GROUP BY ProductId) sal
    ON p.Id = sal.ProductId
WHERE ISNULL(sup.Qty, 0) - ISNULL(sal.Qty, 0) < 0;
*/

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

-- Сброс ID (опционально)
DBCC CHECKIDENT('Product', RESEED, 20);