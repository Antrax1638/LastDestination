using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public enum GenType
{
    None,
    UnityType,
    UnityTypePrime,
    DynamicMoveType,
    CustomType
}

public enum GenValue
{
    Smooth,
    Hard,
    Hills,
    Custom
}

[System.Serializable]
public struct TerrainType
{
    public string Name;
    public Color Colour;
    public float Height;
    public int id;
}

[System.Serializable]
public struct TileProbability
{
    string Name;
    float Probability;
    int ID;
}

[System.Serializable]
public enum EncodingType 
{ 
    None,
    JPG,
    PNG,
    EXR
}

[RequireComponent(typeof(Grid))]
public class Generation : MonoBehaviour 
{
    public Rect Transform;
    public NInterpolation InterpolationType;
    public GenType Type;
    public bool ClampChunk = false;
    public bool SortingAuto = true;
    public GenValue Value;
    public string Name;

    public Vector2 Seed = new Vector2(-100000, 1000000);
    public int Elevation = 45;
    public int Smooth = 18;
    public int Multiplier = 5;
    public int Frequency = 12;
    public float Scale = 0.5f;
    public int ChunkSize = 1;

    //Dynamic Move Type:
    private System.Random Rand;
    public int MoveMode;
    public bool ResetOnMove = true;
    //Dynamic Move Type.

    //Unity Prime Type:
    public bool ActiveRegions;
    public TerrainType[] Regions;
    public FilterMode RegionsFilterMode;
    public Vector2 OctavesOffset;
    public Vector2 OctavesRange = new Vector2(-100000,100000);
    public int Octaves = 1;
    public float Persistance = 1;
    public float Lacunarity = 1;
    public EncodingType EncodeType = EncodingType.JPG;
    public Material GeneratedMaterial;
    //Unity Prime Type.

    //public TileBase[] Tiles;
   
    //Tiles Layers:
    public TileBase Cached;
    public TileStorage TileStorage;
    public int SelectedTile = 0;
    public int SelectedLayer = 0;
    //Tiles

    //privados:
    private Grid GridSystem;
    private TilemapCollider2D ColliderSystem;
    private Tilemap[] LayerMap;
    private NoiseBase Noise;
    private Texture2D GeneratedTexture;
    private Vector2 OldLocation;
    private bool Copy;
    private Rect CopyLocation;
    
    void Awake() 
    {
        GridSystem = GetComponent<Grid>();
        ColliderSystem = GetComponentInChildren<TilemapCollider2D>();

        if (TileStorage != null)
        {
            if (TileStorage.PixelsPerUnit <= 0)
                TileStorage.PixelsPerUnit = 100;
            GridSystem.cellSize = new Vector3(TileStorage.Width / TileStorage.PixelsPerUnit,TileStorage.Height / TileStorage.PixelsPerUnit, 0);
        }
    }

    void Start() 
    {
        GenerateLayer();   
    }

    public void Init() 
    {
        LayerMap = GetComponentsInChildren<Tilemap>();
        if (Octaves <= 0)
            Octaves = 1;
        if (ChunkSize <= 0)
            ChunkSize = 1;
    }

    public void GenerateLayer(bool preview = false) 
    {
        Init();
        int TileDeltaX = 0;

        switch (Type) 
        {
            default: break;
            case GenType.UnityType: Noise = new NoiseBase((double)Random.Range(Seed.x, Seed.y)); break;
            case GenType.CustomType: Noise = new NoiseBase((long)Random.Range(Seed.x, Seed.y)); break;
            case GenType.DynamicMoveType: Rand = new System.Random((int)Random.Range(Seed.x,Seed.y)); break;
        }

        LayerMap[SelectedLayer].gameObject.SetActive(false);

        switch (Type) 
        {
            //Custom Type - Unity Type se generan aca
            default:
                int height = 0;

                Noise.Frequency = Frequency;
		        Noise.heightAddition = Elevation;
		        Noise.heightMultplier = Multiplier;
		        Noise.Smooth = Smooth;
		        Noise.Interpolation = InterpolationType;
		        Noise.ClampChunk = ClampChunk;

                for (int x = (int)Transform.x; x < Transform.width; x++) 
                {
                    TileDeltaX++;
                    switch (Type)
                    {
                        default: break;
                        case GenType.UnityType:
                            height = Noise.GetNoise(x); 
                        break;
                        case GenType.CustomType:
                            height = Noise.GetNoise((int)(x - Transform.x), (int)(Transform.height - Transform.y)); 
                        break;
                    }

                    for (int y = (int)Transform.y; y < Transform.y + height; y++)
                    {
                        SetTileAt(x, y, preview);
                        if (Copy)
                            SetTileAt((int)CopyLocation.x + x, (int)CopyLocation.y + y, preview);
                        /*if (TileDeltaX <= ChunkSize)
                            SetTileAt(x - (SelectedLayer * ChunkSize), y, preview);
                        else
                        {
                            if (SelectedLayer < LayerMap.Length)
                                SelectedLayer++;
                            TileDeltaX = 0;
                            SetTileAt(x - (SelectedLayer * ChunkSize), y, preview);
                        }*/
                    }
                }
                
                
            break;
            
            //Unity Prime se genera aca:
            case GenType.UnityTypePrime:
                float[,] noiseMap;
                Noise = new NoiseBase();
                Noise.OctavesOffset = OctavesOffset;
                Noise.OctavesRange = OctavesRange;
                Noise.Seed = (int)Random.Range(Seed.x, Seed.y);
                noiseMap = Noise.GetNoise((int)Transform.width, (int)Transform.height, Scale, Octaves, Persistance, Lacunarity);
                var w = noiseMap.GetLength(0);
                var h = noiseMap.GetLength(1);
                
                //Cosntuctor de Textura y material:
                GeneratedTexture = new Texture2D(w, h);
                Color[] colorMap = new Color[w * h];
                for(int y = 0; y < h; y++ )
                {
                    for(int x = 0; x < w; x++)
                    {
                        if(ActiveRegions){
                            float currentHeight = noiseMap[x, y];
                            for (int i = 0; i < Regions.Length; i++) { 
                                if(currentHeight <= Regions[i].Height)
                                    colorMap[Utils.ToIndex(x,y,w)] = Regions[i].Colour;
                            }
                        }
                        else
                            colorMap[Utils.ToIndex(x, y, w)] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
                    }
                }
                GeneratedTexture.filterMode = RegionsFilterMode;
                GeneratedTexture.SetPixels(colorMap);
                GeneratedTexture.Apply();
                if(GeneratedMaterial != null)
                    GeneratedMaterial.mainTexture = GeneratedTexture;
                //Generacion de Mapa (inicio):
                for (int x = (int)Transform.x; x < w; x++)
                {
                    TileDeltaX++;
                    for (int y = (int)Transform.y; y < h; y++)
                    {
                        for (int i = 0; i < Regions.Length; i++) 
                        {
                            if(colorMap[Utils.ToIndex(x,y,w)] == Regions[i].Colour)
                                SelectedTile = Regions[i].id;
                        }

                        SetTileAt(x , y, preview);
                        if (Copy)
                            SetTileAt((int)CopyLocation.x + x, (int)CopyLocation.y + y, preview);
                            
                        
                        /*if (TileDeltaX <= ChunkSize)
                            SetTileAt(x-(SelectedLayer*ChunkSize), y, preview);
                        else 
                        {
                            if(SelectedLayer < LayerMap.Length)
                                SelectedLayer++;
                            TileDeltaX = 0;
                            SetTileAt(x - (SelectedLayer * ChunkSize), y, preview);
                        }*/
                    }
                }
                //Generacion de Mapa (fin)
            return;

            //Dynamic Move Type se genera aca:
            case GenType.DynamicMoveType:
                if (ResetOnMove) {
                    OldLocation.x = Transform.x;
                    OldLocation.y = Transform.y;
                }
                
                for (int i = 0; i < Octaves; i++) 
                {
                    switch (Rand.Next(0, 5)) 
                    {
                        case 0: Transform.x--; break;
                        case 1: Transform.x++; break;
                        case 2: Transform.y--; break;
                        case 3: Transform.y++; break;
                    }

                    switch (MoveMode) {
                        default: break;
                        case 0: SetTileAt((int)Transform.x, (int)Transform.y, preview); break;
                        case 1: RemoveTileAt((int)Transform.x, (int)Transform.y, preview); break;
                        case 2: Cached = RemoveTileAt((int)Transform.x, (int)Transform.y, preview); break;
                    }

                    
                }
                if (ResetOnMove)
                {
                    Transform.x = OldLocation.x;
                    Transform.y = OldLocation.y;
                }
            break;

        }
        LayerMap[SelectedLayer].gameObject.SetActive(true);

        SelectedLayer = 0;
       
    }

    public void ClearLayer(bool preview = false) 
    {
        if (!preview)
            LayerMap[SelectedLayer].ClearAllTiles();
        else
            LayerMap[SelectedLayer].ClearAllEditorPreviewTiles();
    }

    public void RemoveLayer(bool preview = false) 
    {
        for (int y = 0; y < Transform.height+Elevation; y++)
        {
            for (int x = 0; x < Transform.width; x++)
            {
                RemoveTileAt(x, y, preview);
                
            }     
        }
    }

    public void SetTileAt(int x, int y, bool preview)
    {
        if (SelectedTile >= TileStorage.Data.Count)
        {
            Debug.LogError("Tile ID[" + SelectedTile + "] is equal or greather than maximum tile capacity");
            return;
        }

        if (!preview)
        {
            LayerMap[SelectedLayer].SetTile(new Vector3Int(x, y, 0), SelectTile());
        }
        else
        {
            LayerMap[SelectedLayer].SetEditorPreviewTile(new Vector3Int(x, y, 0), SelectTile());
        }
            
        
    }

    public TileBase RemoveTileAt(int x, int y, bool preview = false) 
    {
        if (LayerMap[SelectedLayer].GetTile(new Vector3Int(x, y, 0)) == null && !preview)
            return null;
        
        TileBase cached = LayerMap[SelectedLayer].GetTile(new Vector3Int(x, y, 0));
        if (!preview)
        {
            LayerMap[SelectedLayer].SetTile(new Vector3Int(x, y, 0), null);
            return cached;
        }
        else 
        {
            LayerMap[SelectedLayer].SetEditorPreviewTile(new Vector3Int(x, y, 0), null);
            return cached;
        }
        
    }

    public void ChangeChunkLocation(Vector3Int oldPos,Vector3Int newPos)
    {
        Rect oldTransform = Transform;
        Transform = new Rect(-ChunkSize, 0, Transform.x, ChunkSize);
        Copy = true;
        CopyLocation = new Rect(ChunkSize + oldTransform.width, 0, 0, 0);
        SelectedLayer = 1;
        SelectedTile = 0;
        GenerateLayer();

        Transform = oldTransform;
    }

    public void SetGenValues()
    {
        switch (Value)
        {
            default: break;
            case GenValue.Smooth:
                Seed = new Vector2(3101631, 138804906);
                Elevation = 53;
                Smooth = 26;
                Multiplier = 8;
                Frequency = 25;
                break;

            case GenValue.Hard:
                Seed = new Vector2(-33101631, 15239720671);
                Elevation = 75;
                Smooth = 19;
                Multiplier = 5;
                Frequency = 11;
                break;

            case GenValue.Hills:
                Seed = new Vector2(-1000000, 10000000);
                Elevation = 80;
                Smooth = 8;
                Multiplier = 15;
                Frequency = 5;
                break;

        }

    }

    public Texture2D GetGeneratedTexture()
    {
        return GeneratedTexture;
    }

    public Tilemap GetLayer(int index){
        return LayerMap[index];
    }

    public Tilemap[] GetLayers()
    {
        return LayerMap;
    }

    private TileBase SelectTile(int x = -1, int y = -1)
    {
        if (x > -1 && y > -1)
            return TileStorage.Data[SelectedTile].GetTileAt(x,y);
        else
            return TileStorage.Data[SelectedTile].GetTileAt(1,1);
    }

}

