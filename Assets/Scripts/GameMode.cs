using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace GameMode
{
	public static class GameMode
	{
		public const int minNumPlayers = 2;
		public const int maxNumPlayers = 4;

		public const int minNumTeams = 2;
		public const int maxNumTeams = maxNumPlayers;


		public static IEnumerable<System.Type> GetAllClassesInheritingClass<T>(bool canBeAbstract = false)
		{
			return Assembly.GetAssembly(typeof(T)).GetTypes().Where
			(
				type => type.IsClass && (canBeAbstract || !type.IsAbstract) && type.IsSubclassOf(typeof(T))
			);
		}
	}

	public class Team
	{
		public class SortByScore : Comparer<Team>
		{
			public override int Compare(Team x, Team y)
			{
				return x.score > y.score ? -1 : (x.score < y.score ? 1 : 0);
			}
		}

		public string name
		{
			get
			{
				return distribution.name;
			}
		}
		public string desc
		{
			get
			{
				return distribution.desc;
			}
		}

		public List<Player> members = new List<Player>();

		public float score = 0;

		public TeamDistribution.TeamDistribution distribution;

		public Team(TeamDistribution.TeamDistribution distribution, params Player[] members)
		{
			this.distribution = distribution;
			this.members = new List<Player>(members);
		}
	}


	namespace TeamDistribution
	{
		[System.Serializable]
		public class TeamDistribution
		{
			public string name;
			public string desc;
			public int[] numPlayersPerCount;

			public int GetMaxMembers(int playerCount)
			{
				if(playerCount < GameMode.minNumPlayers || playerCount > GameMode.maxNumPlayers)
				{
					throw new System.ArgumentOutOfRangeException("playerCount", playerCount,
						"Player count must be within GameMode.minNumPlayers and GameMode.maxNumPlayers (" + GameMode.minNumPlayers + " and " + GameMode.maxNumPlayers + ")");
				}

				return numPlayersPerCount[playerCount - GameMode.minNumPlayers];
			}
		}

		[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
		public class TeamDistributorAttribute : System.Attribute
		{
			public readonly string name;
			public readonly string desc;
			
			public TeamDistributorAttribute(string name, string desc = "")
			{
				this.name = name;
				this.desc = desc;
			}
		}

		/// <summary>
		/// Used to distribute players between teams each round
		/// </summary>
		public abstract class TeamDistributor
		{
			public struct NameDesc
			{
				public string name;
				public string desc;
			}
			private struct DistributorType
			{
				public System.Type type;
				public NameDesc nameDesc;
			}


			private static List<DistributorType> allDistributors;

			[UnityEditor.Callbacks.DidReloadScripts]
			private static void RebuildTeamDistributorList()
			{
				allDistributors = new List<DistributorType>();

				foreach(System.Type type in GameMode.GetAllClassesInheritingClass<TeamDistributor>(false))
				{
					TeamDistributorAttribute attribute = (TeamDistributorAttribute)System.Attribute.GetCustomAttribute(type, typeof(TeamDistributorAttribute));
					if(attribute == null)
					{
						Debug.LogWarning("Team Distributor \"" + type.FullName + "\" doesnt have the TeamDistributor attribute!");
						continue;
					}

					allDistributors.Add(new DistributorType()
					{
						type = type,
						nameDesc = new NameDesc()
						{
							name = attribute.name,
							desc = attribute.desc,
						}
					});
				}
			}

			private static DistributorType GetDistributorType(string name)
			{
				if(allDistributors == null)
				{
					RebuildTeamDistributorList();
				}
				return allDistributors.Find((stats) => { return stats.nameDesc.name == name; });
			}

			public static TeamDistributor MakeDistributor(string name, Team[] teams, Player[] players)
			{
				return (TeamDistributor)System.Activator.CreateInstance(GetDistributorType(name).type, (object)teams, (object)players);
			}
			public static List<NameDesc> GetAllDistributors()
			{
				if(allDistributors == null)
				{
					RebuildTeamDistributorList();
				}

				List<NameDesc> nameDescs = new List<NameDesc>();
				foreach(DistributorType distType in allDistributors)
				{
					nameDescs.Add(distType.nameDesc);
				}
				return nameDescs;
			}


			private static int Repeat(int value, int length)
			{
				value = value % length;
				//C# modulus is weird and can range from (1-y) to (y-1), we want 0 to (y-1)
				if(value < 0)
				{
					value += length;
				}

				return value;
			}



			public TeamDistributor(Team[] teams, Player[] players)
			{
				this.teams = teams;
				this.players = new List<Player>(players);
			}

			protected Team[] teams;
			protected List<Player> players;

			public abstract void Distribute();

			protected void ClearMembers()
			{
				foreach(Team team in teams)
				{
					team.members.Clear();
				}
			}

			protected void DoDefaultDistribution(int offset = 0)
			{
				//Repeat offset so that it correctly indexes onto a player
				offset = Repeat(offset, players.Count);

				int playerId = offset;
				foreach(Team team in teams)
				{
					int maxMembers = team.distribution.GetMaxMembers(players.Count);

					if(team.members.Count >= maxMembers)
					{
						//This team is full, 
						continue;
					}

					Player player = players[Repeat(playerId, players.Count)];

					team.members.Add(player);
					playerId++;
				}
			}
		}

		/// <summary>
		/// Default distribution of players. Assigns players to teams in the order they are given in. Obeys teams max member count.
		/// </summary>
		[TeamDistributor("Default", "Default distribution of players. Always the same.")]
		public class DefaultDistributor : TeamDistributor
		{
			public DefaultDistributor(Team[] teams, Player[] players)
				: base(teams, players)
			{ }

			public override void Distribute()
			{
				//Clear out the previous members
				ClearMembers();

				//Do the default distribution
				DoDefaultDistribution();
			}
		}

		/// <summary>
		/// Each round every player is shifted into the next slot (which may or may not be in the same team)
		/// </summary>
		[TeamDistributor("Shift", "Uses default distribution, but each round it shifts every player into the next slot (which may or may not be in the same team)")]
		public class ShiftDistributor : TeamDistributor
		{
			public ShiftDistributor(Team[] teams, Player[] players)
				: base(teams, players)
			{ }

			private int offset = 0;

			public override void Distribute()
			{
				//Clear out the previous members
				ClearMembers();

				//Do the default distribution with an offset
				DoDefaultDistribution(offset);

				//Increase the offset for next time
				offset++;
			}
		}

		/// <summary>
		/// Random distribution each round
		/// </summary>
		[TeamDistributor("Random", "Each round the players are put into random teams.")]
		public class RandomDistributor : TeamDistributor
		{
			public RandomDistributor(Team[] teams, Player[] players)
				: base(teams, players)
			{ }

			public override void Distribute()
			{
				//Clear out the previous members
				ClearMembers();

				List<Player> openPlayers = new List<Player>(players);
				foreach(Team team in teams)
				{
					AssignPlayersAtRandom(team, openPlayers);
				}
			}

			protected void AssignPlayersAtRandom(Team team, List<Player> openList)
			{
				int maxMembers = team.distribution.GetMaxMembers(players.Count);
				
				for(int i = 0; i < maxMembers; i++)
				{
					if(openList.Count <= 0)
					{
						break;
					}

					//Pick a player at random
					int random = Random.Range(0, openList.Count - 1);
					Player player = openList[random];

					//Add them to the team
					team.members.Add(player);

					//Remove the player from openPlayers efficiently (not preserving order)
					openList[random] = openList[openList.Count - 1]; //Replace the element to remove with the last element
					openList.RemoveAt(openList.Count - 1); //Remove the last element
				}
			}
		}

		/// <summary>
		/// Random distribution, but try to make sure that each player plays in each team an even amount of times
		/// </summary>
		[TeamDistributor("Random Balanced", "Random distribution, but try to make sure that each player plays in each team an even amount of times")]
		public class RandomBalancedDistributor : TeamDistributor
		{
			private struct TeamSortedPlayerList
			{
				public TeamSortedPlayerList(Team team)
				{
					this.team = team;
					playerIdByTimesPlayed = new SortedList<int, int>();
				}

				public Team team;
				public SortedList<int, int> playerIdByTimesPlayed;
			}

			public RandomBalancedDistributor(Team[] teams, Player[] players)
				: base(teams, players)
			{
				timesPlayerInTeam = new int[players.Length, teams.Length];
			}

			public int[,] timesPlayerInTeam;

			public override void Distribute()
			{
				//Clear out the previous members
				ClearMembers();


				//Whether or not each player has been assigned to a team yet (by index in players list)
				bool[] playerAssigned = new bool[players.Count];

				//List of all the teams, sorted by the lowest times played by any player
				SortedList<TeamSortedPlayerList, int> sortedTeams = new SortedList<TeamSortedPlayerList, int>();

				//Populate sortedTeams
				int teamId = 0;
				foreach(Team team in teams)
				{
					TeamSortedPlayerList teamPlayerList = new TeamSortedPlayerList(team);
					int minTimesPlayed = int.MaxValue;
					
					//Add each player to the teams internal list
					for(int playerId = 0; playerId < players.Count; playerId++)
					{
						int timesPlayed = timesPlayerInTeam[playerId, teamId];

						minTimesPlayed = Mathf.Min(minTimesPlayed, timesPlayed);

						teamPlayerList.playerIdByTimesPlayed.Add(playerId, timesPlayed);
					}

					sortedTeams.Add(teamPlayerList, minTimesPlayed);

					teamId++;
				}


				foreach(TeamSortedPlayerList sortedTeam in sortedTeams.Keys)
				{
					int maxMembers = sortedTeam.team.distribution.GetMaxMembers(players.Count);

					//While we need more players in the team
					while(sortedTeam.team.members.Count < maxMembers)
					{
						//Get the next batch of possible members
						List<int> possibleMembers = GetBalancedMembers(playerAssigned, sortedTeam.playerIdByTimesPlayed);

						if(possibleMembers.Count <= 0)
						{
							//There arent enough players in the game to fully populate the teams. Something broke
							Debug.LogError("Not enough players in game to populate teams.");
							return;
						}

						//While this batch isnt empty and we still need more players in the team
						while(possibleMembers.Count > 0 && sortedTeam.team.members.Count < maxMembers)
						{
							//Pick a random eligible member
							int rand = Random.Range(0, possibleMembers.Count - 1);
							int newMemberID = possibleMembers[rand];

							//Add the new member
							sortedTeam.team.members.Add(players[newMemberID]);
							playerAssigned[newMemberID] = true;

							//Remove them from the list
							possibleMembers.RemoveAt(rand);
						}
					}

				}
			}

			private static List<int> GetBalancedMembers(bool[] playerAssigned, SortedList<int, int> playerIdByTimesPlayed)
			{
				List<int> possibleMembers = new List<int>();
				int minValue = int.MaxValue;
				
				foreach(KeyValuePair<int, int> playerKeyValue in playerIdByTimesPlayed)
				{
					int playerId = playerKeyValue.Key;
					if(playerAssigned[playerId])
					{
						//The player is already assigned, skip them
						continue;
					}

					int timesPlayed = playerKeyValue.Value;
					if(possibleMembers.Count == 0)
					{
						//This is our first possible member
						minValue = timesPlayed;
					}
					else if(timesPlayed > minValue)
					{
						//That is the last of the possible members
						break;
					}

					possibleMembers.Add(playerId);
				}

				return possibleMembers;
			}
		}
	}
}
