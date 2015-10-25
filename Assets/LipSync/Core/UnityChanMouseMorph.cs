using UnityEngine;
using System.Collections;
using System.Linq;

public class UnityChanMouseMorph : MonoBehaviour
{
	public const string mouseSkinnedMeshName = "MTH_DEF";

	[System.Serializable]
	public class MorphBlendShape
	{
		public string name;
		public int index;
		[Range(0, 1)]
		public float value;
		public MorphBlendShape(string blendShapeName, float blendShapeValue)
		{
			name  = blendShapeName;
			index = -1;
			value = blendShapeValue;
		}
	}

	[System.Serializable]
	public class Morph
	{
		public string name;
		[Range(0, 1)]
		public float weight;
		public MorphBlendShape[] shapes;
		public Morph(string morphName, MorphBlendShape[] blendShapes)
		{
			name   = morphName;
			weight = 0;
			shapes = blendShapes;
		}
	}

	public Morph[] morphs = new Morph[] {
		new Morph("笑1", new MorphBlendShape[] {
			new MorphBlendShape("blendShape1.MTH_SMILE1", 1f)
		}),
		new Morph("笑2", new MorphBlendShape[] {
			new MorphBlendShape("blendShape1.MTH_SMILE2", 1f)
		}),
		new Morph("驚", new MorphBlendShape[] {
			new MorphBlendShape("blendShape1.MTH_SAP", 1f)
		}),
		new Morph("喜", new MorphBlendShape[] {
			new MorphBlendShape("blendShape1.MTH_CONF", 1f)
		}),
		new Morph("怒1", new MorphBlendShape[] {
			new MorphBlendShape("blendShape1.MTH_ANG1", 1f)
		}),
		new Morph("怒2", new MorphBlendShape[] {
			new MorphBlendShape("blendShape1.MTH_ANG2", 1f)
		}),
		new Morph("あ", new MorphBlendShape[] {
			new MorphBlendShape("blendShape1.MTH_A", 1f)
		}),
		new Morph("い", new MorphBlendShape[] {
			new MorphBlendShape("blendShape1.MTH_I", 1f)
		}),
		new Morph("う", new MorphBlendShape[] {
			new MorphBlendShape("blendShape1.MTH_U", 1f)
		}),
		new Morph("え", new MorphBlendShape[] {
			new MorphBlendShape("blendShape1.MTH_E", 1f)
		}),
		new Morph("お", new MorphBlendShape[] {
			new MorphBlendShape("blendShape1.MTH_O", 1f)
		})
	};

	private SkinnedMeshRenderer skinnedMeshRenderer_;

	void Awake()
	{
		skinnedMeshRenderer_ =
			GetComponentsInChildren<SkinnedMeshRenderer>().First(s => s.name == mouseSkinnedMeshName);
		InitMorphs();
	}

	void LateUpdate()
	{
		UpdateMorphs();
	}

	void InitMorphs()
	{
		var mesh = skinnedMeshRenderer_.sharedMesh;
		foreach (var morph in morphs) {
			for (int i = 0; i < morph.shapes.Length; ++i) {
				for (int j = 0; j < mesh.blendShapeCount; ++j) {
					if (morph.shapes[i].name == mesh.GetBlendShapeName(j)) {
						morph.shapes[i].index = j;
						break;
					}
				}
			}
		}
	}

	void UpdateMorphs()
	{
		foreach (var morph in morphs) {
			foreach (var shape in morph.shapes) {
				if (shape.index == -1) continue;
				var weight = shape.value * morph.weight * 100;
				skinnedMeshRenderer_.SetBlendShapeWeight(shape.index, weight);
			}
		}
	}
}
