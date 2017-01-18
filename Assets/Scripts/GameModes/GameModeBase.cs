using UnityEngine;
using System.Collections.Generic;
using GameMode;
using GameMode.TeamDistribution;

/// <summary>
/// Base GameMode class. Inherit off this to create a custom GameMode.
/// </summary>
public abstract class GameModeBase : ScriptableObject
{
	[SerializeField, HideInInspector]
	protected TeamDistributionSettings[] _teamDistribution;
	public TeamDistributionSettings[] teamDistribution
	{
		get
		{
			return _teamDistribution;
		}
	}

	[SerializeField, HideInInspector]
	protected int _numRounds = 3;
	public int numRounds
	{
		get
		{
			return _numRounds;
		}
	}

	[SerializeField, HideInInspector, Tooltip("If at the end of the rounds two or more players are tied, should we do additonal rounds until there is no longer a tie?")]
	protected bool _doTieBreaker = true;
	public bool doTieBreaker
	{
		get
		{
			return _doTieBreaker;
		}
	}

	[SerializeField, HideInInspector]
	protected string distributorTypeName;


	private TeamDistributor distributor;

	[System.NonSerialized]
	public List<Team> teams = new List<Team>();

	protected virtual void OnValidate()
	{
		_numRounds = Mathf.Max(1, _numRounds); //Make sure we always have atleast 1 round
	}

	/// <summary>
	/// Called once when the game/application is loaded
	/// </summary>
	public virtual void GameModeLoaded() { }

	/// <summary>
	/// Called every time the gamemode is started, before any countdowns or team selection
	/// </summary>
	public virtual void Start()
	{
		
	}

	/// <summary>
	/// Populates teams with all players. Override to use custom assignment
	/// </summary>
	public virtual void AssignTeamMembers(Player[] players)
	{
		distributor = TeamDistributor.MakeDistributor(distributorTypeName, teams.ToArray(), players);
		distributor.Distribute();
	}

	/// <summary>
	/// Called once the countdown has ended
	/// </summary>
	public virtual void GameStart() { }


	/// <summary>
	/// Called once per frame when the countdown has finished
	/// </summary>
	public virtual void Update() { }

	/// <summary>
	/// Called once per frame immediately after Update, when the countdown has finished
	/// </summary>
	public virtual void LateUpdate() { }

	/// <summary>
	/// Called once per physics frame, when the countdown has finished
	/// </summary>
	public virtual void FixedUpdate() { }


	/// <summary>
	/// Called when the gamemode ends, before the victor is calculated
	/// This wont be called if the gamemode ends ungracefully
	/// </summary>
	public virtual void GameEnd() { }

	/// <summary>
	/// Called when the gamemode is exited (after the victor is announced)
	/// Use this to reset any variables that are being used
	/// </summary>
	public virtual void GameExit()
	{
		distributor = null;
	}
}
