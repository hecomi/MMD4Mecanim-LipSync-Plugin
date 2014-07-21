using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent (typeof(UnityChanMorph))]
public class UnityChanLipSyncMorphHelper : MonoBehaviour
{
	public string morphName;
	public float morphSpeed = 0.1f;
	public float morphWeight = 0.0f;

	private float morphStep {
		get { return Time.deltaTime / morphSpeed; }
	}

	private UnityChanMorph.Morph morph_;

	void Start()
	{
		morph_ = GetComponent<UnityChanMorph>().morphs.First(morph => { 
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
