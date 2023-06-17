using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOServerNSE
{
        public class HistoryJson
        {
            public Chart chart { get; set; }
        }

        public class Chart
        {
            public List<Result> result { get; set; }
            public object error { get; set; }
        }

        public class Result
        {
            public Meta meta { get; set; }
            public List<int> timestamp { get; set; }
            public Indicators indicators { get; set; }
        }

        public class Meta
        {
        public string symbol { get; set; }
        }

        public class Indicators
        {
            public List<Quote1> quote { get; set; }
            public List<Adjclose> adjclose { get; set; }
        }

        public class Quote1
        {
            public List<double> open { get; set; }
            public List<double> volume { get; set; }
            public List<double> high { get; set; }
            public List<double> low { get; set; }
            public List<double> close { get; set; }
        }

        public class Adjclose
        {
            public List<double> adjclose { get; set; }
        }

        public class StockList
        {
            public string Name { get; set; }
            public string Symbol { get; set; }
            public string LotSize { get; set; }
        }

        public class Root
        {
            public string Exchange { get; set; }
            public string Version { get; set; }
            public List<StockList> StockLists { get; set; }
        }
}
