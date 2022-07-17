using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ParserDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        
        bool isReadyRequest;
        bool isIncludeRU09;
        bool isIncludeAvito;
        bool isIncludeYla;
        DateTime timeClick;
        long valueTimeRequest;
        long timeRequest;
        int countApartments;
        int countErrors;
        List<DataForRequest> dataForRequest;

        //avito
        string urlAvito;
        ParserAvito parserAvito;

        //ru09
        string urlRU09;
        ParserRU09 parserRU09;

        ParserYla parserYla;

        // Etagi etagi;

        TelegramBot bot;

        public MainWindow()
        {
            InitializeComponent();
            
            buttonStart.Background = Brushes.Chartreuse;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            
            dataForRequest = new List<DataForRequest>();
            //avito
            urlAvito =
                "https://www.avito.ru/tomsk/kvartiry/prodam-ASgBAgICAUSSA8YQ?f=ASgBAQICAUSSA8YQAUCQvg0Ulq41&s=104";
            parserAvito = new ParserAvito(urlAvito);

            //ru09
            urlRU09 = "https://www.tomsk.ru09.ru/realty/?type=1&otype=1&pubdate[sort]=desc";
            parserRU09 = new ParserRU09(urlRU09);
            
            //Yla
            var urlYla =
                "https://youla.ru/tomsk/nedvijimost/prodaja-kvartiri/posutochnaya-arenda-kvartir-ot-sobstvennikov?attributes[sort_field]=date_published";
            parserYla = new ParserYla(urlYla);
            
            // var client = new RestClient("https://api-gw.youla.io/federation/graphql");
            // var request = new RestRequest("", Method.Post);
            // var requestJsonData =
            //     "{ \"operationName\": \"catalogProductsBoard\", \"variables\": { \"sort\": \"DATE_PUBLISHED_DESC\", \"attributes\": [ { \"slug\": \"sort_field\", \"value\": null, \"from\": null, \"to\": null }, { \"slug\": \"sobstvennik_ili_agent\", \"value\": [ \"10705\" ], \"from\": null, \"to\": null }, { \"slug\": \"categories\", \"value\": [ \"prodaja-kvartiri\" ], \"from\": null, \"to\": null } ], \"datePublished\": null, \"location\": { \"latitude\": null, \"longitude\": null, \"city\": \"576d0619d53f3d80945f9805\", \"distanceMax\": null }, \"search\": \"\", \"cursor\": \"\"}, \"extensions\": { \"persistedQuery\": { \"version\": 1, \"sha256Hash\": \"bf7a22ef077a537ba99d2fb892ccc0da895c8454ed70358c0c7a18f67c84517f\" } } }";
            // request.AddBody(requestJsonData, "application/json");
            //
            // var response = await client.ExecuteAsync(request).ConfigureAwait(false);
            //
            // var jsonData = (JObject)JsonConvert.DeserializeObject(response.Content);
            // var result = jsonData["data"]["feed"]["items"][1]["product"]["url"];

            // var etagi = new Etagi();
            // etagi.CreateAd();

            bot = new TelegramBot();
        }

        async void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                //Секундомер
                var tick = DateTime.Now.Ticks - timeClick.Ticks;
                var stopwatch = new DateTime();
                stopwatch = stopwatch.AddTicks(tick);
                labelStopWatch.Content = string.Format("{0:HH:mm:ss}", stopwatch);

                isIncludeAvito = checkBoxAvito.IsChecked.Value;
                isIncludeRU09 = checkBoxRU09.IsChecked.Value;
                isIncludeYla = checkBoxYla.IsChecked.Value;

                if (timeRequest == 0 && !isReadyRequest)
                {
                    isReadyRequest = true;
                    
                    await Task.Run(() =>
                    {
                        if(isIncludeRU09)
                            parserRU09.Launch(ref dataForRequest, bot);
                        if(isIncludeYla)
                            parserYla.Launch(ref dataForRequest, bot);
                        if(isIncludeAvito)
                            parserAvito.Launch(ref dataForRequest, bot);
                        
                        timeRequest = valueTimeRequest;
                        isReadyRequest = false;
                    });
                }
                countApartments += dataForRequest.Count;
                labelCountApartments.Content = countApartments.ToString();
                labelCountApartments.Foreground = Brushes.Green;
                if (BlockApartments.Text == "Квартир не найдено"
                    && dataForRequest.Count > 0)
                    BlockApartments.Text = "";
                foreach (var data in dataForRequest)
                {
                    BlockApartments.Text += data.All;
                }

                dataForRequest.Clear();
                TimeSpan time = TimeSpan.FromSeconds(timeRequest);
                labelTimeRequest.Content = time.ToString(@"hh\:mm\:ss");
                timeRequest = (timeRequest > 0) ? timeRequest - 1 : timeRequest;
            }
            catch (Exception exception)
            {
                timeRequest = valueTimeRequest;
                isReadyRequest = false;
                countErrors++;
                labelCountErrors.Content = countErrors.ToString();
                labelCountErrors.Foreground = Brushes.Crimson;
                if (BlockErrors.Text == "Ошибок не обнаружено")
                    BlockErrors.Text = "";
                BlockErrors.Text += exception.Message + " " + exception.StackTrace + "\n\n";
            }
        }

        private void Button_Start(object sender, RoutedEventArgs e)
        {
            buttonStart.Background = Brushes.Azure;
            buttonStart.Content = "Перезапуск";
            BlockErrors.Text = "Ошибок не обнаружено";
            timeClick = DateTime.Now;
            valueTimeRequest = long.Parse(textTimeRequest.Text);
            timeRequest = valueTimeRequest;
            labelCountErrors.Content = "0";
            labelCountErrors.Foreground = Brushes.Gray;
            labelCountApartments.Foreground = Brushes.Gray;
            countErrors = 0;
            countApartments = 0;
            timer.Stop();
            timer.Start();
        }
    }
}