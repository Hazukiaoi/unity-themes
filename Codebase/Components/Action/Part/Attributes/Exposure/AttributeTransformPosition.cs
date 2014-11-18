#pragma warning disable 0649
#pragma warning disable 0414
using Zios;
using UnityEngine;
[AddComponentMenu("Zios/Component/Action/Part/Attribute Transform (Position)")]
public class AttributeTransformPosition : AttributeExposer{
	private string alias = "Transform";
	private AttributeVector3 position = Vector3.zero;
	public override void Awake(){
		this.position.Setup("Position",this);
		this.position.getMethod = ()=>this.transform.position;
		this.position.setMethod = value=>this.transform.position = value;
	}
}