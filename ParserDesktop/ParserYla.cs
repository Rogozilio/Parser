using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ParserDesktop
{
    public class ParserYla : IParser
    {
        private HtmlNode _node;
        private string _url;
        private string _oldItemUrl;
        private List<string> _listItemUrl;
        private string _allTextPage;
        private RestClient client;

        public ParserYla(string url)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _url = url;
            _oldItemUrl = String.Empty;
            _listItemUrl = new List<string>();
        }

        private void GetItems()
        {
            _listItemUrl = new List<string>();
            client = new RestClient("https://api-gw.youla.io/federation/graphql");
            var request = new RestRequest("", Method.Post);
            // var requestJsonData =
            //     "{ \"operationName\": \"catalogProductsBoard\", \"variables\": { \"sort\": \"DATE_PUBLISHED_DESC\", \"attributes\": [ { \"slug\": \"sort_field\", \"value\": null, \"from\": null, \"to\": null }, { \"slug\": \"sobstvennik_ili_agent\", \"value\": [ \"10705\" ], \"from\": null, \"to\": null }, { \"slug\": \"categories\", \"value\": [ \"prodaja-kvartiri\" ], \"from\": null, \"to\": null } ], \"datePublished\": null, \"location\": { \"latitude\": null, \"longitude\": null, \"city\": \"576d0619d53f3d80945f9805\", \"distanceMax\": null }, \"search\": \"\", \"cursor\": \"\"}, \"extensions\": { \"persistedQuery\": { \"version\": 1, \"sha256Hash\": \"bf7a22ef077a537ba99d2fb892ccc0da895c8454ed70358c0c7a18f67c84517f\" } } }";
            var requestJsonData =
                "{\"operationName\":\"catalogProductsBoard\",\"variables\":{\"sort\":\"DATE_PUBLISHED_DESC\",\"attributes\":[{\"slug\":\"sobstvennik_ili_agent\",\"value\":[\"10705\"],\"from\":null,\"to\":null},{\"slug\":\"categories\",\"value\":[\"prodaja-kvartiri\"],\"from\":null,\"to\":null}],\"datePublished\":null,\"location\":{\"latitude\":null,\"longitude\":null,\"city\":\"576d0619d53f3d80945f9805\",\"distanceMax\":null},\"search\":\"\",\"cursor\":\"\"},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"6e7275a709ca5eb1df17abfb9d5d68212ad910dd711d55446ed6fa59557e2602\"}}}";
            request.AddBody(requestJsonData, "application/json");
            
            var response = client.Execute(request);
            
            var jsonData = (JObject)JsonConvert.DeserializeObject(response.Content);
            var allItems = jsonData["data"]["feed"]["items"];
            var countItems = allItems.Count();

            // if (_node == null)
            //     throw new Exception("Значение _node = null");

            var topUrl = String.Empty;

            for(var i = 1; i <= countItems; i++)
            {
                var itemUrl = allItems[i]["product"]["url"].Value<string>();

                if (_oldItemUrl == String.Empty)
                {
                    _oldItemUrl = itemUrl;
                    break;
                }

                if (_oldItemUrl != itemUrl)
                {
                    if (topUrl == String.Empty)
                        topUrl = itemUrl;
                    var nextUrl = itemUrl;
                    _listItemUrl.Add("https://youla.ru" + nextUrl);
                }
                else break;
            }

            _oldItemUrl = (topUrl != String.Empty) ? topUrl : _oldItemUrl;
        }

        private void SetDataForRequest()
        {
            
        }

        public void Launch(ref List<DataForRequest> data, TelegramBot bot)
        {
            GetItems();
            // _listItemUrl.Add(
            //     "https://youla.ru/tomsk/nedvijimost/prodaja-kvartiri/kvartira-1-komnata-39-m2-62988392b19669424b1bd730");
            foreach (var itemUrl in _listItemUrl)
            {
                var newData = new DataForRequest();
                
                _node = new HtmlWeb().Load(itemUrl).DocumentNode;
                _allTextPage = _node.OuterHtml.Split("window.__YOULA_STATE__ = ")[1]
                    .Split("window.__YOULA_TEST__ = {")[0];
                
                newData.Name = GetName();
                newData.Phone = GetPhone();
                newData.Price = GetPrice();
                newData.Address = GetAddress();
                newData.Floor = GetFloor();
                newData.NumbersOfRoom = GetNumberOfRoom();
                newData.TotalArea = GetTotalArea();
                newData.KitchenArea = GetKitchenArea();
                newData.Renovation = GetRenovation();
                newData.Ref = itemUrl;
                bot.Execute(newData.All);
                data.Add(newData);
            }
        }

        private string Decoder(string value)
        {
            var decodingString = value.Split("\\u").Select(x =>
            {
                if (string.IsNullOrWhiteSpace(x) || x.Length != 4 && x.Length != 5)
                {
                    return x;
                }

                if (x.Length == 5)
                {
                    x = x.Substring(0,4);
                    int value = int.Parse(x, NumberStyles.HexNumber);
                    var result = char.ConvertFromUtf32(value) + ' ';
                    return result;
                }
                int value2 = int.Parse(x, NumberStyles.HexNumber);
                return char.ConvertFromUtf32(value2);
            });
            return string.Join(null, decodingString);
        }

        public string GetName()
        {
            var name = _allTextPage.Split("owner")[1].Split("name\":\"")[1].Split(" ")[0];
            name = Decoder(name);
            return name;
        }

        public string GetPhone()
        {
            return "Неизвестно";
        }

        public string GetPrice()
        {
            var price = _allTextPage.Split("\"price\":")[1].Split(",")[0];
            price = price?.Substring(0, price.Length - 2);
            return price;
        }

        public string GetAddress()
        {
            var address = _allTextPage.Split("location\":{\"description\":\"")[2].Split("\"")[0];
            var decodingAddress = address.Split(", ").Select(x => Decoder(x) + " ");
            return string.Join(null, decodingAddress).Split("Томск ")[1];
        }

        public string GetFloor()
        {
            var floor = _allTextPage.Split("realty_etaj\",\"values")[1].Split("value\":\"")[1].Split("\"")[0];
            return floor;
        }

        public string GetNumberOfRoom()
        {
            return _node.SelectSingleNode("//title").InnerText.Split("Квартира, ")[1].Split(" ")[0];
        }

        public string GetTotalArea()
        {
            return _node.SelectSingleNode("//title").InnerText.Split(", ")[2].Split(" ")[0];
        }

        public string GetKitchenArea()
        {
            var kitchenArea = _allTextPage.Split("realty_ploshad_kuhni\",\"values")[1].Split("value\":\"")[1].Split("\"")[0];
            return kitchenArea.Substring(0, kitchenArea.Length - 2);;
        }

        public string GetRenovation()
        {
            return "Косметический";
        }
    }
}