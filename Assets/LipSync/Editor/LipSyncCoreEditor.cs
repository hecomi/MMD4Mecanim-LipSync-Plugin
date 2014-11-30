using UnityEngine;
using UnityEditor;

public abstract class LipSyncCoreEditor : Editor
{
	#region [ Fold Out Flags ]
	private bool micFoldOut_          = true;
	private bool playFoldOut_         = true;
	private bool lpcFoldOut_          = true;
	private bool lpcVowelFreqFoldOut_ = true;
	private bool calibrationFoldOut_  = true;
	private bool otherParamsFoldOut_  = true;
	#endregion

	#region [ Callibration for Vowel by Mic ]
	private GameObject micHelper_;
	#endregion

    #region [ Abstract Members ]
    protected abstract void DrawMorphSettingGUI();
    protected virtual void DrawSetLpcDefaultParamGUI()
    {
    }
    #endregion

    #region [ Member Functions ]
    LipSyncCore lipSync
    {
        get { return target as LipSyncCore; }
    }


	protected void DrawLipSyncCoreGUI()
	{
        DrawMicGUI();
        DrawPlayGUI();
        DrawCalibrationGUI();
        DrawLPCParamsGUI();
        DrawMorphSettingGUI();
        DrawOtherParamsGUI();
	}


    protected void DrawMicGUI()
    {
		micFoldOut_ = EditorGUILayout.Foldout(micFoldOut_, "Microphone");
		if (micFoldOut_) {
			EditorGUI.indentLevel++;
			var useMic = EditorGUILayout.Toggle("Use Mic", lipSync.useMic);
			if (useMic != lipSync.useMic) lipSync.useMic = useMic;
			EditorGUILayout.Separator();
			EditorGUI.indentLevel--;
		}
    }


    protected void DrawPlayGUI()
    {
        playFoldOut_ = EditorGUILayout.Foldout(playFoldOut_, "Play Voice Sound");
		if (playFoldOut_) {
			EditorGUI.indentLevel++;

			// 3D Sound setting
			// --------------------------------------------------------------------------------
            var is3dSound = EditorGUILayout.Toggle("3D Sound", lipSync.is3dSound);
            if (is3dSound != lipSync.is3dSound) lipSync.is3dSound = is3dSound;
		    if (lipSync.is3dSound && LipSyncCore.isUseProFunction) {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "※ Audio Clip については各々の 3D Sound 設定が優先されます",
                    MessageType.None);
                EditorGUI.indentLevel--;
		    }
            EditorGUILayout.Separator();

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
    }


    protected void DrawCalibrationGUI()
    {
        calibrationFoldOut_ = EditorGUILayout.Foldout(calibrationFoldOut_, "Callibration");
		if (calibrationFoldOut_) {
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
				if (aClip != null) lipSync.Callibration(aClip, LipSyncCore.Vowel.A);
				if (iClip != null) lipSync.Callibration(iClip, LipSyncCore.Vowel.I);
				if (uClip != null) lipSync.Callibration(uClip, LipSyncCore.Vowel.U);
				if (eClip != null) lipSync.Callibration(eClip, LipSyncCore.Vowel.E);
				if (oClip != null) lipSync.Callibration(oClip, LipSyncCore.Vowel.O);
			}
			EditorGUILayout.Separator();

			EditorGUI.indentLevel--;
		}
    }


    protected void DrawLPCParamsGUI()
    {
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
					if (GUILayout.Button("Use MeiChan's default parameters", EditorStyles.miniButton)) {
						lipSync.lpcOrder  = LipSyncCore.meiLpcOrder;
						lipSync.sampleNum = LipSyncCore.meiSampleNum;
						lipSync.aCenterF1 = LipSyncCore.aCenterMeiF1;
						lipSync.aCenterF2 = LipSyncCore.aCenterMeiF2;
						lipSync.iCenterF1 = LipSyncCore.iCenterMeiF1;
						lipSync.iCenterF2 = LipSyncCore.iCenterMeiF2;
						lipSync.uCenterF1 = LipSyncCore.uCenterMeiF1;
						lipSync.uCenterF2 = LipSyncCore.uCenterMeiF2;
						lipSync.eCenterF1 = LipSyncCore.eCenterMeiF1;
						lipSync.eCenterF2 = LipSyncCore.eCenterMeiF2;
						lipSync.oCenterF1 = LipSyncCore.oCenterMeiF1;
						lipSync.oCenterF2 = LipSyncCore.oCenterMeiF2;
					}
                    DrawSetLpcDefaultParamGUI();
				} EditorGUILayout.EndHorizontal();
				EditorGUILayout.Separator();

				EditorGUI.indentLevel--;
			}

			EditorGUI.indentLevel--;
		}
    }

    protected void DrawOtherParamsGUI()
    {
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
