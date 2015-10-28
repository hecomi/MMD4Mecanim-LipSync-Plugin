using UnityEngine;

[RequireComponent( typeof(UnityChanMouseMorph) )]
public class UnityChanLipSync : LipSyncCore
{
	// Typical lpc parameter and formant frequencies for each vowel (for unitychan talking)
	public const int   unityChanLpcOrder  = 32;
	public const int   unityChanSampleNum = 376;
	public const float aCenterUnityChanF1 = 1205;
	public const float aCenterUnityChanF2 = 1840;
	public const float iCenterUnityChanF1 = 792;
	public const float iCenterUnityChanF2 = 3296;
	public const float uCenterUnityChanF1 = 516;
	public const float uCenterUnityChanF2 = 2067;
	public const float eCenterUnityChanF1 = 820;
	public const float eCenterUnityChanF2 = 2684;
	public const float oCenterUnityChanF1 = 885;
	public const float oCenterUnityChanF2 = 1796;

	#if UNITY_EDITOR
	[HideInInspector] public bool morphFoldOut      = false;
	[HideInInspector] public bool morphNamesFoldOut = true;
	#endif

	protected UnityChanLipSyncMorphHelper[] morphs_ = null;
	public UnityChanLipSyncMorphHelper[] morphHelpers
	{
		get { return morphs_; }
	}

	protected override void Initialize()
	{
		InitializeMorph();
		OnTalkUpdate += UpdateMouth;
	}

	void InitializeMorph()
	{
		morphs_ = new UnityChanLipSyncMorphHelper[morphNames.Length];
		for (int i = 0; i < morphNames.Length; ++i) {
			morphs_[i] = gameObject.AddComponent<UnityChanLipSyncMorphHelper>();
			morphs_[i].morphSpeed = morphSpeed;
			morphs_[i].morphName  = morphNames[i];
		}
	}

	void UpdateMouth(string vowel, float volume)
	{
		for (int i = 0; i < morphNames.Length; ++i) {
			if (vowel == morphNames[i] && volume > minVolume) {
				float weight = volume / normalizedVolume;
				if (weight > maxMorphWeight) {
					weight = maxMorphWeight;
				}
				morphs_[i].morphWeight = weight;
			} else {
				morphs_[i].morphWeight *= morphDampingRate;
			}
		}

		if (outputter != null) {
			outputter.text = "";
			for (int i = 0; i < morphNames.Length; ++i) {
				outputter.text += "[" + morphNames[i] + "] " + morphs_[i].morphWeight + "\n";
			}
		}
	}
}
