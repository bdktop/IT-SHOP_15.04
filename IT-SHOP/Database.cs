using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using System.Data.SQLite;
using Microsoft.Data.Sqlite;

namespace ShopApp
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string Phone { get; set; }
        public string PasswordHash { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Stock { get; set; }
        public string ImagePath { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OrderDate { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }

    public static class Database
    {
        private static string _dbPath;
        private static IDbConnection _connection;

        public static void Initialize()
        {
            _dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shop.db");
            bool isNewDatabase = !File.Exists(_dbPath);
            _connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;");
            _connection.Open();
            if (isNewDatabase) CreateTablesIfNotExists();
        }

        private static void CreateTablesIfNotExists()
        {
            _connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Email TEXT NOT NULL,
                    Login TEXT NOT NULL,
                    Phone TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL
                );");
            _connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    Price REAL NOT NULL,
                    Stock TEXT NOT NULL,
                    ImagePath TEXT
                );");
            _connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Orders (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    OrderDate TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    Total REAL,
                    FOREIGN KEY(UserId) REFERENCES Users(Id)
                );");
            _connection.Execute(@"
                CREATE TABLE IF NOT EXISTS OrderItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId INTEGER NOT NULL,
                    ProductId INTEGER NOT NULL,
                    Quantity INTEGER NOT NULL,
                    Price REAL NOT NULL,
                    FOREIGN KEY(OrderId) REFERENCES Orders(Id),
                    FOREIGN KEY(ProductId) REFERENCES Products(Id)
                );");
            _connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Favorites (
                    UserId INTEGER NOT NULL,
                    ProductId INTEGER NOT NULL,
                    PRIMARY KEY (UserId, ProductId),
                    FOREIGN KEY(UserId) REFERENCES Users(Id),
                    FOREIGN KEY(ProductId) REFERENCES Products(Id)
                );");
        }

        public static void Close()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }

        public static bool RegisterUser(User newUser)
        {
            var existing = _connection.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Phone = @Phone", new { newUser.Phone });
            if (existing != null) return false;
            _connection.Execute(@"
                INSERT INTO Users (Email, Login, Phone, PasswordHash)
                VALUES (@Email, @Login, @Phone, @PasswordHash);", newUser);
            return true;
        }

        public static User Authenticate(string login, string passwordHash)
        {
            return _connection.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Login = @Login AND PasswordHash = @PasswordHash", new { Login = login, PasswordHash = passwordHash });
        }

        public static List<Product> GetAllProducts()
        {
            return _connection.Query<Product>("SELECT * FROM Products").ToList();
        }

        public static int AddProduct(Product product)
        {
            _connection.Execute(@"
                INSERT INTO Products (Name, Description, Price, Stock, ImagePath)
                VALUES (@Name, @Description, @Price, @Stock, @ImagePath);", product);
            return _connection.QuerySingle<int>("SELECT last_insert_rowid();");
        }

        public static void UpdateProduct(Product product)
        {
            _connection.Execute(@"
                UPDATE Products 
                SET Name=@Name, Description=@Description, Price=@Price, Stock=@Stock, ImagePath=@ImagePath
                WHERE Id=@Id;", product);
        }

        public static void DeleteProduct(int productId)
        {
            _connection.Execute("DELETE FROM Products WHERE Id = @Id", new { Id = productId });
            _connection.Execute("DELETE FROM Favorites WHERE ProductId = @Id", new { Id = productId });
        }

        public static int CreateOrder(int userId, string address, List<CartItem> cartItems)
        {
            string orderDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string status = "Новый";
            decimal total = cartItems.Sum(ci => ci.Product.Price * ci.Quantity);
            _connection.Execute(@"
                INSERT INTO Orders (UserId, OrderDate, Status, Total)
                VALUES (@UserId, @OrderDate, @Status, @Total);", new { UserId = userId, OrderDate = orderDate, Status = status, Total = total });
            int orderId = _connection.QuerySingle<int>("SELECT last_insert_rowid();");
            foreach (var item in cartItems)
            {
                _connection.Execute(@"
                    INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price)
                    VALUES (@OrderId, @ProductId, @Quantity, @Price);", new { OrderId = orderId, ProductId = item.Product.Id, Quantity = item.Quantity, Price = item.Product.Price });
                int currentStock = 0;
                int.TryParse(item.Product.Stock, out currentStock);
                int remaining = currentStock - item.Quantity;
                if (remaining < 0) remaining = 0;
                _connection.Execute("UPDATE Products SET Stock = @Stock WHERE Id = @ProductId", new { Stock = remaining.ToString(), ProductId = item.Product.Id });
            }
            return orderId;
        }

        public static List<Order> GetOrdersByUser(int userId)
        {
            return _connection.Query<Order>("SELECT * FROM Orders WHERE UserId = @UserId ORDER BY OrderDate DESC", new { UserId = userId }).ToList();
        }

        public static List<Order> GetAllOrders()
        {
            return _connection.Query<Order>("SELECT * FROM Orders ORDER BY OrderDate DESC").ToList();
        }

        public static List<OrderItem> GetOrderItems(int orderId)
        {
            return _connection.Query<OrderItem>("SELECT * FROM OrderItems WHERE OrderId = @OrderId", new { OrderId = orderId }).ToList();
        }

        public static void UpdateOrderStatus(int orderId, string newStatus)
        {
            _connection.Execute("UPDATE Orders SET Status = @Status WHERE Id = @Id", new { Status = newStatus, Id = orderId });
        }

        public static void AddFavorite(int userId, int productId)
        {
            _connection.Execute(@"
                INSERT OR IGNORE INTO Favorites (UserId, ProductId)
                VALUES (@UserId, @ProductId);", new { UserId = userId, ProductId = productId });
        }

        public static void RemoveFavorite(int userId, int productId)
        {
            _connection.Execute("DELETE FROM Favorites WHERE UserId = @UserId AND ProductId = @ProductId", new { UserId = userId, ProductId = productId });
        }

        public static List<Product> GetFavoriteProducts(int userId)
        {
            return _connection.Query<Product>("SELECT P.* FROM Products P JOIN Favorites F ON P.Id = F.ProductId WHERE F.UserId = @UserId", new { UserId = userId }).ToList();
        }
    }
}