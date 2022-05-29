using Binance.Net.Objects.Models;
using Binance.Net.Objects.Models.Spot;
using BinanceAutoScalp.Binance;
using BinanceAutoScalp.ConnectDB;
using BinanceAutoScalp.Errors;
using Newtonsoft.Json;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BinanceAutoScalp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int PERCENT { get; set; } = 1;
        public int SETTING_CHART { get; set; } = 0;
        public bool SOUND { get; set; } = false;
        public string API_KEY { get; set; } = "";
        public string SECRET_KEY { get; set; } = "";
        public string CLIENT_NAME { get; set; } = "";
        public decimal USDT_BET { get; set; } = 20m;
        public bool START_BET { get; set; } = false;
        public List<string> list_sumbols_name = new List<string>();
        public Socket socket;
        public ScatterPlot tick_sell_plot;
        public ScatterPlot tick_buy_plot;
        public ScatterPlot chart_scatter;
        public ScatterPlot ask_scatter;
        public ScatterPlot bid_scatter;
        public ScatterPlot ask_percent_scatter;
        public ScatterPlot bid_percent_scatter; 
        public Thread ping;
        public Thread thread_bid_ask;
        public Thread tick;
        public MainWindow()
        {
            InitializeComponent();
            ErrorWatcher();
            Chart();
            Clients();
            LIST_SYMBOLS.ItemsSource = list_sumbols_name;
            EXIT_GRID.Visibility = Visibility.Hidden;
            LOGIN_GRID.Visibility = Visibility.Visible;
            this.DataContext = this;
        }
        
        #region - Reload Symbols -
        private void LIST_SYMBOLS_DropDownClosed(object sender, EventArgs e)
        {
            ReloadSymbol();
        }

        private void ReloadSymbol()
        {
            try
            {
                if (list_sell_x.Count > 0 || list_buy_x.Count > 0)
                {
                    StopAsync();
                    list_sell_x.Clear();
                    list_sell_y.Clear();
                    list_buy_x.Clear();
                    list_buy_y.Clear();
                    Array.Clear(chart_x, 0, 2);
                    Array.Clear(chart_y, 0, 2);
                    Array.Clear(ask_y, 0, 2);
                    Array.Clear(bid_y, 0, 2);
                }
                string symbol = LIST_SYMBOLS.Text;
                ChartLoadingLines();
                Thread tick = new Thread(() => { SubscribeToAggregatedTrade(socket, symbol); });
                tick.Start();
                ping = new Thread(() => { Ping(socket); });
                ping.Start();
                thread_bid_ask = new Thread(() => { SubscribeOrderBook(socket, symbol); });
                thread_bid_ask.Start();
            }
            catch (Exception c)
            {
                ErrorText.Add(c.Message);
            }
        }
        #endregion

        #region - Stop Async -
        private void STOP_ASYNC_Click(object sender, RoutedEventArgs e)
        {
            StopAsync();
        }
        private void StopAsync()
        {
            try
            {
                socket.socketClient.UnsubscribeAllAsync();
                tick.Abort();
                ping.Abort();
                thread_bid_ask.Abort();
            }
            catch (Exception c)
            {
                ErrorText.Add($"STOP_ASYNC_Click {c.Message}");
            }
        }
        #endregion

        #region - Subscribe Trade Async -
        async void SubscribeToAggregatedTrade(Socket socket_thread, string symbol)
        {
            try
            {
                int count = 0;
                await socket_thread.socketClient.UsdFuturesStreams.SubscribeToAggregatedTradeUpdatesAsync(symbol, Message =>
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        double price = Decimal.ToDouble(Message.Data.Price);
                        double date = Message.Data.TradeTime.ToOADate();
                        if (Message.Data.BuyerIsMaker)
                        {
                            list_sell_x.Add(date);
                            list_sell_y.Add(price);
                        }
                        else
                        {
                            list_buy_x.Add(date);
                            list_buy_y.Add(price);
                        }
                        chart_x[0] = Message.Data.TradeTime.AddMinutes(-1).ToOADate();
                        chart_x[1] = date;
                        chart_y[0] = (price + (price / 50));
                        chart_y[1] = (price - (price / 50));
                        if (list_sell_x[0] < chart_x[0])
                        {
                            list_sell_x.RemoveAt(0);
                            list_sell_y.RemoveAt(0);
                        }
                        if (list_buy_x[0] < chart_x[0])
                        {
                            list_buy_x.RemoveAt(0);
                            list_buy_y.RemoveAt(0);
                        }
                        ask_percent_y[0] = price + (price / 100 * PERCENT);
                        ask_percent_y[1] = ask_percent_y[0];
                        bid_percent_y[0] = price - (price / 100 * PERCENT);
                        bid_percent_y[1] = bid_percent_y[0];
                        PRICE.Text = price.ToString();

                        count++;
                        if (count > SETTING_CHART)
                        {
                            ChartLoading();
                            plt.Plot.AxisAuto();
                            plt.Render();
                            count = 0;
                        }
                    }));
                });
            }
            catch (Exception c)
            {
                ErrorText.Add($"SubscribeToAggregatedTrade {c.Message}");
            }
        }
        #endregion

        #region - Subscribe Order Book Async -
        async public void SubscribeOrderBook(Socket socket_thread, string symbol)
        {
            try
            {
                await socket_thread.futuresSocket.SubscribeToPartialOrderBookUpdatesAsync(symbol, 20, 500, (Message => {

                    Dispatcher.Invoke(new Action(() =>
                    {
                        List<BinanceOrderBookEntry> list_ask = Message.Data.Asks.ToList();
                        decimal sum_ask = 0m;
                        decimal price_ask = 0m;
                        foreach (var it in list_ask)
                        {
                            sum_ask += it.Quantity;
                            price_ask = it.Price;
                        }

                        List<BinanceOrderBookEntry> list_bid = Message.Data.Bids.ToList();
                        decimal sum_bid = 0m;

                        decimal price_bid = 0m;
                        foreach (var it in list_bid)
                        {
                            sum_bid += it.Quantity;
                            price_bid = it.Price;
                        }

                        if (price_ask != 0m)
                        {
                            ask_y[0] = Decimal.ToDouble(price_ask);
                            ask_y[1] = ask_y[0];
                        }
                        if (sum_ask != 0m) ASK.Text = sum_ask.ToString();

                        if (price_bid != 0m)
                        {
                            bid_y[0] = Decimal.ToDouble(price_bid);
                            bid_y[1] = bid_y[0];
                        }
                        if (sum_bid != 0m) BID.Text = sum_bid.ToString();
                    }));
                    
                }));
            }
            catch (Exception c)
            {
                ErrorText.Add($"STOP_ASYNC_Click {c.Message}");
            }
            
        }
        #endregion

        #region - Order Book Async -
        private void BidAskLoading(Socket socket_thread, string symbol)
        {
            try
            {
                for (; ; )
                {
                    double ask = 0;
                    double bid = 0;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        ask = ask_percent_y[0];
                        bid = bid_percent_y[0];
                    }));

                    Thread thread_loading = new Thread(() => { GetOrderBook(socket_thread, symbol, ask, bid); });
                    thread_loading.Start();
                    Thread.Sleep(1000);
                }
            }
            catch (Exception c)
            {
                ErrorText.Add($"BidAsk {c.Message}");
            }
        }
        private void GetOrderBook(Socket socket_thread, string symbol, double ask, double bid)
        {
            try
            {
                var result = socket_thread.futures.ExchangeData.GetOrderBookAsync(symbol);
                List<BinanceOrderBookEntry> list_ask = result.Result.Data.Asks.ToList();
                decimal sum_ask = 0m;
                decimal price_ask = 0m;
                foreach (var it in list_ask)
                {
                    if (Decimal.ToDouble(it.Price) < ask)
                    {
                        sum_ask += it.Quantity;
                    }
                    else
                    {
                        price_ask = it.Price;
                        break;
                    }
                }

                List<BinanceOrderBookEntry> list_bid = result.Result.Data.Bids.ToList();
                decimal sum_bid = 0m;

                decimal price_bid = 0m;
                foreach (var it in list_bid)
                {
                    if (Decimal.ToDouble(it.Price) > bid)
                    {
                        sum_bid += it.Quantity;
                    }
                    else
                    {
                        price_bid = it.Price;
                        break;
                    }
                }

                Dispatcher.Invoke(new Action(() =>
                {
                    if (price_ask != 0m)
                    {
                        ask_y[0] = Decimal.ToDouble(price_ask);
                        ask_y[1] = ask_y[0];
                    }
                    if (sum_ask != 0m) ASK.Text = sum_ask.ToString();

                    if (price_bid != 0m)
                    {
                        bid_y[0] = Decimal.ToDouble(price_bid);
                        bid_y[1] = bid_y[0];
                    }
                    if (sum_bid != 0m) BID.Text = sum_bid.ToString();
                }));
            }
            catch (Exception c)
            {
                ErrorText.Add($"BidAskLoading {c.Message}");
            }
        }
        #endregion

        #region - Ping Async -
        async private void Ping(Socket socket_thread)
        {
            try
            {
                await Task.Run(() => {
                    for (; ; )
                    {
                        var result = socket_thread.futures.ExchangeData.PingAsync();
                        Dispatcher.Invoke(new Action(() =>
                        {
                            PING.Text = result.Result.Data.ToString();
                        }));
                        Thread.Sleep(1000);
                    }
                });
            }
            catch (Exception c)
            {
                ErrorText.Add($"Ping {c.Message}");
            }
        }
        #endregion

        #region - Loading Chart -
        List<double> list_sell_x = new List<double>();
        List<double> list_sell_y = new List<double>();
        List<double> list_buy_x = new List<double>();
        List<double> list_buy_y = new List<double>();
        double[] chart_x = new double[2];
        double[] chart_y = new double[2];
        double[] ask_y = new double[2];
        double[] bid_y = new double[2];
        double[] ask_percent_y = new double[2];
        double[] bid_percent_y = new double[2];

        private void ChartLoadingLines()
        {

            plt.Plot.Remove(chart_scatter);
            plt.Plot.Remove(ask_scatter);
            plt.Plot.Remove(bid_scatter);
            plt.Plot.Remove(ask_percent_scatter);
            plt.Plot.Remove(bid_percent_scatter);
            chart_scatter = plt.Plot.AddScatterLines(chart_x, chart_y, Color.Transparent, lineStyle: LineStyle.Dash);
            chart_scatter.YAxisIndex = 1;
            ask_scatter = plt.Plot.AddScatterLines(chart_x, ask_y, Color.Red, lineStyle: LineStyle.Dash);
            ask_scatter.YAxisIndex = 1;
            bid_scatter = plt.Plot.AddScatterLines(chart_x, bid_y, Color.Green, lineStyle: LineStyle.Dash);
            bid_scatter.YAxisIndex = 1;
            ask_percent_scatter = plt.Plot.AddScatterLines(chart_x, ask_percent_y, Color.LightPink, lineStyle: LineStyle.Dash);
            ask_percent_scatter.YAxisIndex = 1;
            bid_percent_scatter = plt.Plot.AddScatterLines(chart_x, bid_percent_y, Color.LightGreen, lineStyle: LineStyle.Dash);
            bid_percent_scatter.YAxisIndex = 1;
        }

        private void ChartLoading()
        {
            plt.Plot.Remove(tick_sell_plot);
            plt.Plot.Remove(tick_buy_plot);
            tick_sell_plot = plt.Plot.AddScatter(list_sell_x.ToArray(), list_sell_y.ToArray(), color: Color.Red, lineWidth: 0, markerSize: 3);
            tick_sell_plot.YAxisIndex = 1;
            tick_buy_plot = plt.Plot.AddScatter(list_buy_x.ToArray(), list_buy_y.ToArray(), color: Color.Green, lineWidth: 0, markerSize: 3);
            tick_buy_plot.YAxisIndex = 1;
        }
        #endregion

        #region - Event CheckBox -
        private void START_BET_Click(object sender, RoutedEventArgs e)
        {
            CheckBox box = e.Source as CheckBox;
            START_BET = (bool)box.IsChecked;
        }

        #endregion

        #region - Chart -
        private void Chart()
        {
            plt.Plot.Layout(padding: 12);
            plt.Plot.Style(figureBackground: Color.Black, dataBackground: Color.Black);
            plt.Plot.Frameless();
            plt.Plot.XAxis.TickLabelStyle(color: Color.White);
            plt.Plot.XAxis.TickMarkColor(ColorTranslator.FromHtml("#000000"));
            plt.Plot.XAxis.MajorGrid(color: ColorTranslator.FromHtml("#000000"));

            plt.Plot.YAxis.Ticks(false);
            plt.Plot.YAxis.Grid(false);
            plt.Plot.YAxis2.Ticks(true);
            plt.Plot.YAxis2.Grid(true);
            plt.Plot.YAxis2.TickLabelStyle(color: ColorTranslator.FromHtml("#00FF00"));
            plt.Plot.YAxis2.TickMarkColor(ColorTranslator.FromHtml("#000000"));
            plt.Plot.YAxis2.MajorGrid(color: ColorTranslator.FromHtml("#000000"));

            var legend = plt.Plot.Legend();
            legend.FillColor = System.Drawing.Color.Transparent;
            legend.OutlineColor = Color.Transparent;
            legend.Font.Color = Color.White;
            legend.Font.Bold = true;
        }
        #endregion

        #region - List Sumbols -
        private void GetSumbolName()
        {
            foreach (var it in ListSymbols())
            {
                list_sumbols_name.Add(it.Symbol);
            }
            list_sumbols_name.Sort();
            LIST_SYMBOLS.Items.Refresh();
            LIST_SYMBOLS.SelectedIndex = 0;
        }
        public List<BinancePrice> ListSymbols()
        {
            try
            {
                var result = socket.futures.ExchangeData.GetPricesAsync().Result;
                if (!result.Success) ErrorText.Add("Error GetKlinesAsync");
                return result.Data.ToList();
            }
            catch (Exception e)
            {
                ErrorText.Add($"ListSymbols {e.Message}");
                return ListSymbols();
            }
        }

        #endregion

        #region - Sound Order-
        private void SOUND_Click(object sender, RoutedEventArgs e)
        {
            CheckBox box = e.Source as CheckBox;
            SOUND = (bool)box.IsChecked;
        }
        private void SoundOpenOrder()
        {
            try
            {
                if (SOUND) new SoundPlayer(Properties.Resources.wav_2).Play();
            }
            catch (Exception c)
            {
                ErrorText.Add($"SoundOpenOrder {c.Message}");
            }
        }
        private void SoundCloseOrder()
        {
            try
            {
                if (SOUND) new SoundPlayer(Properties.Resources.wav_1).Play();
            }
            catch (Exception c)
            {
                ErrorText.Add($"LoadingCandlesToChart {c.Message}");
            }
        }
        #endregion

        #region - Login -
        private void Button_Save(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CLIENT_NAME != "" && API_KEY != "" && SECRET_KEY != "")
                {
                    if (ConnectTrial.Check(CLIENT_NAME))
                    {
                        string path = System.IO.Path.Combine(Environment.CurrentDirectory, "clients");
                        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                        if (!File.Exists(path + "/" + CLIENT_NAME))
                        {

                            Client client = new Client(CLIENT_NAME, API_KEY, SECRET_KEY);
                            string json = JsonConvert.SerializeObject(client);
                            File.WriteAllText(path + "/" + CLIENT_NAME, json);
                            Clients();
                            CLIENT_NAME = "";
                            API_KEY = "";
                            SECRET_KEY = "";
                        }
                    }
                    else ErrorText.Add("Сlient name not found!");
                }
            }
            catch (Exception c)
            {
                ErrorText.Add($"Button_Save {c.Message}");
            }
        }
        private void Clients()
        {
            try
            {
                string path = System.IO.Path.Combine(Environment.CurrentDirectory, "clients");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                List<string> filesDir = (from a in Directory.GetFiles(path) select System.IO.Path.GetFileNameWithoutExtension(a)).ToList();
                if (filesDir.Count > 0)
                {
                    ClientList file_list = new ClientList(filesDir);
                    BOX_NAME.ItemsSource = file_list.BoxNameContent;
                    BOX_NAME.SelectedItem = file_list.BoxNameContent[0];
                }
            }
            catch (Exception e)
            {
                ErrorText.Add($"Clients {e.Message}");
            }
        }
        private void Button_Login(object sender, RoutedEventArgs e)
        {
            try
            {
                if (API_KEY != "" && SECRET_KEY != "" && CLIENT_NAME != "")
                {
                    if (ConnectTrial.Check(CLIENT_NAME))
                    {
                        socket = new Socket(API_KEY, SECRET_KEY);
                        Login_Click();
                        CLIENT_NAME = "";
                        API_KEY = "";
                        SECRET_KEY = "";
                    }
                    else ErrorText.Add("Сlient name not found!");
                }
                else if (BOX_NAME.Text != "")
                {
                    string path = System.IO.Path.Combine(Environment.CurrentDirectory, "clients");
                    string json = File.ReadAllText(path + "\\" + BOX_NAME.Text);
                    Client client = JsonConvert.DeserializeObject<Client>(json);
                    if (ConnectTrial.Check(client.ClientName))
                    {
                        socket = new Socket(client.ApiKey, client.SecretKey);
                        Login_Click();
                    }
                    else ErrorText.Add("Сlient name not found!");
                }
            }
            catch (Exception c)
            {
                ErrorText.Add($"Button_Login {c.Message}");
            }
        }
        private void Login_Click()
        {
            LOGIN_GRID.Visibility = Visibility.Hidden;
            EXIT_GRID.Visibility = Visibility.Visible;
            GetSumbolName();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            EXIT_GRID.Visibility = Visibility.Hidden;
            LOGIN_GRID.Visibility = Visibility.Visible;
            socket = null;
            list_sumbols_name.Clear();
        }
        #endregion

        #region - Error -
        // ------------------------------------------------------- Start Error Text Block --------------------------------------
        private void ErrorWatcher()
        {
            try
            {
                FileSystemWatcher error_watcher = new FileSystemWatcher();
                error_watcher.Path = ErrorText.Directory();
                error_watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                error_watcher.Changed += new FileSystemEventHandler(OnChanged);
                error_watcher.Filter = ErrorText.Patch();
                error_watcher.EnableRaisingEvents = true;
            }
            catch (Exception e)
            {
                ErrorText.Add($"ErrorWatcher {e.Message}");
            }
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => { ERROR_LOG.Text = File.ReadAllText(ErrorText.FullPatch()); }));
        }
        private void Button_ClearErrors(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(ErrorText.FullPatch(), "");
        }
        // ------------------------------------------------------- End Error Text Block ----------------------------------------
        #endregion

    }
}
