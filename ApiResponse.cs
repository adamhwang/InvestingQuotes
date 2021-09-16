using System.Collections.Generic;

namespace InvestingQuotes
{
    class ApiResponse
    {
        public List<int> t { get; set; }
        public List<decimal> c { get; set; }
        public List<decimal> o { get; set; }
        public List<decimal> h { get; set; }
        public List<decimal> l { get; set; }
    }
}
