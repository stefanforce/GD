using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Map : MonoBehaviour
{
    public class BoardTile
    {
        public BoardTile(int x,int y, Tile tile)
        {
            this.x = x;
            this.y = y;
            this.tile = tile;
        }
        public int x;
        public int y;
        public Tile tile = null;
        public bool isPond = false;
    }


    public Canvas debugCanvas;

    private Grid grid;
    [SerializeField] private Tilemap interactiveMap = null;
    [SerializeField] private Tilemap pathMap = null;
    [SerializeField] private Tile hoverTile = null;
    [SerializeField] private Tile centerTile = null;
    public Tile[] FlatTilesArray = new Tile[3];
    BoardTile[,] board = new BoardTile[100, 100];


    private Vector3Int previousMousePos = new Vector3Int();

    // Start is called before the first frame update
    void Start()
    {
        grid = gameObject.GetComponent<Grid>();
    }

   

    void Update()
    {
        Tile SelectedTile;
        Vector3Int mousePos = GetMousePosition();
        Vector3Int pond = new Vector3Int(5, 5, 0);
        PlaceTile(pond, pathMap, centerTile);
        board[pond.x, pond.y].isPond = true;

        //Mouse over -> highlight tile
        if (!mousePos.Equals(previousMousePos))
        {
            interactiveMap.SetTile(previousMousePos, null); // Remove old hoverTile
            interactiveMap.SetTile(mousePos, hoverTile);
            previousMousePos = mousePos;
        }

        // Left mouse click -> add path tile
        if (Input.GetMouseButton(0))
        {
            SelectedTile = FlatTilesArray[Random.Range(0, 3)];
            if (neighbourCount(mousePos) >= 2)
            {
                PlaceTile(mousePos, pathMap, SelectedTile);
            }

        }

        // Right mouse click -> remove path tile
        if (Input.GetMouseButton(1))
        {
            PlaceTile(mousePos, pathMap, null);
        }
        //debugCanvas.GetComponentInChildren<UnityEngine.UI.Button>().GetComponentInChildren<UnityEngine.UI.Text>().text = "Mouse pos: " + GetMousePosition().x.ToString();
    }

    Vector3Int GetMousePosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        return grid.WorldToCell(mouseWorldPos);
    }
    void PlaceTile(Vector3Int pos,Tilemap tilemap, Tile tile)
    {
        
            board[pos.x, pos.y] = new BoardTile(pos.x,pos.y,tile);
            tilemap.SetTile(pos, tile);
   
    }
    int neighbourCount(Vector3Int pos)
    {
        int count = 0;
        if (pos.x > 0 && pos.y > 0)
        {
            count += verifyNeigbour(pos.x - 1, pos.y);
            count += verifyNeigbour(pos.x + 1, pos.y);

            if (pos.y % 2 == 0)
            {
                count += verifyNeigbour(pos.x - 1, pos.y + 1);
                count += verifyNeigbour(pos.x, pos.y - 1);
                count += verifyNeigbour(pos.x - 1, pos.y - 1);
                count += verifyNeigbour(pos.x, pos.y + 1);
            }

            if (pos.y % 2 == 1)
            {
                count += verifyNeigbour(pos.x, pos.y - 1);
                count += verifyNeigbour(pos.x + 1, pos.y - 1);
                count += verifyNeigbour(pos.x, pos.y + 1);
                count += verifyNeigbour(pos.x + 1, pos.y + 1);
            }
        }
        return count;
    }
    int verifyNeigbour(int x, int y) { 
    if (board[x, y] != null) {
        if (board[x, y].isPond) {
            return 2;
        }
        return 1;
    }
    return 0;
}
}