using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BinanceAutoScalp.Model
{
    public class Symbol : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private string _SymbolName { get; set; }
        public string SymbolName
        { 
            get { return _SymbolName; } 
            set 
            {
                _SymbolName = value;
                OnPropertyChanged("SymbolName");
            } 
        }
        private bool _Start { get; set; }
        public bool Start {
            get { return _Start; } 
            set 
            { 
                _Start = value;
                OnPropertyChanged("Start");
            } 
        }
        private decimal _Bid { get; set; }
        public decimal Bid { 
            get { return _Bid; } 
            set 
            {
                _Bid = value;
                OnPropertyChanged("Bid");
            } 
        }
        private decimal _Ask { get; set; }
        public decimal Ask {
            get { return _Ask; } 
            set 
            { 
                _Ask = value;
                OnPropertyChanged("Ask");
            } 
        }
        private decimal _PriceBid { get; set; }
        public decimal PriceBid {
            get { return _PriceBid; } 
            set 
            { 
                _PriceBid = value;
                OnPropertyChanged("PriceBid");
            } 
        }
        private decimal _PriceAsk { get; set; }
        public decimal PriceAsk { 
            get { return _PriceAsk; }
            set { 
                _PriceAsk = value;
                OnPropertyChanged("PriceAsk");
            } 
        }
    }
}
