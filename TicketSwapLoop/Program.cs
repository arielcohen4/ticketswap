using GraphQL.Client;
using GraphQL.Common.Request;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushbulletSharp;
using PushbulletSharp.Models.Requests;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TicketSwapLoop
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = ConfigurationSettings.AppSettings.Get("event");
            int aaa = 0;


            while (true)
            {
                try
                {
                    Console.WriteLine(++aaa);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                    request.AutomaticDecompression = DecompressionMethods.GZip;
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";
                    //IWebProxy proxy = new WebProxy("45.83.184.12", 21287);
                    //string proxyUsername = @"morroma11102";
                    //string proxyPassword = @"r7rvptogqp";
                    //proxy.Credentials = new NetworkCredential(proxyUsername, proxyPassword);
                    //request.Proxy = proxy;
                    request.Timeout = 5000;

                    JObject jsonObj = null;

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string htmlCode = reader.ReadToEnd();
                        jsonObj = GetWindowSharedJson(htmlCode);
                    }

                    var dic = jsonObj["props"]["apolloState"].ToObject<Dictionary<string, JObject>>();

                    foreach (var item in dic)
                    {
                        if (item.Value.ToString().Contains("numberOfTicketsStillForSale") && item.Value["status"].ToString() == "AVAILABLE")
                        {
                            string link = dic["$" + item.Key + ".uri"]["path"].ToString();
                            Console.WriteLine("https://www.ticketswap.com/" + link);

                            var Client = new PushbulletClient("o.csiq6uFbgIejWZxujhIim6gqhZVshD5q");
                            string body = "https://www.ticketswap.com/" + link;


                            PushNoteRequest reqeust = new PushNoteRequest()
                            {
                                ChannelTag = "fqwfgnqwdlf",
                                Title = "כרטיס חדש",
                                Body = body
                            };

                            Client.PushNote(reqeust);
                        }
                    }
                }
                catch (Exception)
                {
                    
                }
            }
        }

        private static void BuyTicket(string link)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://api.ticketswap.com/graphql/public/batch");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";
            request.Headers.Add("Authorization", "Bearer ODk0MThiN2RhNzA0Mzk3ZmU1ZmY3ZmY0ZWYxYjYwOTQ1YTlhOTVlZDExNjg2NjUzYmIwY2U1ZWY1YzU1NzI2Nw");

            var postData = "[{\"operationName\":\"addTicketsToCart\",\"variables\":{\"input\":{\"listingId\":\"TGlzdGluZzo0MDAyNDUx\",\"listingHash\":\"6a097f2099\",\"amountOfTickets\":1}},\"query\":\"mutation addTicketsToCart($input: AddTicketsToCartInput!) {\n addTicketsToCart(input: $input) {\n cart {\n id\n __typename\n    }\n errors {\n code\n message\n __typename\n    }\n __typename\n  }\n}\n\"}]";
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentLength = data.Length;
            request.ContentType = "application/json";

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        private static JObject GetWindowSharedJson(string htmlCode)
        {
            int startOfData = htmlCode.IndexOf("__NEXT_DATA__");
            string htmlCodeData = htmlCode.Substring(startOfData + 39);
            int end = htmlCodeData.IndexOf("<script");
            string jsonString = htmlCodeData.Substring(0, end - 9).ToString();
            JObject json = JObject.Parse(jsonString);


            return json;
        }

        public static CookieContainer CreateCoockie(List<Tuple<string, string>> cookies, string domain)
        {
            if (cookies.Count == 0)
            {
                return new CookieContainer();
            }

            var cookieContainer = new CookieContainer(cookies.Count);

            foreach (var cookie in cookies)
            {
                Cookie coockie = new Cookie(cookie.Item1, cookie.Item2);
                Uri url = new Uri(domain);
                cookieContainer.Add(url, coockie);
            }

            return cookieContainer;
        }
    }
}
