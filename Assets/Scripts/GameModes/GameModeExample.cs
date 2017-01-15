using UnityEngine;
using System.Collections;

public class GameModeExample : GameModeBase
{

	public string testVariable;

	public override void GameModeLoaded()
	{
		Debug.Log("GameModeLoaded");
	}

	public override void Start()
	{
		Debug.Log("Start");
	}

	public override void GameStart()
	{
		Debug.Log("GameStart");
	}

	public override void Update()
	{
		//Debug.Log("Update");
	}

	public override void LateUpdate()
	{
		//Debug.Log("LateUpdate");
	}

	public override void FixedUpdate()
	{
		//Debug.Log("FixedUpdate");
	}

	public override void GameEnd()
	{
		Debug.Log("GameEnd");
	}

	public override void GameExit()
	{
		Debug.Log("GameExit");
	}

}
