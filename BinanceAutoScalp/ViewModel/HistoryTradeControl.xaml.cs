using BinanceAutoScalp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BinanceAutoScalp.ViewModel
{
    /// <summary>
    /// Логика взаимодействия для HistoryTradeControl.xaml
    /// </summary>
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
