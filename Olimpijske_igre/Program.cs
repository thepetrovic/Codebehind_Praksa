using System;

namespace Olimpijske_igre
{
    public class Program()
    {
        static void Main(string[] args)
        {
            Simulacija simulacija= new Simulacija();
            simulacija.CreateGroups();

            simulacija.PlayFirstRound();
            simulacija.PlaySecondRound();
            simulacija.PlayThirdRound();
            simulacija.RankGroup();
            simulacija.WriteGroup();

            simulacija.RankTeamsOneToEight();
            simulacija.AddPots();
            simulacija.WritePots();
            simulacija.EliminationPhase();
            simulacija.PlayEliminationPhase();
            simulacija.PlaySemiFinalsMatch();
            simulacija.FinalMatchs();
            

        }
    }

}
