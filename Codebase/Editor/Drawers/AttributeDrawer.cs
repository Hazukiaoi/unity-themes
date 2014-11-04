#pragma warning disable 0219
using Zios;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
namespace Zios{
	[CustomPropertyDrawer(typeof(AttributeBool))]
	[CustomPropertyDrawer(typeof(AttributeString))]
	[CustomPropertyDrawer(typeof(AttributeInt))]
	[CustomPropertyDrawer(typeof(AttributeFloat))]
	[CustomPropertyDrawer(typeof(AttributeVector3))]
	public class AttributeDrawer : PropertyDrawer{
		public SerializedProperty property;
		public GUIContent label;
		public float overallHeight;
		public Rect fullRect;
		public Rect labelRect;
		public Rect valueRect;
		public Rect iconRect;
		public bool contextOpen;
		public Dictionary<AttributeData,bool> targetMode = new Dictionary<AttributeData,bool>();
		public bool formulaExpanded;
		public override float GetPropertyHeight(SerializedProperty property,GUIContent label){
			return this.overallHeight;
		}
		public void DrawDirect<BaseType,Type,DataType,Operator,Special>(Type current,AttributeData data,GUIContent label,bool? drawOperator=null)
			where Operator : struct
			where Special  : struct
			where Type     : Attribute<BaseType,Type,DataType,Operator,Special>
			where DataType : AttributeData<BaseType,Type,Operator,Special>,new(){
			EditorGUIUtility.labelWidth = this.labelRect.width;
			EditorGUIUtility.fieldWidth = this.valueRect.width;
			if(drawOperator != null){
				this.DrawOperator<BaseType,Type,DataType,Operator,Special>((DataType)data,(bool)!drawOperator);
				EditorGUIUtility.labelWidth += 101;
				EditorGUIUtility.fieldWidth += 81;
			}
			if(current is AttributeFloat){
				AttributeFloatData floatData = (AttributeFloatData)data;
				floatData.value = EditorGUI.FloatField(this.fullRect,label,floatData.value);
			}
			if(current is AttributeInt){
				AttributeIntData intData = (AttributeIntData)data;
				intData.value = EditorGUI.IntField(this.fullRect,label,intData.value);
			}
			if(current is AttributeString){
				AttributeStringData stringData = (AttributeStringData)data;
				stringData.value = EditorGUI.TextField(this.fullRect,label,stringData.value);
			}
			if(current is AttributeBool){
				AttributeBoolData boolData = (AttributeBoolData)data;
				boolData.value = EditorGUI.Toggle(this.fullRect,label,boolData.value);
			}
			if(current is AttributeVector3){
				AttributeVector3Data vector3Data = (AttributeVector3Data)data;
				vector3Data.value = EditorGUI.Vector3Field(this.fullRect,label,vector3Data.value);
			}
		}
		public void Draw<BaseType,Type,DataType,Operator,Special>()
			where Operator : struct
			where Special  : struct
			where Type     : Attribute<BaseType,Type,DataType,Operator,Special>
			where DataType : AttributeData<BaseType,Type,Operator,Special>,new(){
			Type attribute = this.property.GetObject<Type>();
			DataType firstData = attribute.data[0];
			SerializedProperty firstProperty = property.FindPropertyRelative("data").GetArrayElementAtIndex(0);
			this.DrawContext(attribute,firstData);
			if(attribute.mode == AttributeMode.Normal){
				if(firstData.usage == AttributeUsage.Direct){
					this.DrawDirect<BaseType,Type,DataType,Operator,Special>(attribute,firstData,this.label);
				}
				if(firstData.usage == AttributeUsage.Shaped){
					GUI.Box(this.iconRect,"",GUI.skin.GetStyle("IconShaped"));
					this.labelRect = this.labelRect.AddX(16);
					this.DrawShaped<BaseType,Type,DataType,Operator,Special>(attribute,firstProperty,this.label);
				}
			}
			if(attribute.mode == AttributeMode.Linked){
				GUI.Box(this.iconRect,"",GUI.skin.GetStyle("IconLinked"));
				this.labelRect = this.labelRect.AddX(16);
				this.DrawShaped<BaseType,Type,DataType,Operator,Special>(attribute,firstProperty,this.label,false);
			}
			if(attribute.mode == AttributeMode.Formula){
				this.DrawFormula<BaseType,Type,DataType,Operator,Special>(attribute,this.label);
			}
		}
		public void DrawShaped<BaseType,Type,DataType,Operator,Special>(Type attribute,SerializedProperty property,GUIContent label,bool drawSpecial=true,bool? drawOperator=null)
			where Operator : struct
			where Special  : struct
			where Type     : Attribute<BaseType,Type,DataType,Operator,Special>
			where DataType : AttributeData<BaseType,Type,Operator,Special>,new(){
			EditorGUI.LabelField(labelRect,label);
			DataType data = property.GetObject<DataType>();
			Target target = data.target;
			Rect toggleRect = this.valueRect.SetWidth(16);
			bool toggleActive = this.targetMode.ContainsKey(data) ? this.targetMode[data] : target.direct != null;
			this.targetMode[data] = EditorGUI.Toggle(toggleRect,toggleActive,GUI.skin.GetStyle("CheckmarkToggle"));
			if(!this.targetMode[data]){
				SerializedProperty targetProperty = property.FindPropertyRelative("target");
				Rect targetRect = this.valueRect.Add(18,0,-18,0);
				EditorGUI.PropertyField(targetRect,targetProperty,GUIContent.none);
				return;
			}
			var lookup = attribute.GetLookupTable();
			List<string> attributeList = new List<string>();
			int attributeIndex = 0;
			if(target.direct != null){
				if(lookup.ContainsKey(target.direct)){
					attributeList = lookup[target.direct].Keys.ToList();
				}
				attributeList.Remove(attribute.path);
				if(data.reference != null){
					attributeIndex = attributeList.IndexOf(data.reference.path);
				}
			}
			if(attributeList.Count > 0){
				Rect attributeRect = this.valueRect.Add(18,0,-18,0);
				Rect specialRect = this.valueRect.Add(18,0,0,0).SetWidth(50);
				if(attributeIndex == -1){attributeIndex = 0;}
				if(drawOperator != null){
					this.DrawOperator<BaseType,Type,DataType,Operator,Special>(data,(bool)!drawOperator);
					attributeRect = attributeRect.Add(81,0,-81,0);
					specialRect.x += 81;
				}
				if(drawSpecial){
					List<string> specialList = new List<string>();
					int specialIndex = 0;
					if(target.direct != null){
						string specialName = Enum.GetName(typeof(Special),data.special);
						specialList = Enum.GetNames(typeof(Special)).ToList();
						specialIndex = specialList.IndexOf(specialName);
					}
					if(specialIndex == -1){specialIndex = 0;}
					attributeRect = attributeRect.Add(51,0,-51,0);
					specialIndex = EditorGUI.Popup(specialRect,specialIndex,specialList.ToArray());
					data.special = ((Special[])Enum.GetValues(typeof(Special)))[specialIndex];
				}
				attributeIndex = EditorGUI.Popup(attributeRect,attributeIndex,attributeList.ToArray());
				string name = attributeList[attributeIndex];
				data.reference = lookup[target.direct][name];
			}
			else{
				Rect warningRect = this.valueRect.Add(18,0,-18,0);
				string targetName = target.direct == null ? "Target" : target.direct.ToString().Strip("(UnityEngine.GameObject)").Trim();
				string typeName = attribute.GetType().ToString().ToLower().Strip("zios",".","attribute");
				string message = "<b>" + targetName.Truncate(16) + "</b> has no <b>"+typeName+"</b> attributes.";
				//EditorGUI.HelpBox(warningRect,message,MessageType.None);
				EditorGUI.LabelField(warningRect,message,GUI.skin.GetStyle("WarningLabel"));
			}
		}
		public void DrawOperator<BaseType,Type,DataType,Operator,Special>(DataType data,bool disabled=false)
		where Operator : struct
		where Special  : struct
		where Type     : Attribute<BaseType,Type,DataType,Operator,Special>
		where DataType : AttributeData<BaseType,Type,Operator,Special>,new(){
			Rect operatorRect = this.valueRect.Add(18,0,0,0).SetWidth(80);
			EditorGUIUtility.AddCursorRect(operatorRect,MouseCursor.Arrow);
			List<string> operatorList = new List<string>();
			GUIStyle style = new GUIStyle(EditorStyles.popup);
			style.alignment = TextAnchor.MiddleRight;
			style.contentOffset = new Vector2(-3,0);
			style.fontStyle = FontStyle.Bold;
			if(disabled){
				GUI.enabled = false;
				operatorList.Add("=");
				EditorGUI.Popup(operatorRect,0,operatorList.ToArray(),style);
				GUI.enabled = true;
				return;
			}
			operatorList = Enum.GetNames(typeof(Operator)).ToList();
			string operatorName = Enum.GetName(typeof(Operator),data.sign);
			int operatorIndex = operatorList.IndexOf(operatorName);
			if(operatorIndex == -1){operatorIndex = 0;}
			for(int index=0;index<operatorList.Count;++index){
				string operatorAlias = operatorList[index];
				if(operatorAlias.Contains("Add")){operatorAlias="+";}
				if(operatorAlias.Contains("Sub")){operatorAlias="-";}
				if(operatorAlias.Contains("Mul")){operatorAlias="×";}
				if(operatorAlias.Contains("Div")){operatorAlias="÷";}
				operatorList[index] = operatorAlias;
			}
			//EditorStyles.standardFont = FileManager.GetAsset<Font>("SansationRegular.ttf");
			/*if(operatorList[operatorIndex].Length == 1){
				operatorRect = operatorRect.SetWidth(30).AddX(50);
			}*/
			operatorIndex = EditorGUI.Popup(operatorRect,operatorIndex,operatorList.ToArray(),style);
			data.sign = ((Operator[])Enum.GetValues(typeof(Operator)))[operatorIndex];
		}
		public void DrawFormula<BaseType,Type,DataType,Operator,Special>(Type attribute,GUIContent label)
			where Operator : struct
			where Special  : struct
			where Type     : Attribute<BaseType,Type,DataType,Operator,Special>
			where DataType : AttributeData<BaseType,Type,Operator,Special>,new(){
			//GUI.Box(this.iconRect,"",GUI.skin.GetStyle("IconFormula"));
			Rect labelRect = this.labelRect.AddX(12);
			//Rect valueRect = this.fullRect.Add(labelRect.width,0,-labelRect.width,0);
			EditorGUIUtility.AddCursorRect(this.fullRect,MouseCursor.ArrowPlus);
			if(this.labelRect.AddX(16).Clicked() || this.valueRect.Clicked()){
				GUI.changed = true;
				this.formulaExpanded = !this.formulaExpanded;
			}
			this.formulaExpanded = EditorGUI.Foldout(labelRect,this.formulaExpanded,label,GUI.skin.GetStyle("IconFormula"));
			if(this.formulaExpanded){
				float lineHeight = EditorGUIUtility.singleLineHeight+2;
				this.fullRect = this.fullRect.SetX(45).AddWidth(-55);
				this.labelRect = this.labelRect.SetX(45).SetWidth(25);
				this.valueRect = this.valueRect.SetX(70).SetWidth(this.fullRect.width);
				SerializedProperty dataProperty = this.property.FindPropertyRelative("data");
				for(int index=0;index<attribute.data.Length;++index){
					SerializedProperty currentProperty = dataProperty.GetArrayElementAtIndex(index);
					DataType currentData = attribute.data[index];
					GUIContent formulaLabel = new GUIContent("#"+(index+1));
					this.fullRect = this.fullRect.AddY(lineHeight);
					this.labelRect = this.labelRect.AddY(lineHeight);
					this.valueRect.y += lineHeight;
					this.overallHeight += lineHeight;
					bool? operatorState = index == 0 ? (bool?)false : (bool?)true;
					if(currentData.usage == AttributeUsage.Direct){
						this.fullRect = this.fullRect.AddWidth(25);
						this.DrawDirect<BaseType,Type,DataType,Operator,Special>(attribute,currentData,formulaLabel,operatorState);
						this.fullRect = this.fullRect.AddWidth(-25);
					}
					else if(currentData.usage == AttributeUsage.Shaped){
						this.DrawShaped<BaseType,Type,DataType,Operator,Special>(attribute,currentProperty,formulaLabel,true,operatorState);
					}
					this.DrawContext(attribute,currentData,false,index!=0);
				}
				this.labelRect.y += lineHeight;
				this.overallHeight += lineHeight;
				if(GUI.Button(this.labelRect.SetWidth(100),"Add Attribute")){
					attribute.Add();
					GUI.changed = true;
				}
			}
			else{
				EditorGUI.LabelField(this.valueRect,"[expand for details]",GUI.skin.GetStyle("WarningLabel"));
			}
		}
		public void DrawContext(Attribute attribute,AttributeData data,bool showMode=true,bool showRemove=false){
			if(this.labelRect.Clicked(1)){
				this.contextOpen = true;
				GenericMenu menu = new GenericMenu();
				AttributeMode mode = attribute.mode;
				AttributeUsage usage = data.usage;
				MenuFunction removeAttribute = ()=>{attribute.Remove(data);};
				MenuFunction modeNormal  = ()=>{attribute.mode = AttributeMode.Normal;};
				MenuFunction modeLinked  = ()=>{attribute.mode = AttributeMode.Linked;};
				MenuFunction modeFormula = ()=>{attribute.mode = AttributeMode.Formula;};
				MenuFunction usageDirect = ()=>{data.usage = AttributeUsage.Direct;};
				MenuFunction usageShaped = ()=>{data.usage = AttributeUsage.Shaped;};
				bool normal = attribute.mode == AttributeMode.Normal;
				if(attribute.locked){
					menu.AddDisabledItem(new GUIContent("Attribute Mode Locked"));
					menu.ShowAsContext();
					return;
				}
				if(showMode){
					menu.AddItem(new GUIContent("Normal/Direct"),normal&&(usage==AttributeUsage.Direct),modeNormal+usageDirect);
					menu.AddItem(new GUIContent("Normal/Shaped"),normal&&(usage==AttributeUsage.Shaped),modeNormal+usageShaped);
					menu.AddItem(new GUIContent("Linked"),(mode==AttributeMode.Linked),modeLinked);
					menu.AddItem(new GUIContent("Formula"),(mode==AttributeMode.Formula),modeFormula);
				}
				else{
					menu.AddItem(new GUIContent("Direct"),normal&&(usage==AttributeUsage.Direct),usageDirect);
					menu.AddItem(new GUIContent("Shaped"),normal&&(usage==AttributeUsage.Shaped),usageShaped);
				}
				if(showRemove){
					menu.AddItem(new GUIContent("Remove"),false,removeAttribute);	
				}
				menu.ShowAsContext();
			}
			if(this.contextOpen && Event.current.button == 0){
				GUI.changed = true;
				this.ForceUpdate();
				this.contextOpen = false;
			}
		}
		public void ForceUpdate(){
			SerializedProperty forceUpdate = property.FindPropertyRelative("path");
			string path = forceUpdate.stringValue;
			forceUpdate.stringValue = "";
			forceUpdate.stringValue = path;
		}
		public override void OnGUI(Rect area,SerializedProperty property,GUIContent label){
			this.property = property;
			this.label = label;
			this.overallHeight = base.GetPropertyHeight(property,label);
			string skinName = EditorGUIUtility.isProSkin ? "Dark" : "Light";
			GUI.skin = FileManager.GetAsset<GUISkin>("Gentleface-" + skinName + ".guiskin");
			//float xOffset = GUI.skin.label.CalcSize(label).x;
			//Rect iconRect = this.fullRect.AddX(xOffset+4).SetSize(14,14);
			this.fullRect = area.SetHeight(EditorGUIUtility.singleLineHeight);
			this.iconRect = this.fullRect.SetSize(14,14);
			this.labelRect = this.fullRect.SetWidth(EditorGUIUtility.labelWidth);
			this.valueRect = this.fullRect.Add(labelRect.width,0,-labelRect.width,0);
			this.iconRect = this.fullRect.SetSize(14,14);
			GUI.changed = false;
			EditorGUI.BeginProperty(area,label,property);
			object generic = property.GetObject<object>();
			if(generic is AttributeFloat){this.Draw<float,AttributeFloat,AttributeFloatData,OperatorNumeral,SpecialNumeral>();}
			if(generic is AttributeInt){this.Draw<int,AttributeInt,AttributeIntData,OperatorNumeral,SpecialNumeral>();}
			if(generic is AttributeString){this.Draw<string,AttributeString,AttributeStringData,OperatorString,SpecialString>();}
			if(generic is AttributeBool){this.Draw<bool,AttributeBool,AttributeBoolData,OperatorBool,SpecialBool>();}
			if(generic is AttributeVector3){this.Draw<Vector3,AttributeVector3,AttributeVector3Data,OperatorVector3,SpecialVector3>();}
			EditorGUI.EndProperty();
			if(GUI.changed){
				property.serializedObject.ApplyModifiedProperties();
				if(EditorWindow.mouseOverWindow != null){
					EditorWindow.mouseOverWindow.Repaint();
				}
				EditorUtility.SetDirty(property.serializedObject.targetObject);
			}
		}
	}
}