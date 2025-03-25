using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadWallet.BLL.Models
{
    public class Result
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class Result<ResultT> : Result
    {
        public ResultT? Res { get; set; }
    }
}
