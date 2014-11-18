#pragma warning disable 0649
#pragma warning disable 0414
using Zios;
using System.Collections;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Attribute Transform (Scale)")]
public class AttributeTransformScale : MonoBehaviour{
	private string alias = "Transform";
	private AttributeVector3 scale = Vector3.zero;
	public void Reset(){this.Awake();}
	public void OnApplicationQuit(){this.Awake();}
	public void Awake(){
		this.scale.Setup("Scale",this);
		this.scale.getMethod = ()=>this.transform.localScale;
		this.scale.setMethod = value=>this.transform.localScale = value;
	}
}