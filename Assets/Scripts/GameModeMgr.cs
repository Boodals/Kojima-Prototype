using UnityEngine;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

public class GameModeMgr : MonoBehaviour
{
	public enum Phase
	{
		Lobby, //Not in a game
		CountDown, //During the countdown
		GameStarted, //When in game, Update etc being called
		GameFinished, //Scoreboard
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

		public string name;
		public HashSet<Player> members = new HashSet<Player>();

		public float score = 0;


		public Team(string name, params Player[] members)
		{
			this.members = new HashSet<Player>(members);
		}
	}

	public delegate void CountDownHandler(int timeLeft);

	public delegate void TeamVictorHandler(ReadOnlyCollection<Team> teamsSortedByScore);



	public const int minNumPlayers = 2;
	public const int maxNumPlayers = 4;

	public const int minNumTeams = 2;
	public const int maxNumTeams = maxNumPlayers;

	public const float countDownTime = 3f;

	
	public static GameModeBase activeGameMode { get; private set; }

	private static List<Team> activeTeams;
	public static ReadOnlyCollection<Team> teams
	{
		get
		{
			return new ReadOnlyCollection<Team>(teams);
		}
	}
	
	public static Phase currentPhase { get; private set; }
	
	//Register to this to display countdown visuals/FX, eg "3, 2, 1, GO!"
	public static event CountDownHandler AnnounceCountdown;



	private static GameModeMgr singleton;

	private static float endOfCountdownTime;
	private static int countdownTimeSec;

	//I use this as a safety check to make sure nobody tries ending other GameModes
	private static bool isInGameModeFunctions = false;


	public static GameModeBase FindGameModeByName(string gameModeName)
	{
		foreach(GameModeBase gameMode in singleton.allGameModes)
		{
			if(gameMode.name == gameModeName)
			{
				return gameMode;
			}
		}

		return null;
	}
	
	public static void StartGameMode(GameModeBase newGameMode)
	{
		if(currentPhase != Phase.Lobby)
		{
			Debug.LogWarning("Attempted to start gamemode when not in lobby!");
			return;
		}
		
		SetActiveGameMode(newGameMode);
	}

	/// <summary>
	/// Call this from within the GameMode's Update functions to end the game
	/// </summary>
	public static void EndGameMode()
	{
		if(!isInGameModeFunctions)
		{
			Debug.LogWarning("You can only call EndGameMode from within the current active GameMode's Update functions");
			return;
		}
		if(currentPhase != Phase.GameStarted)
		{
			Debug.LogWarning("Attempted to set end gamemode when not in lobby!");
			return;
		}

		EndGame();
	}

	/// <summary>
	/// Forcefully set the current gamemode. Use this for debugging.
	/// If possible, use SetActiveGameMode(GameModeBase newGameMode) instead of this, as it is faster.
	/// </summary>
	/// <param name="gameModeName">The name of the gamemode to start</param>
	public static void SetActiveGameMode(string gameModeName)
	{
		SetActiveGameMode(FindGameModeByName(gameModeName));
	}
	/// <summary>
	/// Forcefully set the current gamemode. Use this for debugging
	/// </summary>
	/// <param name="newGameMode">The gamemode to start</param>
	public static void SetActiveGameMode(GameModeBase newGameMode)
	{
		if(activeGameMode != null)
		{
			activeGameMode.GameExit();
		}
		
		activeGameMode = newGameMode;

		if(newGameMode != null)
		{
			newGameMode.Start();

			AssignTeams();

			StartCountdown();
		}
	}




	private static void AssignTeams()
	{

	}

	private static void StartCountdown()
	{
		currentPhase = Phase.CountDown;
		endOfCountdownTime = Time.time + countDownTime;
		countdownTimeSec = 0;
	}

	private static void HandleCountDown()
	{
		float timeLeft = endOfCountdownTime - Time.time;
		float timeCountUp = countDownTime - timeLeft; //Starts at 0 and increases
		if(Mathf.FloorToInt(timeCountUp) > countdownTimeSec - 1)
		{
			countdownTimeSec++;
			if(AnnounceCountdown != null)
			{
				AnnounceCountdown(Mathf.CeilToInt(timeLeft));
			}
		}

		if(Time.time >= endOfCountdownTime)
		{
			StartGame();
		}
	}

	private static void AnnounceCountDown_DebugLog(int timeLeft)
	{
		Debug.Log("Countdown: " + timeLeft);
	}

	private static void StartGame()
	{
		currentPhase = Phase.GameStarted;
		activeGameMode.GameStart();
	}

	private static void EndGame()
	{
		//Tell the gameMode that its ending
		//This can be used to calculate scores for all teams
		activeGameMode.GameEnd();

		//Sort teams by score
		List<Team> sortedTeams = new List<Team>(activeTeams);
		sortedTeams.Sort(new Team.SortByScore());

		foreach(Team team in sortedTeams)
		{
			Debug.Log("Team " + team.name + " had a score of " + team.score);
		}
		
		currentPhase = Phase.GameFinished;
	}

	private static void ExitGame()
	{
		activeGameMode.GameExit();

		currentPhase = Phase.Lobby;
	}



	[SerializeField, HideInInspector]
	//This is populated automatically
	private List<GameModeBase> allGameModes = new List<GameModeBase>();

	void Awake()
	{
		if(singleton != null)
		{
			Debug.LogWarning("Multiple GameMode Managers!");
			Destroy(this);
			return;
		}

		singleton = this;

		foreach(GameModeBase gameMode in allGameModes)
		{
			gameMode.GameModeLoaded();
		}

		AnnounceCountdown += AnnounceCountDown_DebugLog;
	}

	void Update()
	{
		if(activeGameMode != null && currentPhase == Phase.GameStarted)
		{
			isInGameModeFunctions = true;
			activeGameMode.Update();
			isInGameModeFunctions = false;
		}
	}

	void LateUpdate()
	{
		if(activeGameMode != null && currentPhase == Phase.GameStarted)
		{
			isInGameModeFunctions = true;
			activeGameMode.LateUpdate();
			isInGameModeFunctions = false;
		}

		switch(currentPhase)
		{
		case Phase.Lobby:
			break;
		case Phase.CountDown:

			HandleCountDown();
			break;
		case Phase.GameStarted:
			break;
		case Phase.GameFinished:
			break;
		}
	}

	void FixedUpdate()
	{
		if(activeGameMode != null && currentPhase == Phase.GameStarted)
		{
			isInGameModeFunctions = true;
			activeGameMode.FixedUpdate();
			isInGameModeFunctions = false;
		}
	}

	void OnDestroy()
	{
		AnnounceCountdown -= AnnounceCountDown_DebugLog;
	}

}
