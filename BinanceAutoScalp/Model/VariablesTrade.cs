using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BinanceAutoScalp.Model
{
    public class VariablesTrade : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private decimal _PriceOpen { get; set; }
        public decimal PriceOpen
        {
            get { return _PriceOpen; }
            set
            {
                _PriceOpen = value;
                OnPropertyChanged("PriceOpen");
            }
        }
        private decimal _PriceClose { get; set; }
        public decimal PriceClose
        {
            get { return _PriceClose; }
            set
            {
                _PriceClose = value;
                OnPropertyChanged("PriceClose");
            }
        }
        private string _Position { get; set; }
        public string Position
        {
            get { return _Position; }
            set
            {
                _Position = value;
                OnPropertyChanged("Position");
                if (value == "Long")
                {
                    isLong = true;
                    Profit = ((_PriceClose - _PriceOpen) / _PriceOpen * 100);
                }
                else
                {
                    isLong = false;
                    Profit = ((_PriceOpen - _PriceClose) / _PriceClose * 100);
                }
            }
        }
        private bool _isLong { get; set; }
        public bool isLong
        {
            get { return _isLong; }
            set
            {
                _isLong = value;
                OnPropertyChanged("isLong");
            }
        }
        private decimal _Profit { get; set; }
        public decimal Profit
        {
            get { return _Profit; }
            set
            {
                _Profit = value;
                OnPropertyChanged("PriceClose");
                if (value < 0m) isPlusProfit = false;
                else isPlusProfit = true;
            }
        }
        private bool _isPlusProfit { get; set; }
        public bool isPlusProfit
        {
            get { return _isPlusProfit; }
            set
            {
                _isPlusProfit = value;
                OnPropertyChanged("isPlusProfit");
            }
        }
    }
}
