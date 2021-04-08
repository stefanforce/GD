using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;




public class Map : MonoBehaviour
{
    public class BoardTile
    {
        public BoardTile(int x, int y, Tile tile)
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
        public bool hasNoGrowth = false;
        public bool hasDoubleGrowth = false;
    }

    public class Player
    {
        public string playerName;
        public int score;
        public Dictionary<string, int> PlayerBamboo =new Dictionary<string, int>();
        public Player()
        {
            PlayerBamboo.Add("Yellow", 0);//Yellow
            PlayerBamboo.Add("Red", 0);//Red
            PlayerBamboo.Add("Green", 0);//Green

        }
        public Player(string playerName):this()
        {
            this.playerName = playerName;
            
        }
    }

    public Canvas debugCanvas;


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
    public Button confirmActionButton;
    public Button eventText;
    public GameObject ScoreBox;
    public GameObject[] tileButtons = new GameObject[3];
    public GameObject[] diceButtons = new GameObject[5];
    GameObject[] ScoreBoxList = new GameObject[4];

    public Tile[] FlatTilesArray = new Tile[3];
    public Tile[] bambooTileArray = new Tile[5];
    public Tile[] diceSprites = new Tile[5];
    BoardTile[,] board = new BoardTile[100, 100];
    Player[] Players = new Player[2];

    private Vector3Int previousMousePos = new Vector3Int();
    private Vector3Int previousPandaPos = new Vector3Int(5, 5, 0);
    private Vector3Int previousFarmerPos = new Vector3Int(5, 5, 0);
    private Vector3Int pond = new Vector3Int(5, 5, 0);
    public enum State
    {

        WEATHER,
        WEATHER2,
        ACTION1,
        ACTION2,
        ACTION3

    };
    State currentState;

    int currentAction = 0;
    int actionNumber = 2;
    bool WeatherOnce = true;
    bool Weather2Once = true;
    bool Action1Once = true;
    bool Action2Once = true;
    bool Action3Once = true;
    bool sameAction = false;
    bool selected=false;
    int FirstAction = 4;
    int randomWeatherCondition;
    int turnNumber=0;
    int selectedButton = 3;
    int selectedNumber = 3;
    Tile TileToBePlaced;
    Tile[] SelectedTile = new Tile[10];
    int[] number = new int[10];
    int playerCount = 2;
    bool selectedDice = false;

    // Start is called before the first frame update
    void Start()
    {
        grid = gameObject.GetComponent<Grid>();
        PlaceTile(pond, pathMap, centerTile);
        pandaMap.SetTile(previousPandaPos, pandaTile);
        farmerMap.SetTile(previousFarmerPos, farmerTile);
        board[pond.x, pond.y].isPond = true;
        currentState = State.ACTION1;
        for (int i = 0; i < 3; i++)
            tileButtons[i].SetActive(false);
        for(int i=0;i<5;i++)
            diceButtons[i].SetActive(false);
        Players[0] = new Player("Player1");
        Players[1] = new Player("Player2");
        instantiateScoreBoxes();

       
    }


    void Update()
    {


        //if(myturn) do function below
        if (currentState == State.WEATHER)
        {
            
            Action2Once = true;
            Action3Once = true;
            FirstAction = 4;
            if (WeatherOnce == true)
                WeatherSelection();
        }
       
        if(currentState==State.WEATHER2)
        {
            WeatherOnce = true;
            if (Weather2Once == true)
                doWeatherCondition();
        }

        if (currentState == State.ACTION1)
        {
            
            Weather2Once = true;
            WeatherOnce = true;

            
            if (Action1Once == true && selected==true)
            {
                makeHoverTiles();
                performAction(currentAction);
            }
            else
            {
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Press confirm to end this action!";
                interactiveMap.SetTile(GetMousePosition(), null);

            }
        }
        if (currentState == State.ACTION2)
        {
       
            Action1Once = true;
            
            if (Action2Once == true)
            {
                performAction(currentAction);
                makeHoverTiles();
            }
            else
            {
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Press confirm to end this action!";
                interactiveMap.SetTile(GetMousePosition(), null);

            }
        }
        if (currentState == State.ACTION3)
        {
            Action2Once = true;
           
            if (Action3Once == true)
            {
                performAction(currentAction);
                makeHoverTiles();
            }
            else
            {
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Press confirm to end this action!";
                interactiveMap.SetTile(GetMousePosition(), null);

            }
        }
        for(int k=0;k<playerCount;k++)
        {
            ScoreBoxList[k].transform.Find("PlayerName").GetComponentInChildren<UnityEngine.UI.Text>().text = Players[k].playerName;
            ScoreBoxList[k].transform.Find("ScoreText").GetComponentInChildren<UnityEngine.UI.Text>().text = "Score : " + Players[k].score.ToString();
            ScoreBoxList[k].transform.Find("RedBambooText").GetComponentInChildren<UnityEngine.UI.Text>().text = "Red bamboo : " + Players[k].PlayerBamboo["Red"].ToString();
            ScoreBoxList[k].transform.Find("YellowBambooText").GetComponentInChildren<UnityEngine.UI.Text>().text = "Yellow bamboo : " + Players[k].PlayerBamboo["Yellow"].ToString();
            ScoreBoxList[k].transform.Find("GreenBambooText").GetComponentInChildren<UnityEngine.UI.Text>().text = "Green bamboo : " + Players[k].PlayerBamboo["Green"].ToString();
        }
        

        debugCanvas.GetComponentInChildren<UnityEngine.UI.Button>().GetComponentInChildren<UnityEngine.UI.Text>().text = "PlayerName : "+Players[turnNumber%playerCount].playerName +" CurrentState: " + currentState.ToString();
    }

    private void WeatherSelection()
    {
        actionNumber = 2;
        randomWeatherCondition= UnityEngine.Random.Range(0, 6);
        switch (randomWeatherCondition)
        {
            case 0://Sun

                actionNumber += 1;
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Rolled the Sun dice,you have an extra action! Press confrim";
                
                //eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Press the confirm button to validate the roll!";
                break;
            case 1://Rain
              
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Rolled the Rain dice,you can place a bamboo on a tile!";
                break;
            case 2://Wind
                sameAction = true;
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Rolled the Wind dice,you can do same action twice! Press confirm";
                //Thread.Sleep(5000);
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Press the confirm button to validate the roll!";
                break;
            case 3://Storm
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Rolled the Storm dice,you can take a bamboo from any tile!";
                break;
            case 4://Clouds
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Rolled the Cloud dice,you can take any tile improvement!";
                break;
            case 5://Random
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Rolled the ? dice,you can decide which condition to take!";
               // while (selectedDice != true)
                   // SelectDice();
                break;
        }
 
        WeatherOnce = false;
    }

     void SelectDice()
    {

        for(int i=0;i<5;i++)
        {
            diceButtons[i].SetActive(true);
            diceButtons[i].GetComponent<Image>().sprite =diceSprites[i].sprite;

        }
       
    }

    private void doWeatherCondition()
    {
        switch (randomWeatherCondition)
        {
            case 0://Sun
                break;
            case 1://Rain
                doRainCondition();
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Select the tile you want to grow a bamboo on!";
                break;
            case 2://Wind
                break;
            case 3://Storm
                doStormCondition();
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Select the tile you want to take a bamboo from!";
                break;
            case 4://Clouds
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "To be implemented";
                break;
            case 5://Random
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "To be implemented";
                break;
        }

        WeatherOnce = false;
    }

    private void doRainCondition()
    {
        Vector3Int mousePos = GetMousePosition();
        if (!mousePos.Equals(previousMousePos))
        {
            interactiveMap.SetTile(previousMousePos, null); // Remove old hoverTile
            interactiveMap.SetTile(mousePos, farmerTile);
            previousMousePos = mousePos;
        }
        if (Input.GetMouseButtonDown(0))
        {
            addBamboo(mousePos);
            Weather2Once = false;
            interactiveMap.SetTile(mousePos, null);
            eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Press the confirm button to validate the roll!";

        }
    }
    private void doStormCondition()
    {
        Vector3Int mousePos = GetMousePosition();
        if (!mousePos.Equals(previousMousePos))
        {
            interactiveMap.SetTile(previousMousePos, null); // Remove old hoverTile
            interactiveMap.SetTile(mousePos, pandaTile);
            previousMousePos = mousePos;
        }
        if (Input.GetMouseButtonDown(0))
        {
            removeBamboo(mousePos);
            Weather2Once = false;
            interactiveMap.SetTile(mousePos, null);
            eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Press the confirm button to validate the roll!";

        }
    }

    Vector3Int GetMousePosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        return grid.WorldToCell(mouseWorldPos);
    }
    void PlaceTile(Vector3Int pos, Tilemap tilemap, Tile tile)
    {

        board[pos.x, pos.y] = new BoardTile(pos.x, pos.y, tile);
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
    int verifyNeigbour(int x, int y)
    {
        if (board[x, y] != null)
        {
            if (board[x, y].isPond)
            {
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
            if (board[pos.x - 1, pos.y] != null && board[pos.x, pos.y].Color == board[pos.x - 1, pos.y].Color)
                addBamboo(new Vector3Int(pos.x - 1, pos.y, 0));
            if (board[pos.x + 1, pos.y] != null && board[pos.x, pos.y].Color == board[pos.x + 1, pos.y].Color)
                addBamboo(new Vector3Int(pos.x + 1, pos.y, 0));

            if (pos.y % 2 == 0)
            {
                if (board[pos.x - 1, pos.y + 1] != null && board[pos.x, pos.y].Color == board[pos.x - 1, pos.y + 1].Color)
                {
                    addBamboo(new Vector3Int(pos.x - 1, pos.y + 1, 0));
                }

                if (board[pos.x, pos.y - 1] != null && board[pos.x, pos.y].Color == board[pos.x, pos.y - 1].Color)
                {
                    addBamboo(new Vector3Int(pos.x, pos.y - 1, 0));
                }
                if (board[pos.x - 1, pos.y - 1] != null && board[pos.x, pos.y].Color == board[pos.x - 1, pos.y - 1].Color)
                {
                    addBamboo(new Vector3Int(pos.x - 1, pos.y - 1, 0));
                }

                if (board[pos.x, pos.y + 1] != null && board[pos.x, pos.y].Color == board[pos.x, pos.y + 1].Color)
                {
                    addBamboo(new Vector3Int(pos.x, pos.y + 1, 0));

                }


            }

            if (pos.y % 2 == 1)
            {
                if (board[pos.x, pos.y - 1] != null && board[pos.x, pos.y].Color == board[pos.x, pos.y - 1].Color)
                {
                    addBamboo(new Vector3Int(pos.x, pos.y - 1, 0));
                }

                if (board[pos.x + 1, pos.y - 1] != null && board[pos.x, pos.y].Color == board[pos.x + 1, pos.y - 1].Color)
                {
                    addBamboo(new Vector3Int(pos.x + 1, pos.y - 1, 0));
                }

                if (board[pos.x, pos.y + 1] != null && board[pos.x, pos.y].Color == board[pos.x, pos.y + 1].Color)
                {
                    addBamboo(new Vector3Int(pos.x, pos.y + 1, 0));
                }

                if (board[pos.x + 1, pos.y + 1] != null && board[pos.x, pos.y].Color == board[pos.x + 1, pos.y + 1].Color)
                {
                    addBamboo(new Vector3Int(pos.x + 1, pos.y + 1, 0));
                }

            }
        }
    }
    public void changeActionToLand()
    {
        selected = false;
        currentAction = 1;
        selectedButton = 3;
        if(FirstAction!=1)
        TileSelection();
        
 
    }
    public void changeActionToFarmer()
    {
        currentAction = 2;
    }
    public void changeActionToPanda()
    {
        currentAction = 3;
    }
    public void changeState()
    {
        if (currentState == State.WEATHER)
        {
            if (randomWeatherCondition == 0 || randomWeatherCondition == 2)
                currentState = State.ACTION1;
            else
                currentState = State.WEATHER2;
        }
        else if (currentState == State.WEATHER2)
            currentState = State.ACTION1;
        else if (currentState == State.ACTION1)
        {
            currentState = State.ACTION2;
            currentAction = 0;
        }

        else
        {
            if (actionNumber == 3)
            {
                if (currentState == State.ACTION2)
                {
                    currentState = State.ACTION3;
                    currentAction = 0;
                }
                else if (currentState == State.ACTION3)
                {
                    currentState = State.WEATHER;
                    WeatherOnce = true;
                    currentAction = 0;
                    turnNumber = turnNumber + 1;

                }
            }
            else
            {
                if (currentState == State.ACTION2)
                {
                    currentState = State.WEATHER;
                    WeatherOnce = true;
                    currentAction = 0;
                    turnNumber = turnNumber + 1;
                }
            }
        }


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
        bambooMap.SetTile(pos, bambooTileArray[count]);
        Players[turnNumber % playerCount].PlayerBamboo[board[pos.x, pos.y].Color] += 1;
    }
    void setColor(int number, Vector3Int pos)
    {
        if (number == 0)
            board[pos.x, pos.y].Color = "Yellow";
        if (number == 1)
            board[pos.x, pos.y].Color = "Green";
        if (number == 2)
            board[pos.x, pos.y].Color = "Red";
    }
    void performAction(int currentAction)
    {
        
        Vector3Int mousePos = GetMousePosition();
        if (currentAction == 0)
        {
            switch (currentState)
            {
                case State.ACTION1:
                    eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Select your first action!";
                    break;
                case State.ACTION2:
                    eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Select your second action!";
                    break;
                case State.ACTION3:
                    eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Select your third action!";
                    break;
            }
            
        }
        if (currentAction == 1)
        {
            eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "You may place a land tile!";
            // Left mouse click -> add path tile
            if (Input.GetMouseButtonDown(0))
            {
                

                if (neighbourCount(mousePos) >= 2 && board[mousePos.x, mousePos.y] == null && (FirstAction!=1 || sameAction==true) && selectedNumber != 3)
                {
                    
                    PlaceTile(mousePos, pathMap, TileToBePlaced);
                    setColor(selectedNumber, mousePos);
                    switch (currentState)
                    {
                        case State.ACTION1:
                        Action1Once = false;
                        FirstAction = 1;
                            break;
                        case State.ACTION2:
                            Action2Once = false;
                            break;
                        case State.ACTION3:
                            Action3Once = false;
                            break;
                        default:
                            break;
                    }
                    currentAction = 0;
                }
            }
            //Mouse over -> highlight tile

        }

        if (currentAction == 2)
        {
            eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "You may move the farmer to grow bamboo!";       
            // Left mouse click -> add path tile
            if (Input.GetMouseButtonDown(0))
            {
                if (board[mousePos.x, mousePos.y] != null && (isSameLine1(previousFarmerPos, mousePos) ||
                    isSameLine2(previousFarmerPos, mousePos) ||
                    isSameLine3(previousFarmerPos, mousePos) ||
                    isSameLine4(previousFarmerPos, mousePos) ||
                   previousFarmerPos.y == mousePos.y) &&
                  (FirstAction != 2 || sameAction == true))
                {
                    farmerMap.SetTile(previousFarmerPos, null);
                    farmerMap.SetTile(mousePos, farmerTile);
                    previousFarmerPos = mousePos;
                    if (mousePos != pond)
                    {
                        addBamboo(mousePos);
                        GrowBambooNeighour(mousePos);
                        switch (currentState)
                        {
                            case State.ACTION1:
                                Action1Once = false;
                                FirstAction = 2;
                                break;
                            case State.ACTION2:
                                Action2Once = false;
                                break;
                            case State.ACTION3:
                                Action3Once = false;
                                break;
                            default:
                                break;
                        }
                        currentAction = 0;
                    }
                }

            }
        }

        if (currentAction == 3)
        {
            eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "You may move the panda to eat bamboo!";

            // Left mouse click -> add path tile
            if (Input.GetMouseButtonDown(0))
            {
                if (board[mousePos.x, mousePos.y] != null &&
                    (isSameLine1(previousPandaPos, mousePos) ||
                    isSameLine2(previousPandaPos, mousePos) ||
                    isSameLine3(previousPandaPos, mousePos) ||
                    isSameLine4(previousPandaPos, mousePos) ||
                    previousPandaPos.y == mousePos.y) &&
                    (FirstAction != 3 || sameAction == true))
                {
                    pandaMap.SetTile(previousPandaPos, null);
                    pandaMap.SetTile(mousePos, pandaTile);
                    previousPandaPos = mousePos;
                    if (mousePos != pond)
                    {
                        removeBamboo(mousePos);
                        switch (currentState)
                        {
                            case State.ACTION1:
                                Action1Once = false;
                                FirstAction = 3;
                                break;
                            case State.ACTION2:
                                Action2Once = false;
                                break;
                            case State.ACTION3:
                                Action3Once = false;
                                break;
                            default:
                                break;
                        }
                        currentAction = 0;
                    }

                }
            }
        }
    }
    void TileSelection()
    {
        
        for (int i=0;i<3;i++)
        {
             number[i]= UnityEngine.Random.Range(0, 3);
            int improvement = UnityEngine.Random.Range(0, 3);
            SelectedTile[i] = FlatTilesArray[number[i]];
            //switch (improvement)
            //{ 
            //    case 0:
            //    break;
            //    case 1:
            //    SelectedTile[i].hasNoGrowth = true;
            //    break;
            //    case 2:
            //        SelectedTile[i].hasDoubleGrowth = true;
            //    default:
            //    break;
            //}
        }


        
        tileButtons[0].SetActive(true);
        tileButtons[1].SetActive(true);
        tileButtons[2].SetActive(true);
        tileButtons[0].GetComponent<Image>().sprite = SelectedTile[0].sprite;
        tileButtons[1].GetComponent<Image>().sprite = SelectedTile[1].sprite;
        tileButtons[2].GetComponent<Image>().sprite = SelectedTile[2].sprite;
        
    }
    public void SelectTile1()
    {
        selectedButton=0;
       tileButtons[0].SetActive(false);
       tileButtons[1].SetActive(false);
       tileButtons[2].SetActive(false);
        (TileToBePlaced, selectedNumber) = (SelectedTile[selectedButton], number[selectedButton]);
        selected = true;
    }
    
    public void SelectTile2()
    {
        selectedButton = 1;
        tileButtons[0].SetActive(false);
        tileButtons[1].SetActive(false);
        tileButtons[2].SetActive(false);
        (TileToBePlaced, selectedNumber) = (SelectedTile[selectedButton], number[selectedButton]);
        selected = true;
    }

    public void SelectTile3()
    {
        selectedButton = 2;
    tileButtons[0].SetActive(false);
    tileButtons[1].SetActive(false);
    tileButtons[2].SetActive(false);
    (TileToBePlaced, selectedNumber) = (SelectedTile[selectedButton], number[selectedButton]);
        selected = true;
    }

    void makeHoverTiles()
    {
        Vector3Int mousePos = GetMousePosition();
        if (!mousePos.Equals(previousMousePos))
        {
            switch (currentAction)
            {
                case 1:
                    interactiveMap.SetTile(previousMousePos, null); // Remove old hoverTile
                    interactiveMap.SetTile(mousePos, hoverTile);
                    previousMousePos = mousePos;
                    break;
                case 2:
                    interactiveMap.SetTile(previousMousePos, null); // Remove old hoverTile
                    interactiveMap.SetTile(mousePos, farmerTile);
                    previousMousePos = mousePos;
                    break;
                case 3:
                    interactiveMap.SetTile(previousMousePos, null); // Remove old hoverTile
                    interactiveMap.SetTile(mousePos, pandaTile);
                    previousMousePos = mousePos;
                    break;
            }

        }
      
    }

    public void instantiateScoreBoxes()
    {
        int j = 740;
        for (int k = 0; k < playerCount; k++)
        {
            ScoreBoxList[k] = Instantiate(ScoreBox, new Vector3(1660, j, 0), Quaternion.identity, debugCanvas.transform);
            j -= 690 / playerCount;
        }
    }
}   
