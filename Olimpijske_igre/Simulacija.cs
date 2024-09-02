using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Olimpijske_igre
{
    internal class Simulacija
    {
        public UcitajJSON loadJSON = new UcitajJSON();
        public Dictionary<string, List<TeamJSON>> loadGroups;
        public Dictionary<string, List<FriendlyMatch>> allFriendlyMatchs;
        public List<Group> groups;
        public List<TeamJSON> allTeams;
        public List<Team> AllTeams;
        public List<Team> potD;
        public List<Team> potE;
        public List<Team> potF;
        public List<Team> potG;
        public List<Result> eliminationResults;
        public List<Result> semiFinalsMatch;
        public Result finalMatch;
        public Result forThirdPlace;
        public Simulacija()
        {
            loadGroups = loadJSON.LoadJSON<Dictionary<string, List<TeamJSON>>>("groups.json");
            allFriendlyMatchs = loadJSON.LoadJSON<Dictionary<string, List<FriendlyMatch>>>("exibitions.json");
            groups = new List<Group>();
            allTeams = new List<TeamJSON>();
            AllTeams = new List<Team>();
            potD = new List<Team>();
            potE = new List<Team>();
            potF = new List<Team>();
            potG = new List<Team>();
            eliminationResults=new List<Result>();
            semiFinalsMatch=new List<Result>();
            finalMatch = null;
            forThirdPlace = null;
        }

        public Team getTeamByISOCode(string ISOCode)
        {
            Team newTeam;
            foreach (TeamJSON team in allTeams)
            {
                if (team.ISOCode.Equals(ISOCode))
                {
                    newTeam = new Team(team.Team, team.ISOCode, team.FIBARanking);
                    return newTeam;
                }
            }
            return null;
        }
        public int[] GetResultInt(string result)
        {
            int[] newResult = new int[2];
            string[] newResultStr = result.Split('-');
            newResult[0] = int.Parse(newResultStr[0]);
            newResult[1] = int.Parse(newResultStr[1]);
            return newResult;
        }
        public List<Result> AddPreseasonResultForTeam(Team foundTeam )  
        {
            Result result;
            List<Result> results= new List<Result>();
            string resultStr;
            foreach (var team in allFriendlyMatchs)
            { 
                if (team.Key.Equals(foundTeam.ISOCode))
                {
                    foreach (var match in team.Value)
                    {
                        resultStr= match.Result;         
                        result = new Result(foundTeam, getTeamByISOCode(match.Opponent), GetResultInt(resultStr)[0], GetResultInt(resultStr)[1]);
                        results.Add(result);
                    }
                }
            }           
            return results;
        }
        public void AddPreseasonResults()
        {
            foreach(Group group in groups)
            {
                foreach(Team team in group.Teams)
                {
                    team.PreseasonResults = AddPreseasonResultForTeam(team);
                }
            }
        }
        public void CreateGroups()
        {
            Group newGroup;
            Team newTeam;
            foreach (var group in loadGroups)
            {
                newGroup = new Group(group.Key);
                
                foreach (var team in group.Value)
                {
                    newTeam = new Team(team.Team, team.ISOCode, team.FIBARanking);
                    newGroup.Teams.Add(newTeam);
                    allTeams.Add(new TeamJSON(newTeam.Name,newTeam.ISOCode,newTeam.FIBARanking));
                }
                groups.Add(newGroup);
            }
            AddPreseasonResults();
        }

      
        public double CalculatTeamForm(Team team)
        {
            double form = 0;
            double opponentRank;
            double pointDifference;
            foreach(var math in team.PreseasonResults)
            {
                opponentRank=math.Guest.FIBARanking;
                pointDifference=math.NumberOfPointsScoredHome-math.NumberOfPointsScoredGuest;
                form += opponentRank * pointDifference;
            }
            return form;
        }
        public double SigmoidFunction(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }
        public double CalculateWinProbability(Team team1, Team team2)
        {
            double rankDifference = team1.FIBARanking - team2.FIBARanking;
            double formDifference = CalculatTeamForm(team1) - CalculatTeamForm(team2);
            return SigmoidFunction(rankDifference + formDifference);
        }
        public int GeneratePoints(Team team1, Team team2)
        {
            double averagePoints = 80;
            double standardDeviation = 10;

            //double formFactor = (CalculatTeamForm(team1) - CalculatTeamForm(team2)) / 20.0;
            //double rankFactor = (team1.FIBARanking - team2.FIBARanking) / 10.0;

            return (int)(new Random().NextDouble() * standardDeviation + averagePoints);
        }
        public Result GetResult(Team team1, Team team2)
        {
            double winProbability = CalculateWinProbability(team1, team2);
            bool team1Wins = new Random().NextDouble() < winProbability;

            
            int team1Points = GeneratePoints(team1, team2);
            int team2Points = GeneratePoints(team2, team1);
         
            Result result;
            if (team1Wins)
            {
                if (team1Points == team2Points)
                    team1Points += 1;
                result=new Result(team1,team2,team1Points,team2Points);
            }
            else
            {
                if (team1Points == team2Points)
                    team2Points += 1;
                result = new Result(team1, team2, team2Points, team1Points);
            }
            return result;
        }

        public void PlayFirstRound()
        {
            List<Result> results;
            foreach(Group group in groups)
            {
                Result result1 = GetResult(group.Teams[0], group.Teams[2]);
                Result result2 = GetResult(group.Teams[1], group.Teams[3]);
                
                group.ResultsFirstRound.Add(result1);
                group.ResultsFirstRound.Add(result2);
                if(result1.NumberOfPointsScoredHome>result1.NumberOfPointsScoredGuest)
                {
                    group.Teams[0].NumberOfWins += 1;
                    group.Teams[0].NumberOfPointsScored += result1.NumberOfPointsScoredHome;
                    group.Teams[0].NumberOfPointsConceded += result1.NumberOfPointsScoredGuest;
                    group.Teams[0].PointDifference+= result1.NumberOfPointsScoredHome- result1.NumberOfPointsScoredGuest;
                    group.Teams[0].NumberOfPoints += 2;
                    group.Teams[0].PreseasonResults.Add(result1);

                    group.Teams[2].NumberOfLosses += 1;
                    group.Teams[2].NumberOfPointsConceded += result1.NumberOfPointsScoredHome;
                    group.Teams[2].NumberOfPointsScored += result1.NumberOfPointsScoredGuest;
                    group.Teams[2].PointDifference += result1.NumberOfPointsScoredGuest - result1.NumberOfPointsScoredHome;
                    group.Teams[2].NumberOfPoints += 1;
                    group.Teams[2].PreseasonResults.Add(new Result(group.Teams[2], group.Teams[0], result1.NumberOfPointsScoredHome,result1.NumberOfPointsScoredGuest));
                }
                else
                {
                    group.Teams[2].NumberOfWins += 1;
                    group.Teams[2].NumberOfPointsScored += result1.NumberOfPointsScoredGuest;
                    group.Teams[2].NumberOfPointsConceded += result1.NumberOfPointsScoredHome;
                    group.Teams[2].PointDifference += result1.NumberOfPointsScoredGuest - result1.NumberOfPointsScoredHome;
                    group.Teams[2].NumberOfPoints += 2;
                    group.Teams[2].PreseasonResults.Add(new Result(group.Teams[2], group.Teams[0], result1.NumberOfPointsScoredHome, result1.NumberOfPointsScoredGuest));

                    group.Teams[0].NumberOfLosses += 1;
                    group.Teams[0].NumberOfPointsConceded += result1.NumberOfPointsScoredGuest;
                    group.Teams[0].NumberOfPointsScored += result1.NumberOfPointsScoredHome;
                    group.Teams[0].PointDifference += result1.NumberOfPointsScoredHome - result1.NumberOfPointsScoredGuest;
                    group.Teams[0].NumberOfPoints += 1;
                    group.Teams[0].PreseasonResults.Add(result1);
                }

                if (result2.NumberOfPointsScoredHome > result2.NumberOfPointsScoredGuest)
                {
                    group.Teams[1].NumberOfWins += 1;
                    group.Teams[1].NumberOfPointsScored += result2.NumberOfPointsScoredHome;
                    group.Teams[1].NumberOfPointsConceded += result2.NumberOfPointsScoredGuest;
                    group.Teams[1].PointDifference += result2.NumberOfPointsScoredHome - result2.NumberOfPointsScoredGuest;
                    group.Teams[1].NumberOfPoints += 2;
                    group.Teams[1].PreseasonResults.Add(result2);

                    group.Teams[3].NumberOfLosses += 1;
                    group.Teams[3].NumberOfPointsConceded += result2.NumberOfPointsScoredHome;
                    group.Teams[3].NumberOfPointsScored += result2.NumberOfPointsScoredGuest;
                    group.Teams[3].PointDifference += result2.NumberOfPointsScoredGuest - result2.NumberOfPointsScoredHome;
                    group.Teams[3].NumberOfPoints += 1;
                    group.Teams[3].PreseasonResults.Add(new Result(group.Teams[3], group.Teams[1], result2.NumberOfPointsScoredHome, result2.NumberOfPointsScoredGuest));
                }
                else
                {
                    group.Teams[3].NumberOfWins += 1;
                    group.Teams[3].NumberOfPointsScored += result2.NumberOfPointsScoredGuest;
                    group.Teams[3].NumberOfPointsConceded += result2.NumberOfPointsScoredHome;
                    group.Teams[3].PointDifference += result2.NumberOfPointsScoredGuest - result2.NumberOfPointsScoredHome;
                    group.Teams[3].NumberOfPoints += 2;
                    group.Teams[3].PreseasonResults.Add(new Result(group.Teams[3], group.Teams[1], result2.NumberOfPointsScoredHome, result2.NumberOfPointsScoredGuest));

                    group.Teams[1].NumberOfLosses += 1;
                    group.Teams[1].NumberOfPointsConceded += result2.NumberOfPointsScoredGuest;
                    group.Teams[1].NumberOfPointsScored += result2.NumberOfPointsScoredHome;
                    group.Teams[1].PointDifference += result2.NumberOfPointsScoredHome - result2.NumberOfPointsScoredGuest;
                    group.Teams[1].NumberOfPoints += 1;
                    group.Teams[1].PreseasonResults.Add(result2);
                }
            }
        }        
        public void PlaySecondRound()
        {
            List<Result> results;
            foreach (Group group in groups)
            {
                Result result1 = GetResult(group.Teams[0], group.Teams[3]);
                Result result2 = GetResult(group.Teams[1], group.Teams[2]);
                group.ResultSecondRound.Add(result1);
                group.ResultSecondRound.Add(result2);
                if (result1.NumberOfPointsScoredHome > result1.NumberOfPointsScoredGuest)
                {
                    group.Teams[0].NumberOfWins += 1;
                    group.Teams[0].NumberOfPointsScored += result1.NumberOfPointsScoredHome;
                    group.Teams[0].NumberOfPointsConceded += result1.NumberOfPointsScoredGuest;
                    group.Teams[0].PointDifference += result1.NumberOfPointsScoredHome - result1.NumberOfPointsScoredGuest;
                    group.Teams[0].NumberOfPoints += 2;
                    group.Teams[0].PreseasonResults.Add(result1);

                    group.Teams[3].NumberOfLosses += 1;
                    group.Teams[3].NumberOfPointsConceded += result1.NumberOfPointsScoredHome;
                    group.Teams[3].NumberOfPointsScored += result1.NumberOfPointsScoredGuest;
                    group.Teams[3].PointDifference += result1.NumberOfPointsScoredGuest - result1.NumberOfPointsScoredHome;
                    group.Teams[3].NumberOfPoints += 1;
                    group.Teams[3].PreseasonResults.Add(new Result(group.Teams[3], group.Teams[0], result1.NumberOfPointsScoredHome, result1.NumberOfPointsScoredGuest));
                }
                else
                {
                    group.Teams[3].NumberOfWins += 1;
                    group.Teams[3].NumberOfPointsScored += result1.NumberOfPointsScoredGuest;
                    group.Teams[3].NumberOfPointsConceded += result1.NumberOfPointsScoredHome;
                    group.Teams[3].PointDifference +=result1.NumberOfPointsScoredGuest - result1.NumberOfPointsScoredHome;
                    group.Teams[3].NumberOfPoints += 2;
                    group.Teams[3].PreseasonResults.Add(new Result(group.Teams[3], group.Teams[0], result1.NumberOfPointsScoredHome, result1.NumberOfPointsScoredGuest));

                    group.Teams[0].NumberOfLosses += 1;
                    group.Teams[0].NumberOfPointsConceded += result1.NumberOfPointsScoredGuest;
                    group.Teams[0].NumberOfPointsScored += result1.NumberOfPointsScoredHome;
                    group.Teams[0].PointDifference += result1.NumberOfPointsScoredHome - result1.NumberOfPointsScoredGuest;
                    group.Teams[0].NumberOfPoints += 1;
                    group.Teams[0].PreseasonResults.Add(result1);
                }

                if (result2.NumberOfPointsScoredHome > result2.NumberOfPointsScoredGuest)
                {
                    group.Teams[1].NumberOfWins += 1;
                    group.Teams[1].NumberOfPointsScored += result2.NumberOfPointsScoredHome;
                    group.Teams[1].NumberOfPointsConceded += result2.NumberOfPointsScoredGuest;
                    group.Teams[1].PointDifference += result2.NumberOfPointsScoredHome - result2.NumberOfPointsScoredGuest;
                    group.Teams[1].NumberOfPoints += 2;
                    group.Teams[1].PreseasonResults.Add(result2);

                    group.Teams[2].NumberOfLosses += 1;
                    group.Teams[2].NumberOfPointsConceded += result2.NumberOfPointsScoredHome;
                    group.Teams[2].NumberOfPointsScored += result2.NumberOfPointsScoredGuest;
                    group.Teams[2].PointDifference += result2.NumberOfPointsScoredGuest - result2.NumberOfPointsScoredHome;
                    group.Teams[2].NumberOfPoints += 1;
                    group.Teams[2].PreseasonResults.Add(new Result(group.Teams[2], group.Teams[1], result2.NumberOfPointsScoredHome, result2.NumberOfPointsScoredGuest));
                }
                else
                {
                    group.Teams[2].NumberOfWins += 1;
                    group.Teams[2].NumberOfPointsScored += result2.NumberOfPointsScoredGuest;
                    group.Teams[2].NumberOfPointsConceded += result2.NumberOfPointsScoredHome;
                    group.Teams[2].PointDifference += result2.NumberOfPointsScoredGuest - result2.NumberOfPointsScoredHome;
                    group.Teams[2].NumberOfPoints += 2;
                    group.Teams[2].PreseasonResults.Add(new Result(group.Teams[2], group.Teams[1], result2.NumberOfPointsScoredHome, result2.NumberOfPointsScoredGuest));

                    group.Teams[1].NumberOfLosses += 1;
                    group.Teams[1].NumberOfPointsConceded += result2.NumberOfPointsScoredGuest;
                    group.Teams[1].NumberOfPointsScored += result2.NumberOfPointsScoredHome;
                    group.Teams[1].PointDifference += result2.NumberOfPointsScoredHome - result2.NumberOfPointsScoredGuest;
                    group.Teams[1].NumberOfPoints += 1;
                    group.Teams[1].PreseasonResults.Add(result2);
                }
            }
        }
        public void PlayThirdRound()
        {
            List<Result> results;
            foreach (Group group in groups)
            {
                Result result1 = GetResult(group.Teams[0], group.Teams[1]);
                Result result2 = GetResult(group.Teams[2], group.Teams[3]);

                group.ResultThirdRound.Add(result1);
                group.ResultThirdRound.Add(result2);
                if (result1.NumberOfPointsScoredHome > result1.NumberOfPointsScoredGuest)
                {
                    group.Teams[0].NumberOfWins += 1;
                    group.Teams[0].NumberOfPointsScored += result1.NumberOfPointsScoredHome;
                    group.Teams[0].NumberOfPointsConceded += result1.NumberOfPointsScoredGuest;
                    group.Teams[0].PointDifference += result1.NumberOfPointsScoredHome - result1.NumberOfPointsScoredGuest;
                    group.Teams[0].NumberOfPoints += 2;
                    group.Teams[0].PreseasonResults.Add(result1);

                    group.Teams[1].NumberOfLosses += 1;
                    group.Teams[1].NumberOfPointsConceded += result1.NumberOfPointsScoredHome;
                    group.Teams[1].NumberOfPointsScored += result1.NumberOfPointsScoredGuest;
                    group.Teams[1].PointDifference += result1.NumberOfPointsScoredGuest - result1.NumberOfPointsScoredHome;
                    group.Teams[1].NumberOfPoints += 1;
                    group.Teams[1].PreseasonResults.Add(new Result(group.Teams[1], group.Teams[0], result1.NumberOfPointsScoredHome, result1.NumberOfPointsScoredGuest));
                }
                else
                {
                    group.Teams[1].NumberOfWins += 1;
                    group.Teams[1].NumberOfPointsScored += result1.NumberOfPointsScoredGuest;
                    group.Teams[1].NumberOfPointsConceded += result1.NumberOfPointsScoredHome;
                    group.Teams[1].PointDifference +=  result1.NumberOfPointsScoredGuest - result1.NumberOfPointsScoredHome;
                    group.Teams[1].NumberOfPoints += 2;
                    group.Teams[1].PreseasonResults.Add(new Result(group.Teams[1], group.Teams[0], result1.NumberOfPointsScoredHome, result1.NumberOfPointsScoredGuest));

                    group.Teams[0].NumberOfLosses += 1;
                    group.Teams[0].NumberOfPointsConceded += result1.NumberOfPointsScoredGuest;
                    group.Teams[0].NumberOfPointsScored += result1.NumberOfPointsScoredHome;
                    group.Teams[0].PointDifference += result1.NumberOfPointsScoredHome - result1.NumberOfPointsScoredGuest;
                    group.Teams[0].NumberOfPoints += 1;
                    group.Teams[0].PreseasonResults.Add(result1);
                }

                if (result2.NumberOfPointsScoredHome > result2.NumberOfPointsScoredGuest)
                {
                    group.Teams[2].NumberOfWins += 1;
                    group.Teams[2].NumberOfPointsScored += result2.NumberOfPointsScoredHome;
                    group.Teams[2].NumberOfPointsConceded += result2.NumberOfPointsScoredGuest;
                    group.Teams[2].PointDifference += result2.NumberOfPointsScoredHome - result2.NumberOfPointsScoredGuest;
                    group.Teams[2].NumberOfPoints += 2;
                    group.Teams[2].PreseasonResults.Add(result2);

                    group.Teams[3].NumberOfLosses += 1;
                    group.Teams[3].NumberOfPointsConceded += result2.NumberOfPointsScoredHome;
                    group.Teams[3].NumberOfPointsScored += result2.NumberOfPointsScoredGuest;
                    group.Teams[3].PointDifference += result2.NumberOfPointsScoredGuest - result2.NumberOfPointsScoredHome;
                    group.Teams[3].NumberOfPoints += 1;
                    group.Teams[3].PreseasonResults.Add(new Result(group.Teams[3], group.Teams[2], result2.NumberOfPointsScoredHome, result2.NumberOfPointsScoredGuest));
                }
                else
                {
                    group.Teams[3].NumberOfWins += 1;
                    group.Teams[3].NumberOfPointsScored += result2.NumberOfPointsScoredGuest;
                    group.Teams[3].NumberOfPointsConceded += result2.NumberOfPointsScoredHome;
                    group.Teams[3].PointDifference += result2.NumberOfPointsScoredGuest - result2.NumberOfPointsScoredHome;
                    group.Teams[3].NumberOfPoints += 2;
                    group.Teams[3].PreseasonResults.Add(new Result(group.Teams[3], group.Teams[2], result2.NumberOfPointsScoredHome, result2.NumberOfPointsScoredGuest));

                    group.Teams[2].NumberOfLosses += 1;
                    group.Teams[2].NumberOfPointsConceded += result2.NumberOfPointsScoredGuest;
                    group.Teams[2].NumberOfPointsScored += result2.NumberOfPointsScoredHome;
                    group.Teams[2].PointDifference += result2.NumberOfPointsScoredHome - result2.NumberOfPointsScoredGuest;
                    group.Teams[2].NumberOfPoints += 1;
                    group.Teams[2].PreseasonResults.Add(result2);
                }
            }
        }
        public void RankGroup()
        {
            foreach(Group group in groups)
            {
                group.Teams.Sort((team1, team2) => team2.NumberOfPoints.CompareTo(team1.NumberOfPoints));
    
                for (int i = 0; i < group.Teams.Count - 1; i++)
                {
                    for (int j = i + 1; j < group.Teams.Count; j++)
                    {
                        if (group.Teams[i].NumberOfPoints == group.Teams[j].NumberOfPoints)
                        {
                            if (group.Teams.Count(t => t.NumberOfPoints == group.Teams[i].NumberOfPoints) == 2)
                            {
                                var team1 = group.Teams[i];
                                var team2 = group.Teams[j];
                                foreach(Result result in team1.PreseasonResults)
                                {
                                    if(result.Guest.Name.Equals(team2.Name))
                                    {
                                        if(result.NumberOfPointsScoredHome <  result.NumberOfPointsScoredGuest)
                                        {
                                            group.Teams[i]= team2;
                                            group.Teams[j]=team1;
                                        }
                                    }
                                }
                            }
                            else if (group.Teams.Count(t => t.NumberOfPoints == group.Teams[i].NumberOfPoints) >= 3)
                            {
                                var circleTeams = group.Teams.Where(t => t.NumberOfPoints == group.Teams[i].NumberOfPoints).ToList();
                                circleTeams.Sort(delegate (Team t1, Team t2)
                                {
                                    int t1CircleDifference = 0;
                                    int t2CircleDifference = 0;

                                    foreach (var opponent in circleTeams)
                                    {
                                        foreach (Result res in t1.PreseasonResults)
                                        {
                                            if (res.Guest.Equals(t2.Name))
                                            {
                                                t1CircleDifference += res.NumberOfPointsScoredHome - res.NumberOfPointsScoredGuest;
                                                t2CircleDifference += res.NumberOfPointsScoredGuest - res.NumberOfPointsScoredHome;
                                            }
                                        }
                                    }
                                    return t2CircleDifference.CompareTo(t1CircleDifference);
                                });


                                for (int k = 0; k < circleTeams.Count; k++)
                                {
                                    group.Teams[i + k] = circleTeams[k];
                                }

                                i += circleTeams.Count - 1;
                                break;
                            }
                        }
                    }
                }
                
               
            }

        }

        public List<Team> RankThreeTeam(List<Team> teams)
        {
            List<Team> newTeams = new List<Team>();
            Team team1;
            Team team2;
            newTeams = teams.OrderByDescending(team => team.NumberOfPoints).ToList();
            for(int i=0;i< newTeams.Count-1;i++)
            {
                for(int j=i+1;j< newTeams.Count;j++)
                {
                    if (newTeams[i].NumberOfPoints == newTeams[j].NumberOfPoints)
                    {
                        if (newTeams[i].PointDifference < newTeams[j].PointDifference)
                        {
                            team1=newTeams[i];
                            team2=newTeams[j];

                            newTeams[i] = team2;
                            newTeams[j] = team1;
                        }
                        else if(newTeams[i].PointDifference == newTeams[j].PointDifference)
                        {
                            if (newTeams[i].NumberOfPointsScored < newTeams[j].NumberOfPointsScored)
                            {
                                team1 = newTeams[i];
                                team2 = newTeams[j];

                                newTeams[i] = team2;
                                newTeams[j] = team1;
                            }
                        }
                    }
                }
            }
            return newTeams;
        }
        public void RankTeamsOneToEight()
        {
            List<Team> teams1= new List<Team>();
            List<Team> teams2= new List<Team>();
            List<Team> teams3= new List<Team>();

            foreach(Group group in groups)
            {
                teams1.Add(group.Teams[0]);
                teams2.Add(group.Teams[1]);
                teams3.Add(group.Teams[2]);
            }

            AllTeams.AddRange(RankThreeTeam(teams1));
            AllTeams.AddRange(RankThreeTeam(teams2));
            AllTeams.AddRange(RankThreeTeam(teams3));
            AllTeams.RemoveAt(AllTeams.Count-1);

        }

        public void AddPots()
        {
            potD.AddRange(new List<Team> { AllTeams[0], AllTeams[1] });
            potE.AddRange(new List<Team> { AllTeams[2], AllTeams[3] });
            potF.AddRange(new List<Team> { AllTeams[4], AllTeams[5] });
            potG.AddRange(new List<Team> { AllTeams[6], AllTeams[7] });
        }
        public void WritePot(List<Team> teams)
        {
            foreach(Team team in teams)
            {
                team.WriteName();
            }
        }
        public void WritePots()
        {
            Console.WriteLine("\n-----------------------------------------------\n");
            Console.WriteLine("Sesiri:\n");
            Console.WriteLine("\n   Sesir D:");
            WritePot(potD);
            Console.WriteLine("\n   Sesir E:");
            WritePot(potE);
            Console.WriteLine("\n   Sesir F:");
            WritePot(potF);
            Console.WriteLine("\n   Sesir G:");
            WritePot(potG);
        }

        public void FormPairs(List<Team> pot1,List<Team> pot2)
        {
            Random random = new Random();
            int randNumber=random.Next(1,3);
            Result result;
            int number = 0;
            if(randNumber == 1)
            {
                number = pot1[0].PreseasonResults.Count(res => res.Guest.Name.Equals(pot2[0].Name));
                if(number == 0)
                {
                    result = new Result(pot1[0], pot2[0]);
                    eliminationResults.Add(result);
                    result = new Result(pot1[1], pot2[1]);
                    eliminationResults.Add(result);
                }
                else
                {
                    result = new Result(pot1[0], pot2[1]);
                    eliminationResults.Add(result);
                    result = new Result(pot1[1], pot2[0]);
                    eliminationResults.Add(result);
                }
            }
            else
            {
                number = pot1[0].PreseasonResults.Count(res => res.Guest.Name.Equals(pot2[1].Name));
                if (number == 0)
                {
                    result = new Result(pot1[0], pot2[1]);
                    eliminationResults.Add(result);
                    result = new Result(pot1[1], pot2[0]);
                    eliminationResults.Add(result);
                }
                else
                {
                    result = new Result(pot1[0], pot2[0]);
                    eliminationResults.Add(result);
                    result = new Result(pot1[1], pot2[1]);
                    eliminationResults.Add(result);
                }
            }
            
        }
        public void EliminationPhase()
        {
            FormPairs(potD,potG);
            FormPairs(potE, potF);

            List<Result> neweliminationResults=new List<Result>();
            Random random = new Random();
            int randNumber = random.Next(2, 5);
            if(randNumber == 2)
            {
                neweliminationResults.AddRange(new List<Result> { eliminationResults[0], eliminationResults[1], eliminationResults[2], eliminationResults[3] });
            }
            else if (randNumber == 3)
            {
                neweliminationResults.AddRange(new List<Result> { eliminationResults[0], eliminationResults[2], eliminationResults[1], eliminationResults[3] });
            }
            else
            {
                neweliminationResults.AddRange(new List<Result> { eliminationResults[0], eliminationResults[3], eliminationResults[1], eliminationResults[2] });
            }
            eliminationResults = neweliminationResults;

            Console.WriteLine("\nEliminaciona faza:\n");
            foreach(Result res in eliminationResults)
            {
                res.WritePair();
            }
        }

        public Team GetWinMatch(Result result)
        {
            if(result.NumberOfPointsScoredHome>result.NumberOfPointsScoredGuest)
            {
                return result.Home;
            }
            else
                return result.Guest;            
        }
        public Team GetLostMatch(Result result)
        {
            if (result.NumberOfPointsScoredHome > result.NumberOfPointsScoredGuest)
            {
                return result.Guest;
            }
            else
                return result.Home;
        }
        public void PlayEliminationPhase()
        {
            Result result1 = GetResult(eliminationResults[0].Home, eliminationResults[0].Guest);
            Result result2 = GetResult(eliminationResults[1].Home, eliminationResults[1].Guest);
            Result result3 = GetResult(eliminationResults[2].Home, eliminationResults[2].Guest);
            Result result4 = GetResult(eliminationResults[3].Home, eliminationResults[3].Guest);

            semiFinalsMatch.Add(new Result(GetWinMatch(result1), GetWinMatch(result2)));
            semiFinalsMatch.Add(new Result(GetWinMatch(result3), GetWinMatch(result4)));

            Console.WriteLine("\n\nCetvrtfinale:\n");
            Console.WriteLine(result1);
            Console.WriteLine(result2);
            Console.WriteLine(result3);
            Console.WriteLine(result4);
        }

        public void PlaySemiFinalsMatch()
        {
            Result result1 = GetResult(semiFinalsMatch[0].Home, semiFinalsMatch[0].Guest);
            Result result2 = GetResult(semiFinalsMatch[1].Home, semiFinalsMatch[1].Guest);

            finalMatch = new Result(GetWinMatch(result1), GetWinMatch(result2));
            forThirdPlace = new Result(GetLostMatch(result1), GetLostMatch(result2));
            Console.WriteLine("\nPolufinale:\n");
            Console.WriteLine(result1);
            Console.WriteLine(result2);

        }

        public void FinalMatchs()
        {
            Result result1 = GetResult(finalMatch.Home, finalMatch.Guest);
            Result result2 = GetResult(forThirdPlace.Home, forThirdPlace.Guest);

            Team first=GetWinMatch(result1);
            Team second=GetLostMatch(result1);
            Team third = GetWinMatch(result2);
            
            Console.WriteLine("\nUtakmica za trece mesto:\n");
            Console.WriteLine(result2);
            Console.WriteLine("\nFinale:\n");
            Console.WriteLine(result1);

            Console.WriteLine("\nMedalje:\n");
            Console.WriteLine("\t"+ first.Name);
            Console.WriteLine("\t" + second.Name);
            Console.WriteLine("\t" + third.Name);

        }



        public void WriteGroup()
        {
            Console.WriteLine("\nGrupna faza - I kolo\n");
            foreach(Group group in groups)
            {
                group.WriteFirstResults(); 
            }
            Console.WriteLine("\nGrupna faza - II kolo\n");
            foreach (Group group in groups)
            {
                group.WriteSecondResults();
            }
            Console.WriteLine("\nGrupna faza - III kolo\n");
            foreach (Group group in groups)
            {
                group.WriteThirdResults();
            }
            Console.WriteLine("\n------------------------------------\n");
            Console.WriteLine("\nKonacni plasman po grupama:\n");
            foreach (Group group in groups)
            {
                group.WriteTeams();
            }
        }

        public void WriteAllRankTeams()
        {
            foreach(Team team in AllTeams)
            {
                team.Write();
            }
        }
        
    }
}
