using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MMD4M_LipSync))]
public sealed class MMD4M_LipSyncEditor : Editor
{
	#region [ Fold Out Flags ]
	private bool micFoldOut_          = true;
	private bool playFoldOut_         = true;
	private bool lpcFoldOut_          = true;
	private bool lpcVowelFreqFoldOut_ = true;
	private bool callibrationFoldOut_ = true;
	private bool morphFoldOut_        = true;
	private bool morphNamesFoldOut_   = true;
	private bool otherParamsFoldOut_  = true;
	#endregion

	#region [ Callibration for Vowel by Mic ]
	private GameObject micHelper_;
	#endregion


	#region [ Member Functions ]
	public override void OnInspectorGUI()
	{
		// Get target instance
		var lipSync = target as MMD4M_LipSync;

		// Use Mic.
		// --------------------------------------------------------------------------------
		micFoldOut_ = EditorGUILayout.Foldout(micFoldOut_, "Microphone");
		if (micFoldOut_) {
			EditorGUI.indentLevel++;
			var useMic = EditorGUILayout.Toggle("Use Mic", lipSync.useMic);
			if (useMic != lipSync.useMic) lipSync.useMic = useMic;
			EditorGUILayout.Separator();
			EditorGUI.indentLevel--;
		}

		// Play
		// --------------------------------------------------------------------------------
		playFoldOut_ = EditorGUILayout.Foldout(playFoldOut_, "Play Voice Sound");
		if (playFoldOut_) {
			EditorGUI.indentLevel++;

			// From word
			// --------------------------------------------------------------------------------
			// Label
			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.LabelField("Word");
				GUILayout.FlexibleSpace();
			} EditorGUILayout.EndHorizontal();

			// Filed and button
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginHorizontal(); {
				string word = EditorGUILayout.TextField("", lipSync.word);
				if (word != lipSync.word) lipSync.word = word;
				if (GUILayout.Button("Talk", EditorStyles.miniButton)) {
					if (Application.isPlaying) {
						lipSync.Talk(word);
					} else {
						Debug.LogWarning("Not running.");
					}
				}
			} EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel--;

			// From file
			// --------------------------------------------------------------------------------
			// Label
			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.LabelField("Audio File");
				GUILayout.FlexibleSpace();
			} EditorGUILayout.EndHorizontal();

			// Textfield and buttons
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginHorizontal(); {
				string audioPath = EditorGUILayout.TextField("", lipSync.audioPath);
				if (audioPath != lipSync.audioPath) lipSync.audioPath = audioPath;
				if (GUILayout.Button("Select", EditorStyles.miniButtonLeft)) {
					lipSync.audioPath = "file://" + EditorUtility.OpenFilePanel("Select .wav file", "", "wav");
				}
				if (GUILayout.Button("Play", EditorStyles.miniButtonRight)) {
					if (Application.isPlaying) {
						lipSync.Play(audioPath);
					} else {
						Debug.LogWarning("Not running.");
					}
				}
			} EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel--;

			// From audio clip
			// --------------------------------------------------------------------------------
			// Label
			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.LabelField("Audio Clip");
				GUILayout.FlexibleSpace();
			} EditorGUILayout.EndHorizontal();

			// Filed and button
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginHorizontal(); {
				var audioClip = EditorGUILayout.ObjectField("", lipSync.audioClip, typeof(AudioClip), false) as AudioClip;
				if (audioClip != lipSync.audioClip) lipSync.audioClip = audioClip;
				if (GUILayout.Button("Play", EditorStyles.miniButton)) {
					if (Application.isPlaying) {
						if (audioClip != null) {
							lipSync.Play(audioClip);
						}
					} else {
						Debug.LogWarning("Not running.");
					}
				}
			} EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel--;

			// Playing Position
			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.LabelField("Playing Position");
				GUILayout.FlexibleSpace();
			} EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel++;
			var playingPosition = EditorGUILayout.ObjectField("", lipSync.playingPosition, typeof(GameObject), true) as GameObject;
			if (playingPosition != lipSync.playingPosition) lipSync.playingPosition = playingPosition;
			EditorGUI.indentLevel--;

			EditorGUILayout.Separator();

			// Stop
			// --------------------------------------------------------------------------------
			if ( GUILayout.Button("Stop", EditorStyles.miniButton) ) {
				lipSync.Stop();
			}
			EditorGUILayout.Separator();

			EditorGUI.indentLevel--;
		}

		// Callibration
		// --------------------------------------------------------------------------------
		callibrationFoldOut_ = EditorGUILayout.Foldout(callibrationFoldOut_, "Callibration");
		if (callibrationFoldOut_) {
			EditorGUI.indentLevel++;

			// Callibartion by each vowel data
			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.LabelField("Each Vowel Wave Data");
				GUILayout.FlexibleSpace();
			} EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel++;

			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("A", "", GUILayout.MinWidth(20), GUILayout.MaxWidth(20));
				var aClip = EditorGUILayout.ObjectField("", lipSync.aClip, typeof(AudioClip), true) as AudioClip;
				if (aClip != lipSync.aClip) lipSync.aClip = aClip;
				CreateRecordAndStopButton(ref lipSync.aClip, "Vowel A");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("I", "", GUILayout.MinWidth(20), GUILayout.MaxWidth(20));
				var iClip = EditorGUILayout.ObjectField("", lipSync.iClip, typeof(AudioClip), true) as AudioClip;
				if (iClip != lipSync.aClip) lipSync.iClip = iClip;
				CreateRecordAndStopButton(ref lipSync.iClip, "Vowel I");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("U", "", GUILayout.MinWidth(20), GUILayout.MaxWidth(20));
				var uClip = EditorGUILayout.ObjectField("", lipSync.uClip, typeof(AudioClip), true) as AudioClip;
				if (uClip != lipSync.uClip) lipSync.uClip = uClip;
				CreateRecordAndStopButton(ref lipSync.uClip, "Vowel U");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("E", "", GUILayout.MinWidth(20), GUILayout.MaxWidth(20));
				var eClip = EditorGUILayout.ObjectField("", lipSync.eClip, typeof(AudioClip), true) as AudioClip;
				if (eClip != lipSync.eClip) lipSync.eClip = eClip;
				CreateRecordAndStopButton(ref lipSync.eClip, "Vowel E");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("O", "", GUILayout.MinWidth(20), GUILayout.MaxWidth(20));
				var oClip = EditorGUILayout.ObjectField("", lipSync.oClip, typeof(AudioClip), true) as AudioClip;
				if (oClip != lipSync.oClip) lipSync.oClip = oClip;
				CreateRecordAndStopButton(ref lipSync.oClip, "Vowel O");
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel--;

			EditorGUILayout.Separator();
			if ( GUILayout.Button("Callibration", EditorStyles.miniButton) ) {
				if (aClip != null) lipSync.Callibration(aClip, MMD4M_LipSync.Vowel.A);
				if (iClip != null) lipSync.Callibration(iClip, MMD4M_LipSync.Vowel.I);
				if (uClip != null) lipSync.Callibration(uClip, MMD4M_LipSync.Vowel.U);
				if (eClip != null) lipSync.Callibration(eClip, MMD4M_LipSync.Vowel.E);
				if (oClip != null) lipSync.Callibration(oClip, MMD4M_LipSync.Vowel.O);
			}
			EditorGUILayout.Separator();

			EditorGUI.indentLevel--;
		}

		// LPC Parameters
		// --------------------------------------------------------------------------------
		lpcFoldOut_ = EditorGUILayout.Foldout(lpcFoldOut_, "LPC Parameters");
		if (lpcFoldOut_) {
			EditorGUI.indentLevel++;

			// LPC Order
			int lpcOrder = EditorGUILayout.IntField("LPC Order", lipSync.lpcOrder);
			if (lpcOrder != lipSync.lpcOrder) lipSync.lpcOrder = lpcOrder;

			// Sample Number
			int sampleNum = EditorGUILayout.IntField("Sample Num", lipSync.sampleNum);
			if (sampleNum != lipSync.sampleNum) lipSync.sampleNum = sampleNum;

			// Typical frequencies for vowel
			lpcVowelFreqFoldOut_ = EditorGUILayout.Foldout(lpcVowelFreqFoldOut_, "Typical Formant Frequencies for Each Vowel");
			if (lpcVowelFreqFoldOut_) {
				EditorGUI.indentLevel++;

				// a
				float aCenterF1 = EditorGUILayout.FloatField("A-F1", lipSync.aCenterF1);
				if (aCenterF1 != lipSync.aCenterF1) lipSync.aCenterF1 = aCenterF1;
				float aCenterF2 = EditorGUILayout.FloatField("A-F2", lipSync.aCenterF2);
				if (aCenterF2 != lipSync.aCenterF2) lipSync.aCenterF2 = aCenterF2;
				EditorGUILayout.Separator();

				// i
				float iCenterF1 = EditorGUILayout.FloatField("I-F1", lipSync.iCenterF1);
				if (iCenterF1 != lipSync.iCenterF1) lipSync.iCenterF1 = iCenterF1;
				float iCenterF2 = EditorGUILayout.FloatField("I-F2", lipSync.iCenterF2);
				if (iCenterF2 != lipSync.iCenterF2) lipSync.iCenterF2 = iCenterF2;
				EditorGUILayout.Separator();

				// u
				float uCenterF1 = EditorGUILayout.FloatField("U-F1", lipSync.uCenterF1);
				if (uCenterF1 != lipSync.uCenterF1) lipSync.uCenterF1 = uCenterF1;
				float uCenterF2 = EditorGUILayout.FloatField("U-F2", lipSync.uCenterF2);
				if (uCenterF2 != lipSync.uCenterF2) lipSync.uCenterF2 = uCenterF2;
				EditorGUILayout.Separator();

				// e
				float eCenterF1 = EditorGUILayout.FloatField("E-F1", lipSync.eCenterF1);
				if (eCenterF1 != lipSync.eCenterF1) lipSync.eCenterF1 = eCenterF1;
				float eCenterF2 = EditorGUILayout.FloatField("E-F2", lipSync.eCenterF2);
				if (eCenterF2 != lipSync.eCenterF2) lipSync.eCenterF2 = eCenterF2;
				EditorGUILayout.Separator();

				// o
				float oCenterF1 = EditorGUILayout.FloatField("O-F1", lipSync.oCenterF1);
				if (oCenterF1 != lipSync.oCenterF1) lipSync.oCenterF1 = oCenterF1;
				float oCenterF2 = EditorGUILayout.FloatField("O-F2", lipSync.oCenterF2);
				if (oCenterF2 != lipSync.oCenterF2) lipSync.oCenterF2 = oCenterF2;
				EditorGUILayout.Separator();

				EditorGUILayout.BeginHorizontal(); {
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Use default", EditorStyles.miniButton)) {
						lipSync.aCenterF1 = MMD4M_LipSync.aCenterMeiF1;
						lipSync.aCenterF2 = MMD4M_LipSync.aCenterMeiF2;
						lipSync.iCenterF1 = MMD4M_LipSync.iCenterMeiF1;
						lipSync.iCenterF2 = MMD4M_LipSync.iCenterMeiF2;
						lipSync.uCenterF1 = MMD4M_LipSync.uCenterMeiF1;
						lipSync.uCenterF2 = MMD4M_LipSync.uCenterMeiF2;
						lipSync.eCenterF1 = MMD4M_LipSync.eCenterMeiF1;
						lipSync.eCenterF2 = MMD4M_LipSync.eCenterMeiF2;
						lipSync.oCenterF1 = MMD4M_LipSync.oCenterMeiF1;
						lipSync.oCenterF2 = MMD4M_LipSync.oCenterMeiF2;
					}
				} EditorGUILayout.EndHorizontal();
				EditorGUILayout.Separator();

				EditorGUI.indentLevel--;
			}

			EditorGUI.indentLevel--;
		}

		// Morph names fold out
		morphFoldOut_ = EditorGUILayout.Foldout(morphFoldOut_, "Morph Parameters");
		if (morphFoldOut_) {
			EditorGUI.indentLevel++;

			// Morph Speed
			float morphSpeed = EditorGUILayout.FloatField("Morph Speed", lipSync.morphSpeed);
			if (morphSpeed != lipSync.morphSpeed) {
				lipSync.morphSpeed = morphSpeed;
				if (Application.isPlaying) {
					foreach (MMD4MecanimMorphHelper morph in lipSync.morphHelpers) {
						morph.morphSpeed = morphSpeed;
					}
				}
			}

			// Max Morph Weight
			float maxMorphWeight = EditorGUILayout.FloatField("Max Morph Weight", lipSync.maxMorphWeight);
			if (maxMorphWeight != lipSync.maxMorphWeight) lipSync.maxMorphWeight = maxMorphWeight;

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

		// Other Params
		otherParamsFoldOut_ = EditorGUILayout.Foldout(otherParamsFoldOut_, "Other Parameters");
		if (otherParamsFoldOut_) {
			EditorGUI.indentLevel++;

			// Minimum Volume
			float minVolume = EditorGUILayout.FloatField("Minimum Volume", lipSync.minVolume);
			if (minVolume != lipSync.minVolume) lipSync.minVolume = minVolume;

			// Normalized Volume
			float normalizedVolume = EditorGUILayout.FloatField("Normalized Volume", lipSync.normalizedVolume);
			if (normalizedVolume != lipSync.normalizedVolume) lipSync.normalizedVolume = normalizedVolume;

			// Delay Time
			float delayTime = EditorGUILayout.FloatField("Lip Sync Delay Time", lipSync.delayTime);
			if (delayTime != lipSync.delayTime) lipSync.delayTime = delayTime;

			// Outputter
			var outputter = EditorGUILayout.ObjectField("Result Outputter", lipSync.outputter, typeof(GUIText), true) as GUIText;
			if (outputter != lipSync.outputter) lipSync.outputter = outputter;

			EditorGUI.indentLevel--;
		}
	}

	void CreateRecordAndStopButton(ref AudioClip clip, string clipName)
	{
		var lipSync = target as MMD4M_LipSync;
		if (GUILayout.Button("Record", EditorStyles.miniButtonLeft)) {
			if (!micHelper_) {
				if (lipSync.useMic) {
					lipSync.useMic = false;
					lipSync.Stop();
				}

				// Create mic helper and start recording
				micHelper_ = new GameObject();
				micHelper_.name = "Callibration Mic Helper";
				var mic = micHelper_.AddComponent<MicHandler>();
				mic.Initialize(lipSync.sampleNum);
				mic.Record();
				Debug.Log("=== START RECORDING " + clipName + " ====");
			} else {
				Debug.LogWarning("Already recording!");
			}
		}
		if (GUILayout.Button("Stop", EditorStyles.miniButtonRight)) {
			if (micHelper_) {
				// Copy mic.clip to new AudioClip
				var mic = micHelper_.AddComponent<MicHandler>();
				var data = new float[mic.clip.samples];
				mic.clip.GetData(data, 0);
				clip = AudioClip.Create(clipName, mic.clip.samples, 1, mic.clip.frequency, true, false);
				clip.SetData(data, 0);

				// Destroy mic helper
				mic.Stop();
				Destroy(micHelper_);
				Debug.Log("=== STOP RECORDING " + clipName + " ====");
			} else {
				Debug.LogWarning("Not recording!");
			}
		}
	}
	#endregion
}
