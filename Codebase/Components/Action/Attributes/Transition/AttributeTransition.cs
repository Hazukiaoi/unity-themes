using UnityEngine;
namespace Zios.Actions.TransitionComponents{
	using Attributes;
	using Events;
	public enum TransitionState{Idle,Acceleration,Travel,Deceleration};
	[AddComponentMenu("")]
	public class AttributeTransition : StateMonoBehaviour{
		[Advanced] public AnimationCurve acceleration = AnimationCurve.EaseInOut(0,0,1,1);
		[Advanced] public AnimationCurve deceleration = AnimationCurve.EaseInOut(0,1,1,0);
		public AttributeFloat transitionSeconds = 1;
		protected float startTime;
		protected float endTime;
		protected float transitionDistance;
		protected float overallDistance;
		protected TransitionState state;
		protected bool finished;
		public override void Awake(){
			base.Awake();
			this.transitionSeconds.Setup("Speed Transition Seconds",this);
			Event.Register(this.alias+"/Start",this.gameObject);
			Event.Register(this.alias+"/End",this.gameObject);
		}
	}
}