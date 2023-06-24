/*
 * OptionsOracle Interface Class Library
 * Copyright 2006-2012 SamoaSky
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Xml;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

using OOServerLib.Web;
using OOServerLib.Interface;
using OOServerLib.Global;
using OOServerLib.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static OOServerNSE.HAPDownload;
using System.Web.Script.Serialization;
using System.Linq;
using Microsoft.CSharp;
using System.Text.RegularExpressions;

namespace OOServerNSE
{
    public class Main : WebSite, IServer
    {
        // yahoo exchange suffix
        private const string suffix = ".NS";

        // host
        private IServerHost host = null;
        private WorldInterestRate wir = null;

        // connection status
        private bool connect = false;

        //symbol storage
        List<string> symbolSet = null;
        // feature and server array list
        private ArrayList feature_list = new ArrayList();
        private ArrayList server_list = new ArrayList();

        // culture
        CultureInfo ci = new CultureInfo("en-US", false);

        private WebForm wbf;

        public Main()
        {
            wir = new WorldInterestRate(cap);

            // update feature list
            feature_list.Add(FeaturesT.SUPPORTS_DELAYED_OPTIONS_CHAIN);
            feature_list.Add(FeaturesT.SUPPORTS_DELAYED_STOCK_QUOTE);

            // update server list
            server_list.Add(Name);

            wbf = new WebForm();
            wbf.Show();
            wbf.Hide();
        }

        public void Initialize(string config)
        {
        }

        public void Dispose()
        {
        }

        // get server feature list
        public ArrayList FeatureList { get { return feature_list; } }

        // get list of servers 
        public ArrayList ServerList { get { return server_list; } }

        // get server operation mode list
        public ArrayList ModeList { get { return null; } }

        // get display accuracy
        public int DisplayAccuracy { get { return 2; } }

        // get server assembly data
        public string Author { get { return "Shlomo Shachar"; } }
        public string Description { get { return "Delayed Quote for NSE Exchange"; } }
        public string Name { get { return "PlugIn Server India (NSE)"; } }
        public string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }

        // plugin authentication
        public string Authentication(string oo_version, string phrase)
        {
            try
            {
                Crypto crypto = new Crypto(@"<RSAKeyValue><Modulus>otysuKHd8wjQn9Oe9m3zAJ1oXtgs9ukfvBOeEjgM/xIMpAk3pFbyT6lGBjGjBvdMTP4kyMRgBYT1SXUXKU85VulcJjvTVH6kCfq04fktoZrKswahz7XCs5tmt7E1yxnavfZddSdhwOWyjgYyCVjXMpOKIZc04XeSJO6COYptQV8=</Modulus><Exponent>AQAB</Exponent><P>0TRDDBI6gZvxDZokegiocMKejl5RINKSEGc7kHARB3G0MwZ1ZvrOaHMsDeS+feHZlX1MGIJUcP0oM0UdmWXuIw==</P><Q>x0q0fPbhLbM06hNiSCIWDxwC5yNprrLEuyJlqTKQFPTd1xZJ6wLf0c/Zr93KeTaepR7nMBdSsABm16ivo+StlQ==</Q><DP>Rpdd8FrORyG5ix9yI4N8YuAo5F1K/spO4x4SaUCHXn2tknIhd2g18eS6/s0qwgtNgjXPUY3YtG+X+wTdYf+VBQ==</DP><DQ>PxMPyLVCU3pydtsnsfjHzoRpDsqQejAuP6QFVOWh4GAXjimJv42rVPZZyWWC3ZZB47TCKuBW1UlrQzoqTM7leQ==</DQ><InverseQ>Pu9T/OTeCLirNvs/pc4CS3fGfPlNA0K9SpaNyWQMi8FIW9q8ggCCoyVxc3Ij3Ote6cl1xTXa7LRyn3kbtJOiIw==</InverseQ><D>DB1UL8vCodB3DFyGh5g4KkSLPfrgpWFD/g6LhJlsxhCGpjEVVYEuNyTFU7KfiOYeY9/HxrNs3Rw9zsAKAAWnoyQHv/CGwGET1H4xLuTRrykShGACPeu+hsfjj0dHyCjVWmsRiTUdY5IjEsUoniknMd9pm393ZoiINvod0UyPljk=</D></RSAKeyValue>");
                return crypto.Decrypt(phrase);
            }
            catch { return ""; }
        }

        // username and password
        public string Username { set { } } // not-required
        public string Password { set { } } // not-required

        // get configuration string
        public string Configuration { get { return null; } }

        // set/get server
        public string Server { get { return Name; } set { } } // not-supported

        // set/get operation mode
        public string Mode { get { return null; } set { } } // not-supported

        // set/get callback host
        public IServerHost Host { get { return host; } set { host = value; } }

        // connect/disconnect to server
        public bool Connect { get { return connect; } set { connect = value; } }

        // connection settings
        //public int ConnectionsRetries { get; set; } // implemented by parent class WebSite
        //public string ProxyAddress { get; set; }    // implemented by parent class WebSite
        //public bool UseProxy { get; set; }          // implemented by parent class WebSite

        // debug log control
        public bool LogEnable { get { return false; } set { } }
        public string DebugLog { get { return null; } }

        // configuration form
        public void ShowConfigForm(object form) { }

        // default symbol
        public string DefaultSymbol { get { return ""; } }

        public string CorrectSymbol(string ticker)
        {
            ticker = ticker.ToUpper().Trim().Replace(suffix, "");

            switch (ticker)
            {
                case "NIFTY":
                case "NIFTY 50":
                    return "^NIFTY";
                case "BANKNIFTY":
                case "NIFTY BANK":
                    return "^BANKNIFTY";
                case "CNXIT":
                    return "^CNXIT";
                case "MINIFTY":
                case "JUNIOR":
                case "^JUNIOR":
                    return "^MINIFTY";
                case "NFTYMCAP50":
                    return "^NFTYMCAP50";
                default:
                    break;
            }

            return ticker;
        }

        public string IndexName(string ticker)
        {
            ticker = ticker.ToUpper().Trim().Replace(suffix, "");

            switch (ticker)
            {
                case "^NIFTY":
                    return "NIFTY 50";
                case "^BANKNIFTY":
                    return "NIFTY BANK";
                case "^CNXIT":
                    return "CNX IT";
                case "^MINIFTY":
                    return "Nifty Next 50";
                case "^NFTYMCAP50":
                    return "Nifty MidCap 50";
                default:
                    break;
            }

            return ticker;
        }

        public string YahooSymbol(string ticker)
        {
            ticker = ticker.ToUpper().Trim().Replace(suffix, "");

            switch (ticker)
            {
                case "^NIFTY":
                case "^MINIFTY":
                    return "^NSEI";
                case "^BANKNIFTY":
                    return "^NSEBANK";
                case "^CNXIT":
                    return "^NSMIDCP";                    
                default:
                    return ticker + suffix;
            }
        }

        public string GenericHAPDownload(string Url, string Referrer = "")
        {
            // first, get the URL
            HAPDownload dl = new HAPDownload();
            dl.downloadMethod = DownloadMethod.Get;
            dl.SourceURL = Url;
            if (Referrer != "")
            {
                dl.AddReferrer = true;
                dl.Referrer = Referrer;
            }

            dl.Execute();

            StreamReader sr = new StreamReader(dl.memStream);
            string sBaseHTML = sr.ReadToEnd();
            sr.Close();

            return sBaseHTML;
        }

        public Quote GetQuote(string ticker)
        {
            ticker = CorrectSymbol(ticker);

            Quote q = null;
            if (ticker.StartsWith("^"))
            {
                try
                {
                    string url = string.Format("https://www.nseindia.com/api/allIndices", ticker.TrimStart(new char[]
                    {
                '^'
                    }));
                    string url2 = string.Format("https://www.nseindia.com/api/allIndices", ticker.TrimStart(new char[]
                    {
                '^'
                    }));
                    string text = this.GenericHAPDownload(url);
                    if (!text.Contains("indexSymbol"))
                    {
                        Thread.Sleep(1000);
                        text = this.GenericHAPDownload(url2);
                    }

                    string text2 = text;

                    if (text2 == null || string.IsNullOrEmpty(text2)) return null;

                    text2 = text2.Replace("<BODY><PRE>{\"data\":[", "");

                    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                    javaScriptSerializer.MaxJsonLength = int.MaxValue;

                    object arg = javaScriptSerializer.Deserialize<object>(text2);
                    var data = (JObject)JsonConvert.DeserializeObject(text2);

                    string tickername = IndexName(ticker);

                    JArray dataArray = (JArray)data["data"];
                    JObject INDICES = (JObject)dataArray.FirstOrDefault(x => (string)x["index"] == tickername);
                    Quote quote = new Quote();


                    if (INDICES != null)
                    {
                        quote = new Quote();
                        foreach (var property in INDICES.Properties())
                        {

                            quote.name = tickername;
                            quote.price.last = Convert.ToDouble((property.Name == "last") ? property.Value : quote.price.last);
                            quote.price.change = Convert.ToDouble((property.Name == "last") ? property.Value : quote.price.change);
                            quote.price.open = Convert.ToDouble((property.Name == "open") ? property.Value : quote.price.open);
                            quote.price.high = Convert.ToDouble((property.Name == "high") ? property.Value : quote.price.high);
                            quote.price.low = Convert.ToDouble((property.Name == "low") ? property.Value : quote.price.low);
                            quote.stock = ticker.TrimStart(new char[] { '^' });
                            quote.update_timestamp = DateTime.Now;

                            //quote.price.change = quote.price.last - Convert.ToDouble((property.Name == "last") ? property.Value : quote.price.last);

                            quote.price.bid = double.NaN;
                            quote.price.ask = double.NaN;

                            quote.volume.total = double.NaN;
                            quote.general.dividend_rate = 0;
                        }
                        quote.lotSize = Convert.ToDouble(GetJsonValues(quote.stock).LotSize);
                        quote.price.change = quote.price.last - quote.price.open;

                    }
                    return quote;
                }
                catch { }
            }

            else
            {
                // stock
                string url = string.Format(@"http://www.nseindia.com/api/quote-equity?symbol={0}", ticker);
                string url2 = string.Format(@"http://www.nseindia.com/api/quote-equity?symbol={0}", ticker);


                string text = this.GenericHAPDownload(url);
                if (!text.Contains("companyName"))
                {
                    Thread.Sleep(1000);
                    text = this.GenericHAPDownload(url2);
                }


                if (text == null || string.IsNullOrEmpty(text)) return null;

                text = text.Replace("<BODY><PRE>{\"data\":[", "");

                try
                {
                    dynamic equityResult = JsonConvert.DeserializeObject(text);
                    Quote quote = new Quote();

                    quote.name = equityResult.info.companyName;
                    quote.stock = ticker;

                    quote.price.last = equityResult.priceInfo.lastPrice;
                    quote.price.change = Convert.ToDouble(equityResult.priceInfo.change);
                    quote.price.open = Convert.ToDouble(equityResult.priceInfo.open);

                    quote.price.high = Convert.ToDouble(equityResult.priceInfo.intraDayHighLow.max);
                    quote.price.low = Convert.ToDouble(equityResult.priceInfo.intraDayHighLow.min);

                    quote.update_timestamp = DateTime.Now;
                    quote.price.bid = double.NaN;
                    quote.price.ask = double.NaN;

                    quote.volume.total = double.NaN;
                    quote.general.dividend_rate = 0;
                    quote.lotSize = Convert.ToDouble(GetJsonValues(quote.stock).LotSize);

                    return quote;

                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            }
            return q;
        }

        // get stock latest options chain
        public ArrayList GetOptionsChain(string ticker)
        {
            //BackgroundWorker bw = null;
            //if (host != null) bw = host.BackgroundWorker;
            //int last = -1;

            try
            {

                // correct symbol
                ticker = CorrectSymbol(ticker);

                string url;

                url = ticker.StartsWith("^") ? string.Format(@"https://www.nseindia.com/api/option-chain-indices?symbol={0}", ticker.TrimStart(new char[] { '^' })) :
                    string.Format(@"https://www.nseindia.com/api/option-chain-equities?symbol={0}", ticker.TrimStart(new char[] { '^' }));
                string text = this.GenericHAPDownload(url);


                JObject jsonResponse = JsonConvert.DeserializeObject<JObject>(text);

                JArray optionDataArray = (JArray)jsonResponse["records"]["data"];


                ArrayList options_list = new ArrayList();
                options_list.Clear();
                options_list.Capacity = 1024;

                foreach (JObject optionData in optionDataArray)
                {
                    if (optionData["CE"] != null)
                    {
                        Option callOption = ParseOptionData((JObject)optionData["CE"], Option.OptionT.Call);
                        options_list.Add(callOption);
                    }

                    if (optionData["PE"] != null)
                    {
                        Option putOption = ParseOptionData((JObject)optionData["PE"], Option.OptionT.Put);
                        options_list.Add(putOption);
                    }

                }

                if (options_list.Count > 0) return options_list;
            }
            catch {; }
            
            return null;
        }

        public static Option ParseOptionData(JObject optionData, Option.OptionT optionType)
        {
            Option option = new Option
            {
                type = optionType == Option.OptionT.Call ? "Call" : "Put",
                stock = (string)optionData["underlying"],
                symbol = optionData["identifier"].ToString(),
                strike = optionData["strikePrice"].Value<double>(),
                expiration = DateTime.Parse(optionData["expiryDate"].ToString()),
                open_int = optionData["openInterest"].Value<int>(),
                update_timestamp = DateTime.Now, // or use another field if available in the API
                ChangeOI = optionData["changeinOpenInterest"].Value<double>(),
                stocks_per_contract = Option.DEFAULT_STOCKS_PER_CONTRACT
            };

            option.price.last = optionData["lastPrice"].Value<double>();
            option.price.change = optionData["change"].Value<double>();
            option.price.bid = optionData["bidprice"].Value<double>();
            option.price.ask = optionData["askPrice"].Value<double>();

            option.volume.total = optionData["totalTradedVolume"].Value<double>();

            return option;
        }

        // get stock/option historical prices 
        public ArrayList GetHistoricalData(string ticker, DateTime start, DateTime end)
        {
            // correct symbol
            ticker = CorrectSymbol(ticker);

            ArrayList list = new ArrayList();

            string json = GenericHAPDownload("https://query1.finance.yahoo.com/v7/finance/chart/" + System.Web.HttpUtility.UrlEncode(this.YahooSymbol(ticker)) + "?range=1y&interval=1d&indicators=quote&includeTimestamps=true&includePrePost=false&corsDomain=finance.yahoo.com");

            HistoryJson historyJson = JsonConvert.DeserializeObject<HistoryJson>(json);

            for (int i = 0; i < historyJson.chart.result[0].timestamp.Count; i++)
            {
                try
                {
                    History history = new History();
                    history.stock = historyJson.chart.result[0].meta.symbol;
                    history.price.open = historyJson.chart.result[0].indicators.quote[0].open[i];
                    history.price.low = historyJson.chart.result[0].indicators.quote[0].low[i];
                    history.price.high = historyJson.chart.result[0].indicators.quote[0].high[i];
                    history.price.close = historyJson.chart.result[0].indicators.quote[0].close[i];
                    history.price.close_adj = historyJson.chart.result[0].indicators.adjclose[0].adjclose[i];
                    history.volume.total = historyJson.chart.result[0].indicators.quote[0].volume[i];
                    history.date = DateTimeOffset.FromUnixTimeSeconds(historyJson.chart.result[0].timestamp[i]).DateTime;

                    list.Add(history);
                }
                catch{; }
                
            }

            // update open values            
            for (int i = 0; i < list.Count - 1; i++)
                ((History)list[i]).price.open = ((History)list[i + 1]).price.close;
            if (list.Count > 0)
                ((History)list[list.Count - 1]).price.open = ((History)list[list.Count - 1]).price.close;

            return list;
        }

        // get stock name lookup results
        public ArrayList GetStockSymbolLookup(string name)
        {
            string lookup_url = @"http://finance.yahoo.com/lookup?s=" + name.Replace(suffix, "") + @"&t=S&m=ALL";

            XmlDocument xml = cap.DownloadXmlWebPage(lookup_url);
            if (xml == null) return null;

            ArrayList symbol_list = new ArrayList();
            symbol_list.Capacity = 256;

            for (int i = 0; i < symbol_list.Capacity; i++)
            {
                string entry = "";

                XmlNode nd, root_node = prs.GetXmlNodeByPath(xml.FirstChild, @"body\div\br\br\table\tr(3)\td\table(2)\tr(3)\td\table\tr\td\table\tr(" + (i + 2).ToString() + @")");
                if (root_node == null) break;

                // stock name
                nd = prs.GetXmlNodeByPath(root_node, @"td(2)");
                if (nd == null) break;
                entry = nd.InnerText.Replace('(', '[').Replace(')', ']');

                // stock ticker
                nd = prs.GetXmlNodeByPath(root_node, @"td(1)\a");
                if (nd == null) break;

                int x = nd.InnerText.IndexOf('.');
                if (x >= 0) entry += nd.InnerText.Substring(0, x);
                else entry += " (" + nd.InnerText + ")";

                // add name + ticker entry
                if (entry.Contains(suffix)) symbol_list.Add(entry.Replace(suffix, ""));
            }

            symbol_list.TrimToSize();
            return symbol_list;
        }

        // get default annual interest rate for specified duration [in years]
        public double GetAnnualInterestRate(double duration)
        {
            return wir.GetAnnualInterestRate("INR");
        }

        // get default historical volatility for specified duration [in years]
        public double GetHistoricalVolatility(string ticker, double duration)
        {
            // get historical data
            ArrayList list = GetHistoricalData(ticker, DateTime.Now.AddDays(-duration * 365), DateTime.Now);

            // calculate historical value
            return 100.0 * HistoryVolatility.HighLowParkinson(list);
        }

        // get and set generic parameters
        public string GetParameter(string name)
        {
            return null;
        }

        public void SetParameter(string name, string value)
        {
        }

        // get and set generic parameters list
        public ArrayList GetParameterList(string name)
        {
            return null;
        }

        public void SetParameterList(string name, ArrayList value)
        {
        }
        List<string> stocknames = null;
        Root stockJsonObject = null;
        public List<string> GetAllStockList()
        {
           
            try
            {
                string fileDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\OptionsOracle\plugin_nse_symbols.json";
                string jsonString = File.ReadAllText(fileDir);
                stockJsonObject = JsonConvert.DeserializeObject<Root>(jsonString);
                stocknames = stockJsonObject.StockLists.Select(stock => stock.Symbol).ToList();
            }
            catch {; }
            return stocknames;
        }

        public StockList GetJsonValues(string stockName)
        {
            return stockJsonObject.StockLists.Where(x => (x.Symbol == stockName || x.Name == stockName)).First();
        }
    }
}
