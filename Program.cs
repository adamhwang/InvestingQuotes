using CsvHelper;
using Flurl.Http;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InvestingQuotes
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var symbol = args[0];
            var result = await $"https://api.investing.com/api/financialdata/{symbol}/historical/chart/?period=MAX&interval=P1D&pointscount=120".GetJsonAsync<ApiResponse>();
            var ohlcs = result.data.Select(row => new OHLC()
            {
                Date = DateTimeOffset.FromUnixTimeMilliseconds((long)row[0]).DateTime,
                Open = (decimal)row[1],
                High = (decimal)row[2],
                Low = (decimal)row[3],
                Close = (decimal)row[4],
            });

            using (var writer = new StreamWriter($"{symbol}.{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(ohlcs.OrderBy(o => o.Date));
            }
        }
    }
}
