using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Jint;
using RestSharp;

namespace ParserDesktop
{
    public class ParserRU09 : IParser
    {
        private CookieContainer _cookie;
        private HtmlNode _node;
        private string _url;
        private string _oldItemUrl = String.Empty;
        private List<string> _listItemUrl;

        public ParserRU09(string url)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _url = url;
        }

        private void GetItems()
        {
            _cookie = new CookieContainer();
            _listItemUrl = new List<string>();
            var topUrl = String.Empty;

            _node = new HtmlWeb().Load(_url).DocumentNode;
            var htmlItemNodes = _node.SelectNodes(".//td[@class='last']");
            if (htmlItemNodes == null)
                throw new Exception("Значение htmlItemNodes = null");

            //Console.WriteLine(htmlItemNodes.Count);

            foreach (var item in htmlItemNodes)
            {
                var itemUrl = item.SelectSingleNode(".//*[@class='visited_ads']").Attributes["href"]
                    .Value.Replace("amp;", "");
                if (_oldItemUrl == String.Empty)
                {
                    _oldItemUrl = itemUrl;
                    break;
                }

                if (_oldItemUrl != itemUrl)
                {
                    if (topUrl == String.Empty)
                        topUrl = itemUrl;
                }
                else break;

                var agentstvo = item.SelectSingleNode(".//p[@class='absmiddle']")
                    .SelectNodes(".//span[@class='nobr']");

                var isAgentstvo = !(agentstvo == null || agentstvo.Count == 2);

                if (isAgentstvo)
                    continue;

                _listItemUrl.Add("https://www.tomsk.ru09.ru" + itemUrl);
            }

            _oldItemUrl = (topUrl != String.Empty) ? topUrl : _oldItemUrl;
        }

        public void Launch(ref List<DataForRequest> data, TelegramBot bot)
        {
            GetItems();
            _listItemUrl.Add("https://www.tomsk.ru09.ru/realty?subaction=detail&id=4792942");
            foreach (var itemUrl in _listItemUrl)
            {
                _node = new HtmlWeb().Load(itemUrl).DocumentNode;
                if (!IsNewAd()) break;
                
                GetCookie(itemUrl);
                var newData = new DataForRequest();
                newData.Ref = itemUrl;
                newData.Name = GetName();
                newData.Phone = GetPhone();
                newData.Price = GetPrice();
                newData.Address = GetAddress();
                newData.Floor = GetFloor();
                newData.NumbersOfRoom = GetNumberOfRoom();
                newData.TotalArea = GetTotalArea();
                newData.KitchenArea = GetKitchenArea();
                newData.Renovation = GetRenovation();
                bot.Execute(newData.All);
                data.Add(newData);
            }
        }

        private bool IsNewAd()
        {
            if (_node == null) return false;

            var sumViewsPerDays = 0;
            var hrefStatistics = "https://www.tomsk.ru09.ru" + _node.SelectSingleNode("//*[text()[contains(., 'Статистика показов')]]")
                .Attributes["href"].Value;
            var nodeStatistics = new HtmlWeb().Load(hrefStatistics).DocumentNode;
            var nodesPerMonth = nodeStatistics.SelectNodes(".//a[@class='graph_x_bar']");
            for (int i = 0; i < nodesPerMonth.Count; i++)
            {
                var viewsPerDay = int.Parse(nodesPerMonth[i].Attributes["title"].Value);
                sumViewsPerDays += viewsPerDay;

                if (i == nodesPerMonth.Count - 1)
                    sumViewsPerDays -= viewsPerDay;
            }

            return sumViewsPerDays == 0;
        }

        public string GetName()
        {
            var result = _node.SelectSingleNode("//*[text()[contains(., 'Контактное')]]");
            return (result == null)
                ? "Продавец"
                : result.ParentNode.ParentNode.SelectNodes(".//td")[1].InnerText.Trim();
        }

        public string GetPhone()
        {
            var code = _node.OuterHtml.Split("SEC_CODE = '")[1].Split("'")[0];
            var salt = _node.OuterHtml.Split("decrypt_phone_(")[1].Split(")")[0];
            var backEnd = GetBackEndForPhone(code, salt);

            var frontEnd =
                _node.OuterHtml.Split("window['phone" + salt + "'] = '")[1].Split("'")[0];
            var result = DecodingPhone(frontEnd, backEnd.Result);
            return result;
        }

        public string GetPrice()
        {
            var result = _node.SelectSingleNode("//td[@class='realty_detail_price']").InnerText
                .Trim().Replace("&nbsp;", " ").Split("руб")[0];
            return result;
        }

        public string GetAddress()
        {
            var result = _node.SelectNodes("//td[@class='-padding-left']")[1].InnerText.Trim();
            result += ", ";
            result += _node.SelectNodes("//td[@class='-padding-left']")[0].InnerText.Trim()
                .Replace("&nbsp;", " ").Split("(")[0];
            return result;
        }

        public string GetFloor()
        {
            var result = _node.SelectSingleNode("//*[text()[contains(., 'Этаж/этажность')]]")
                .ParentNode.ParentNode.SelectSingleNode(".//td").InnerText.Trim().Split("/")[0];
            return result;
        }

        public string GetNumberOfRoom()
        {
            var result = _node.SelectSingleNode("//*[text()[contains(., 'Количество комнат')]]")
                .ParentNode.ParentNode.SelectSingleNode(".//td").InnerText.Trim();
            return result;
        }

        public string GetTotalArea()
        {
            var result = _node.SelectSingleNode("//*[text()[contains(., 'Общая площадь')]]")
                .ParentNode.ParentNode.SelectSingleNode(".//td").InnerText.Trim().Split("&nbsp")[0];
            return result;
        }

        public string GetKitchenArea()
        {
            var result = _node.SelectSingleNode("//span[text()[contains(., 'кухня')]]")
                .ParentNode.ParentNode.SelectSingleNode(".//td").InnerText.Trim().Split("&nbsp")[0];
            return result == null ? "5" : result;
        }

        public string GetRenovation()
        {
            return "Косметический";
        }

        private string GetCookie(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            request.CookieContainer = _cookie;

            var response = (HttpWebResponse)request.GetResponse();

            string content;
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                content = streamReader.ReadToEnd();
            }

            return content;
        }

        private async Task<string> GetBackEndForPhone(string okeyData, string saltData)
        {
            var client = new RestClient("https://www.tomsk.ru09.ru/ajax.php?");
            var request = new RestRequest("", Method.Post);
            request.AddHeader("Accept", "*/*");
            var cok = _cookie.GetCookieHeader(new Uri(_url));
            request.AddHeader("Cookie", cok);
            request.AddQueryParameter("do", "crypt");
            request.AddQueryParameter("v3", "");
            request.AddParameter("okey", okeyData, ParameterType.GetOrPost);
            request.AddParameter("salt", saltData, ParameterType.GetOrPost);
            var response = await client.ExecuteAsync(request).ConfigureAwait(false);
            // foreach (var head in response.Headers)
            // {
            //     Console.WriteLine(head.Name + " : " + head.Value);
            // }
            return response.Content;
        }

        private string DecodingPhone(string frontEnd, string backEnd)
        {
            var ebanyScript = @"
var DSX = {
    $1: {
        $15: [99, 124, 119, 123, 242, 107, 111, 197, 48, 1, 103, 43, 254, 215, 171, 118, 202, 130, 201, 125, 250, 89, 71, 240, 173, 212, 162, 175, 156, 164, 114, 192, 183, 253, 147, 38, 54, 63, 247, 204, 52, 165, 229, 241, 113, 216, 49, 21, 4, 199, 35, 195, 24, 150, 5, 154, 7, 18, 128, 226, 235, 39, 178, 117, 9, 131, 44, 26, 27, 110, 90, 160, 82, 59, 214, 179, 41, 227, 47, 132, 83, 209, 0, 237, 32, 252, 177, 91, 106, 203, 190, 57, 74, 76, 88, 207, 208, 239, 170, 251, 67, 77, 51, 133, 69, 249, 2, 127, 80, 60, 159, 168, 81, 163, 64, 143, 146, 157, 56, 245, 188, 182, 218, 33, 16, 255, 243, 210, 205, 12, 19, 236, 95, 151, 68, 23, 196, 167, 126, 61, 100, 93, 25, 115, 96, 129, 79, 220, 34, 42, 144, 136, 70, 238, 184, 20, 222, 94, 11, 219, 224, 50, 58, 10, 73, 6, 36, 92, 194, 211, 172, 98, 145, 149, 228, 121, 231, 200, 55, 109, 141, 213, 78, 169, 108, 86, 244, 234, 101, 122, 174, 8, 186, 120, 37, 46, 28, 166, 180, 198, 232, 221, 116, 31, 75, 189, 139, 138, 112, 62, 181, 102, 72, 3, 246, 14, 97, 53, 87, 185, 134, 193, 29, 158, 225, 248, 152, 17, 105, 217, 142, 148, 155, 30, 135, 233, 206, 85, 40, 223, 140, 161, 137, 13, 191, 230, 66, 104, 65, 153, 45, 15, 176, 84, 187, 22],
        $14: [82, 9, 106, 213, 48, 54, 165, 56, 191, 64, 163, 158, 129, 243, 215, 251, 124, 227, 57, 130, 155, 47, 255, 135, 52, 142, 67, 68, 196, 222, 233, 203, 84, 123, 148, 50, 166, 194, 35, 61, 238, 76, 149, 11, 66, 250, 195, 78, 8, 46, 161, 102, 40, 217, 36, 178, 118, 91, 162, 73, 109, 139, 209, 37, 114, 248, 246, 100, 134, 104, 152, 22, 212, 164, 92, 204, 93, 101, 182, 146, 108, 112, 72, 80, 253, 237, 185, 218, 94, 21, 70, 87, 167, 141, 157, 132, 144, 216, 171, 0, 140, 188, 211, 10, 247, 228, 88, 5, 184, 179, 69, 6, 208, 44, 30, 143, 202, 63, 15, 2, 193, 175, 189, 3, 1, 19, 138, 107, 58, 145, 17, 65, 79, 103, 220, 234, 151, 242, 207, 206, 240, 180, 230, 115, 150, 172, 116, 34, 231, 173, 53, 133, 226, 249, 55, 232, 28, 117, 223, 110, 71, 241, 26, 113, 29, 41, 197, 137, 111, 183, 98, 14, 170, 24, 190, 27, 252, 86, 62, 75, 198, 210, 121, 32, 154, 219, 192, 254, 120, 205, 90, 244, 31, 221, 168, 51, 136, 7, 199, 49, 177, 18, 16, 89, 39, 128, 236, 95, 96, 81, 127, 169, 25, 181, 74, 13, 45, 229, 122, 159, 147, 201, 156, 239, 160, 224, 59, 77, 174, 42, 245, 176, 200, 235, 187, 60, 131, 83, 153, 97, 23, 43, 4, 126, 186, 119, 214, 38, 225, 105, 20, 99, 85, 33, 12, 125],
        $16: [141, 1, 2, 4, 8, 16, 32, 64, 128, 27, 54, 108, 216, 171, 77, 154, 47, 94, 188, 99, 198, 151, 53, 106, 212, 179, 125, 250, 239, 197, 145, 57, 114, 228, 211, 189, 97, 194, 159, 37, 74, 148, 51, 102, 204, 131, 29, 58, 116, 232, 203, 141, 1, 2, 4, 8, 16, 32, 64, 128, 27, 54, 108, 216, 171, 77, 154, 47, 94, 188, 99, 198, 151, 53, 106, 212, 179, 125, 250, 239, 197, 145, 57, 114, 228, 211, 189, 97, 194, 159, 37, 74, 148, 51, 102, 204, 131, 29, 58, 116, 232, 203, 141, 1, 2, 4, 8, 16, 32, 64, 128, 27, 54, 108, 216, 171, 77, 154, 47, 94, 188, 99, 198, 151, 53, 106, 212, 179, 125, 250, 239, 197, 145, 57, 114, 228, 211, 189, 97, 194, 159, 37, 74, 148, 51, 102, 204, 131, 29, 58, 116, 232, 203, 141, 1, 2, 4, 8, 16, 32, 64, 128, 27, 54, 108, 216, 171, 77, 154, 47, 94, 188, 99, 198, 151, 53, 106, 212, 179, 125, 250, 239, 197, 145, 57, 114, 228, 211, 189, 97, 194, 159, 37, 74, 148, 51, 102, 204, 131, 29, 58, 116, 232, 203, 141, 1, 2, 4, 8, 16, 32, 64, 128, 27, 54, 108, 216, 171, 77, 154, 47, 94, 188, 99, 198, 151, 53, 106, 212, 179, 125, 250, 239, 197, 145, 57, 114, 228, 211, 189, 97, 194, 159, 37, 74, 148, 51, 102, 204, 131, 29, 58, 116, 232, 203],
        $17: function(a, b, c) {
            c = a[0];
            for (b = 0; b < 3; b++)
                a[b] = a[b + 1];
            a[3] = c;
            return a
        },
        $18: function(a, b, c) {
            a = this.$17(a);
            for (c = 0; c < 4; ++c)
                a[c] = this.$15[a[c]];
            a[0] = a[0] ^ this.$16[b];
            return a
        },
        $3: function(a, b, c, d, e, f, g, h) {
            c = (16 * ((b ? 10 : null) + (e = 1))) + (d = 0);
            f = [];
            g = [];
            for (h = 0; h < c; h++)
                g[h] = 0;
            for (h = 0; h < b; h++)
                g[h] = a[h];
            d += b;
            while (d < c) {
                for (h = 0; h < 4; h++)
                    f[h] = g[(d - 4) + h];
                if (d % b == 0)
                    f = this.$18(f, e++);
                for (h = 0; h < 4; h++)
                    g[d] = g[d++ - b] ^ f[h]
            }
            return g
        },
        $4: function(a, b, c) {
            for (c = 0; c < 16; c++)
                a[c] ^= b[c];
            return a
        },
        $5: function(a, b, c, d, e) {
            c = [];
            for (d = 0; d < 4; d++)
                for (e = 0; e < 4; e++)
                    c[e * 4 + d] = a[b + d * 4 + e];
            return c
        },
        $8: function(a, b) {
            for (var i = 0; i < 16; i++)
                a[i] = b ? this.$14[a[i]] : this.$15[a[i]];
            return a
        },
        $7: function(a, b) {
            for (var i = 0; i < 4; i++)
                a = this.$13(a, i * 4, i, b);
            return a
        },
        $13: function(a, b, c, d, e, f, g) {
            for (f = 0; f < c; f++) {
                e = a[b + (d ? 3 : 0)];
                if (d)
                    for (g = 3; g > 0; g--)
                        a[b + g] = a[b + g - 1];
                else
                    for (g = 0; g < 3; g++)
                        a[b + g] = a[b + g + 1];
                a[b + (!d ? 3 : 0)] = e
            }
            return a
        },
        $12: function(a, b, c, d, e) {
            d = 0;
            for (c = 0; c < 8; c++) {
                if ((b & 1) == 1)
                    d ^= a;
                if (d > 256)
                    d ^= 256;
                e = (a & 128);
                a <<= 1;
                if (e == 128)
                    a ^= 27;
                if (a > 256)
                    a ^= 256;
                b >>= 1;
                if (b > 256)
                    b ^= 256
            }
            return d
        },
        $10: function(a, b, c, d, e) {
            c = [];
            for (d = 0; d < 4; d++) {
                for (e = 0; e < 4; e++)
                    c[e] = a[(e * 4) + d];
                c = this.$11(c, b);
                for (e = 0; e < 4; e++)
                    a[(e * 4) + d] = c[e]
            }
            return a
        },
        $11: function(a, b, c) {
            c = b ? [14, 9, 13, 11] : [2, 1, 1, 3];
            return [this.$12(a[0], c[0]) ^ this.$12(a[3], c[1]) ^ this.$12(a[2], c[2]) ^ this.$12(a[1], c[3]), this.$12(a[1], c[0]) ^ this.$12(a[0], c[1]) ^ this.$12(a[3], c[2]) ^ this.$12(a[2], c[3]), this.$12(a[2], c[0]) ^ this.$12(a[1], c[1]) ^ this.$12(a[0], c[2]) ^ this.$12(a[3], c[3]), this.$12(a[3], c[0]) ^ this.$12(a[2], c[1]) ^ this.$12(a[1], c[2]) ^ this.$12(a[0], c[3])]
        },
        $9: function(a, b) {
            return this.$4(this.$10(this.$7(this.$8(a, false), false), false), b)
        },
        $6: function(a, b) {
            return this.$10(this.$4(this.$8(this.$7(a, true), true), b), true)
        },
        $2: function(a, b, c) {
            a = this.$4(a, this.$5(b, 16 * c));
            for (var i = c - 1; i > 0; i--)
                a = this.$6(a, this.$5(b, 16 * i));
            return this.$4(this.$8(this.$7(a, true), true), this.$5(b, 0))
        },
        $1: function(a, b, c, d, e, f, g, h, i) {
            d = [];
            e = [];
            f = c ? 10 : null;
            for (g = 0; g < 4; g++)
                for (h = 0; h < 4; h++)
                    e[(g + (h * 4))] = a[(g * 4) + h];
            i = this.$3(b, c);
            e = this.$2(e, i, f);
            for (g = 0; g < 4; g++)
                for (h = 0; h < 4; h++)
                    d[(g * 4) + h] = e[(g + (h * 4))];
            return d
        }
    },
    $2: function(a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) {
        k = !!((d = b.length) && (o = a.length));
        i = [];
        j = [];
        if (a !== null) {
            for (l = 0; l < Math.ceil(o / 16); l++) {
                n = (m = l << 4) + 16;
                if ((l << 4) + 16 > o)
                    n = o;
                f = a.slice(m, n - m > 16 ? m + 16 : n);
                h = this.$1.$1(f, b, d);
                for (p = 0; p < 16; p++)
                    i[p] = ((k) ? c[p] : g[p]) ^ h[p];
                k = false;
                for (p = 0; p < n - m; p++)
                    j.push(i[p]);
                g = f
            }
            this.$4(j)
        }
        return j
    },
    $4: function(a, b, c, d) {
        d = (b = (c = 0) - 1) + 17;
        if (a.length > 16) {
            for (var i = a.length - 1; i >= a.length - 1 - d; i--) {
                if (a[i] <= d) {
                    if (b == -1)
                        b = a[i];
                    if (a[i] != b) {
                        c = 0;
                        break
                    }
                    c++
                } else
                    break;
                if (c == b)
                    break
            }
            if (c > 0)
                a.splice(a.length - c, c)
        }
    }
};

function $2n(b, c) {
    c = [];
    b.replace(/(..)/g, function(a) {
        c.push(parseInt(a, 10 + 6))
    });
    return c
}
function $2s(a) {
    for (var b = 0, $3 = ''; b < a.length; b++)
        $3 += String.fromCharCode(a[b]);
    return $3
}

function getNumber(strBack, strFront){
    var f = strBack.split('l');
    var i = f.length > 1 ? $2s(DSX.$2($2n(strFront), $2n(f[0]), $2n(f[1]))) : '';
    i = i.replace(/(\x05)/g, '');
    return i;
}
";
            var engine = new Engine().Execute(ebanyScript);

            return engine.Invoke("getNumber", backEnd, frontEnd).ToString();
        }
    }
}