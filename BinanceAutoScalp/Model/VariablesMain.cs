using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BinanceAutoScalp.Model
{
    public class VariablesMain : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        private int _PositiveTrade { get; set; } = 0;
        public int PositiveTrade
        {
            get { return _PositiveTrade; }
            set
            {
                _PositiveTrade = value;
                OnPropertyChanged("PositiveTrade");
            }
        }
        private int _NegativeTrade { get; set; } = 0;
        public int NegativeTrade
        {
            get { return _NegativeTrade; }
            set
            {
                _NegativeTrade = value;
                OnPropertyChanged("NegativeTrade");
            }
        }
        private int _AllPositiveTrade { get; set; } = 0;
        public int AllPositiveTrade
        {
            get { return _AllPositiveTrade; }
            set
            {
                _AllPositiveTrade = value;
                OnPropertyChanged("AllPositiveTrade");
            }
        }
        private int _AllNegativeTrade { get; set; } = 0;
        public int AllNegativeTrade
        {
            get { return _AllNegativeTrade; }
            set
            {
                _AllNegativeTrade = value;
                OnPropertyChanged("AllNegativeTrade");
            }
        }
        private int _CountSymbols { get; set; } = 0;
        public int CountSymbols
        {
            get { return _CountSymbols; }
            set
            {
                _CountSymbols = value;
                OnPropertyChanged("CountSymbols");
            }
        }
    }
}
