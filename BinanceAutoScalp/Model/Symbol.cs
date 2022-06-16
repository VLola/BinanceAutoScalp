using System;
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
        private decimal mul_start_bid;
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
                    mul_start_bid = MulStart;
                }
                _Bid = value;
                OnPropertyChanged("Bid");
            } 
        }
        private decimal mul_start_ask;
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
                    mul_start_ask = MulStart;
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
                    UpdateTimeBid = UpdateTime;
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
                    UpdateTimeAsk = UpdateTime;
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
        private List<Trade> _ListTrade { get; set; } = new List<Trade>();
        public List<Trade> ListTrade
        {
            get { return _ListTrade; }
            set
            {
                _ListTrade = value;
                OnPropertyChanged("ListTrade");
            }

        }
        public void AddAsk()
        {
            _ListTrade.Add(new Trade(price_open_ask, price_close_ask, "Long", UpdateTimeAsk, mul_start_ask));
            OnPropertyChanged("ListTrade"); 
            SetProfit();
        }
        public void AddBid()
        {
            _ListTrade.Add(new Trade(price_open_bid, price_close_bid, "Short", UpdateTimeBid, mul_start_bid));
            OnPropertyChanged("ListTrade");
            SetProfit();
        }

        private DateTime _UpdateTimeAsk { get; set; }
        public DateTime UpdateTimeAsk
        {
            get { return _UpdateTimeAsk; }
            set
            {
                _UpdateTimeAsk = value;
                OnPropertyChanged("UpdateTimeAsk");
            }
        }
        private DateTime _UpdateTimeBid { get; set; }
        public DateTime UpdateTimeBid
        {
            get { return _UpdateTimeBid; }
            set
            {
                _UpdateTimeBid = value;
                OnPropertyChanged("UpdateTimeBid");
            }
        }
        private DateTime _UpdateTime { get; set; }
        public DateTime UpdateTime
        {
            get { return _UpdateTime; }
            set
            {
                _UpdateTime = value;
                OnPropertyChanged("UpdateTime");
            }
        }
        private int _CheckTimeUpdate { get; set; } = 5;
        public int CheckTimeUpdate
        {
            get { return _CheckTimeUpdate; }
            set
            {
                _CheckTimeUpdate = value;
                OnPropertyChanged("CheckTimeUpdate");
            }
        }
        private decimal _Profit { get; set; }
        public decimal Profit
        {
            get { return _Profit; }
            set
            {
                _Profit = value;
                OnPropertyChanged("Profit");
                if (value < 0m) isPositiveProfit = false;
                else isPositiveProfit = true;
            }
        }
        private bool _isPositiveProfit { get; set; }
        public bool isPositiveProfit
        {
            get { return _isPositiveProfit; }
            set
            {
                _isPositiveProfit = value;
                OnPropertyChanged("isPositiveProfit");
            }
        }
        private void SetProfit()
        {
            decimal profit = 0m;
            foreach (var it in ListTrade)
            {
                profit += it.Profit;
            }
            Profit = profit;
        }
    }
    public class Trade
    {
        public decimal PriceOpen { get; set; }
        public decimal PriceClose { get; set; }
        public string Position { get; set; }
        public bool isLong { get; set; }
        public decimal Profit { get; set; }
        public bool isPositive { get; set; }
        public DateTime UpdateTime { get; set; }
        public decimal MulStart { get; set; }
        public Trade(decimal priceOpen, decimal priceClose, string position, DateTime updateTime, decimal mulStart)
        {
            try {
                PriceOpen = priceOpen;
                PriceClose = priceClose;
                Position = position;
                UpdateTime = updateTime; 
                MulStart = mulStart;
                if (position == "Long")
                {
                    isLong = true;
                    Profit = ((priceClose - priceOpen) / priceOpen);
                }
                else
                {
                    isLong = false;
                    Profit = ((priceOpen - priceClose) / priceClose);
                }
                if (Profit < 0m) isPositive = false;
                else isPositive = true;
            } 
            catch { }
        }
    }
}
