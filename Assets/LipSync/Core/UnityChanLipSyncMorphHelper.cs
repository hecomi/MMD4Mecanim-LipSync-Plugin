using UnityEngine;
using System.Linq;

[RequireComponent (typeof(UnityChanMouseMorph))]
public class UnityChanLipSyncMorphHelper : MonoBehaviour
{
    public string morphName   = "";
    public float  morphSpeed  = 1f;
	public float  morphWeight = 0f;

	private float morphStep {
		get { return Time.deltaTime / morphSpeed; }
	}

	private UnityChanMouseMorph.Morph morph_;

	void Start()
	{
		morph_ = GetComponent<UnityChanMouseMorph>().morphs.First(morph => {
			return morph.name == morphName;
		});
	}

	void Update()
	{
		var deltaMorph = morphWeight - morph_.weight;
		if (Mathf.Abs(deltaMorph) > morphStep) {
			morph_.weight += ( (deltaMorph > 0) ? 1f : -1f ) * morphStep;
		} else {
			morph_.weight += deltaMorph * 0.5f;
		}
	}
}
