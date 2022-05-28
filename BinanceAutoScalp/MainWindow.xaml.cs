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
        public bool SOUND { get; set; } = false;
        public string API_KEY { get; set; } = "";
        public string SECRET_KEY { get; set; } = "";
        public string CLIENT_NAME { get; set; } = "";
        public decimal USDT_BET { get; set; } = 20m;
        public bool START_BET { get; set; } = false;
        public List<string> list_sumbols_name = new List<string>();
        public Socket socket;
        public ScatterPlot tick_plot; 
        public ScatterPlot chart_scatter;
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
        private void LIST_SYMBOLS_DropDownClosed(object sender, EventArgs e)
        {
            StartKlineAsync();
        }
        private void STOP_ASYNC_Click(object sender, RoutedEventArgs e)
        {
            StopAsync();
        }
        private void StopAsync()
        {
            try
            {
                socket.socketClient.UnsubscribeAllAsync();
            }
            catch (Exception c)
            {
                ErrorText.Add($"STOP_ASYNC_Click {c.Message}");
            }
        }
        List<double> list_x = new List<double>();
        List<double> list_y = new List<double>();
        double[] chart_x = new double[2];
        double[] chart_y = new double[2];
        public void StartKlineAsync()
        {
            StopAsync();
            if (list_x.Count > 0 || list_y.Count > 0)
            {
                list_x.Clear();
                list_y.Clear(); 
                Array.Clear(chart_x, 0, 2);
                Array.Clear(chart_y, 0, 2);
            }
            socket.socketClient.UsdFuturesStreams.SubscribeToAggregatedTradeUpdatesAsync(LIST_SYMBOLS.Text, Message =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    decimal price = Message.Data.Price;
                    list_x.Add(Message.Data.TradeTime.ToOADate());
                    list_y.Add(Decimal.ToDouble(Message.Data.Price));
                    chart_x[0] = Message.Data.TradeTime.AddMinutes(-5).ToOADate();
                    chart_x[1] = Message.Data.TradeTime.ToOADate();
                    chart_y[0] = (Decimal.ToDouble(price + (price / 50)));
                    chart_y[1] = (Decimal.ToDouble(price - (price / 50)));
                    if(list_x.Count > 180)
                    {
                        list_x.RemoveAt(0);
                        list_y.RemoveAt(0);
                    }
                    ChartLoading();
                }));
            });
        }

        private void ChartLoading()
        {
            plt.Plot.Remove(tick_plot);
            plt.Plot.Remove(chart_scatter);
            tick_plot = plt.Plot.AddScatter(list_x.ToArray(), list_y.ToArray(), color: Color.Green, lineWidth: 0, markerSize: 5);
            tick_plot.YAxisIndex = 1;
            chart_scatter = plt.Plot.AddScatterLines(chart_x, chart_y, Color.Transparent, lineStyle: LineStyle.Dash, label: chart_y[0] + " - price");
            chart_scatter.YAxisIndex = 1;
            plt.Refresh();
        }

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
            plt.Plot.XAxis.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            plt.Plot.XAxis.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

            plt.Plot.YAxis.Ticks(false);
            plt.Plot.YAxis.Grid(false);
            plt.Plot.YAxis2.Ticks(true);
            plt.Plot.YAxis2.Grid(true);
            plt.Plot.YAxis2.TickLabelStyle(color: ColorTranslator.FromHtml("#00FF00"));
            plt.Plot.YAxis2.TickMarkColor(ColorTranslator.FromHtml("#333333"));
            plt.Plot.YAxis2.MajorGrid(color: ColorTranslator.FromHtml("#333333"));

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
