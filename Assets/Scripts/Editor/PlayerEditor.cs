using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor 
{
    private Player PL;
    private bool ToggleWalk,ToggleRun,ToggleJump,ToggleGeneral,ToggleAim;

	void OnEnable() {
        PL = target as Player;
	}

	public override void OnInspectorGUI() 
    {
        GUILayout.Space(10);
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUI.skin.label.fontStyle = FontStyle.Bold;
        //General:
        GUILayout.BeginVertical("Box");
        ToggleGeneral = EditorGUILayout.Foldout(ToggleGeneral, "General System", true);
        if (ToggleGeneral) 
        {
            GUILayout.Label("General Properties");
            PL.Dash = EditorGUILayout.Toggle("Dash", PL.Dash);
            PL.DashForce = EditorGUILayout.FloatField("Dash Force", PL.DashForce);
            //Genero:
            PL.PlayerGender = (Gender)EditorGUILayout.EnumPopup("Player Gender", PL.PlayerGender);
            switch (PL.PlayerGender)
            {
                case Gender.Female: PL.FemaleIdleName = EditorGUILayout.TextField("Female Idle Name", PL.FemaleIdleName); break;
                case Gender.Male: PL.MaleIdleName = EditorGUILayout.TextField("Male Idle Name", PL.MaleIdleName); break;
            }
            PL.IdleIndex = EditorGUILayout.IntField("Animation Idle", PL.IdleIndex);
            
        }
        GUILayout.EndVertical();
        GUILayout.Space(5);
        //Walk
        GUILayout.BeginVertical("Box");
        ToggleWalk = EditorGUILayout.Foldout(ToggleWalk, "Walking System", true);
        if (ToggleWalk) 
        {
            GUILayout.Label("Walk Control Properties");
            PL.WalkName = EditorGUILayout.TextField("Walk Animation Name", PL.WalkName);
            PL.Walk = EditorGUILayout.Toggle("Walk Enabled", PL.Walk);
            PL.WalkSpeed = EditorGUILayout.FloatField("Walk Speed", PL.WalkSpeed);
            PL.WalkSpeedMultiplier = EditorGUILayout.FloatField("Walk Speed Multiplier", PL.WalkSpeedMultiplier);
            PL.WalkSmooth = EditorGUILayout.FloatField("Walk Smooth", PL.WalkSmooth);
            PL.WalkDeadZone = EditorGUILayout.FloatField("Walk Dead Zone", PL.WalkDeadZone);
			GUILayout.Label ("Stance State");
			PL.StanceState = (Stance)EditorGUILayout.EnumPopup ("Stance", PL.StanceState);
			PL.CrouchSpeed = EditorGUILayout.FloatField ("Crouch Speed", PL.CrouchSpeed);
			PL.DownSpeed = EditorGUILayout.FloatField ("Crouch Speed", PL.DownSpeed);
            EUtils.EditorPropertyField(target, "StanceBounds");
            GUILayout.Label("Ladder Properties");
            PL.Ladder = EditorGUILayout.Toggle("Ladder", PL.Ladder);
            PL.LadderUpSpeed = EditorGUILayout.FloatField("Ladder Up Speed", PL.LadderUpSpeed);
            PL.LadderDownSpeed = EditorGUILayout.FloatField("Ladder Down Speed", PL.LadderDownSpeed);
            PL.LadderTag = EditorGUILayout.TextField("Ladder Tag", PL.LadderTag);
            PL.LadderMultiplier = EditorGUILayout.Vector2Field("Ladder Multiplier", PL.LadderMultiplier);
        }
        GUILayout.EndVertical();
        GUILayout.Space(5);
        //Run
        GUILayout.BeginVertical("Box");
        ToggleRun = EditorGUILayout.Foldout(ToggleRun, "Run System", true);
        if (ToggleRun)
        {
            GUILayout.Label("Run Properties");
            PL.RunName = EditorGUILayout.TextField("Run Animation Name", PL.RunName);
            PL.Run = EditorGUILayout.Toggle("Run", PL.Run);
            PL.RunSpeed = EditorGUILayout.FloatField("Run Speed",PL.RunSpeed);
            PL.RunSpeedSmooth = EditorGUILayout.FloatField("Run Speed Smooth", PL.RunSpeedSmooth);
            PL.RunSpeedMultiply = EditorGUILayout.FloatField("Run Speed Multiplier", PL.RunSpeedMultiply);
            PL.RunSpeedDeadZone = EditorGUILayout.Vector2Field("Run Speed DeadZone", PL.RunSpeedDeadZone);
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontStyle = FontStyle.Normal;
            GUI.skin.label.normal.textColor = Color.red;
            GUILayout.Label("Run Speed is Acumulative!.");
            GUI.skin.label.normal.textColor = Color.black;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontStyle = FontStyle.Bold;
        }
        GUILayout.EndVertical();
        GUILayout.Space(5);
        //Jump
        GUILayout.BeginVertical("Box");
        ToggleJump = EditorGUILayout.Foldout(ToggleJump, "Jumping System", true);
        if (ToggleJump) 
        {
            GUILayout.Label("Jump Control Properties");
            PL.JumpName = EditorGUILayout.TextField("Jump Animation Name", PL.JumpName);
            PL.Jump = EditorGUILayout.Toggle("Jump Enabled", PL.Jump);
            PL.JumpHeight = EditorGUILayout.FloatField("Jump Height", PL.JumpHeight);
            PL.JumpLenght = EditorGUILayout.FloatField("Jump Lenght", PL.JumpLenght);
            PL.JumpOnAir = EditorGUILayout.IntField("Air Jumps Count", PL.JumpOnAir);
            PL.AirControl = EditorGUILayout.Vector2Field("Air Control", PL.AirControl);
            PL.JumpOnAirReset = EditorGUILayout.Toggle("Air Jump Reset", PL.JumpOnAirReset);
        }
        GUILayout.EndVertical();
        GUILayout.Space(5);
        //Aim
        GUILayout.BeginVertical("Box");
        ToggleAim = EditorGUILayout.Foldout(ToggleAim, "Aiming System", true);
        if (ToggleAim)
        {
            GUILayout.Label("Aim Properties");
            PL.FaceToMouse = EditorGUILayout.Toggle("Face to mouse", PL.FaceToMouse);
            PL.LookAtMouse = EditorGUILayout.Toggle("Look At Mouse", PL.LookAtMouse);
            PL.LookAtMouseIdle = EditorGUILayout.IntField("Look At Mouse Idle", PL.LookAtMouseIdle);
            EUtils.EditorPropertyField(target, "LookAtMouseIndex");
            PL.Image = (Sprite)EditorGUILayout.ObjectField("LookAtMouse Image", PL.Image, typeof(Sprite), true);
        }
        GUILayout.EndVertical();
        GUILayout.Space(5);
	}
}
