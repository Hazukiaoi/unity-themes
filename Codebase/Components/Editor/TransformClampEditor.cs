using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(TransformClamp))] 
public class TransformClampEditor : Editor{
	//public MonoBehaviour script;
	public bool[] showArea = new bool[3];
	public override void OnInspectorGUI(){
		TransformClamp target = (TransformClamp)this.target;
		EditorGUIUtility.labelWidth = 15;
		EditorGUI.indentLevel += 1;
		//this.script = (MonoBehaviour)this.target;
		//this.script = (MonoBehaviour)EditorGUILayout.ObjectField("Script",target,typeof(MonoBehaviour),false);
		this.showArea[0] = this.DrawRegion(this.showArea[0],"Position",target.positionClamp,target.minPosition,target.maxPosition);
		this.showArea[1] = this.DrawRegion(this.showArea[1],"Rotation",target.rotationClamp,target.minRotation,target.maxRotation);
		this.showArea[2] = this.DrawRegion(this.showArea[2],"Scale",target.scaleClamp,target.minScale,target.maxScale);
	}
	public bool DrawRegion(bool state,string name,bool[] active,float[] min,float[] max){
		bool foldState = EditorGUILayout.Foldout(state,name);
		if(foldState){
			this.DrawRow(0,"X",active,min,max);
			this.DrawRow(1,"Y",active,min,max);
			this.DrawRow(2,"Z",active,min,max);
		}
		return foldState;
	}
	public void DrawRow(int index,string name,bool[] state,float[] min,float[] max){
		EditorGUILayout.BeginHorizontal();
		state[index] = GUI.enabled = EditorGUILayout.Toggle("",state[index],GUILayout.Width(12));
		min[index] = EditorGUILayout.FloatField(name,min[index],GUILayout.Width(100));
		max[index] = EditorGUILayout.FloatField("",max[index],GUILayout.Width(100));
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();
	}
}