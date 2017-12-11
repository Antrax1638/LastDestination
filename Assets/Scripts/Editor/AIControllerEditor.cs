using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AIController))]
public class AIControllerEditor : Editor 
{
    AIController AIC;
    int Tab;

    void OnEnable()
    {
        AIC = (AIController)target;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        Tab = GUILayout.Toolbar(Tab, new string[] { "Basic" , "Advanced", "Extended" });
        switch (Tab)
        {
            default: break;
            case 0: BasicControls(); break;
            case 1: AdvancedControls(); break;
            case 2: ExtendedControls(); break;
        }
    }

    void BasicControls()
    {
        AIC.Mode = (AIMode)EditorGUILayout.EnumPopup("Mode", AIC.Mode);
        if (AIC.Mode == AIMode.Target)
            AIC.Target = EditorGUILayout.ObjectField("Target", AIC.Target, typeof(Transform),true) as Transform;
        else
            AIC.Location = EditorGUILayout.Vector3Field("Target Location", AIC.Location);
        AIC.UpdateRate = EditorGUILayout.FloatField("Update Rate", AIC.UpdateRate);
        AIC.NextWaypontDistance = EditorGUILayout.FloatField("Next Waypoint Distance", AIC.NextWaypontDistance);
        GUILayout.Space(5);

        GUILayout.Label("Memory System");
        AIC.Memory = EditorGUILayout.Toggle("Memory", AIC.Memory);
        AIC.MemoryTime = EditorGUILayout.FloatField("Memory Time", AIC.MemoryTime);
        GUILayout.Space(5);

        GUILayout.Label("Search System");
        GUILayout.BeginVertical("Button");
        GUILayout.BeginHorizontal();
        if (AIC.Mode == AIMode.Target)
        {
            AIC.Search = EditorGUILayout.ToggleLeft("Search", AIC.Search, GUILayout.MaxWidth(60));
            AIC.SearchPlayer = EditorGUILayout.ToggleLeft("Search Player", AIC.SearchPlayer, GUILayout.MaxWidth(100));
            AIC.AutomaticSearch = EditorGUILayout.ToggleLeft("Auto Search", AIC.AutomaticSearch, GUILayout.MaxWidth(100));
        }
        else
            AIC.SearchLocation = EditorGUILayout.Toggle("Search Location", AIC.SearchLocation);
        GUILayout.EndHorizontal();
        AIC.SearchMode = (AISearch)EditorGUILayout.EnumPopup("Search Mode", AIC.SearchMode);
        AIC.SearchRate = EditorGUILayout.FloatField("Search Rate", AIC.SearchRate);
        AIC.SearchLength = EditorGUILayout.FloatField("Search Lenght", AIC.SearchLength);
        AIC.SearchTag = EditorGUILayout.TextField("Search Tag", AIC.SearchTag);
        GUILayout.EndVertical();
        GUILayout.Space(5);
    }

    void AdvancedControls()
    {
        GUILayout.Label("Movement System");
        AIC.ControlMode = (AIControl)EditorGUILayout.EnumPopup("Control Mode", AIC.ControlMode);
        AIC.Speed = EditorGUILayout.FloatField("Speed", AIC.Speed);
        AIC.Jump = EditorGUILayout.Toggle("Jump", AIC.Jump);
        AIC.JumpHeight = EditorGUILayout.FloatField("Jump Height", AIC.JumpHeight);
        AIC.JumpProbability = EditorGUILayout.Slider("Jump Probability", AIC.JumpProbability, 0.0f, 100.0f);
        
        GUILayout.Space(5);

        GUILayout.Label("Attack System");
        AIC.Attack = EditorGUILayout.Toggle("Attack",AIC.Attack);
        AIC.AType = (AIAttackType)EditorGUILayout.EnumPopup("Type", AIC.AType);
        switch (AIC.AType) {
            case AIAttackType.Ranged: 
                AIC.Prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", AIC.Prefab, typeof(GameObject), true);
                AIC.AttackLength = EditorGUILayout.FloatField("Attack Length", AIC.AttackLength);
                AIC.AMode = (AIAttackMode)EditorGUILayout.EnumPopup("Attack Mode", AIC.AMode);
                AIC.Accuracy = EditorGUILayout.FloatField("Accuracy", AIC.Accuracy);
            break;
        }
        
        AIC.AttackRate = EditorGUILayout.FloatField("Attack Rate", AIC.AttackRate);
        AIC.AttackSpeed = EditorGUILayout.FloatField("Attack Speed", AIC.AttackSpeed);
        AIC.Length = EditorGUILayout.FloatField("Sight Length", AIC.Length);
        GUILayout.Space(5);

        GUILayout.Label("Aggression System");
        EUtils.EditorPropertyField(target, "AggressionTable");
        GUILayout.Space(5);
    }

    void ExtendedControls(){
        
    }
}
