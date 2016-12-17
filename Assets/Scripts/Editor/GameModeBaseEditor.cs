using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(GameModeBase), true)]
public class GameModeBaseEditor : Editor
{

	private const string gameModeFolder = "Assets/GameModes";
	
	public override void OnInspectorGUI()
	{
		//serializedObject.FindProperty("numTeams").intValue



		DrawDefaultInspector();
	}

	[UnityEditor.Callbacks.DidReloadScripts]
	private static void OnReloadScripts()
	{
		//This will be called when unity reloads scripts


		//Make sure the GameModes folder exists
		CreateFolder(gameModeFolder);


		//Find the GUID of all assets in the gameModeFolder
		string[] allGameModesGUIDs = AssetDatabase.FindAssets("", new string[] { gameModeFolder });

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
				Debug.Log("Discovered new GameMode class '" + type.Name + "'. Creating a settings asset in " + gameModeFolder);

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
		string path = gameModeFolder + "/" + type.Name + ".asset";
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
