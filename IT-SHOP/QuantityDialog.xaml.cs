using System.Windows;

namespace ShopApp
{
    public partial class QuantityDialog : Window
    {
        public int Quantity { get; private set; }
        public QuantityDialog()
        {
            InitializeComponent();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityBox.Text.Trim(), out int qty) && qty > 0)
            {
                Quantity = qty;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Введите положительное целое число.");
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}