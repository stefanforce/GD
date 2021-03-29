using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;




public class Map : MonoBehaviour
{
    public class BoardTile
    {
        public BoardTile(int x,int y, Tile tile)
        {
            this.x = x;
            this.y = y;
            this.tile = tile;
            this.bambooCount = 0;
        }
        public int x;
        public int y;
        public Tile tile = null;
        public bool isPond = false;
        public int bambooCount;
        public string Color;
    }

    public Canvas debugCanvas;
    int currentAction = 1;

    private Grid grid;
    [SerializeField] private Tilemap interactiveMap = null;
    [SerializeField] private Tilemap pathMap = null;
    [SerializeField] private Tilemap pandaMap = null;
    [SerializeField] private Tilemap farmerMap = null;
    [SerializeField] private Tilemap bambooMap = null;
    [SerializeField] private Tile pandaTile = null;
    [SerializeField] private Tile farmerTile = null;
    [SerializeField] private Tile hoverTile = null;
    [SerializeField] private Tile centerTile = null;
    public Button landButton;
    public Button farmerButton;
    public Button pandaButton;
    public Tile[] FlatTilesArray = new Tile[3];
    public Tile[] bambooTileArray = new Tile[5];
    BoardTile[,] board = new BoardTile[100, 100];

    private Vector3Int previousMousePos = new Vector3Int();
    private Vector3Int previousPandaPos = new Vector3Int(5, 5, 0);
    private Vector3Int previousFarmerPos = new Vector3Int(5, 5, 0);


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
        pandaMap.SetTile(previousPandaPos, pandaTile);
        farmerMap.SetTile(previousFarmerPos, farmerTile);
        board[pond.x, pond.y].isPond = true;

        if (currentAction == 1)
        {
            //Mouse over -> highlight tile
            if (!mousePos.Equals(previousMousePos))
            {
                interactiveMap.SetTile(previousMousePos, null); // Remove old hoverTile
                interactiveMap.SetTile(mousePos, hoverTile);
                previousMousePos = mousePos;
            }

            // Left mouse click -> add path tile
            if (Input.GetMouseButtonDown(0))
            {
                int number = UnityEngine.Random.Range(0, 3);
                SelectedTile = FlatTilesArray[number];
                
                if (neighbourCount(mousePos) >= 2 && board[mousePos.x,mousePos.y]==null)
                {
                    PlaceTile(mousePos, pathMap, SelectedTile);
                    setColor(number, mousePos);
                }

            }

            // Right mouse click -> remove path tile
            if (Input.GetMouseButtonDown(1))
            {
                PlaceTile(mousePos, pathMap, null);
            }
        }

        if (currentAction == 2)
        {
            //Mouse over -> highlight tile
            if (!mousePos.Equals(previousMousePos))
            {
                interactiveMap.SetTile(previousMousePos, null); // Remove old hoverTile
                interactiveMap.SetTile(mousePos, farmerTile);
                previousMousePos = mousePos;
            }

            // Left mouse click -> add path tile
            if (Input.GetMouseButtonDown(0))
            {
                if(board[mousePos.x, mousePos.y] != null && (isSameLine1(previousFarmerPos, mousePos) ||
                    isSameLine2(previousFarmerPos, mousePos) ||
                    isSameLine3(previousFarmerPos, mousePos) ||
                    isSameLine4(previousFarmerPos, mousePos) ||
                   previousFarmerPos.y == mousePos.y))
                {
                    farmerMap.SetTile(previousFarmerPos, null);
                    farmerMap.SetTile(mousePos, farmerTile);
                    previousFarmerPos = mousePos;
                    if(mousePos!=pond)
                    addBamboo(mousePos);
                    GrowBambooNeighour(mousePos);
                }

            }
        }

        if (currentAction == 3)
        {
            //Mouse over -> highlight tile
            if (!mousePos.Equals(previousMousePos))
            {
                interactiveMap.SetTile(previousMousePos, null); // Remove old hoverTile
                interactiveMap.SetTile(mousePos, pandaTile);
                previousMousePos = mousePos;
            }

            // Left mouse click -> add path tile
            if (Input.GetMouseButtonDown(0))
            {
                if (board[mousePos.x, mousePos.y] != null && 
                    (isSameLine1(previousPandaPos,mousePos) ||
                    isSameLine2(previousPandaPos, mousePos) || 
                    isSameLine3(previousPandaPos, mousePos) || 
                    isSameLine4(previousPandaPos, mousePos) || 
                    previousPandaPos.y==mousePos.y))
                {
                    pandaMap.SetTile(previousPandaPos, null);
                    pandaMap.SetTile(mousePos, pandaTile);
                    previousPandaPos = mousePos;
                    if (mousePos != pond)
                       removeBamboo(mousePos);
                }
            }
        }
        debugCanvas.GetComponentInChildren<UnityEngine.UI.Button>().GetComponentInChildren<UnityEngine.UI.Text>().text = "Mouse pos: " + mousePos.ToString();
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
    void GrowBambooNeighour(Vector3Int pos)
    {
        if (pos.x > 0 && pos.y > 0)
        {
            if (board[pos.x - 1, pos.y] != null && board[pos.x, pos.y].Color == board[pos.x - 1, pos.y].Color )
                addBamboo(new Vector3Int(pos.x - 1, pos.y, 0));
            if (board[pos.x + 1, pos.y] != null && board[pos.x, pos.y].Color == board[pos.x + 1, pos.y].Color )
                addBamboo(new Vector3Int(pos.x + 1, pos.y, 0));

            if (pos.y % 2 == 0)
            {
                if (board[pos.x - 1, pos.y + 1] != null && board[pos.x, pos.y].Color == board[pos.x - 1, pos.y+1].Color )
                {
                    addBamboo(new Vector3Int(pos.x - 1, pos.y + 1, 0));
                }
                    
                if (board[pos.x, pos.y - 1] != null && board[pos.x, pos.y].Color == board[pos.x , pos.y -1].Color )
                {
                    addBamboo(new Vector3Int(pos.x, pos.y - 1, 0));
                }
                if (board[pos.x - 1, pos.y - 1] != null && board[pos.x, pos.y].Color == board[pos.x - 1, pos.y -1].Color )
                    {
                    addBamboo(new Vector3Int(pos.x - 1, pos.y - 1, 0));
                }
                
                if (board[pos.x, pos.y + 1] != null && board[pos.x, pos.y].Color == board[pos.x, pos.y +1].Color )
                {
                    addBamboo(new Vector3Int(pos.x, pos.y + 1, 0));

                }


            }

            if (pos.y % 2 == 1)
            {
                if (board[pos.x, pos.y - 1] != null && board[pos.x, pos.y].Color == board[pos.x , pos.y - 1].Color )
                {
                    addBamboo(new Vector3Int(pos.x, pos.y - 1, 0));
                }
                
                if (board[pos.x + 1, pos.y - 1] != null && board[pos.x, pos.y].Color == board[pos.x +1, pos.y - 1].Color )
                {
                    addBamboo(new Vector3Int(pos.x + 1, pos.y - 1, 0));
                }
                
                if (board[pos.x, pos.y + 1] != null && board[pos.x, pos.y].Color == board[pos.x , pos.y + 1].Color )
                { 
                    addBamboo(new Vector3Int(pos.x, pos.y + 1, 0));
                }
                
                if (board[pos.x + 1, pos.y + 1] != null && board[pos.x, pos.y].Color == board[pos.x + 1, pos.y + 1].Color )
                {
                    addBamboo(new Vector3Int(pos.x + 1, pos.y + 1, 0));
                }
                
            }
        }
    }
   public void changeActionToLand()
    {
        currentAction = 1;
    }
    public void changeActionToFarmer()
    {
        currentAction = 2;
    }
    public void changeActionToPanda()
    {
        currentAction = 3;
    }
    bool isSameLine1(Vector3Int pos1, Vector3Int pos2)
    {
        var coords = new (int x, int y)[]
        {
            (1,1),(0,1),(-1,-1),(0,-1),(-1,1),(0,1),(1,-1),(0,-1)
        };
        while (pos2.x > pos1.x || pos2.y > pos1.y)
        {
            if (pos1.y % 2 == 1)
            {
                pos1.x += coords[0].x;
                pos1.y += coords[0].y;
                if (pos1.x == pos2.x && pos1.y == pos2.y)
                    return true;
            }
            if (pos1.y % 2 == 0)
            {
                pos1.x += coords[1].x;
                pos1.y += coords[1].y;
                if (pos1.x == pos2.x && pos1.y == pos2.y)
                    return true;
            }
        }

            return false;
    }
    bool isSameLine2(Vector3Int pos1, Vector3Int pos2)
    {
        var coords = new (int x, int y)[]
        {
            (-1,-1),(0,-1)
        };

        while (pos2.x < pos1.x || pos2.y < pos1.y)
        {
            if (pos1.y % 2 == 0)
            {
                pos1.x += coords[0].x;
                pos1.y += coords[0].y;
                if (pos1.x == pos2.x && pos1.y == pos2.y)
                    return true;
            }
            if (pos1.y % 2 == 1)
            {
                pos1.x += coords[1].x;
                pos1.y += coords[1].y;
                if (pos1.x == pos2.x && pos1.y == pos2.y)
                    return true;
            }
        }

        return false;
    }
    bool isSameLine3(Vector3Int pos1, Vector3Int pos2)
    {
        var coords = new (int x, int y)[]
        {
            (-1,1),(0,1)
        };

        while (pos2.x < pos1.x || pos2.y > pos1.y)
        {
            if (pos1.y % 2 == 0)
            {
                pos1.x += coords[0].x;
                pos1.y += coords[0].y;
                if (pos1.x == pos2.x && pos1.y == pos2.y)
                    return true;
            }
            if (pos1.y % 2 == 1)
            {
                pos1.x += coords[1].x;
                pos1.y += coords[1].y;
                if (pos1.x == pos2.x && pos1.y == pos2.y)
                    return true;
            }
        }

        return false;
    }
    bool isSameLine4(Vector3Int pos1, Vector3Int pos2)
    {
        var coords = new (int x, int y)[]
        {
            (1,-1),(0,-1)
        };

        while (pos2.x > pos1.x || pos2.y < pos1.y)
        {
            if (pos1.y % 2 == 1)
            {
                pos1.x += coords[0].x;
                pos1.y += coords[0].y;
                if (pos1.x == pos2.x && pos1.y == pos2.y)
                    return true;
            }
            if (pos1.y % 2 == 0)
            {
                pos1.x += coords[1].x;
                pos1.y += coords[1].y;
                if (pos1.x == pos2.x && pos1.y == pos2.y)
                    return true;
            }
        }

        return false;
    }
    void addBamboo(Vector3Int pos)
    {
        board[pos.x, pos.y].bambooCount += 1;
        int count = board[pos.x, pos.y].bambooCount;
        if (count > 4)
        {
            board[pos.x, pos.y].bambooCount = 4;
            count = 4;
        }
        bambooMap.SetTile(pos, bambooTileArray[count]);
    }
    void removeBamboo(Vector3Int pos)
    {
        board[pos.x, pos.y].bambooCount -= 1;
        int count = board[pos.x, pos.y].bambooCount;
        if (count < 0)
        {
            board[pos.x, pos.y].bambooCount = 0;
            count = 0;
        }
        bambooMap.SetTile(pos, bambooTileArray[count ]);
    }
    void setColor(int number,Vector3Int pos)
    {
        if (number == 0)
            board[pos.x, pos.y].Color = "Yellow";
        if (number == 1)
            board[pos.x, pos.y].Color = "Green";
        if (number == 2)
            board[pos.x, pos.y].Color = "Red";
    }
 
}