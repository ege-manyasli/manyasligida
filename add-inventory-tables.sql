-- Stok sistemi tabloları
-- Inventory (Mal alımı kayıtları)
CREATE TABLE dbo.Inventories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ItemName NVARCHAR(200) NOT NULL,
    Category NVARCHAR(100) NULL,
    UnitPrice DECIMAL(18,2) NOT NULL DEFAULT(0),
    Quantity DECIMAL(18,3) NOT NULL DEFAULT(0),
    Unit NVARCHAR(20) NULL,
    TotalAmount AS (UnitPrice * Quantity),
    Supplier NVARCHAR(200) NULL,
    PurchaseDate DATETIME2 NOT NULL DEFAULT(GETDATE()),
    Notes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT(GETDATE()),
    UpdatedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT(1)
);

-- InventoryStock (Stok takibi)
CREATE TABLE dbo.InventoryStocks (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ItemName NVARCHAR(200) NOT NULL,
    Category NVARCHAR(100) NULL,
    CurrentStock DECIMAL(18,3) NOT NULL DEFAULT(0),
    MinimumStock DECIMAL(18,3) NOT NULL DEFAULT(0),
    Unit NVARCHAR(20) NULL,
    AverageCost DECIMAL(18,2) NOT NULL DEFAULT(0),
    StockValue AS (CurrentStock * AverageCost),
    LastUpdated DATETIME2 NOT NULL DEFAULT(GETDATE()),
    IsActive BIT NOT NULL DEFAULT(1)
);

-- InventoryTransaction (Stok işlemleri)
CREATE TABLE dbo.InventoryTransactions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ItemName NVARCHAR(200) NOT NULL,
    TransactionType NVARCHAR(20) NOT NULL, -- IN, OUT
    Quantity DECIMAL(18,3) NOT NULL DEFAULT(0),
    Unit NVARCHAR(20) NULL,
    UnitPrice DECIMAL(18,2) NOT NULL DEFAULT(0),
    TotalAmount AS (UnitPrice * Quantity),
    Description NVARCHAR(200) NULL,
    TransactionDate DATETIME2 NOT NULL DEFAULT(GETDATE()),
    CreatedAt DATETIME2 NOT NULL DEFAULT(GETDATE())
);

-- İndeksler
CREATE INDEX IX_Inventories_ItemName ON dbo.Inventories(ItemName);
CREATE INDEX IX_Inventories_PurchaseDate ON dbo.Inventories(PurchaseDate);
CREATE INDEX IX_Inventories_Category ON dbo.Inventories(Category);
CREATE INDEX IX_Inventories_Supplier ON dbo.Inventories(Supplier);

CREATE INDEX IX_InventoryStocks_ItemName ON dbo.InventoryStocks(ItemName);
CREATE INDEX IX_InventoryStocks_Category ON dbo.InventoryStocks(Category);
CREATE INDEX IX_InventoryStocks_CurrentStock ON dbo.InventoryStocks(CurrentStock);

CREATE INDEX IX_InventoryTransactions_ItemName ON dbo.InventoryTransactions(ItemName);
CREATE INDEX IX_InventoryTransactions_TransactionType ON dbo.InventoryTransactions(TransactionType);
CREATE INDEX IX_InventoryTransactions_TransactionDate ON dbo.InventoryTransactions(TransactionDate);

-- Örnek veriler
INSERT INTO dbo.Inventories (ItemName, Category, UnitPrice, Quantity, Unit, Supplier, PurchaseDate, Notes)
VALUES 
('Süt', 'Süt Ürünleri', 15.50, 100.0, 'lt', 'Çiftlik A', GETDATE(), 'Günlük süt alımı'),
('Peynir', 'Süt Ürünleri', 45.00, 25.0, 'kg', 'Çiftlik B', GETDATE(), 'Beyaz peynir'),
('Yoğurt', 'Süt Ürünleri', 12.00, 50.0, 'kg', 'Çiftlik A', GETDATE(), 'Doğal yoğurt'),
('Tereyağı', 'Süt Ürünleri', 120.00, 10.0, 'kg', 'Çiftlik C', GETDATE(), 'Taze tereyağı');

INSERT INTO dbo.InventoryStocks (ItemName, Category, CurrentStock, MinimumStock, Unit, AverageCost)
VALUES 
('Süt', 'Süt Ürünleri', 100.0, 20.0, 'lt', 15.50),
('Peynir', 'Süt Ürünleri', 25.0, 5.0, 'kg', 45.00),
('Yoğurt', 'Süt Ürünleri', 50.0, 10.0, 'kg', 12.00),
('Tereyağı', 'Süt Ürünleri', 10.0, 2.0, 'kg', 120.00);

INSERT INTO dbo.InventoryTransactions (ItemName, TransactionType, Quantity, Unit, UnitPrice, Description)
VALUES 
('Süt', 'IN', 100.0, 'lt', 15.50, 'İlk stok girişi'),
('Peynir', 'IN', 25.0, 'kg', 45.00, 'İlk stok girişi'),
('Yoğurt', 'IN', 50.0, 'kg', 12.00, 'İlk stok girişi'),
('Tereyağı', 'IN', 10.0, 'kg', 120.00, 'İlk stok girişi');

PRINT 'Inventory tables created successfully with sample data!';
