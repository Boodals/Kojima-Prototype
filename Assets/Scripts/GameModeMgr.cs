using UnityEngine;
using System.Collections.Generic;

public class GameModeMgr : MonoBehaviour
{

	public class Team
	{
		public string name;
		public HashSet<Player> members = new HashSet<Player>();

		public int score = 0;


		public Team(string name, params Player[] members)
		{
			this.members = new HashSet<Player>(members);
		}
	}

	public static GameModeMgr singleton { get; private set; }

	public static GameModeBase activeGameMode { get; private set; }




	public static void SetActiveGameMode(GameModeBase newGameMode)
	{

	}


	void Awake()
	{
		if(singleton != null)
		{
			Debug.LogError("Multiple GameMode Managers!");
			Destroy(this);
			return;
		}

		singleton = this;
	}



}
