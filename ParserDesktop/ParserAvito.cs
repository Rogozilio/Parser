using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using ParserDesktop.Sample;
using RestSharp;
using Tesseract;

namespace ParserDesktop
{
    public class ParserAvito : IParser
    {
        private HtmlNode _node;
        private List<string> _listItemUrl;
        private string _url;
        private string _oldItemUrl;
        private HtmlWeb _htmlWeb;

        public ParserAvito(string url)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            _url = url;
            _oldItemUrl = String.Empty;
            _htmlWeb = new HtmlWeb();
        }

        private void GetItems()
        {
            _listItemUrl = new List<string>();
            _node = _htmlWeb.Load(_url).DocumentNode;

            if (_node == null)
                throw new Exception("Значение _node = null");

            var topUrl = String.Empty;

            HtmlNodeCollection htmlItemNodes;
            htmlItemNodes = _node.SelectNodes(".//div[@data-marker='item']");

            if (htmlItemNodes == null)
                throw new Exception("Значение htmlItemNodes = null");

            foreach (var item in htmlItemNodes)
            {
                var itemUrl = item.SelectSingleNode(".//*[@itemprop='url']").Attributes["href"]
                    .Value;

                if (_oldItemUrl == String.Empty)
                {
                    _oldItemUrl = itemUrl;
                    break;
                }

                if (_oldItemUrl != itemUrl)
                {
                    if (topUrl == String.Empty)
                        topUrl = itemUrl;
                    _listItemUrl.Add("https://www.avito.ru" + itemUrl);
                }
                else break;
            }

            _oldItemUrl = (topUrl != String.Empty) ? topUrl : _oldItemUrl;
        }

        public void Launch(ref List<DataForRequest> data, TelegramBot bot)
        {
            GetItems();
            //_listItemUrl.Add("https://www.avito.ru/tomsk/kvartiry/1-k._kvartira_47m_1010et._2485898141");
            foreach (var itemUrl in _listItemUrl)
            {
                var newData = new DataForRequest();
                _node = _htmlWeb.Load(itemUrl).DocumentNode;
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

        public string GetName()
        {
            var result = _node
                .SelectSingleNode("//span[contains(@class, 'sticky-header-seller-text')]")
                .InnerText.Trim();
            return result;
        }

        public string GetPhone()
        {
            var id = _node.SelectSingleNode("//div[@data-item-id]").Attributes["data-item-id"]
                .Value;
            return GetTextFromImage(GetBase64(id).Result);
        }

        public string GetPrice()
        {
            var result = _node
                .SelectSingleNode("//span[@itemprop='price']")
                .InnerText.Trim();
            return result;
        }

        public string GetAddress()
        {
            var result = _node
                .SelectSingleNode("//span[contains(@class, 'item-address__string')]")
                .InnerText.Trim();
            return result;
        }

        public string GetFloor()
        {
            var result = GetFromListData("Этаж").Split(" ")[0];
            return result;
        }

        public string GetNumberOfRoom()
        {
            var result = GetFromListData("Количество комнат");
            return result;
        }

        public string GetTotalArea()
        {
            var result = GetFromListData("Общая площадь").Split("&")[0];
            return result;
        }

        public string GetKitchenArea()
        {
            var result = GetFromListData("Площадь кухни").Split("&")[0];
            return result;
        }

        public string GetRenovation()
        {
            var result = GetFromListData("Ремонт").Split(" ")[0];
            return result;
        }

        private string GetFromListData(string nameField)
        {
            var nodes = _node.SelectNodes("//li[contains(@class, 'params-paramsList__item')]");
            for (int i = 0; i < nodes.Count; i++)
            {
                var index = nodes[i].InnerText.Trim();
                var value = index.Split(":")[1].Trim();
                if (index.Split(":")[0].Trim() == nameField)
                {
                    return value;
                }
            }

            return "не_указано";
        }

        private string GetTextFromImage(string base64)
        {
            var bytes = Convert.FromBase64String(base64);
            using (var engine = new TesseractEngine("tessdata", "eng", EngineMode.Default))
            {
                using (var img = Pix.LoadFromMemory(bytes))
                {
                    using (var page = engine.Process(img))
                    {
                        return page.GetText().Trim();
                    }
                }
            }
        }

        private async Task<string> GetBase64(string id)
        {
            var client =
                new RestClient("https://www.avito.ru/web/1/items/phone/" + id + "?&vsrc=r");
            var request = new RestRequest("");

            var response = await client.ExecuteAsync(request).ConfigureAwait(false);

            var result = JsonConvert.DeserializeObject<ImagePhone>(response.Content);

            return result.image64.Split(",")[1];
        }
    }
}