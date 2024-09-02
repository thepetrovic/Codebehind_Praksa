using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olimpijske_igre
{
    internal class TeamJSON
    {
        public string Team { get; set; }
        public string ISOCode { get; set; }
        public int FIBARanking { get; set; }
        public TeamJSON(string Team, string iSOCode, int fIBARanking)
        {
            this.Team = Team;
            this.ISOCode = iSOCode;
            this.FIBARanking = fIBARanking;
        }
        public override string ToString()
        {
            return "Team: "+ Team + "("+ISOCode+") - FIBARanking: "+FIBARanking;
        }
    }
}
