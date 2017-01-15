using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base GameMode class. Inherit off this to create a custom GameMode.
/// </summary>
public abstract class GameModeBase : ScriptableObject
{
	[System.Serializable]
	private class TeamDistribution
	{
		public string name;
		public int[] numPlayersPerCount;
	}

	[SerializeField, HideInInspector]
	private TeamDistribution[] teamDistribution;



	/// <summary>
	/// Called once when the game/application is loaded
	/// </summary>
	public virtual void GameModeLoaded() { }

	/// <summary>
	/// Called every time the gamemode is started, before any countdowns or team selection
	/// </summary>
	public virtual void Start() { }

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
	public virtual void GameExit() { }
}
