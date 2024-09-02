using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olimpijske_igre
{
    internal class Result
    {
        public Team Home {  get; set; }
        public Team Guest {  get; set; }
        public int NumberOfPointsScoredHome { get; set; }
        public int NumberOfPointsScoredGuest { get; set; }

        public Result(Team home, Team guest) 
        {
            Home = home;
            Guest = guest;
            NumberOfPointsScoredHome = 0;
            NumberOfPointsScoredGuest = 0;
        }
        public Result(Team home, Team guest, int numberOfPointsScoredHome, int numberOfPointsScoredGuest)
        {
            Home = home;
            Guest = guest;
            NumberOfPointsScoredHome = numberOfPointsScoredHome;
            NumberOfPointsScoredGuest = numberOfPointsScoredGuest;
        }

        public void WritePair()
        {
            Console.WriteLine("\t"+Home.Name+" - "+Guest.Name);
        }
        public override string ToString()
        {
            return "\t"+Home.Name + " - " + Guest.Name + " (" + NumberOfPointsScoredHome + " : "+ NumberOfPointsScoredGuest + ")";
        }
    }
}
