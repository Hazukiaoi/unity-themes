using UnityEngine;
using System.Collections;
[RequireComponent(typeof(ColliderController))]
[AddComponentMenu("Zios/Component/Physics/Gravity")]
public class Gravity : MonoBehaviour{
	public Vector3 intensity = new Vector3(0,-9.8f,0);
	public bool disabled;
	public MFloat scale = 1.0f;
	public void Awake(){
		Events.AddGet("IsGravityEnabled",this.OnCheckGravity);
		Events.Add("SetGravityScale",(MethodFloat)this.OnSetGravityScale);
		Events.Add("DisableGravity",this.OnDisableGravity);
		Events.Add("EnableGravity",this.OnEnableGravity);
	}
	public void FixedUpdate(){
		bool blocked = ColliderController.Get(this.gameObject).blocked["down"];
		if(!this.disabled && !blocked){
			Vector3 amount = (this.intensity*this.scale)* Time.fixedDeltaTime;
			this.gameObject.Call("AddForce",amount);
		}
	}
	public object OnCheckGravity(){
		return !this.disabled;
	}
	public void OnDisableGravity(){
		this.disabled = true;
	}
	public void OnEnableGravity(){
		this.disabled = false;
	}
	public void OnSetGravityScale(float scale){
		this.scale.Set(scale);
		if(scale == -1){this.scale.Revert();}
	}
}