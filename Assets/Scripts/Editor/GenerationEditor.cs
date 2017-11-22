using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

//Custom Inspector Editor:
[CustomEditor(typeof(Generation))]
public class GenerationEditor : Editor
{
    private Generation Gen;
    private int Tab, TabMode, LayerSelect = 0, TileSelect = 0;
    private int LastTileLength;
    private bool RandomBaseTile = true, AutoUpdate = false, Preview = true;
    private string FilePath = "GeneratedImage";

    //Estilos:
    private GUIStyle labelStyle;

    public void OnEnable()
    {
        Gen = (Generation)target;
        labelStyle = new GUIStyle();
    }

    public override void OnInspectorGUI()
    {

        GUILayout.Space(10);
        Gen.Name = EditorGUILayout.TextField("Name", Gen.Name);
        Tab = GUILayout.Toolbar(Tab, new string[] { "Generator", "Layers", "Tiles" });
        switch (Tab)
        {
            default: break;
            case 0: GeneratorTab(); break;
            case 1: LayersTab(); break;
            case 2: TilesTab(); break;
        }

        if (AutoUpdate && Gen.Type == GenType.UnityTypePrime)
            Gen.GenerateLayer(true);
    }

    private void GeneratorTab()
    {
        GUILayout.Space(10);
        Gen.Type = (GenType)EditorGUILayout.EnumPopup("Generator Type:", Gen.Type);
        GUILayout.BeginVertical("Box");
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Properties: [" + Gen.Type + "]");
        GUILayout.Space(5);
        Gen.Transform = EditorGUILayout.RectField("Transform", Gen.Transform);
        GUILayout.Space(5);

        if (Gen.Type != GenType.UnityTypePrime)
        {
            Gen.Value = (GenValue)EditorGUILayout.EnumPopup("[Generator Values]", Gen.Value);
            Gen.SetGenValues();
            GUILayout.Space(5);
        }

        Gen.ChunkSize = EditorGUILayout.IntField("Chunk Size", Gen.ChunkSize);
        switch (Gen.Type)
        {
            default: GUILayout.Label("No Type Selected"); break;

            case GenType.UnityType:
                Gen.Seed = EditorGUILayout.Vector2Field("[Float]Seed", Gen.Seed);
                Gen.ClampChunk = EditorGUILayout.Toggle("Clamp Chunk", Gen.ClampChunk);
                Gen.Elevation = EditorGUILayout.IntField("Elevation", Gen.Elevation);
                Gen.Smooth = EditorGUILayout.IntField("Smooth", Gen.Smooth);
                Gen.Multiplier = EditorGUILayout.IntField("Multiplier", Gen.Multiplier);
                Gen.Frequency = EditorGUILayout.IntField("Frequency", Gen.Frequency);
                break;

            case GenType.UnityTypePrime:
                Gen.Transform.x = 0; Gen.Transform.y = 0;
                Gen.Seed = EditorGUILayout.Vector2Field("[Float]Seed", Gen.Seed);

                Gen.GeneratedMaterial = (Material)EditorGUILayout.ObjectField("Material", Gen.GeneratedMaterial, typeof(Material), true);
                Gen.Scale = EditorGUILayout.FloatField("Scale", Gen.Scale);

                Gen.Octaves = EditorGUILayout.IntField("Octaves", Gen.Octaves);
                Gen.OctavesRange = EditorGUILayout.Vector2Field("Octaves Range", Gen.OctavesRange);
                Gen.OctavesOffset = EditorGUILayout.Vector2Field("Octaves Offset", Gen.OctavesOffset);
                Gen.Persistance = EditorGUILayout.Slider("Persistance", Gen.Persistance, 0.0f, 1.0f);
                Gen.Lacunarity = EditorGUILayout.FloatField("Lacunarity", Gen.Lacunarity);

                break;

            case GenType.CustomType:
                Gen.Seed = EditorGUILayout.Vector2Field("[Long]Seed", Gen.Seed);
                Gen.ClampChunk = EditorGUILayout.Toggle("Clamp Chunk", Gen.ClampChunk);
                Gen.Elevation = EditorGUILayout.IntField("Elevation", Gen.Elevation);
                Gen.Frequency = EditorGUILayout.IntField("Frequency", Gen.Frequency);
                Gen.InterpolationType = (NInterpolation)EditorGUILayout.EnumPopup("Interpolation", Gen.InterpolationType);
                break;

            case GenType.DynamicMoveType:
                Gen.Transform.width = 0; Gen.Transform.height = 0;
                Gen.Octaves = EditorGUILayout.IntField("Moves Count", Gen.Octaves);
                break;
        }
        GUILayout.Space(10);

        GUILayout.EndVertical();
    }

    private void LayersTab()
    {
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<-") && LayerSelect > 0)
            LayerSelect--;
        LayerSelect = EditorGUILayout.IntField(LayerSelect, GUILayout.MaxWidth(30.0f));
        if (GUILayout.Button("->") && LayerSelect < Gen.transform.childCount - 1)
            LayerSelect++;
        GUILayout.EndHorizontal();
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Selected Layer [" + LayerSelect + "]");
        Gen.SelectedLayer = LayerSelect;

        //EUtils.EditorPropertyField(target, "SubLayers");

        switch (Gen.Type)
        {
            default:
                if (LayerSelect >= 0)
                {
                    GUILayout.BeginVertical("Box");
                    labelStyle.normal.textColor = Color.blue;
                    labelStyle.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("Layer Name: " + Gen.transform.GetChild(LayerSelect).name, labelStyle);

                    RandomBaseTile = EditorGUILayout.Toggle("Random Base Tile", RandomBaseTile);
                    Gen.SelectedTile = RandomBaseTile ? Random.Range(0, Gen.TileStorage.Data.Count) : EditorGUILayout.IntField("Base Layer Tile: ", Gen.SelectedTile);

                    GUILayout.EndVertical();
                }
                break;

            case GenType.UnityTypePrime:
                AutoUpdate = EditorGUILayout.Toggle("Auto Update", AutoUpdate);
                Gen.ActiveRegions = EditorGUILayout.Toggle("Active Regions", Gen.ActiveRegions);
                if (Gen.ActiveRegions)
                {
                    Gen.RegionsFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Regions Filter Mode", Gen.RegionsFilterMode);
                    //Array en el inspector personalizado:
                    var serializedObject = new SerializedObject(target);
                    var property = serializedObject.FindProperty("Regions");
                    serializedObject.Update();
                    EditorGUILayout.PropertyField(property, true);
                    serializedObject.ApplyModifiedProperties();
                    //fin de array personalizado
                }


                if (Gen.GetGeneratedTexture() != null)
                {
                    string path = FilePath;
                    GUILayout.BeginVertical("Box");
                    byte[] data = new byte[Gen.GetGeneratedTexture().width * Gen.GetGeneratedTexture().height];
                    FilePath = EditorGUILayout.TextField("Filename", FilePath);
                    Gen.EncodeType = (EncodingType)EditorGUILayout.EnumPopup("Encoding Type", Gen.EncodeType);
                    if (GUILayout.Button("SaveToFile"))
                    {
                        switch (Gen.EncodeType)
                        {
                            default: data = Gen.GetGeneratedTexture().EncodeToJPG(); break;
                            case EncodingType.JPG: FilePath += ".jpg"; data = Gen.GetGeneratedTexture().EncodeToJPG(); break;
                            case EncodingType.PNG: FilePath += ".png"; data = Gen.GetGeneratedTexture().EncodeToPNG(); break;
                            case EncodingType.EXR: FilePath += ".exr"; data = Gen.GetGeneratedTexture().EncodeToEXR(); break;
                        }
                        System.IO.File.WriteAllBytes(FilePath, data);
                        FilePath = path;
                    }
                    GUILayout.EndVertical();
                }
                break;

            case GenType.DynamicMoveType:

                if (LayerSelect >= 0)
                {
                    GUILayout.BeginVertical("Box");
                    labelStyle.normal.textColor = Color.blue;
                    labelStyle.alignment = TextAnchor.MiddleCenter;
                    GUILayout.Label("Layer Name: " + Gen.transform.GetChild(LayerSelect).name, labelStyle);

                    Gen.ResetOnMove = EditorGUILayout.Toggle("Reset On Move", Gen.ResetOnMove);
                    RandomBaseTile = EditorGUILayout.Toggle("Random Base Tile", RandomBaseTile);
                    Gen.SelectedTile = RandomBaseTile ? Random.Range(0, Gen.TileStorage.Data.Count) : EditorGUILayout.IntField("Base Layer Tile: ", Gen.SelectedTile);
                    GUILayout.Space(10);
                    TabMode = GUILayout.Toolbar(TabMode, new string[] { "Populate", "Erase", "Cut" });
                    Gen.MoveMode = TabMode;

                    GUILayout.EndVertical();
                }
                break;
        }

        GUILayout.Label("Editor Layer Generator");
        Preview = EditorGUILayout.Toggle("Is Editor Preview", Preview);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate"))
            Gen.GenerateLayer(Preview);
        if (GUILayout.Button("Clear"))
            Gen.RemoveLayer(Preview);//Gen.ClearLayer(true);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    private void TilesTab()
    {
        Rect LastRect;
        TileData tData = new TileData();
        GUILayout.Space(10);
        //Gen.TilesLenght = EditorGUILayout.IntField("Size", Gen.TilesLenght);

        /*var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty("Tiles");
        serializedObject.Update();
        EditorGUILayout.PropertyField(property, true);
        serializedObject.ApplyModifiedProperties();*/

        Gen.TileStorage = EditorGUILayout.ObjectField("Tile Storage", Gen.TileStorage, typeof(TileStorage), true) as TileStorage;

        GUILayout.BeginHorizontal("Box");
        if (GUILayout.Button("<-") && TileSelect > 0)
            TileSelect--;
        GUILayout.Label("Tile: [" + TileSelect + "]");
        if (GUILayout.Button("->") && TileSelect < Gen.TileStorage.Data.Count - 1)
            TileSelect++;
        GUILayout.EndHorizontal();
        //Propiedades:

        if (Gen.TileStorage.Data.Count > 0)
        {
            GUILayout.BeginVertical();
            //Gen.Tiles[TileSelect] = (TileBase)EditorGUILayout.ObjectField("Tile", Gen.Tiles[TileSelect], typeof(TileBase),true);
            if (Gen.TileStorage.Data[TileSelect].GetTileAt(1, 1) != null)
            {
                GUILayout.Label("Name: " + Gen.TileStorage.Data[TileSelect].Name);
                tData = Utils.GetTileData(Gen.TileStorage.Data[TileSelect].GetTileAt(1, 1));
                GUILayout.Space(80);
                LastRect = GUILayoutUtility.GetLastRect();
                LastRect.width = 50; LastRect.height = 50;
                LastRect.y += 15; LastRect.x = Screen.width / 2 - (LastRect.width / 2);
                GUI.DrawTexture(LastRect, tData.sprite.texture);
            }
            GUILayout.EndVertical();
        }

        if (GUILayout.Button("Chunk Location"))
        {
            Gen.ChangeChunkLocation(new Vector3Int(0, 0, 0), new Vector3Int(256, 0, 0));
        }
    }
}
