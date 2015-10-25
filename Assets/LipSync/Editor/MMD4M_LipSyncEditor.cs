using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MMD4M_LipSync))]
public sealed class MMD4M_LipSyncEditor : LipSyncCoreEditor
{
	private bool morphFoldOut_        = false;
	private bool morphNamesFoldOut_   = false;


	UnityChanLipSync lipSync
	{
		get { return target as UnityChanLipSync; }
	}


	public override void OnInspectorGUI()
	{
		DrawLipSyncCoreGUI();
	}


	protected override void DrawMorphSettingGUI()
	{
		morphFoldOut_ = EditorGUILayout.Foldout(morphFoldOut_, "Morph Parameters");
		if (morphFoldOut_) {
			EditorGUI.indentLevel++;

			// Morph Speed
			float morphSpeed = EditorGUILayout.FloatField("Morph Speed", lipSync.morphSpeed);
			if (morphSpeed != lipSync.morphSpeed) {
				lipSync.morphSpeed = morphSpeed;
				if (Application.isPlaying) {
					foreach (var morph in lipSync.morphHelpers) {
						morph.morphSpeed = morphSpeed;
					}
				}
			}

			// Max Morph Weight
			float maxMorphWeight = EditorGUILayout.FloatField("Max Morph Weight", lipSync.maxMorphWeight);
			if (maxMorphWeight != lipSync.maxMorphWeight) lipSync.maxMorphWeight = maxMorphWeight;

			// Morph Weight Damping Rate
			float morphDampingRate = EditorGUILayout.FloatField("Morph Damping Rate", lipSync.morphDampingRate);
			if (morphDampingRate != lipSync.morphDampingRate) lipSync.morphDampingRate = morphDampingRate;

			morphNamesFoldOut_ = EditorGUILayout.Foldout(morphNamesFoldOut_, "Morph Names for Each Vowel");
			if (morphNamesFoldOut_) {
				EditorGUI.indentLevel++;

				// a
				string aMorphName = EditorGUILayout.TextField("A", lipSync.morphNames[0]);
				if (aMorphName != lipSync.morphNames[0]) lipSync.morphNames[0] = aMorphName;

				// i
				string iMorphName = EditorGUILayout.TextField("I", lipSync.morphNames[1]);
				if (iMorphName != lipSync.morphNames[1]) lipSync.morphNames[1] = iMorphName;

				// u
				string uMorphName = EditorGUILayout.TextField("U", lipSync.morphNames[2]);
				if (uMorphName != lipSync.morphNames[2]) lipSync.morphNames[2] = uMorphName;

				// e
				string eMorphName = EditorGUILayout.TextField("E", lipSync.morphNames[3]);
				if (eMorphName != lipSync.morphNames[3]) lipSync.morphNames[3] = eMorphName;

				// o
				string oMorphName = EditorGUILayout.TextField("O", lipSync.morphNames[4]);
				if (oMorphName != lipSync.morphNames[4]) lipSync.morphNames[4] = oMorphName;

				EditorGUI.indentLevel--;
			}

			EditorGUI.indentLevel--;
		}
	}
}
