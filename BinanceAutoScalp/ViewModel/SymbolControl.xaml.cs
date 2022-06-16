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
using System.Windows.Threading;

namespace BinanceAutoScalp.ViewModel
{
    /// <summary>
    /// Логика взаимодействия для SymbolControl.xaml
    /// </summary>
    public partial class SymbolControl : UserControl
    {
        public DispatcherTimer timer = new DispatcherTimer();
        public Symbol symbol { get; set; } = new Symbol();
        public Socket socket { get; set; } = new Socket("Si5U4TSmpX4ByMDQEiWu9aGnHaX7o66Hw1erDl5tsfOKw1sjXTpUrP0JhonXrGJR", "ddKGxVke1y7Y0WRMBeuMeKAfqNdU7aBC8eOeHXHMY6CqYGzl0MPfuM60UkX7Dnoa");
        public SymbolControl(string symbol_name)
        {
            InitializeComponent();
            this.DataContext = this;
            symbol.SymbolName = symbol_name;
            symbol.PropertyChanged += Symbol_PropertyChanged;
            timer.Interval = TimeSpan.FromMinutes(1);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (symbol.Start)
            {
                if (symbol.ListTrade.Count == 0 && symbol.MulStart >= 3m || symbol.ListTrade.Count > 0 && symbol.MulStart >= 3m && symbol.UpdateTime - symbol.ListTrade[symbol.ListTrade.Count - 1].UpdateTime > TimeSpan.FromMinutes(symbol.CheckTimeUpdate))
                {
                    symbol.MulStart = symbol.MulStart - 1m;
                }
            }
        }

        private void Symbol_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Start")
            {
                if (symbol.Start)
                {
                    SubscribeOrderBook();
                    timer.Start();
                }
                else
                {
                    timer.Stop();
                    StopAsync();
                }
            }
            if (e.PropertyName == "ListTrade")
            {
                if (symbol.ListTrade.Count > 2 && !symbol.ListTrade[symbol.ListTrade.Count - 1].isPositive && !symbol.ListTrade[symbol.ListTrade.Count - 2].isPositive)
                {
                    symbol.MulStart = symbol.MulStart + 1m;
                }
            }
        }
        
        private async void StopAsync()
        {
            await Task.Run(() => {
                Stop();
            });
        }
        private void Stop()
        {
            var result = socket.socketClient.UnsubscribeAllAsync();
            result.Wait(100);
            //symbol.Ask = 0m;
            //symbol.Bid = 0m;
            //symbol.PriceAsk = 0m;
            //symbol.PriceBid = 0m;
            //symbol.CountAsk = 0;
            //symbol.CountBid = 0;
            //symbol.BidStart = false;
            //symbol.AskStart = false;
            //symbol.ListTrade = new List<Trade>();
        }
        async public void SubscribeOrderBook()
        {
            try
            {
                await socket.futuresSocket.SubscribeToPartialOrderBookUpdatesAsync(symbol.SymbolName, 20, 500, (Message => {

                    List<BinanceOrderBookEntry> list_ask = Message.Data.Asks.ToList();
                    decimal price_ask = list_ask[0].Price;
                    decimal sum_ask = list_ask.Sum(it => it.Quantity);

                    List<BinanceOrderBookEntry> list_bid = Message.Data.Bids.ToList();
                    decimal price_bid = list_bid[0].Price;
                    decimal sum_bid = list_bid.Sum(it => it.Quantity);

                    Dispatcher.Invoke(new Action(() =>
                    {
                        //if (price_ask != 0m) symbol.PriceAsk = price_ask;
                        //if (sum_ask != 0m) symbol.Ask = sum_ask;
                        //if (price_bid != 0m) symbol.PriceBid = price_bid;
                        //if (sum_bid != 0m) symbol.Bid = sum_bid;
                        symbol.PriceAsk = price_ask;
                        symbol.PriceBid = price_bid;
                        symbol.Ask = sum_ask;
                        symbol.Bid = sum_bid;
                        symbol.UpdateTime = Message.Data.TransactionTime;
                    }));
                }));
            }
            catch (Exception c)
            {
                
            }
        }
    }
}
