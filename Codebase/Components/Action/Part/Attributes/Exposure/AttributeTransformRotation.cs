#pragma warning disable 0649
#pragma warning disable 0414
using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Attribute Transform (Rotation)")]
public class AttributeTransformRotation : AttributeExposer{
	private string alias = "Transform";
	private AttributeVector3 rotation = Vector3.zero;
	public override void Awake(){
		this.rotation.Setup("Rotation",this);
		this.rotation.getMethod = ()=>this.transform.eulerAngles;
		this.rotation.setMethod = value=>this.transform.eulerAngles = value;
	}
}