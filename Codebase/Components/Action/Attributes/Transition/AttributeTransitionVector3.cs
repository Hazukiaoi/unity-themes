using UnityEngine;
namespace Zios.Actions.TransitionComponents{
	using Attributes;
	[AddComponentMenu("Zios/Component/Action/Attribute/Transition/Transition Vector3")]
	public class AttributeTransitionVector3 : StateMonoBehaviour{
		public AttributeVector3 target = Vector3.zero;
		public AttributeVector3 goal = Vector3.zero;
		public Transition transitionX;
		public Transition transitionY;
		public Transition transitionZ;
		public override void Awake(){
			base.Awake();
			this.transitionX.Setup("X",this);
			this.transitionX.Setup("Y",this);
			this.transitionX.Setup("Z",this);
			this.target.Setup("Current",this);
			this.goal.Setup("Goal",this);
		}
		public override void Use(){
			var current = Vector3.zero;
			current.x = this.transitionX.Step(this.target.x,this.goal.x);
			current.y = this.transitionY.Step(this.target.y,this.goal.y);
			current.z = this.transitionZ.Step(this.target.z,this.goal.z);
			this.target.Set(current);
			base.Use();
		}
	}
}