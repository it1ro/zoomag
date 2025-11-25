// Zoomag/Models/ReceiptItem.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Zoomag.Models
{
    public class ReceiptItem : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public int Price { get; set; }

        private int _maxQuantity;
        public int MaxQuantity
        {
            get => _maxQuantity;
            set
            {
                if (_maxQuantity != value)
                {
                    _maxQuantity = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set
            {
                // Ограничиваем значение
                var clamped = Math.Max(1, Math.Min(value, _maxQuantity));
                if (_quantity != clamped)
                {
                    _quantity = clamped;
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
