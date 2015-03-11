﻿#pragma warning disable 0649
#pragma warning disable 0414
using Zios;
using UnityEngine;
namespace Zios{
    [AddComponentMenu("Zios/Component/Action/Attribute/Expose/Expose Transform (Position)")]
    public class ExposeTransformPosition : DataMonoBehaviour{
	    [HideInInspector] public AttributeVector3 position = Vector3.zero;
	    public override void Awake(){
		    this.alias = this.alias.SetDefault("Transform");
		    base.Awake();
		    this.position.Setup("Position",this);
		    this.position.getMethod = ()=>this.transform.position;
		    this.position.setMethod = value=>this.transform.position = value;
	    }
    }
}