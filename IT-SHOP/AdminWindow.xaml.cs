using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShopApp
{
    public partial class AdminWindow : Window
    {
        private User _adminUser;
        public ObservableCollection<Product> ProductsList { get; set; }
        public ObservableCollection<Order> OrdersList { get; set; }
        public AdminWindow(User adminUser)
        {
            InitializeComponent();
            _adminUser = adminUser;
            ProductsList = new ObservableCollection<Product>(Database.GetAllProducts());
            OrdersList = new ObservableCollection<Order>(Database.GetAllOrders());
            ProductsGrid.ItemsSource = ProductsList;
            OrdersGrid.ItemsSource = OrdersList;
            RefreshReports();
        }
        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product prod)
            {
                ProdNameBox.Text = prod.Name;
                ProdDescBox.Text = prod.Description;
                ProdPriceBox.Text = prod.Price.ToString();
                ProdStockBox.Text = prod.Stock;
                ProdImagePathBox.Text = prod.ImagePath;
            }
        }
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProdNameBox.Text) || string.IsNullOrWhiteSpace(ProdPriceBox.Text) || string.IsNullOrWhiteSpace(ProdStockBox.Text))
            {
                MessageBox.Show("Заполните обязательные поля: название, цена и количество.");
                return;
            }
            var product = new Product
            {
                Name = ProdNameBox.Text.Trim(),
                Description = ProdDescBox.Text.Trim(),
                Price = decimal.Parse(ProdPriceBox.Text.Trim()),
                Stock = ProdStockBox.Text.Trim(),
                ImagePath = ProdImagePathBox.Text.Trim()
            };
            int newId = Database.AddProduct(product);
            product.Id = newId;
            ProductsList.Add(product);
            MessageBox.Show("Товар добавлен.");
        }
        private void UpdateProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product prod)
            {
                prod.Name = ProdNameBox.Text.Trim();
                prod.Description = ProdDescBox.Text.Trim();
                prod.Price = decimal.Parse(ProdPriceBox.Text.Trim());
                prod.Stock = ProdStockBox.Text.Trim();
                prod.ImagePath = ProdImagePathBox.Text.Trim();
                Database.UpdateProduct(prod);
                MessageBox.Show("Данные товара обновлены.");
                ProductsGrid.Items.Refresh();
            }
        }
        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Product prod)
            {
                if (MessageBox.Show($"Удалить товар \"{prod.Name}\"?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Database.DeleteProduct(prod.Id);
                    ProductsList.Remove(prod);
                    MessageBox.Show("Товар удалён.");
                }
            }
        }
        private void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersGrid.SelectedItem is Order order)
            {
                StatusTextBox.Text = order.Status;
            }
        }
        private void UpdateStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersGrid.SelectedItem is Order order && !string.IsNullOrWhiteSpace(StatusTextBox.Text))
            {
                Database.UpdateOrderStatus(order.Id, StatusTextBox.Text.Trim());
                order.Status = StatusTextBox.Text.Trim();
                MessageBox.Show("Статус заказа обновлён.");
                OrdersGrid.Items.Refresh();
            }
        }
        private void RefreshReports_Click(object sender, RoutedEventArgs e)
        {
            RefreshReports();
        }
        private void RefreshReports()
        {
            int totalOrders = OrdersList.Count;
            decimal totalRevenue = OrdersList.Sum(o => o.Total);
            var report = $"Всего заказов: {totalOrders}\nОбщая выручка: {totalRevenue} руб.";
            ReportsTextBlock.Text = report;
        }
        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string backupFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                File.Copy(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shop.db"), backupFile, true);
                MessageBox.Show($"База успешно скопирована в {backupFile}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при копировании базы: " + ex.Message);
            }
        }
    }
}