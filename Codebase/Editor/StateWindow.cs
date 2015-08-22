using UnityEngine;
using UnityEditor;
using Zios;
using Zios.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
namespace Zios.Controller{
    public class StateWindow : EditorWindow{
		public static StateWindow instance;
		public StateTable target;
		public int row = -1;
		public int column = -1;
		public float cellSize;
		public float headerSize;
	    public int tableIndex = 0;
		public Dictionary<TableRow,TableField[]> active = new Dictionary<TableRow,TableField[]>();
	    public Dictionary<StateRow,int> rowIndex = new Dictionary<StateRow,int>();
		public Vector2 scroll = Vector2.zero;
		private GameObject lastTarget;
		private StateRow[] data;
	    private float nextStep;
	    private Table tableGUI = new Table();
		public static StateWindow Get(){return StateWindow.instance;}
        [MenuItem ("Zios/Window/State")]
	    public static void Begin(){
		    var window = EditorWindow.GetWindow<StateWindow>();
			if(StateWindow.instance == null){
				window.position = new Rect(100,150,600,500);
			}
			window.titleContent = new GUIContent("State");
        }
		public void OnHierarchyChange(){this.nextStep = 0;}
		public void Update(){
			StateWindow.instance = this;
			this.wantsMouseMove = !Application.isPlaying;
			var target = Selection.activeGameObject;
			if(!target.IsNull() && (this.lastTarget != target || this.target.IsNull())){
				var table = target.GetComponent<StateTable>();
				if(!table.IsNull()){
					this.lastTarget = target;
					this.target = table;
					this.nextStep = 0;
				}
			}
			if(this.target.IsNull()){return;}
			this.CheckRebuild();
			if(Application.isPlaying){
				Events.Add("On State Update",this.Repaint,this.target.gameObject);
				Events.Add("On State Refresh",this.Repaint,this.target.gameObject);
				this.row = -1;
				this.column = -1;
			}
		}
	    public void OnGUI(){
			if(this.target.IsNull()){return;}
			if(!Event.current.IsUseful()){return;}
			this.scroll = GUILayout.BeginScrollView(this.scroll);
		    this.FixLabels();
		    this.tableGUI.Draw();
			GUILayout.Space(10);
			GUILayout.EndScrollView();
			if(Event.current.type == EventType.MouseMove){
				this.Repaint();
			}
	    }
	    public void FixLabels(){
			if(this.target.tables.Count-1 < this.tableIndex){return;}
		    StateRow[] activeTable = this.target.tables[this.tableIndex];
		    if(activeTable.Length > 0){
			    this.tableGUI.GetSkin().label.fixedWidth = 0;
			    foreach(StateRow stateRow in activeTable){
				    int size = (int)(GUI.skin.label.CalcSize(new GUIContent(stateRow.name)).x) + 24;
				    size = (size / 8) * 8 + 1;
				    if(size > this.tableGUI.GetSkin().label.fixedWidth){
					    this.tableGUI.GetSkin().label.fixedWidth = size;
				    }
			    }
		    }
	    }
	    public void CheckRebuild(){
			bool timeGlitch = this.nextStep-Time.realtimeSinceStartup > 2;
		    if(timeGlitch || Time.realtimeSinceStartup > this.nextStep){
				StateTable stateTable = this.target;
			    stateTable.Refresh();
			    stateTable.UpdateTableList();
			    StateRow[] activeTable = stateTable.tables[this.tableIndex];
			    if(this.data != activeTable){
				    this.data = activeTable;
				    this.BuildTable(true);
					this.Repaint();
			    }
			    if(!stateTable.advanced){
				    this.tableIndex = 0;
			    }
			    this.nextStep = Time.realtimeSinceStartup + 1f;
		   }
	    }
	    public virtual void BuildTable(bool verticalHeader=false,bool force=false){
		    StateTable stateTable = this.target;
		    StateRow[] activeTable = stateTable.tables[this.tableIndex];
		    if(force || (stateTable != null && activeTable != null)){
			    this.tableGUI = new Table();
				TableRow tableRow = this.tableGUI.AddRow();
				tableRow.AppendField(new TitleField(stateTable.gameObject.name));
			    if(activeTable.Length > 0){
					tableRow = this.tableGUI.AddRow();
					tableRow.AppendField(new HeaderField(""));
				    foreach(StateRow stateRow in activeTable){
						var field = new HeaderField(stateRow);
						tableRow.AppendField(field);
				    }
				    foreach(StateRow stateRow in activeTable){
					    if(!this.rowIndex.ContainsKey(stateRow)){
						    this.rowIndex[stateRow] = 0;
					    }
					    int rowIndex = this.rowIndex[stateRow];
					    tableRow = this.tableGUI.AddRow(stateRow);
						tableRow.AppendField(new LabelField(stateRow));
					    foreach(StateRequirement requirement in stateRow.requirements[rowIndex].data){
							tableRow.AppendField(new StateField(requirement));
					    }
				    }
			    }
		    }
	    }
		public static void Clip(UnityLabel label,GUIStyle style,float xClip=0,float yClip=0){
			Rect next = GUILayoutUtility.GetRect(label,style);
			StateWindow.Clip(next,label,style,xClip,yClip);
		}
		public static void Clip(Rect next,UnityLabel label,GUIStyle style,float xClip=0,float yClip=0){
			Vector2 scroll = StateWindow.Get().scroll;
			float x = next.x - scroll.x;
			float y = next.y - scroll.y;
			if(xClip == -1){next.x += scroll.x;}
			if(yClip == -1){next.y += scroll.y;}
			if(xClip > 0){style.overflow.left = (int)Mathf.Min(x-xClip,0);}
			if(yClip > 0){style.overflow.top  = (int)Mathf.Min(y-yClip,0);}
			bool xPass = xClip == -1 || (x + next.width  > xClip);
			bool yPass = yClip == -1 || (y + next.height > yClip);
			label.value.text = style.overflow.left >= -(next.width/4)+9 ? label.value.text : "";
			label.value.text = style.overflow.top >= -(next.height/4) ? label.value.text : "";
			if(xPass && yPass){label.DrawLabel(next,style);}
		}
    }
	//===================================
	// Title Field
	//===================================
	public class TitleField : TableField{
		public TitleField(object target=null,TableRow row=null) : base(target,row){}
		public override void Draw(){
			var title = new GUIContent((string)this.target);
			var style = Style.Get("title");
			style.fixedWidth = Screen.width-24;
			Rect next = GUILayoutUtility.GetRect(title,style);
			title.DrawLabel(next.AddXY(StateWindow.Get().scroll),style);
		}
	}
	//===================================
	// Header Field
	//===================================
	public class HeaderField : TableField{
		public HeaderField(object target=null,TableRow row=null) : base(target,row){}
		public override void Draw(){
			var window = StateWindow.Get();
			var label = this.target is string ? new GUIContent("") : new GUIContent(this.target.GetVariable<string>("name"));
			GUIStyle style = new GUIStyle(GUI.skin.label);
			int mode = EditorPrefs.GetInt("StateWindow-Mode",2);
			bool darkSkin = EditorGUIUtility.isProSkin;
			if(label.text == ""){
				window.active.Clear();
				window.headerSize = 64;
				style.margin.left = 5;
				string background = darkSkin ? "BoxBlackA30" : "BoxWhiteBWarm";
				style.normal.background = FileManager.GetAsset<Texture2D>(background);
				if(mode == 0){
					window.headerSize = 35;
					style.fixedHeight = style.fixedWidth;
					StateWindow.Clip(label,style,0,window.headerSize);
				}
				if(mode != 0){StateWindow.Clip(label,style,-1,-1);}
				return;
			}
			var script = this.target.As<StateRow>().target;
			if(!script.IsEnabled()){return;}
			bool hovered = window.column == this.order;
			if(hovered){
				string background = darkSkin ? "BoxBlackHighlightBlueAWarm" : "BoxBlackHighlightBlueDWarm";
				style.normal.textColor = darkSkin ? Colors.Get("ZestyBlue") : Colors.Get("White");
				style.normal.background = FileManager.GetAsset<Texture2D>(background);
			}
			if(mode == 0){
				float halfWidth = style.fixedWidth / 2;
				float halfHeight = style.fixedHeight / 2;
				GUIStyle rotated = new GUIStyle(style).Rotate90();
				Rect last = GUILayoutUtility.GetRect(new GUIContent(""),rotated);
				GUIUtility.RotateAroundPivot(90,last.center);
				Rect position = new Rect(last.x,last.y,0,0);
				position.x +=  halfHeight-halfWidth;
				position.y += -halfHeight+halfWidth;
				style.overflow.left = (int)-window.scroll.y;
				label.text = style.overflow.left >= -(position.width/4)-9 ? label.text : "";
				label.DrawLabel(position,style);
				GUI.matrix = Matrix4x4.identity;
			}
			else{
				var active = window.active;
				if(!active.ContainsKey(this.row)){
					Func<TableField,bool> isEnabled = x=>x.target.As<StateRow>().target.IsEnabled();
					active[this.row] = this.row.fields.Skip(1).Where(isEnabled).Cast<TableField>().ToArray();
				}
				style.margin.right = active[this.row].Last() == this ? 13 : style.margin.right;
				float area = window.cellSize = ((Screen.width-style.fixedWidth-17)/active[this.row].Length);
				area = window.cellSize = Mathf.Floor(area-2);
				style.fixedWidth = mode == 2 ? area : style.fixedWidth;
				style.alignment = TextAnchor.MiddleCenter;
				StateWindow.Clip(label,style,GUI.skin.label.fixedWidth+7,-1);
			}
			this.CheckClicked();
		}
		public override void Clicked(int button){
			if(button == 0){
				int mode = (EditorPrefs.GetInt("StateWindow-Mode",2)+1)%3;
				EditorPrefs.SetInt("StateWindow-Mode",mode);
				StateWindow.Get().Repaint();
				return;
			}
			/*GenericMenu menu = new GenericMenu();
			GUIContent toggleUpdateText = new GUIContent(" Always Update");
			MenuFunction toggleUpdate = ()=>Utility.ToggleEditorPref("StateWindow-AlwaysUpdate");
			menu.AddItem(toggleUpdateText,false,toggleUpdate);
			menu.ShowAsContext();*/
		}
	}
	//===================================
	// Label Field
	//===================================
	public class LabelField : TableField{
		public LabelField(object target=null,TableRow row=null) : base(target,row){}
		public override void Draw(){
			var script = this.row.target.As<StateRow>().target;
			if(!script.IsEnabled()){return;}
			GUIContent content = new GUIContent(script.alias);
			GUIStyle style = this.GetStyle(GUI.skin.label);
			float headerSize = StateWindow.Get().headerSize;
			StateWindow.Clip(content,style,-1,headerSize);
			this.CheckClicked();
		}
		public GUIStyle GetStyle(GUIStyle style){
			var script = this.row.target.As<StateRow>().target;
			bool darkSkin = EditorGUIUtility.isProSkin;
			string background = darkSkin ? "BoxBlackA30" : "BoxWhiteBWarm";
			Color textColor = darkSkin? Colors.Get("Silver") : Colors.Get("Black");
			style = new GUIStyle(style);
			style.margin.left = 5;
			bool hovered = StateWindow.Get().row == this.row.order;
			if(hovered){
				textColor = darkSkin ? Colors.Get("ZestyBlue") : Colors.Get("White");
				background = darkSkin ? "BoxBlackHighlightBlueAWarm" : "BoxBlackHighlightBlueDWarm";
			}
			if(Application.isPlaying){
				textColor = Colors.Get("Gray");
				background = darkSkin ? "BoxBlackAWarm30" : "BoxWhiteBWarm50";
				if(script.usable){
					textColor = darkSkin ? Colors.Get("Silver") : Colors.Get("Black");
					background = darkSkin ? "BoxBlackA30" : "BoxWhiteBWarm";
				}
				if(script.used){
					textColor = darkSkin ? Colors.Get("White") : Colors.Get("White");
					background = darkSkin ? "BoxBlackHighlightYellowDWarm" : "BoxBlackHighlightYellowDWarm";
				}
				if(script.inUse){
					textColor = darkSkin ? Colors.Get("White") : Colors.Get("White");
					background = darkSkin ? "BoxBlackHighlightPurpleDWarm" : "BoxBlackHighlightPurpleDWarm";
				}
			}
			/*if(hovered){
				textColor = darkSkin ? Colors.Get("ZestyBlue") : Colors.Get("White");
				background = darkSkin ? "BoxBlackHighlightBlueAWarm" : "BoxBlackHighlightBlueDWarm";
			}*/
			style.normal.textColor = textColor;
			style.normal.background = FileManager.GetAsset<Texture2D>(background);
			if(this.selected){style.normal = style.active;}
			return style;
		}
		public override void Clicked(int button){
			var stateRow = (StateRow)this.row.target;
			int rowIndex = StateWindow.Get().rowIndex[stateRow];
			if(button == 0){
				int length = stateRow.requirements.Length;
				rowIndex += Event.current.control ? -1 : 1;
				if(rowIndex < 0){rowIndex = length-1;}
				if(rowIndex >= length){rowIndex = 0;}
				StateWindow.Get().rowIndex[stateRow] = rowIndex;
				StateWindow.Get().BuildTable(true);
			}
			if(button == 1){
				GenericMenu menu = new GenericMenu();
				GUIContent addAlternative = new GUIContent("+ Add Alternate Row");
				GUIContent removeAlternative = new GUIContent("- Remove Alternative Row");
				menu.AddItem(addAlternative,false,new GenericMenu.MenuFunction2(this.AddAlternativeRow),stateRow);
				if(rowIndex != 0){
					menu.AddItem(removeAlternative,false,new GenericMenu.MenuFunction2(this.RemoveAlternativeRow),stateRow);
				}
				menu.ShowAsContext();
			}
			Event.current.Use();
			StateWindow.Get().Repaint();
		}
	    public void AddAlternativeRow(object target){
		    StateRow row = (StateRow)target;
		    List<StateRowData> data = new List<StateRowData>(row.requirements);
		    data.Add(new StateRowData());
		    row.requirements = data.ToArray();
		    StateWindow.Get().target.Refresh();
		    StateWindow.Get().rowIndex[row] = row.requirements.Length-1;
		    StateWindow.Get().BuildTable(true);
	    }
	    public void RemoveAlternativeRow(object target){
		    StateRow row = (StateRow)target;
			int rowIndex = StateWindow.Get().rowIndex[row];
		    List<StateRowData> data = new List<StateRowData>(row.requirements);
		    data.RemoveAt(rowIndex);
		    row.requirements = data.ToArray();
			StateWindow.Get().rowIndex[row] = rowIndex-1;
		    StateWindow.Get().BuildTable(true);
	    }
	}
	//===================================
	// State Field
	//===================================
	public class StateField : TableField{
		public StateField(object target=null,TableRow row=null) : base(target,row){}
		public override void Draw(){
			string value = "";
			var row = this.row.target.As<StateRow>();
			var columnScript = this.target.As<StateRequirement>().target;
			var rowScript = row.target;
			if(!rowScript.IsEnabled() || !columnScript.IsEnabled()){return;}
			var window = StateWindow.Get();
			var requirement = (StateRequirement)this.target;
			int rowIndex = window.rowIndex[row];
			int mode = EditorPrefs.GetInt("StateWindow-Mode",2);
			GUIStyle style = new GUIStyle(GUI.skin.button);
			//bool hovered = window.column == this.order && window.row == this.row.order;
			if(Application.isPlaying){style.hover = style.normal;}
			if(requirement.requireOn){
				value = rowIndex != 0 ? rowIndex.ToString() : "";
				style = Style.Get("buttonOn",true);
			}
			else if(requirement.requireOff){
				value = rowIndex != 0 ? rowIndex.ToString() : "";
				style = Style.Get("buttonOff",true);
			}
			style.fixedWidth = mode == 1 ? GUI.skin.label.fixedWidth : GUI.skin.label.fixedHeight;
			if(mode == 2){
				var active = window.active;
				if(!active.ContainsKey(this.row)){
					Func<TableField,bool> isEnabled = x=>x.target.As<StateRequirement>().target.IsEnabled();
					active[this.row] = this.row.fields.Skip(1).Where(isEnabled).Cast<TableField>().ToArray();
				}
				style.margin.right = active[this.row].Last() == this ? 13 : style.margin.right;
				style.fixedWidth = window.cellSize;
			}
			float headerSize = window.headerSize;
			StateWindow.Clip(value,style,GUI.skin.label.fixedWidth+7,headerSize);
			if(!Application.isPlaying && GUILayoutUtility.GetLastRect().Hovered()){
				window.row = this.row.order;
				window.column = this.order;
			}
			this.CheckClicked();
		}
		public override void Clicked(int button){
			int state = 0;
			var requirement = (StateRequirement)this.target;
			if(requirement.requireOn){state = 1;}
			if(requirement.requireOff){state = 2;}
			int amount = button == 0 ? 1 : -1;
			state += amount;
			state = state.Modulus(3);
			requirement.requireOn = false;
			requirement.requireOff = false;
			if(state == 1){requirement.requireOn = true;}
			if(state == 2){requirement.requireOff = true;}
			Utility.SetDirty(StateWindow.Get().target,false,true);
			StateWindow.Get().target.UpdateStates();
			StateWindow.Get().Repaint();
		}
	}
}