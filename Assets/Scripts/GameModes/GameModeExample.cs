using UnityEngine;
using System.Collections;

public class GameModeExample : GameModeBase
{

	public string gameModeSpecificVar = "You can have variables specific to the GameMode by just defining them in the custom class file.";

	public override void GameModeLoaded()
	{
		base.GameModeLoaded();
		Debug.Log("GameModeLoaded");
	}

	public override void Start()
	{
		base.Start();
		Debug.Log("Start");
	}

	public override void GameStart()
	{
		base.GameStart();
		Debug.Log("GameStart");
	}

	public override void Update()
	{
		base.Update();
		//Debug.Log("Update");
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		//Debug.Log("LateUpdate");
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		//Debug.Log("FixedUpdate");
	}

	public override void GameEnd()
	{
		base.GameEnd();
		Debug.Log("GameEnd");
	}

	public override void GameExit()
	{
		base.GameExit();
		Debug.Log("GameExit");
	}

}
