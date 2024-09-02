using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Olimpijske_igre
{
    internal class Team
    {
        public string Name {  get; set; }
        public string ISOCode { get; set; }
        public int FIBARanking { get; set; }
        public int NumberOfWins { get; set; }
        public int NumberOfLosses { get; set; }
        public int NumberOfPointsScored { get; set; }
        public int NumberOfPointsConceded { get; set; }
        public int PointDifference { get; set; }
        public int NumberOfPoints {  get; set; }
        public List<Result> PreseasonResults { get; set; }

        public Team(string ime, string iSOCode, int fIBARanking)
        {
            Name = ime;
            ISOCode = iSOCode;
            FIBARanking = fIBARanking;
            NumberOfWins = 0;
            NumberOfLosses = 0;
            NumberOfPointsScored = 0;
            NumberOfPointsConceded = 0;
            PointDifference = 0;
            NumberOfPoints = 0;
            PreseasonResults = new List<Result>();
        }

        public override string ToString()
        {
            return Name + "   " + NumberOfWins +" / " + NumberOfLosses + " / " + NumberOfPointsScored + " / " + NumberOfPointsConceded + " / " + PointDifference;
        }
        public void Write()
        {
            Console.Write("\t"+Name + "   " + NumberOfWins + " / " + NumberOfLosses + " / " + NumberOfPointsScored + " / " + NumberOfPointsConceded + " / " + PointDifference+" / "+ NumberOfPoints+"\n");
            
        }
        public void WriteName()
        {
            Console.WriteLine("\t"+Name);
        }
        public void WriteAllResults()
        {
            Console.WriteLine("=========================================\n");
            Console.WriteLine("\n"+Name+"\n");
            Console.WriteLine("\n\nRezultati:\n\n");
            foreach (Result result in PreseasonResults)
            {
                Console.WriteLine(result.ToString());
            }
            Console.WriteLine("=========================================\n");
        }
    }
}
