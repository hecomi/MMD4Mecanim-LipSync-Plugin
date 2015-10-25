using UnityEngine;
using System.Collections;

public class MMD4M_LipSync : LipSyncCore
{
	private MMD4MecanimMorphHelper[] morphs_ = null;
	public  MMD4MecanimMorphHelper[] morphHelpers  {
		get { return morphs_; }
	}

	protected override void Initialize()
	{
		InitializeMorph();
		OnTalkUpdate += UpdateMouth;
	}

	void InitializeMorph()
	{
		morphs_ = new MMD4MecanimMorphHelper[morphNames.Length];
		for (int i = 0; i < morphNames.Length; ++i) {
			morphs_[i] = gameObject.AddComponent<MMD4MecanimMorphHelper>();
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
