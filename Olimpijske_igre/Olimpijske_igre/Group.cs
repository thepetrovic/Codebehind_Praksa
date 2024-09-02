using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Olimpijske_igre
{
    internal class Group
    {
        public string Name {  get; set; }
        public List<Team> Teams { get; set; }
        public List<Result> ResultsFirstRound { get; set; }
        public List<Result> ResultSecondRound { get; set; }

        public List<Result> ResultThirdRound{ get; set; }

        public Group(string name) 
        {
            this.Name = name;
            this.Teams = new List<Team>();
            this.ResultsFirstRound = new List<Result>();
            this.ResultSecondRound = new List<Result>();
            this.ResultThirdRound = new List<Result>();
        }

        public void WriteFirstResults()
        {
            Console.WriteLine("\n\tGroup: " + Name + "\n");
            foreach(var result in ResultsFirstRound)
            {
                Console.WriteLine("\t\t" + result.ToString()); 
            }
        }
        public void WriteSecondResults()
        {
            Console.WriteLine("\n\tGroup: " + Name + "\n");
            foreach (var result in ResultSecondRound)
            {
                Console.WriteLine("\t\t"+result.ToString());
            }
        }
        public void WriteThirdResults()
        {
            Console.WriteLine("\n\tGroup: " + Name + "\n");
            foreach (var result in ResultThirdRound)
            {
                Console.WriteLine("\t\t" + result.ToString());
            }
        }
        public void WriteTeams()
        {
            Console.WriteLine("\nGrupa "+Name+"(Ime - pobeda/poraza/postignuti kosevi/primljeni kosevi/kos razlika/ broj bodova)\n");
            foreach (var team in Teams)
            {
                team.Write();
            }
            Console.WriteLine("------------------------------------------");
        }


    }
}
