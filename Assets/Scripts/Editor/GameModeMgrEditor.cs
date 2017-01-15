using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(GameModeMgr), true)]
public class GameModeMgrEditor : Editor
{
	public const string gameModeFolder = "Assets/GameModes";
	
	private GameModeBase newGameMode;

	void OnEnable()
	{
		SerializedProperty mgrGameModesArray = serializedObject.FindProperty("allGameModes");
		mgrGameModesArray.ClearArray();
		mgrGameModesArray.arraySize = 0;


		//Find the GUID of all assets in the gameModeFolder
		string[] allGameModesGUIDs = AssetDatabase.FindAssets("", new string[] { gameModeFolder });
		
		foreach(string GUID in allGameModesGUIDs)
		{
			//Get the asset path from the GUID
			string assetPath = AssetDatabase.GUIDToAssetPath(GUID);

			//Load the asset
			GameModeBase gameMode = AssetDatabase.LoadAssetAtPath<GameModeBase>(assetPath);

			mgrGameModesArray.arraySize++;
			mgrGameModesArray.GetArrayElementAtIndex(mgrGameModesArray.arraySize - 1).objectReferenceValue = gameMode;
		}

		serializedObject.ApplyModifiedPropertiesWithoutUndo();
	}


	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		if(newGameMode == null)
		{
			newGameMode = GameModeMgr.activeGameMode;
		}

		newGameMode = (GameModeBase)EditorGUILayout.ObjectField("Set GameMode to", newGameMode, typeof(GameModeBase), false);

		if(GUILayout.Button("Set GameMode"))
		{
			GameModeMgr.SetActiveGameMode(newGameMode);
		}
	}


}
