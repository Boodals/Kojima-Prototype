using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base GameMode class. Inherit off this to create a custom GameMode.
/// </summary>
public abstract class GameModeBase : ScriptableObject
{
	[SerializeField, Range(2, 4)]
	private int numTeams;

	

	/// <summary>
	/// Called once when the game/application is loaded
	/// </summary>
	protected abstract void GameModeLoaded();

	/// <summary>
	/// Called every time the gamemode is started, before any countdowns or team selection
	/// </summary>
	protected abstract void Start();

	/// <summary>
	/// Called once per frame
	/// </summary>
	protected virtual void Update() { }

	//Called once per frame, after Update
	protected virtual void LateUpdate() { }

	/// <summary>
	/// Called once per physics frame
	/// </summary>
	protected virtual void FixedUpdate() { }
	
	/// <summary>
	/// Called when the countdown has ended
	/// </summary>
	protected abstract void GameStart();
	
}
