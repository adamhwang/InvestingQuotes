using CsvHelper;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace InvestingQuotes
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var symbol = args[0];

            var guid = Guid.NewGuid().ToString().Replace("-", "");
            var ts = DateTimeOffset.Now.ToUnixTimeSeconds();
            var searchResults = await $"https://tvc4.investing.com/{guid}/{ts}/1/1/8/search?limit=30&query={symbol}&type=&exchange=".GetJsonAsync<SearchResult[]>();

            var instrument_id = searchResults.FirstOrDefault().ticker ?? symbol;

            var toDate = (DateTimeOffset)DateTime.Today;

            var ohlcs = new Dictionary<DateTime, OHLC>();
            IFlurlResponse resp;

            do
            {
                // API returns _first_ 5000 records in specified date range
                // Iterate every 10 years back to ensure we maintain full coverage
                resp = await $"https://tvc4.investing.com/{guid}/{ts}/1/1/8/history?symbol={instrument_id}&resolution=D&from={toDate.AddYears(-10).ToUnixTimeSeconds()}&to={toDate.ToUnixTimeSeconds()}".GetAsync();
                var result = await resp.GetJsonAsync<ApiResponse>();

                var dates = result.t.Select(t => DateTimeOffset.FromUnixTimeSeconds(t).DateTime).ToList();
                var adds = Zip(
                    dates,
                    result.o,
                    result.h,
                    result.l,
                    result.c
                ).Where(add => !ohlcs.ContainsKey(add.Date.Date)).ToList();

                if (!adds.Any()) break;

                foreach (var add in adds)
                {
                    ohlcs.TryAdd(add.Date.Date, add);
                }

                toDate = dates.Min().AddDays(1);
            }
            while (resp.StatusCode == (int)HttpStatusCode.OK);

            using (var writer = new StreamWriter($"{symbol}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(ohlcs.Values.OrderBy(o => o.Date));
            }
        }

        static IEnumerable<OHLC> Zip(IList<DateTime> dates, IList<decimal> opens, IList<decimal> highs, IList<decimal> lows, IList<decimal> closes)
        {
            if (new[] { dates.Count, opens.Count, highs.Count, lows.Count, closes.Count }.Distinct().Count() > 1)
            {
                throw new Exception("Missing data");
            }

            using (var date = dates.GetEnumerator())
            using (var open = opens.GetEnumerator())
            using (var high = highs.GetEnumerator())
            using (var low = lows.GetEnumerator())
            using (var close = closes.GetEnumerator())
            {
                while (date.MoveNext() && open.MoveNext() && high.MoveNext() && low.MoveNext() && close.MoveNext())
                    yield return new OHLC()
                    {
                        Date = date.Current,
                        Open = open.Current,
                        High = high.Current,
                        Low = low.Current,
                        Close = close.Current,
                    };
            }
        }
    }
}
