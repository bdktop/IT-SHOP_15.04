using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ShopApp
{
    public partial class UserWindow : Window
    {
        private User _currentUser;
        private List<Product> _allProducts;
        private List<CartItem> _cartItems;
        public UserWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _cartItems = new List<CartItem>();
            LoadCatalog();
            LoadFavorites();
            LoadUserOrders();
        }
        private void LoadCatalog()
        {
            _allProducts = Database.GetAllProducts();
            CatalogGrid.ItemsSource = _allProducts;
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchBox.Text.Trim().ToLower();
            var filtered = _allProducts.Where(p => p.Name.ToLower().Contains(searchText)).ToList();
            CatalogGrid.ItemsSource = filtered;
        }
        private void CatalogGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CatalogGrid.SelectedItem is Product prod)
            {
                if (MessageBox.Show($"Добавить товар \"{prod.Name}\" в корзину?", "Добавление", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var dialog = new QuantityDialog();
                    dialog.Owner = this;
                    bool? result = dialog.ShowDialog();
                    if (result == true)
                    {
                        int quantity = dialog.Quantity;
                        var existing = _cartItems.FirstOrDefault(ci => ci.Product.Id == prod.Id);
                        if (existing != null)
                            existing.Quantity += quantity;
                        else
                            _cartItems.Add(new CartItem { Product = prod, Quantity = quantity });
                        RefreshCart();
                    }
                }
            }
        }
        private async void AddressBox_GotFocus(object sender, RoutedEventArgs e)
        {
            await GetAddressSuggestions();
        }
        private async void AddressBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AddressBox.Text.Trim().Length >= 2)
                await GetAddressSuggestions();
        }
        private async Task GetAddressSuggestions()
        {
            string query = AddressBox.Text.Trim();
            var addressService = new AddressService();
            var suggestions = await addressService.GetAddressSuggestionsAsync(query);
            if (suggestions != null && suggestions.Any())
            {
                string suggestionText = string.Join("\n", suggestions.Select(s => s.value));
                MessageBox.Show("Подсказки адресов:\n" + suggestionText, "Подсказки", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void RefreshCart()
        {
            CartGrid.ItemsSource = null;
            CartGrid.ItemsSource = _cartItems;
        }
        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста.");
                return;
            }
            string address = AddressBox.Text.Trim();
            if (string.IsNullOrEmpty(address))
            {
                MessageBox.Show("Введите адрес доставки.");
                return;
            }
            int orderId = Database.CreateOrder(_currentUser.Id, address, _cartItems);
            MessageBox.Show($"Заказ оформлен! Номер заказа: {orderId}");
            _cartItems.Clear();
            RefreshCart();
            LoadCatalog();
            LoadUserOrders();
        }
        private void LoadFavorites()
        {
            var favs = Database.GetFavoriteProducts(_currentUser.Id);
            FavoritesGrid.ItemsSource = favs;
        }
        private void LoadUserOrders()
        {
            var orders = Database.GetOrdersByUser(_currentUser.Id);
            UserOrdersGrid.ItemsSource = orders;
        }
        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is CartItem cartItem)
            {
                cartItem.Quantity++;
                RefreshCart();
            }
        }
        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is CartItem cartItem)
            {
                if (cartItem.Quantity > 1)
                    cartItem.Quantity--;
                else
                    _cartItems.Remove(cartItem);
                RefreshCart();
            }
        }
        private void AddToFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Product product)
            {
                Database.AddFavorite(_currentUser.Id, product.Id);
                MessageBox.Show($"Товар «{product.Name}» добавлен в избранное.");
                LoadFavorites();
            }
        }
        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is Product product)
            {
                var dialog = new QuantityDialog();
                dialog.Owner = this;
                bool? res = dialog.ShowDialog();
                if (res == true)
                {
                    int q = dialog.Quantity;
                    var existing = _cartItems.FirstOrDefault(ci => ci.Product.Id == product.Id);
                    if (existing != null)
                        existing.Quantity += q;
                    else
                        _cartItems.Add(new CartItem { Product = product, Quantity = q });
                    RefreshCart();
                }
            }
        }
    }
}