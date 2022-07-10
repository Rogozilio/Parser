using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;

namespace ParserDesktop
{
    public class ParserYla : IParser
    {
        private HtmlNode _node;
        private string _ref;
        private List<string> _listItemUrl;

        public ParserYla(string url)
        {
            _ref = url;
            _listItemUrl = new List<string>();
            _node = new HtmlWeb().Load(url).DocumentNode;

            var htmlItemNodes = _node.SelectNodes("//div[@data-test-component='ProductOrAdCard']");

            //Console.WriteLine(htmlItemNodes.Count);
            
            _listItemUrl.Add("https://youla.ru/tomsk/nedvijimost/prodaja-kvartiri/kvartira-2-komnaty-687-m2-62566a3583a7b824c221d6fc");
            // foreach (var item in htmlItemNodes)
            // {
            //     var textTime = item.SelectSingleNode(".//div[@data-marker='item-date']");
            //     Console.WriteLine(textTime.InnerText);
            //     
            //     var nextUrl = item.SelectSingleNode("//*[@itemprop='url']").Attributes["href"]
            //         .Value;
            //     _listItemUrl.Add("https://www.avito.ru" + nextUrl);
            //     break;
            // }
        }

        public void Launch(ref DataForRequest data)
        {
            foreach (var itemUrl in _listItemUrl)
            {
                _node = new HtmlWeb(){ AutoDetectEncoding = false, OverrideEncoding = Encoding.UTF8}.Load(itemUrl).DocumentNode;
                data.Name = GetName();
                data.Phone = GetPhone();
                data.Price = GetPrice();
                data.Address = GetAddress();
                data.Floor = GetFloor();
                data.NumbersOfRoom = GetNumberOfRoom();
                data.TotalArea = GetTotalArea();
                data.KitchenArea = GetKitchenArea();
                data.Renovation = GetRenovation();
                data.Ref = itemUrl;
                DebugData.Show(data);
            }
        }
        public string GetName()
        {
            return "";
        }

        public string GetPhone()
        {
            return "";
        }

        public string GetPrice()
        {
            return ""; 
        }

        public string GetAddress()
        {
            return "";
        }

        public string GetFloor()
        {
            return "";
        }

        public string GetNumberOfRoom()
        {
            return "";
        }

        public string GetTotalArea()
        {
            return "";
        }

        public string GetKitchenArea()
        {
            return "";
        }

        public string GetRenovation()
        {
            return "";
        }
    }
}