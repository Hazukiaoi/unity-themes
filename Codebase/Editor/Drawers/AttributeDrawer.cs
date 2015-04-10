﻿using Zios;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
using UnityObject = UnityEngine.Object;
namespace Zios{
	[CustomPropertyDrawer(typeof(Attribute),true)]
	public class AttributeDrawer : PropertyDrawer{
		public IAttributeAccess access;
		public new Attribute attribute;
		public float overallHeight;
		public bool isPrefab;
		public float GetBaseHeight(SerializedProperty property,GUIContent label){
			return base.GetPropertyHeight(property,label);
		}
		public override float GetPropertyHeight(SerializedProperty property,GUIContent label){
			if(this.overallHeight == 0){this.overallHeight = base.GetPropertyHeight(property,label);}
			return this.overallHeight;
		}
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			if(!Event.current.IsUseful()){return;}
			if(this.attribute == null){
				this.attribute = property.GetObject<Attribute>();
				MonoBehaviour script = (MonoBehaviour)property.serializedObject.targetObject;
				this.isPrefab = script.IsPrefab();
			}
			if(!this.attribute.showInEditor){
				this.overallHeight = -2;
				return;
			}
			if(this.isPrefab){
				/*this.overallHeight = EditorGUIUtility.singleLineHeight;
				//base.OnGUI(area,property,label);
				Type utility = Utility.GetEditorType("ScriptAttributeUtility");
				Type utility = Utility.GetEditorType("ScriptAttributeUtility");
				SerializedProperty againstProperty = this.property;
				var handler = utility.CallMethod("GetHandler",againstProperty.AsBoxedArray());
				Debug.Log(handler.GetVariable<PropertyDrawer>("m_PropertyDrawer"));
				utility.GetVariable<Stack<PropertyDrawer>>("s_DrawerStack").Pop();
				base.OnGUI(area,property,label);
				utility.GetVariable<Stack<PropertyDrawer>>("s_DrawerStack").Push(this);*/
				return;
			}
			if(!area.InspectorValid()){return;}
			this.overallHeight = this.GetBaseHeight(property,label);
			if(!Attribute.ready && AttributeManager.safe){
				EditorGUI.ProgressBar(area,AttributeManager.percentLoaded,"Updating");
				//Utility.SetDirty(property.serializedObject.targetObject);
				return;
			}
			if(this.access == null){
				if(this.attribute is AttributeFloat){this.access = new AttributeAccess<float,AttributeFloat,AttributeFloatData>();}
				if(this.attribute is AttributeInt){this.access = new AttributeAccess<int,AttributeInt,AttributeIntData>();}
				if(this.attribute is AttributeString){this.access = new AttributeAccess<string,AttributeString,AttributeStringData>();}
				//if(this.attribute is AttributeBool){this.access = new AttributeAccess<bool,AttributeBool,AttributeBoolData>();}
				if(this.attribute is AttributeVector3){this.access = new AttributeAccess<Vector3,AttributeVector3,AttributeVector3Data>();}
				if(this.attribute is AttributeGameObject){this.access = new AttributeAccess<GameObject,AttributeGameObject,AttributeGameObjectData>();}
			}
			if(this.access != null){
				this.access.Setup(this,area,property,label);
			}
		}
	}
	public interface IAttributeAccess{
		void Setup(AttributeDrawer drawer,Rect area,SerializedProperty property,GUIContent label);
	}
	public class AttributeAccess<BaseType,AttributeType,DataType> : IAttributeAccess
		where AttributeType : Attribute<BaseType,AttributeType,DataType>,new()
		where DataType : AttributeData<BaseType,AttributeType,DataType>,new(){
		public Attribute attribute;
		public AttributeData[] activeDataset;
		public AttributeType attributeCast;
		public AttributeDrawer drawer;
		public SerializedProperty property;
		public GUIContent label;
		public Rect fullRect;
		public Rect labelRect;
		public Rect valueRect;
		public Rect iconRect;
		public bool contextOpen;
		public bool dirty;
		public GUISkin skin;
		public List<string> operatorOverride;
		public List<string> specialOverride;
		public Dictionary<AttributeData,bool> targetMode = new Dictionary<AttributeData,bool>();
		public virtual void Setup(AttributeDrawer drawer,Rect area,SerializedProperty property,GUIContent label){
			property.serializedObject.Update();
			if(skin == null){
				string skinName = EditorGUIUtility.isProSkin ? "Dark" : "Light";
				this.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skinName + ".guiskin");
				this.attribute = property.GetObject<Attribute>();
				this.attributeCast = (AttributeType)this.attribute;
			}
			this.activeDataset = this.attribute.info.data;
			GUI.skin = this.skin;
			this.drawer = drawer;
			this.property = property;
			this.label = label;
			Rect fullRect = area.SetHeight(EditorGUIUtility.singleLineHeight);
			this.SetupAreas(fullRect);
			List<UnityObject> sources = new List<UnityObject>(){property.serializedObject.targetObject};
			foreach(var data in this.attribute.data){
				if(!data.IsNull()){sources.Add(data);}
			}
			GUI.changed = false;
			Undo.RecordObjects(sources.ToArray(),"Attribute Changes");
			EditorGUI.BeginProperty(area,label,property);
			this.Draw();
			EditorGUI.EndProperty();
			if(GUI.changed || this.dirty){
				property.serializedObject.targetObject.CallMethod("OnValidate");
				property.serializedObject.ApplyModifiedProperties();
				if(this.dirty){
					Utility.RepaintInspectors();
					this.dirty = false;
				}
			}
		}
		public void SetupAreas(Rect area){
			this.fullRect = area;
			this.iconRect = this.fullRect.SetSize(14,14);
			this.labelRect = this.fullRect.SetWidth(EditorGUIUtility.labelWidth);
			this.valueRect = this.fullRect.Add(this.labelRect.width,0,-labelRect.width,0);
		}
		public virtual void Draw(){
			AttributeData firstData = this.attributeCast.GetFirst();
			if(firstData.IsNull()){return;}
			SerializedObject firstProperty = new SerializedObject(firstData);
			this.DrawContext(firstData);
			if(this.attribute.info.mode == AttributeMode.Normal){
				if(firstData.usage == AttributeUsage.Direct){
					this.DrawDirect(this.fullRect,this.valueRect,firstData,this.label);
				}
				if(firstData.usage == AttributeUsage.Shaped){
					GUI.Box(this.iconRect,"",GUI.skin.GetStyle("IconShaped"));
					this.labelRect = this.labelRect.AddX(16);
					this.DrawShaped(this.valueRect,firstProperty,this.label,true);
				}
			}
			if(this.attribute.info.mode == AttributeMode.Linked){
				this.attributeCast.usage = AttributeUsage.Shaped;
				GUI.Box(this.iconRect,"",GUI.skin.GetStyle("IconLinked"));
				this.labelRect = this.labelRect.AddX(16);
				this.DrawShaped(this.valueRect,firstProperty,this.label);
			}
			if(this.attribute.info.mode == AttributeMode.Formula){
				this.DrawFormula(this.label);
			}
		}
		public virtual void DrawDirect(Rect area,Rect extraArea,AttributeData data,GUIContent label,bool? drawSpecial=null,bool? drawOperator=null){
			float labelSize = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = this.labelRect.width;
			bool showSpecial = drawSpecial != null && (EditorPrefs.GetBool(data.path+"Advanced") || data.special != 0);
			if(drawOperator != null){EditorGUIUtility.labelWidth += 71;}
			if(showSpecial){EditorGUIUtility.labelWidth += 91;}
			if(drawOperator != null){
				this.DrawOperator(extraArea,data,(bool)!drawOperator);
				extraArea = extraArea.Add(51,0,-51,0);
			}
			if(showSpecial){
				this.DrawSpecial(extraArea,data);
				area = area.Add(-41,0,41,0);
			}
			if(data is AttributeFloatData){
				AttributeFloatData floatData = (AttributeFloatData)data;
				floatData.value = floatData.value.DrawLabeled(area,label);
			}
			if(data is AttributeIntData){
				AttributeIntData intData = (AttributeIntData)data;
				intData.value = intData.value.DrawLabeledInt(area,label);
			}
			if(data is AttributeStringData){
				AttributeStringData stringData = (AttributeStringData)data;
				stringData.value = stringData.value.DrawLabeled(area,label);
			}
			if(data is AttributeBoolData){
				AttributeBoolData boolData = (AttributeBoolData)data;
				boolData.value = boolData.value.DrawLabeled(area,label);
			}
			if(data is AttributeVector3Data){
				AttributeVector3Data vector3Data = (AttributeVector3Data)data;
				vector3Data.value = vector3Data.value.DrawLabeled(area,label);
			}
			EditorGUIUtility.labelWidth = labelSize;
		}
		public virtual void DrawShaped(Rect area,SerializedObject property,GUIContent label,bool? drawSpecial=null,bool? drawOperator=null){
			label.DrawLabel(labelRect);
			AttributeData data = (AttributeData)property.targetObject;
			Target target = data.target;
			Rect toggleRect = area.SetWidth(16);
			bool toggleActive = this.targetMode.ContainsKey(data) ? this.targetMode[data] : !data.referenceID.IsEmpty();
			this.targetMode[data] = toggleActive.Draw(toggleRect,GUI.skin.GetStyle("CheckmarkToggle"));
			if(toggleActive != this.targetMode[data]){
				if(this.attribute is AttributeGameObject){
					//data.referenceID = toggleActive ? "" : data.referenceID;
					if(!this.targetMode[data]){
						data.referenceID = "";
						data.referencePath = "";
						data.reference = null;
					}
				}
			}
			if(!this.targetMode[data]){
				Rect targetRect = area.Add(18,0,-18,0);
				property.FindProperty("target").Draw(targetRect);
				return;
			}
			List<string> attributeNames = new List<string>();
			List<string> attributeIDs = new List<string>();
			int attributeIndex = -1;
			GameObject targetScope = target.Get();
			if(!targetScope.IsNull() && Attribute.lookup.ContainsKey(targetScope)){
				var lookup = Attribute.lookup[targetScope];
				foreach(var item in lookup){
					if(item.Value.data.Length < 1){continue;}
					if(item.Value.info.dataType != data.GetType()){continue;}
					bool feedback = (item.Value.info.id == this.attribute.info.id || item.Value.data[0].referenceID == this.attribute.info.id);
					if(!feedback){
						attributeNames.Add(item.Value.info.path);
					}
				}
				attributeNames = attributeNames.Order().OrderBy(item=>item.Contains("/")).ToList();
				foreach(string name in attributeNames){
					string id = lookup.Values.ToList().Find(x=>x.info.path == name).info.id;
					attributeIDs.Add(id);
				}
				if(!data.referenceID.IsEmpty()){
					attributeIndex = attributeIDs.IndexOf(data.referenceID);
				}
			}
			if(attributeNames.Count > 0){
				Rect line = area;
				bool showSpecial = drawSpecial != null && (EditorPrefs.GetBool(data.path+"Advanced") || data.special != 0);
				if(attributeIndex == -1){
					string message = data.referenceID.IsEmpty() ? "[Not Set]" : "[Missing] " + data.referencePath;
					attributeIndex = 0;
					attributeNames.Insert(0,message);
					attributeIDs.Insert(0,"0");
				}
				if(drawOperator != null){
					this.DrawOperator(line,data,(bool)!drawOperator);
					line = line.Add(51,0,-51,0);
				}
				if(showSpecial){
					this.DrawSpecial(line,data);
					line = line.Add(51,0,-51,0);
				}
				Rect attributeRect = line.Add(18,0,-18,0);
				int previousIndex = attributeIndex;
				attributeIndex = attributeNames.Draw(attributeRect,attributeIndex);
				string name = attributeNames[attributeIndex];
				string id = attributeIDs[attributeIndex];
				if(attributeIndex != previousIndex){
					data.referencePath = name;
					data.referenceID = id;
					data.reference = Attribute.lookup[targetScope][data.referenceID];
				}
			}
			else{
				Rect warningRect = area.Add(18,0,-18,0);
				string targetName = targetScope == null ? "Target" : targetScope.ToString().Strip("(UnityEngine.GameObject)").Trim();
				string typeName = data.GetVariableType("value").Name.Replace("Single","Float").Replace("Int32","Int");
				string message = "<b>" + targetName.Truncate(16) + "</b> has no <b>"+typeName+"</b> attributes.";
				message.DrawLabel(warningRect,GUI.skin.GetStyle("WarningLabel"));
			}
		}
		public virtual void DrawSpecial(Rect area,AttributeData data){
			Rect specialRect = area.Add(18,0,0,0).SetWidth(50);
			List<string> specialList = this.specialOverride ?? typeof(AttributeType).GetVariable<string[]>("specialList").ToList();
			data.special = specialList.Draw(specialRect,data.special);
		}
		public virtual void DrawOperator(Rect area,AttributeData data,bool disabled=false){
			Rect operatorRect = area.Add(18,0,0,0).SetWidth(50);
			EditorGUIUtility.AddCursorRect(operatorRect,MouseCursor.Arrow);
			GUIStyle style = new GUIStyle(EditorStyles.popup);
			style.alignment = TextAnchor.MiddleRight;
			style.contentOffset = new Vector2(-3,0);
			style.fontStyle = FontStyle.Bold;
			List<string> operatorList = new List<string>();
			if(disabled){
				GUI.enabled = false;
				operatorList.Add("=");
				operatorList.Draw(operatorRect,0,style);
				GUI.enabled = true;
				return;
			}
			operatorList = this.operatorOverride;
			if(operatorList == null){
				var operatorCollection = typeof(AttributeType).GetVariable<Dictionary<Type,string[]>>("operators");
				operatorList = operatorCollection[data.GetType()].ToList();
			}
			int operatorIndex = Mathf.Clamp(data.operation,0,operatorList.Count-1);
			data.operation = operatorList.Draw(operatorRect,operatorIndex,style);
		}
		public virtual void DrawFormula(GUIContent label){
			Rect labelRect = this.labelRect.AddX(12);
			EditorGUIUtility.AddCursorRect(this.fullRect,MouseCursor.ArrowPlus);
			bool formulaExpanded = EditorPrefs.GetBool(this.attribute.info.path+"FormulaExpanded");
			if(this.labelRect.AddX(16).Clicked() || this.valueRect.Clicked()){
				this.dirty = true;
				formulaExpanded = !formulaExpanded;
			}
			formulaExpanded = EditorGUI.Foldout(labelRect,formulaExpanded,label,GUI.skin.GetStyle("IconFormula"));
			EditorPrefs.SetBool(this.attribute.info.path+"FormulaExpanded",formulaExpanded);
			if(formulaExpanded){
				float lineHeight = EditorGUIUtility.singleLineHeight+2;
				this.SetupAreas(this.fullRect.SetX(45).AddWidth(-55));
				for(int index=0;index<this.attribute.data.Length;++index){
					AttributeData currentData = this.attribute.data[index];
					if(currentData == null){continue;}
					this.DrawFormulaRow(currentData,index);
				}
				this.SetupAreas(this.fullRect.AddY(lineHeight));
				this.drawer.overallHeight += lineHeight;
				if(GUI.Button(this.labelRect.SetWidth(100),"Add Attribute")){
					if(this.attribute.GetFormulaTypes().Length > 1){
						this.DrawAddMenu();
						return;
					}
					this.attribute.Add<DataType>();
					this.dirty = true;
				}
			}
			else{
				string message = "[expand for details]";
				message.DrawLabel(this.valueRect,GUI.skin.GetStyle("WarningLabel"));
			}
		}
		public virtual void DrawFormulaRow(AttributeData data,int index){
			float lineHeight = EditorGUIUtility.singleLineHeight+2;
			SerializedObject currentProperty = new SerializedObject(data);
			//GUIContent formulaLabel = new GUIContent(((char)('A'+index)).ToString());
			GUIContent formulaLabel = new GUIContent(" ");
			this.SetupAreas(this.fullRect.AddY(lineHeight));
			this.labelRect = this.labelRect.SetWidth(1);
			this.valueRect = this.fullRect.Add(this.labelRect.width,0,-labelRect.width,0);
			this.drawer.overallHeight += lineHeight;
			bool? operatorState = index == 0 ? (bool?)false : (bool?)true;
			if(data.usage == AttributeUsage.Direct){
				GUI.Box(this.labelRect.AddX(2),"",GUI.skin.GetStyle("IconDirect"));
				this.DrawDirect(this.fullRect,this.valueRect,data,formulaLabel,true,operatorState);
			}
			else if(data.usage == AttributeUsage.Shaped){
				this.DrawShaped(this.valueRect,currentProperty,formulaLabel,true,operatorState);
			}
			this.DrawContext(data,index!=0,false);
		}
		public virtual void DrawAddMenu(){
			this.contextOpen = true;
			GenericMenu menu = new GenericMenu();
			foreach(Type attributeType in this.attribute.GetFormulaTypes()){
				string name = attributeType.Name.Strip("Attribute","Data");
				string set = this.attribute.defaultSet;
				MethodInfo generic = this.attribute.GetType().GetMethod("Add",new Type[]{typeof(int),typeof(string)}).MakeGenericMethod(attributeType);
				MenuFunction method = ()=>{generic.Invoke(this.attribute,new object[]{-1,set});};
				menu.AddItem(new GUIContent(name),false,method);
			}
			menu.ShowAsContext();
		}
		public void SwapType(int index,Type attributeType,string set){
			MethodInfo generic = this.attribute.GetType().GetMethod("Add",new Type[]{typeof(int),typeof(string)}).MakeGenericMethod(attributeType);
			generic.Invoke(this.attribute,new object[]{index,set});
		}
		public virtual void DrawTypeMenu(AttributeData data,GenericMenu menu=null){
			bool openMenu = menu == null;
			menu = menu ?? new GenericMenu();
			Type[] types = this.attribute.GetFormulaTypes();
			if(types.Length > 0){
				int index = this.activeDataset.IndexOf(data);
				foreach(Type attributeType in types){
					Type type = attributeType;
					string name = type.Name.Strip("Attribute","Data");
					string set = this.attribute.defaultSet;
					MenuFunction swapType = ()=>{this.SwapType(index,type,set);};
					menu.AddItem(new GUIContent("Type/"+name),(data.GetType()==type),swapType);
				}
			}
			if(openMenu){menu.ShowAsContext();}
		}
		public virtual void DrawContext(AttributeData data,bool showRemove=false,bool isRoot=true){
			if(this.labelRect.AddWidth(20).Clicked(1)){
				this.contextOpen = true;
				GenericMenu menu = new GenericMenu();
				AttributeMode mode = this.attribute.info.mode;
				AttributeUsage usage = data.usage;
				bool advanced = EditorPrefs.GetBool(data.path+"Advanced");
				MenuFunction toggleAdvanced = ()=>{EditorPrefs.SetBool(data.path+"Advanced",!advanced);};
				MenuFunction removeAttribute = ()=>{this.attribute.Remove(data);};
				MenuFunction modeNormal  = ()=>{this.attribute.info.mode = AttributeMode.Normal;};
				MenuFunction modeLinked  = ()=>{this.attribute.info.mode = AttributeMode.Linked;};
				MenuFunction modeFormula = ()=>{this.attribute.info.mode = AttributeMode.Formula;};
				MenuFunction usageDirect = ()=>{data.usage = AttributeUsage.Direct;};
				MenuFunction usageShaped = ()=>{data.usage = AttributeUsage.Shaped;};
				bool normal = this.attribute.info.mode == AttributeMode.Normal;
				if(this.attribute.locked){
					menu.AddDisabledItem(new GUIContent("Attribute Locked"));
					menu.ShowAsContext();
					return;
				}
				if(isRoot || (mode != AttributeMode.Formula)){
					if(mode != AttributeMode.Formula && mode != AttributeMode.Linked && usage == AttributeUsage.Shaped){
						menu.AddItem(new GUIContent("Advanced"),advanced,toggleAdvanced);
						menu.AddSeparator("/");
					}
					if(this.attribute.canDirect){menu.AddItem(new GUIContent("Direct"),normal&&(usage==AttributeUsage.Direct),modeNormal+usageDirect);}
					if(this.attribute.canShape){menu.AddItem(new GUIContent("Shaped"),normal&&(usage==AttributeUsage.Shaped),modeNormal+usageShaped);}
					if(this.attribute.canLink){menu.AddItem(new GUIContent("Linked"),(mode==AttributeMode.Linked),modeLinked+usageShaped);}
					menu.AddSeparator("/");
					if(this.attribute.canFormula){menu.AddItem(new GUIContent("Formula"),(mode==AttributeMode.Formula),modeFormula);}
				}
				else{
					menu.AddItem(new GUIContent("Advanced"),advanced,toggleAdvanced);
					this.DrawTypeMenu(data,menu);
					menu.AddSeparator("/");
					if(this.attribute.canDirect){menu.AddItem(new GUIContent("Direct"),usage==AttributeUsage.Direct,usageDirect);}
					if(this.attribute.canShape){menu.AddItem(new GUIContent("Shaped"),usage==AttributeUsage.Shaped,usageShaped);}
				}
				if(showRemove){
					menu.AddSeparator("/");
					menu.AddItem(new GUIContent("Remove"),false,removeAttribute);	
				}
				menu.ShowAsContext();
			}
			if(this.contextOpen && Event.current.button == 0){
				this.dirty = true;
				this.contextOpen = false;
			}
		}
	}
}
