using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Flags]
public enum TileArea 
{ 
    Left = 1,
    Right = 2,
    Up = 4,
    Down = 8
}

public struct TileManagerProperties
{
    public TileArea Flags;
    public bool Surrounded;

}


public class TileManager : MonoBehaviour 
{
    public TileStorage TileStorage;
    public GameObject Map;

    private Tilemap Layer;

    //Tile Actual:
    private TileManagerProperties CurrentProp;
    private Vector2Int NewTile = new Vector2Int(1,1);
    private int Other = -1;

    public TileManagerProperties IsSurrounded(Vector3Int position)
    {
        TileBase[] other = new TileBase[4];
        TileManagerProperties Prop = new TileManagerProperties();

        other[0] = Map.GetComponent<Tilemap>().GetTile( Utils.GetRightTile(position)    );
        other[1] = Map.GetComponent<Tilemap>().GetTile( Utils.GetUpTile(position)       );
        other[2] = Map.GetComponent<Tilemap>().GetTile( Utils.GetLeftTile(position)     );
        other[3] = Map.GetComponent<Tilemap>().GetTile( Utils.GetDownTile(position)     );
        
        Prop.Flags = 0;
        if (other[0] != null)
            Prop.Flags += (int)TileArea.Right;
        if (other[1] != null)
            Prop.Flags += (int)TileArea.Up;
        if (other[2] != null)
            Prop.Flags += (int)TileArea.Left;
        if (other[3] != null)
            Prop.Flags += (int)TileArea.Down;

        Prop.Surrounded = ((Prop.Flags) == (TileArea.Left | TileArea.Right | TileArea.Up | TileArea.Down));
        return Prop;
    }

    public void TileCorrection(Vector3Int position,int tileIndex) 
    {
        TileBase CurrentTile = Layer.GetTile(position);
        if (CurrentTile != null) 
        {
            CurrentProp = IsSurrounded(position);
            NewTile.x = 1; NewTile.y = 1;
            Other = -1;
            
            switch (CurrentProp.Flags)
            {
                //Alone
                default: Other = 7; break;

                //Left
                case TileArea.Left: Other = 3; break;
                case TileArea.Left | TileArea.Right: Other = 4; break;
                case TileArea.Left | TileArea.Up: NewTile.x = 2; NewTile.y = 2; break;
                case TileArea.Left | TileArea.Down: NewTile.x = 2; NewTile.y = 0; break;

                case TileArea.Left | TileArea.Right | TileArea.Up: NewTile.x = 1; NewTile.y = 2; break;
                case TileArea.Left | TileArea.Right | TileArea.Down: NewTile.x = 1; NewTile.y = 0; break;
                case TileArea.Left | TileArea.Up | TileArea.Down: NewTile.x = 2; NewTile.y = 1; break;

                //Up
                case TileArea.Up: Other = 1; break;
                case TileArea.Up | TileArea.Right: NewTile.x = 0; NewTile.y = 2; break;
                case TileArea.Up | TileArea.Down: Other = 5; break;
                case TileArea.Up | TileArea.Right | TileArea.Down: NewTile.x = 0; NewTile.y = 1; break;
                //Down
                case TileArea.Down: Other = 0; break;
                case TileArea.Down | TileArea.Right: NewTile.x = 0; NewTile.y = 0; break;
                //Right
                case TileArea.Right: Other = 2; break;

                //Surrounded
                case TileArea.Left | TileArea.Right | TileArea.Up | TileArea.Down: NewTile.x = 1; NewTile.y = 1; break;
            }

            /*if (Other >= 0 && Other <= 7)
                Layer.SetTile(position, TileStorage.Data[tileIndex].GetTileExAt(Other));
            else*/
            Layer.SetTile(position, TileStorage.Data[tileIndex].GetTileAt(NewTile));
        }
        
    }

    void Awake()
    {
        Layer = Map.GetComponent<Tilemap>();
    }

    void Start() {
        //print(IsSurrounded(new Vector3Int(0, 0, 0)).Surrounded);
        for(int x = 0; x < 256; x++)
        {
            for (int y = 0; y < 59; y++)
            {
                TileCorrection(new Vector3Int(x, y, 0), 0);
            }
        }
    }
}
