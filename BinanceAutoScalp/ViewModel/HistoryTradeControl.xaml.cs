using BinanceAutoScalp.Model;
using System.Windows.Controls;

namespace BinanceAutoScalp.ViewModel
{
    public partial class HistoryTradeControl : UserControl
    {
        public Trade variables { get; set; }
        public HistoryTradeControl(Trade trade)
        {
            InitializeComponent();
            variables = trade;
            this.DataContext = this;
        }
    }
}
