using CsvHelper.Configuration.Attributes;
using System;

namespace InvestingQuotes
{
    public class OHLC
    {
        [Format("yyyy-MM-dd")]
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
    }
}
