using UnityEngine;
using System.Collections;

public abstract class GameModeBase : MonoBehaviour
{

	/// <summary>
	/// Called once when the game is loaded
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
