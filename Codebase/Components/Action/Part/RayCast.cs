using Zios;
using System;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Raycast")]
public class RayCast : ActionPart{
	public float distance = 1;
	public Color rayColor = Color.blue;
	public Vector3 direction = -Vector3.up;
	public Vector3 offset;
	public Target source = new Target();
	public LayerMask layers = -1;
	public bool relative;
	public override void OnValidate(){
		this.DefaultPriority(5);
		base.OnValidate();
		this.source.Update(this);
	}
	public void Start(){
		this.source.Setup(this);
	}
	public Vector3 GetPosition(){
		if(this.source.Get() != null){
			return this.source.direct.transform.position;
		}
		return Vector3.zero;
	}
	public Vector3 AdjustVector(Vector3 value){
		Vector3 adjusted = value;
		if(this.relative){
			Transform target = this.source.direct.transform;
			adjusted = target.right * value.x;
			adjusted += target.up * value.y;
			adjusted += target.forward * value.z;
		}
		return adjusted;
	}
	public override void Use(){
		float distance = this.distance == -1 ? Mathf.Infinity : this.distance;
		Vector3 direction = this.AdjustVector(this.direction);
		Vector3 position = this.GetPosition() + this.AdjustVector(this.offset);
		bool state = Physics.Raycast(position,direction,distance,this.layers.value);
		this.Toggle(state);
	}
	public void OnDrawGizmosSelected(){
		if(this.source.direct != null){
			Gizmos.color = this.rayColor;
			Vector3 direction = this.AdjustVector(this.direction);
			Vector3 start = this.source.direct.transform.position + this.AdjustVector(this.offset);
			Vector3 end = start + (direction * this.distance);
			Gizmos.DrawLine(start,end);
		}
	}
}