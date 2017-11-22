using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

//Editor:
[CustomEditor(typeof(TileStorage))]
public class TileStorageEditor : Editor
{
    private TileStorage Storage;
    private int Index;
    private Vector2Int StSize = new Vector2Int(3, 3);
    private string StName = "New Tile Name";
    private GUIStyle Style;
    private bool Preview;
    private Texture2D Image;
    private bool Sliced;

    void OnEnable()
    {
        Storage = (TileStorage)target;

    }

    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);

        DrawLoopBox();
        if (Storage.Data.Count > 0 && Storage != null)
        {
            DrawMatrix(Index);
            PreviewMatrix(Index);
        }

        GUILayout.Space(10);
    }

    void DrawMatrix(int id)
    {
        GUILayout.Label("Name: " + Storage.Data[id].Name);
        GUILayout.BeginHorizontal("Box");
        for (int x = 0; x < Storage.Data[id].Size.x; x++)
        {
            GUILayout.BeginVertical();
            for (int y = 0; y < Storage.Data[id].Size.y; y++)
            {
                GUILayout.BeginVertical("Box");
                GUILayout.Label(x + "-" + y);
                Storage.Data[id].SetTileAt(x, y, EditorGUILayout.ObjectField(Storage.Data[id].GetTileAt(x, y), typeof(TileBase), true) as TileBase);
                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Clear Matrix"))
            Storage.Data[id].Clear();
    }

    private void PreviewMatrix(int id)
    {
        //Preview de matrix:
        Sliced = EditorGUILayout.Toggle("Sliced Image", Sliced);
        Preview = EditorGUILayout.Foldout(Preview, "Preview Matrix", true);
        if (Preview && !Sliced)
        {
            GUILayout.Label("Matrix Preview");
            GUILayout.BeginHorizontal("Box", GUILayout.MaxWidth(190), GUILayout.MaxHeight(190));
            for (int x = 0; x < Storage.Data[id].Size.x; x++)
            {
                GUILayout.BeginVertical();
                for (int y = 0; y < Storage.Data[id].Size.y; y++)
                {

                    GUILayout.BeginVertical("Button", GUILayout.MinWidth(50), GUILayout.MinHeight(50), GUILayout.MaxWidth(50), GUILayout.MaxHeight(50));
                    GUILayout.Label("");
                    if (Storage.Data[Index].GetTileAt(x, y) == null)
                        Image = EmptyTexture(25, 25);
                    else
                        Image = Utils.GetTileData(Storage.Data[Index].GetTileAt(x, y)).sprite.texture;

                    GUI.DrawTexture(new Rect(GUILayoutUtility.GetLastRect().x, GUILayoutUtility.GetLastRect().y + 5, 40, 40), Image);
                    //GUILayout.Label(Utils.GetTileData(Storage.Data[index].TileData[x, y]).sprite.texture);
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
    }

    private void DrawLoopBox()
    {
        GUILayout.BeginVertical("Box");
        GUILayout.Label("Storage Size [" + Storage.Data.Count + "]");
        /*SerializedProperty sceneNames = this.serializedObject.FindProperty("Data");
        EditorGUILayout.PropertyField(sceneNames.FindPropertyRelative("Array.size"), GUILayout.MaxWidth(300));
        serializedObject.ApplyModifiedProperties();*/
        //Loop Box:
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal("Box", GUILayout.MaxWidth(150));
        if (GUILayout.Button("-") && Index > 0)
            Index--;
        GUILayout.Label("[" + Index + "]");
        if (GUILayout.Button("+") && Index < Storage.Data.Count - 1)
            Index++;
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        //Add - Remove - Clear Box
        GUILayout.BeginVertical();
        //GUILayout.BeginHorizontal();
        StName = EditorGUILayout.TextField("Name", StName);
        StSize = EditorGUILayout.Vector2IntField("Size[width/height]", StSize);
        GUI.color = Color.green;

        if (GUILayout.Button("Add [" + StSize.x + "-" + StSize.y + "]"))
        {
            Storage.Data.Add(new TileInterator(StSize.x, StSize.y, StName));
        }
        GUI.color = Color.red;
        if (GUILayout.Button("Remove") && Storage.Data.Count > 0)
        {
            Storage.Data.Remove(Storage.Data.Last());
            Index = 0;
        }
        if (GUILayout.Button("Remove At [" + Index + "]") && (Index >= 0 && Index < Storage.Data.Count))
        {
            Storage.Data.RemoveAt(Index);
            Index = 0;
        }
        GUI.color = Color.white;

        //GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndVertical();

        Storage.Width = EditorGUILayout.IntField("Width", Storage.Width);
        Storage.Height = EditorGUILayout.IntField("Height", Storage.Height);
        Storage.PixelsPerUnit = EditorGUILayout.FloatField("Pixels Per Unit", Storage.PixelsPerUnit);

        //Size of array

        /*
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal("Box", GUILayout.MaxWidth(150));
        if (GUILayout.Button("-") && Index > 0)
            Index--;
        GUILayout.Label("[" + Index + "]");
        if (GUILayout.Button("+") && Index < Storage.Data.Capacity - 1)
            Index++;
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();*/

    }

    Texture2D EmptyTexture(int w, int h)
    {
        Texture2D temp = new Texture2D(w, h);
        Color[] pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = new Color(1, 1, 1);
        temp.SetPixels(pix);
        return temp;
    }
}

