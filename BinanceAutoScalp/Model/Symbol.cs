using System.Collections.Generic;
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
        public bool Start
        {
            get { return _Start; }
            set
            {
                _Start = value;
                OnPropertyChanged("Start");
            }
        }
        private bool _Select { get; set; } = false;
        public bool Select
        {
            get { return _Select; }
            set
            {
                _Select = value;
                OnPropertyChanged("Select");
            }
        }
        private decimal price_open_bid;
        private decimal price_close_bid;
        private decimal _Bid { get; set; }
        public decimal Bid { 
            get { return _Bid; } 
            set 
            {
                if (_Ask > 0m && value > (_Ask * _MulStart) && !BidStart)
                {
                    BidStart = true;
                    price_open_bid = _PriceBid;
                }
                _Bid = value;
                OnPropertyChanged("Bid");
            } 
        }
        private decimal price_open_ask;
        private decimal price_close_ask;
        private decimal _Ask { get; set; }
        public decimal Ask {
            get { return _Ask; } 
            set 
            { 
                if (_Bid > 0m && value > (_Bid * _MulStart) && !AskStart)
                {
                    AskStart = true;
                    price_open_ask = _PriceAsk;
                }
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
                if (value > (price_open_bid + (price_open_bid * SL)) && BidStart || value < (price_open_bid - (price_open_bid * TP)) && BidStart)
                {
                    BidStart = false;
                    price_close_bid = value;
                    CountBid = _CountBid + 1;
                    AddBid();
                }
            }
        }
        private decimal _PriceAsk { get; set; }
        public decimal PriceAsk
        {
            get { return _PriceAsk; }
            set
            {
                _PriceAsk = value;
                OnPropertyChanged("PriceAsk");
                if (value > (price_open_ask + (price_open_ask * TP)) && AskStart || value < (price_open_ask - (price_open_ask * SL)) && AskStart)
                {
                    AskStart = false;
                    price_close_ask = value;
                    CountAsk = _CountAsk + 1;
                    AddAsk();
                }
            }
        }
        private int _CountAsk { get; set; } = 0;
        public int CountAsk
        {
            get { return _CountAsk; }
            set
            {
                _CountAsk = value;
                OnPropertyChanged("CountAsk");
                if (value == 0) CountAskPlus = false;
                else CountAskPlus = true;
            }
        }
        private int _CountBid { get; set; } = 0;
        public int CountBid
        {
            get { return _CountBid; }
            set
            {
                _CountBid = value;
                OnPropertyChanged("CountBid");
                if (value == 0) CountBidPlus = false;
                else CountBidPlus = true;
            }
        }
        private decimal _MulStart { get; set; } = 5m;
        public decimal MulStart
        {
            get { return _MulStart; }
            set
            {
                if (value > 0m)
                {
                    _MulStart = value;
                    OnPropertyChanged("MulStart");
                }
                
            }
        }
        private decimal _TP { get; set; } = 0.001m;
        public decimal TP
        {
            get { return _TP; }
            set
            {
                if (value > 0m)
                {
                    _TP = value;
                    OnPropertyChanged("TP");
                }
            }
        }
        private decimal _SL { get; set; } = 0.001m;
        public decimal SL
        {
            get { return _SL; }
            set
            {
                if (value > 0m)
                {
                    _SL = value;
                    OnPropertyChanged("SL");
                }
            }
        }

        private bool _AskStart { get; set; } = false;
        public bool AskStart
        {
            get { return _AskStart; }
            set
            {
                _AskStart = value;
                OnPropertyChanged("AskStart");
            }
        }
        private bool _BidStart { get; set; } = false;
        public bool BidStart
        {
            get { return _BidStart; }
            set
            {
                _BidStart = value;
                OnPropertyChanged("BidStart");
            }
        }

        private bool _CountBidPlus { get; set; } = false;
        public bool CountBidPlus
        {
            get { return _CountBidPlus; }
            set
            {
                _CountBidPlus = value;
                OnPropertyChanged("CountBidPlus");
            }
        }
        private bool _CountAskPlus { get; set; } = false;
        public bool CountAskPlus
        {
            get { return _CountAskPlus; }
            set
            {
                _CountAskPlus = value;
                OnPropertyChanged("CountAskPlus");
            }
        }
        private List<Trade> _ListAsk { get; set; } = new List<Trade>();
        public List<Trade> ListAsk
        {
            get { return _ListAsk; }
            set
            {
                _ListAsk = value;
                OnPropertyChanged("ListAsk");
            }

        }
        private List<Trade> _ListBid { get; set; } = new List<Trade>();
        public List<Trade> ListBid
        {
            get { return _ListBid; }
            set
            {
                _ListBid = value;
                OnPropertyChanged("ListBid");
            }
        }
        public void AddAsk()
        {
            _ListAsk.Add(new Trade(price_open_ask, price_close_ask, "Long"));
            OnPropertyChanged("ListAsk");
        }
        public void AddBid()
        {
            _ListBid.Add(new Trade(price_open_bid, price_close_bid, "Short"));
            OnPropertyChanged("ListBid");
        }
    }
    public class Trade
    {
        public decimal PriceOpen { get; set; }
        public decimal PriceClose { get; set; }
        public string Position { get; set; }
        public Trade(decimal PriceOpen, decimal PriceClose, string Position)
        {
            this.PriceOpen = PriceOpen;
            this.PriceClose = PriceClose;
            this.Position = Position;
        }
    }
}
