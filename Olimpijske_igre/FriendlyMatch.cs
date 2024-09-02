using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olimpijske_igre
{
    internal class FriendlyMatch
    {
        public string Date {  get; set; }
        public string Opponent { get; set; }
        public string Result { get; set; }

        public override string ToString()
        {
            return "Datum: " + Date + "  Opponent: " + Opponent + "  Result: " + Result;
        }
    }
}
