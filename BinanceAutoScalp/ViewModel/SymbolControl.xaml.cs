using Binance.Net.Objects.Models;
using BinanceAutoScalp.Binance;
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
    /// Логика взаимодействия для SymbolControl.xaml
    /// </summary>
    public partial class SymbolControl : UserControl
    {
        public Symbol symbol { get; set; } = new Symbol();
        public Socket socket { get; set; } = new Socket("Si5U4TSmpX4ByMDQEiWu9aGnHaX7o66Hw1erDl5tsfOKw1sjXTpUrP0JhonXrGJR", "ddKGxVke1y7Y0WRMBeuMeKAfqNdU7aBC8eOeHXHMY6CqYGzl0MPfuM60UkX7Dnoa");
        public SymbolControl(string symbol_name)
        {
            InitializeComponent();
            this.DataContext = this;
            symbol.SymbolName = symbol_name;
            symbol.PropertyChanged += Symbol_PropertyChanged;
        }

        private void Symbol_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Start")
            {
                if (symbol.Start)
                {
                    SubscribeOrderBook();
                }
                else
                {
                    StopAsync();
                }
            }
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            if (!symbol.Start) StopAsync();
            else SubscribeOrderBook();
        }
        
        private void StopAsync()
        {
            socket.socketClient.UnsubscribeAllAsync();
            //symbol.Ask = 0m;
            //symbol.Bid = 0m;
            //symbol.PriceAsk = 0m;
            //symbol.PriceBid = 0m;

        }
        async public void SubscribeOrderBook()
        {
            try
            {
                await socket.futuresSocket.SubscribeToPartialOrderBookUpdatesAsync(symbol.SymbolName, 20, 500, (Message => {

                    List<BinanceOrderBookEntry> list_ask = Message.Data.Asks.ToList();
                    decimal sum_ask = 0m;
                    decimal price_ask = 0m;

                    sum_ask = list_ask.Sum(it => it.Quantity);
                    price_ask = list_ask[list_ask.Count - 1].Price;

                    List<BinanceOrderBookEntry> list_bid = Message.Data.Bids.ToList();
                    decimal sum_bid = 0m;
                    decimal price_bid = 0m;
                    sum_bid = list_bid.Sum(it => it.Quantity);
                    price_bid = list_bid[list_bid.Count - 1].Price;

                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (price_ask != 0m) symbol.PriceAsk = price_ask;
                        if (sum_ask != 0m) symbol.Ask = sum_ask;
                        if (price_bid != 0m) symbol.PriceBid = price_bid;
                        if (sum_bid != 0m) symbol.Bid = sum_bid;
                    }));
                }));
            }
            catch (Exception c)
            {
                
            }

        }
    }
}
