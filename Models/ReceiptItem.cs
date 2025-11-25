using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Zoomag.Models
{
    public class ReceiptItem : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public int Price { get; set; }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value && value > 0)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        public int Total => Price * Quantity;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
