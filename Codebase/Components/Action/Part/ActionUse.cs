using Zios;
using UnityEngine;
[RequireComponent(typeof(Zios.Action))][AddComponentMenu("Zios/Component/Action/Part/Action Use")]
public class ActionUse : ActionPart{
	public override void OnValidate(){
		this.DefaultPriority(10);
		this.DefaultAlias("@Use");
		base.OnValidate();
	}
	public void Start(){
		Events.Add("ActionEnd",this.OnActionEnd);
	}
	public override void Use(){
		if(!this.inUse){
			this.action.ready = true;
			base.Use();
		}
	}
	public void OnActionEnd(){
		this.action.ready = false;
		base.End();
	}
	public override void End(){
		bool canEnd = !this.action.inUse;
		if(this.action.persist){canEnd = false;}
		if(canEnd){
			this.OnActionEnd();
		}
	}
}