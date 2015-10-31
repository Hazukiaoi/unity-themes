using Zios;
using System;
using UnityEngine;
using System.Collections.Generic;
namespace Zios{
	[Serializable]
	public class AttributeGameObject : Attribute<GameObject,AttributeGameObject,AttributeGameObjectData>{
		public static string[] specialList = new string[]{"Copy","Parent"};
		public AttributeGameObject() : this(default(GameObject)){}
		public AttributeGameObject(GameObject value){this.delayedValue = value;}
		public static implicit operator AttributeGameObject(GameObject current){return new AttributeGameObject(current);}
		public static implicit operator GameObject(AttributeGameObject current){return current.Get();}
		public override GameObject Get(){
			if(this.getMethod != null){return this.getMethod();}
			AttributeGameObjectData data = this.GetFirstRaw();
			if(data.IsNull()){return null;}
			if(this.usage == AttributeUsage.Shaped && data.reference.IsNull()){
				return data.target.Get();
			}
			return data.Get();
		}
		public override void Setup(string path,Component component){
			base.Setup(path,component);
			this.canDirect = this.canFormula = this.canAdvanced = false;
			this.canGroup = true;
			this.usage = AttributeUsage.Shaped;
		}
		public void SetFallback(string target){
			foreach(var data in this.data){
				data.target.SetFallback(target);
			}
		}
	}
}