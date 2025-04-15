using System.ComponentModel;
using System.Runtime.CompilerServices;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Password { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Price { get; set; }      
    public string Quantity { get; set; }   
    public byte[] Image { get; set; }      
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Date { get; set; }      
    public string Status { get; set; }
    public string Address { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public string Quantity { get; set; }
}
public class CartItem : INotifyPropertyChanged
{
    public Product Product { get; set; }

    private int _quantity;
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }

    public decimal TotalPrice => decimal.Parse(Product.Price) * _quantity;

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}