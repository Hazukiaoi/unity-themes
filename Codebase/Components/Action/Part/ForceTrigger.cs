using Zios;
using UnityEngine;
public enum ForceType{Absolute,Relative}
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Force Trigger")]
public class ForceTrigger : ActionPart{
	public ForceType type;
	public Vector3 amount;
	public override void OnValidate(){
		this.DefaultPriority(15);
		base.OnValidate();
	}
	public override void Use(){
		base.Use();
		Vector3 amount = this.action.intensity * this.amount;
		this.action.owner.Call("AddForce",amount);
	}
}
