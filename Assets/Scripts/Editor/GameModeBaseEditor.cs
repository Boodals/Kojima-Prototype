using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(GameModeBase), true)]
public class GameModeBaseEditor : Editor
{
	private static bool teamFoldout;
	private static bool[] teamFoldouts = new bool[GameModeMgr.maxNumTeams];




	public override void OnInspectorGUI()
	{
		//serializedObject.FindProperty("numTeams").intValue

		SerializedProperty teamDis = serializedObject.FindProperty("teamDistribution");

		if(teamFoldout = EditorGUILayout.Foldout(teamFoldout, "Teams"))
		{
			EditorGUI.indentLevel++;

			int newSize = EditorGUILayout.IntSlider("Number of teams", teamDis.arraySize, GameModeMgr.minNumTeams, GameModeMgr.maxNumTeams);

			if(newSize > teamDis.arraySize)
			{
				//Were adding atleast one new team

				int i = teamDis.arraySize;
				teamDis.arraySize = newSize; //Expand the array
				for(; i < newSize; i++)
				{
					//This is a new team
					SerializedProperty team = teamDis.GetArrayElementAtIndex(i);

					team.FindPropertyRelative("name").stringValue = "Team " + (i + 1);
				}
			}
			else
			{
				teamDis.arraySize = newSize;
			}


			//Make sure the team counts add up
			int[] assignedPlayers = new int[GameModeMgr.maxNumPlayers - GameModeMgr.minNumPlayers + 1];

			for(int i = 0; i < teamDis.arraySize; i++)
			{
				SerializedProperty team = teamDis.GetArrayElementAtIndex(i);

				//Make sure it has the rignt number of elements
				team.FindPropertyRelative("numPlayersPerCount").arraySize = GameModeMgr.maxNumPlayers - GameModeMgr.minNumPlayers + 1;

				for(int j = 0; j <= GameModeMgr.maxNumPlayers - GameModeMgr.minNumPlayers; j++)
				{
					SerializedProperty numPlayers = team.FindPropertyRelative("numPlayersPerCount").GetArrayElementAtIndex(j);

					assignedPlayers[j] += numPlayers.intValue;
				}
			}


			//Now actually draw the GUI for the teams
			for(int i = 0; i < teamDis.arraySize; i++)
			{
				SerializedProperty team = teamDis.GetArrayElementAtIndex(i);
				
				DrawTeam(team, i, assignedPlayers);
			}

			EditorGUI.indentLevel--;
		}

		serializedObject.ApplyModifiedProperties();


		DrawDefaultInspector();
	}

	private static void DrawTeam(SerializedProperty team, int teamID, int[] assignedPlayers)
	{
		string readableName = team.FindPropertyRelative("name").stringValue;
		if(readableName == "")
		{
			readableName = "Team " + (teamID + 1);
		}

		if(teamFoldouts[teamID] = EditorGUILayout.Foldout(teamFoldouts[teamID], readableName))
		{
			EditorGUI.indentLevel++;

			EditorGUILayout.PropertyField(team.FindPropertyRelative("name"));

			EditorGUILayout.LabelField("Players on team per total player count:");
			EditorGUI.indentLevel++;

			for(int i = 0; i <= GameModeMgr.maxNumPlayers - GameModeMgr.minNumPlayers; i++)
			{
				SerializedProperty numPlayers = team.FindPropertyRelative("numPlayersPerCount").GetArrayElementAtIndex(i);

				int playerCount = i + GameModeMgr.minNumPlayers;

				Color prevColor = GUI.color;

				//If the number of players doesnt add up
				if(playerCount != assignedPlayers[i])
				{
					//Highlight it in red so you can see easily
					GUI.color = Color.red;
				}

				numPlayers.intValue = EditorGUILayout.IntSlider(playerCount + " Players", numPlayers.intValue, 0, playerCount);

				GUI.color = prevColor;
			}

			EditorGUI.indentLevel--;
			EditorGUI.indentLevel--;
		}
	}

	[UnityEditor.Callbacks.DidReloadScripts]
	private static void OnReloadScripts()
	{
		//This will be called when unity reloads scripts


		//Make sure the GameModes folder exists
		CreateFolder(GameModeMgrEditor.gameModeFolder);


		//Find the GUID of all assets in the gameModeFolder
		string[] allGameModesGUIDs = AssetDatabase.FindAssets("", new string[] { GameModeMgrEditor.gameModeFolder });

		GameModeBase[] allGameModes = new GameModeBase[allGameModesGUIDs.Length];

		int GUIDIndex = 0;
		foreach(string GUID in allGameModesGUIDs)
		{
			//Get the asset path from the GUID
			string assetPath = AssetDatabase.GUIDToAssetPath(GUID);

			//Load the asset
			GameModeBase gameMode = AssetDatabase.LoadAssetAtPath<GameModeBase>(assetPath);

			//Make sure the script hasnt been deleted
			if(gameMode != null)
			{
				//Add it to the array
				allGameModes[GUIDIndex] = gameMode;
			}
			else
			{
				//The script has been deleted. Delete the asset file as well
				Debug.Log("Missing script file for GameMode '" + System.IO.Path.GetFileNameWithoutExtension(assetPath) + "', deleting.");
				AssetDatabase.DeleteAsset(assetPath);
			}

			GUIDIndex++;
		}


		//Find all classes that inherit GameModeBase
		foreach(System.Type type in GetAllClassesInheritingClass<GameModeBase>(false))
		{
			//Make sure this GameMode class has its own asset file
			if(allGameModes.Any(gameMode => gameMode != null && gameMode.name == type.Name))
			{
				//Debug.Log(type.Name + " has an asset file.");
			}
			else
			{
				Debug.Log("Discovered new GameMode class '" + type.Name + "'. Creating a settings asset in " + GameModeMgrEditor.gameModeFolder);

				CreateGameModeAsset(type);
			}
		}
	}

	private static void CreateGameModeAsset(System.Type type)
	{
		//Create a new asset file

		//Create the actual object to save as an asset
		GameModeBase gameMode = CreateInstance(type) as GameModeBase;

		//Save the object as an asset
		string path = GameModeMgrEditor.gameModeFolder + "/" + type.Name + ".asset";
		AssetDatabase.CreateAsset(gameMode, path);

		//Save changes to assets
		AssetDatabase.SaveAssets();

		//Refresh assets to make sure its visible in the inspector
		AssetDatabase.Refresh();
	}

	private static void CreateFolder(string folder)
	{
		if(!AssetDatabase.IsValidFolder(folder))
		{
			string folderLocation = System.IO.Path.GetDirectoryName(folder);
			string folderName = System.IO.Path.GetFileName(folder);

			//Debug.Log("Creating folder '" + folder + "' in '" + folderLocation + "' with name '" + folderName + "'");

			//Make sure the parent folder exists recursively
			CreateFolder(folderLocation);

			AssetDatabase.CreateFolder(folderLocation, folderName);
		}
	}

	private static IEnumerable<System.Type> GetAllClassesInheritingClass<T>(bool canBeAbstract = false)
	{
		return Assembly.GetAssembly(typeof(T)).GetTypes().Where
		(
			type => type.IsClass && (canBeAbstract || !type.IsAbstract) && type.IsSubclassOf(typeof(T))
		);
	}
}
