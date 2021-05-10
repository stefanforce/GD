using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Photon.Pun;
//using Other;

namespace nume
{
    public class Quest
    {

        public string name;
        public int type;
        public int score;
    }
    [Serializable]
    public class JsonTiles
    {
        public string name;
        public string Color;
        public bool hasNoEating = false;
        public bool hasDoubleGrowth = false;
    }
    [Serializable]
    public class JsonQuestsPanda : Quest
    {
        public int Green;
        public int Yellow;
        public int Red;
    }
    [Serializable]
    public class JsonQuestsFarmer : Quest
    {
        public int bambooCount;
        public string Color;
        public bool hasNoEating = false;
        public bool hasDoubleGrowth = false;
    }
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }


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
            public bool hasNoEating = false;
            public bool hasDoubleGrowth = false;
        }



        public class Player
        {
            public string playerName;
            public int score;
            public Dictionary<string, int> PlayerBamboo = new Dictionary<string, int>();
            public Player()
            {
                PlayerBamboo.Add("Yellow", 0);//Yellow
                PlayerBamboo.Add("Red", 0);//Red
                PlayerBamboo.Add("Green", 0);//Green

            }
            public Player(string playerName) : this()
            {
                this.playerName = playerName;

            }
            public string SaveToString()
            {
                return JsonUtility.ToJson(this);
            }
        }

        public Canvas debugCanvas;


        private Grid grid;
        [SerializeField] private Tilemap interactiveMap = null;
        [SerializeField] private Tilemap pathMap = null;
        [SerializeField] private Tilemap pandaMap = null;
        [SerializeField] private Tilemap farmerMap = null;
        [SerializeField] private Tilemap bambooMap = null;
        [SerializeField] private Tilemap improvementMap = null;
        [SerializeField] private Tile pandaTile = null;
        [SerializeField] private Tile farmerTile = null;
        [SerializeField] private Tile hoverTile = null;
        [SerializeField] private Tile centerTile = null;
        [SerializeField] private Tile[] improvementTile = new Tile[2];

        public Button landButton;
        public Button farmerButton;
        public Button pandaButton;
        public Button confirmActionButton;
        public Button questMenuButton;
        public Button eventText;
        public GameObject ScoreBox;
        public GameObject[] tileButtons = new GameObject[3];
        public GameObject[] diceButtons = new GameObject[5];
        public GameObject[] improvementButtons = new GameObject[2];
        public GameObject[] questHandButtons = new GameObject[5];
        public GameObject[] questSelectButtons = new GameObject[2];

        GameObject[] ScoreBoxList = new GameObject[4];

        private Tile[] FlatTilesArray = new Tile[23];
        private Tile[] FarmerQuestSprites = new Tile[7];
        private Tile[] PandaQuestSprites = new Tile[5];
        public Tile[] bambooTileArray = new Tile[5];
        public List<Tile> flatTileList;
        public List<Tile> pandaQuestSpritesList;
        public List<Tile> farmerQuestSpritesList;
        public List<JsonTiles> jsonTileList;
        public List<JsonQuestsFarmer> jsonFarmerQuestList;
        public List<JsonQuestsPanda> jsonPandaQuestList;
        public Tile[] diceSprites = new Tile[5];
        public Tile[] questSelectButtonsSprites = new Tile[2];
        public Tile[] improvementSprites = new Tile[2];
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
        bool selected = false;
        int FirstAction = 4;
        int randomWeatherCondition;
        int turnNumber = 0;
        int selectedImprovement = 2;
        int selectedButton = 3;
        int selectedNumber = 3;
        int selectedQuest = 3;
        Tile TileToBePlaced;
        Tile[] SelectedTile = new Tile[10];
        int[] number = new int[23];
        int playerCount = 2;
        JsonTiles jsonTileAtributes;
        List<List<Quest>> playerQuests = new List<List<Quest>>();
        List<List<Tile>> playerQuestsSprites = new List<List<Tile>>();

        // Start is called before the first frame update
        void Start()
        {
            JsonInitializations();

            grid = gameObject.GetComponent<Grid>();
            PlaceTile(pond, pathMap, centerTile);
            pandaMap.SetTile(previousPandaPos, pandaTile);
            farmerMap.SetTile(previousFarmerPos, farmerTile);
            board[pond.x, pond.y].isPond = true;
            currentState = State.ACTION1;
            deactivateButtons();
            Players[0] = new Player("Player1");
            Players[1] = new Player("Player2");
            instantiateScoreBoxes();

            instantiateQuests();

        }

        private void instantiateQuests()
        {
            for (int i = 0; i < playerCount; i++)
            {
                var quests_for_each_player = new List<Quest>();
                var sprites_for_each_player = new List<Tile>();
                //pick random quest and add it
                int nr1 = UnityEngine.Random.Range(0, jsonFarmerQuestList.Count);
                quests_for_each_player.Add(jsonFarmerQuestList[nr1]);
                sprites_for_each_player.Add(farmerQuestSpritesList[nr1]);
                jsonFarmerQuestList.RemoveAt(nr1);
                farmerQuestSpritesList.RemoveAt(nr1);
                int nr2 = UnityEngine.Random.Range(0, jsonPandaQuestList.Count);
                quests_for_each_player.Add(jsonPandaQuestList[nr2]);
                sprites_for_each_player.Add(pandaQuestSpritesList[nr2]);
                jsonFarmerQuestList.RemoveAt(nr2);
                pandaQuestSpritesList.RemoveAt(nr2);
                playerQuests.Add(quests_for_each_player);
                playerQuestsSprites.Add(sprites_for_each_player);
            }
        }

        private void deactivateButtons()
        {
            for (int i = 0; i < 3; i++)
                tileButtons[i].SetActive(false);
           for (int i = 0; i < 2; i++)
                improvementButtons[i].SetActive(false);
            for (int i = 0; i < 5; i++)
                diceButtons[i].SetActive(false);
            for (int i = 0; i < 5; i++)
                questHandButtons[i].SetActive(false);
            for (int i = 0; i < 2; i++)
                questSelectButtons[i].SetActive(false);
        }

        private void JsonInitializations()
        {
            string jsonString = fixJson(File.ReadAllText("Assets/Scripts/Hexes.json"));
            string jsonString1 = fixJson(File.ReadAllText("Assets/Scripts/Quests_type2.json"));
            string jsonString2 = fixJson(File.ReadAllText("Assets/Scripts/Quests_type3.json"));
            JsonTiles[] jsonTiles = JsonHelper.FromJson<JsonTiles>(jsonString);
            JsonQuestsFarmer[] jsonFarmerQuestClass = JsonHelper.FromJson<JsonQuestsFarmer>(jsonString2);
            JsonQuestsPanda[] jsonPandaQuestClass = JsonHelper.FromJson<JsonQuestsPanda>(jsonString1);
            jsonTileList = new List<JsonTiles>(jsonTiles);
            jsonFarmerQuestList = new List<JsonQuestsFarmer>(jsonFarmerQuestClass);
            jsonPandaQuestList = new List<JsonQuestsPanda>(jsonPandaQuestClass);
            for (int i = 0; i < 23; i++)
            {
                FlatTilesArray[i] = Resources.Load<Tile>("OriginalHexes/plots_" + (i + 1).ToString());
            }
            for (int i = 0; i < 4; i++)
            {
                PandaQuestSprites[i] = Resources.Load<Tile>("Quests/objectiveB_" + (i + 16).ToString());
            }
            for (int i = 0; i < 7; i++)
            {
                FarmerQuestSprites[i] = Resources.Load<Tile>("Quests/objectiveB_" + (i + 20).ToString());
            }
            flatTileList = new List<Tile>(FlatTilesArray);
            pandaQuestSpritesList = new List<Tile>(PandaQuestSprites);
            farmerQuestSpritesList = new List<Tile>(FarmerQuestSprites);
        }

        void Update()
        {


            //if(myturn) do function below
            if (currentState == State.WEATHER)
            {

                Action2Once = true;
                Action3Once = true;
                FirstAction = 4;
                sameAction = false;
                if (WeatherOnce == true)
                    WeatherSelection();
                updateScores();
            }

            if (currentState == State.WEATHER2)
            {
                WeatherOnce = true;
                if (Weather2Once == true)
                    doWeatherCondition();
            }

            if (currentState == State.ACTION1)
            {

                showQuests();
                Weather2Once = true;
                WeatherOnce = true;


                if (Action1Once == true )
                {
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
                }
                else
                {
                    eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Press confirm to end this action!";
                    interactiveMap.SetTile(GetMousePosition(), null);

                }
            }
            for (int k = 0; k < playerCount; k++)
            {
                ScoreBoxList[k].transform.Find("PlayerName").GetComponentInChildren<UnityEngine.UI.Text>().text = Players[k].playerName;
                ScoreBoxList[k].transform.Find("ScoreText").GetComponentInChildren<UnityEngine.UI.Text>().text = "Score : " + Players[k].score.ToString();
                ScoreBoxList[k].transform.Find("RedBambooText").GetComponentInChildren<UnityEngine.UI.Text>().text = "Red bamboo : " + Players[k].PlayerBamboo["Red"].ToString();
                ScoreBoxList[k].transform.Find("YellowBambooText").GetComponentInChildren<UnityEngine.UI.Text>().text = "Yellow bamboo : " + Players[k].PlayerBamboo["Yellow"].ToString();
                ScoreBoxList[k].transform.Find("GreenBambooText").GetComponentInChildren<UnityEngine.UI.Text>().text = "Green bamboo : " + Players[k].PlayerBamboo["Green"].ToString();
            }


            debugCanvas.GetComponentInChildren<UnityEngine.UI.Button>().GetComponentInChildren<UnityEngine.UI.Text>().text = "PlayerName : " + Players[turnNumber % playerCount].playerName + " CurrentState: " + currentState.ToString();
        }
        void updateScores()
        {
            for (int i = playerQuests[turnNumber % playerCount].Count-1; i>=0; i--)
            {
                switch(playerQuests[turnNumber % playerCount][i].type)
                {
                    case 2:
                        JsonQuestsPanda currentQuest = (JsonQuestsPanda)playerQuests[turnNumber % playerCount][i];
                        //if (hasBambooPieces(Players[turnNumber % playerCount], currentQuest))
                        if(Players[turnNumber % playerCount].PlayerBamboo["Red"] >= currentQuest.Red
                            && Players[turnNumber % playerCount].PlayerBamboo["Yellow"] >= currentQuest.Yellow 
                            && Players[turnNumber % playerCount].PlayerBamboo["Green"] >= currentQuest.Green)
                        {
                            reduceBamboo(Players[turnNumber % playerCount], currentQuest);
                            Players[turnNumber % playerCount].score += currentQuest.score;
                            playerQuests[turnNumber % playerCount].RemoveAt(i);
                            questHandButtons[i].SetActive(false);
                        }
                        break;
                    case 3:
                        JsonQuestsFarmer currentQuest1= (JsonQuestsFarmer)playerQuests[turnNumber % playerCount][i];
                        for (int k = 0; k < 100; k++)
                        {
                            bool flag = true;
                         
                            for (int j = 0; j < 100; j++)
                                if (board[k, j] != null)
                                {
                                    if (board[k, j].Color == currentQuest1.Color
                                        && board[k, j].bambooCount == currentQuest1.bambooCount
                                        && board[k, j].hasDoubleGrowth == currentQuest1.hasDoubleGrowth
                                        && board[k, j].hasNoEating == currentQuest1.hasNoEating)
                                    {
                                        Players[turnNumber % playerCount].score += currentQuest1.score;
                                        playerQuests[turnNumber % playerCount].RemoveAt(i);
                                        questHandButtons[i].SetActive(false);
                                        flag = false;
                                        break;
                                    }
                                }
                            if (!flag)
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        bool hasBambooPieces(Player player,JsonQuestsPanda quest)
        {
           
            if (Players[turnNumber % playerCount].PlayerBamboo["Red"] < quest.Red && quest.Red>0)
                return false;
            if (Players[turnNumber % playerCount].PlayerBamboo["Yellow"] < quest.Yellow && quest.Yellow>0)
                return false;
            if (Players[turnNumber % playerCount].PlayerBamboo["Green"] < quest.Green && quest.Green>0)
                return false;
            return true;
        }
        void reduceBamboo(Player player, JsonQuestsPanda quest)
        {
            Players[turnNumber % playerCount].PlayerBamboo["Green"] -= quest.Green;
            Players[turnNumber % playerCount].PlayerBamboo["Red"] -= quest.Red;
            Players[turnNumber % playerCount].PlayerBamboo["Yellow"] -= quest.Yellow;
        }
        void showQuests()
        {
            for (int i = 0; i < 5; i++)
                questHandButtons[i].SetActive(false);
            for (int i=0;i<playerQuests[turnNumber%playerCount].Count;i++)
            {
                questHandButtons[i].SetActive(true);
                questHandButtons[i].GetComponent<Image>().sprite = playerQuestsSprites[turnNumber%playerCount][i].sprite;

            }
        }
        private void WeatherSelection()
        {
            actionNumber = 2;
            //randomWeatherCondition = UnityEngine.Random.Range(0, 6);
            randomWeatherCondition = 2;
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
                    // eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Press the confirm button to validate the roll!";
                    break;
                case 3://Storm
                    eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Rolled the Storm dice,you can take a bamboo from any tile!";
                    break;
                case 4://Clouds
                    SelectImprovement();
                    eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Rolled the Cloud dice,you can take any tile improvement!";
                    break;
                case 5://Random
                    eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "Rolled the ? dice,you can decide which condition to take!";
                    //while (selectedDice != true)
                    SelectDice();
                    break;
            }

            WeatherOnce = false;
        }

        void SelectDice()
        {
            for (int i = 0; i < 5; i++)
            {
                diceButtons[i].SetActive(true);
                diceButtons[i].GetComponent<Image>().sprite = diceSprites[i].sprite;
            }
        }
        private void SelectImprovement()
        {
            for (int i = 0; i < 2; i++)
            {
                improvementButtons[i].SetActive(true);
                improvementButtons[i].GetComponent<Image>().sprite = improvementSprites[i].sprite;
                selectedImprovement = 2;
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
                    doCloudCondition();
                    eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "You may place the improvement tile!";
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
        private void doCloudCondition()
        {
            Vector3Int mousePos = GetMousePosition();
            if (!mousePos.Equals(previousMousePos))
            {
                interactiveMap.SetTile(previousMousePos, null); // Remove old hoverTile
                interactiveMap.SetTile(mousePos, improvementTile[selectedImprovement]);
                previousMousePos = mousePos;
            }
            if (Input.GetMouseButtonDown(0))
            {
                Weather2Once = false;
                interactiveMap.SetTile(mousePos, null);
                improvementMap.SetTile(mousePos, improvementTile[selectedImprovement]);
                if (selectedImprovement == 0)
                    board[mousePos.x, mousePos.y].hasDoubleGrowth = true;
                else if (selectedImprovement == 1)
                    board[mousePos.x, mousePos.y].hasNoEating = true;
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
            if (pos != pond)
                transferAtributes(board[pos.x, pos.y], jsonTileAtributes);

        }
        void transferAtributes(BoardTile to, JsonTiles from)
        {
            to.Color = from.Color;
            to.hasDoubleGrowth = from.hasDoubleGrowth;
            to.hasNoEating = from.hasNoEating;
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
            if (FirstAction != 1 || sameAction==true)
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
        public void changeActionToQuest()
        {
            currentAction = 4;
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
            if (board[pos.x, pos.y].hasDoubleGrowth)
                board[pos.x, pos.y].bambooCount += 2;
            else
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
            if (board[pos.x, pos.y].hasNoEating == false)
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
            if (number >= 1 && number <= 9)
                board[pos.x, pos.y].Color = "Yellow";
            if (number >= 10 && number <= 17)
                board[pos.x, pos.y].Color = "Green";
            if (number >= 18 && number <= 23)
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
                if (selected == true)
                {

                    makeHoverTiles();
                    if (Input.GetMouseButtonDown(0))
                    {


                        if (neighbourCount(mousePos) >= 2 && board[mousePos.x, mousePos.y] == null && (FirstAction != 1 || sameAction == true))
                        {

                            PlaceTile(mousePos, pathMap, TileToBePlaced);
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
                }

            }

            if (currentAction == 2)
            {
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "You may move the farmer to grow bamboo!";

                makeHoverTiles();
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


                makeHoverTiles();
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

            if (currentAction == 4)
            {
                eventText.GetComponentInChildren<UnityEngine.UI.Text>().text = "You may select a quest!";

                for (int i = 0; i < 2; i++)
                {
                    questSelectButtons[i].SetActive(true);
                    questSelectButtons[i].GetComponent<Image>().sprite = questSelectButtonsSprites[i].sprite;
                }


            }
        }
        void TileSelection()
        {

            for (int i = 0; i < 3; i++)
            {
                number[i] = UnityEngine.Random.Range(0, flatTileList.Count);
                SelectedTile[i] = flatTileList[number[i]];

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
            selectedButton = 0;
            tileButtons[0].SetActive(false);
            tileButtons[1].SetActive(false);
            tileButtons[2].SetActive(false);
            jsonTileAtributes = jsonTileList[number[0]];
            jsonTileList.RemoveAt(number[0]);
            TileToBePlaced = SelectedTile[selectedButton];
            flatTileList.Remove(TileToBePlaced);
            selected = true;
        }

        public void SelectTile2()
        {
            selectedButton = 1;
            tileButtons[0].SetActive(false);
            tileButtons[1].SetActive(false);
            tileButtons[2].SetActive(false);
            jsonTileAtributes = jsonTileList[number[1]];
            jsonTileList.RemoveAt(number[1]);
            TileToBePlaced = SelectedTile[selectedButton];
            flatTileList.Remove(TileToBePlaced);
            selected = true;
        }

        public void SelectTile3()
        {
            selectedButton = 2;
            tileButtons[0].SetActive(false);
            tileButtons[1].SetActive(false);
            tileButtons[2].SetActive(false);
            jsonTileAtributes = jsonTileList[number[2]];
            jsonTileList.RemoveAt(number[2]);
            TileToBePlaced = SelectedTile[selectedButton];
            flatTileList.Remove(TileToBePlaced);
            selected = true;
        }

        public void SelectDice0()
        {
            for (int i = 0; i < 5; i++)
                diceButtons[i].SetActive(false);
            actionNumber += 1;
            randomWeatherCondition = 0;

        }
        public void SelectDice1()
        {
            for (int i = 0; i < 5; i++)
                diceButtons[i].SetActive(false);
            randomWeatherCondition = 1;

        }
        public void SelectDice2()
        {
            for (int i = 0; i < 5; i++)
                diceButtons[i].SetActive(false);
            randomWeatherCondition = 2;
            sameAction = true;

        }
        public void SelectDice3()
        {
            for (int i = 0; i < 5; i++)
                diceButtons[i].SetActive(false);
            randomWeatherCondition = 3;

        }
        public void SelectDice4()
        {
            for (int i = 0; i < 5; i++)
                diceButtons[i].SetActive(false);
            randomWeatherCondition = 4;


        }

        public void SelectImprovement1()
        {
            for (int i = 0; i < 2; i++)
                improvementButtons[i].SetActive(false);
            selectedImprovement = 0;


        }
        public void SelectImprovement2()
        {
            for (int i = 0; i < 2; i++)
                improvementButtons[i].SetActive(false);
            selectedImprovement = 1;

        }

        public void SelectQuest1()
        {
            selectedQuest = 0;
            for (int i = 0; i < 2; i++)
                questSelectButtons[i].SetActive(false);
            if (jsonFarmerQuestList.Count != 0)
            {
                int nr1 = UnityEngine.Random.Range(0, jsonFarmerQuestList.Count);
                playerQuests[turnNumber % playerCount].Add(jsonFarmerQuestList[nr1]);
                playerQuestsSprites[turnNumber % playerCount].Add(farmerQuestSpritesList[nr1]);
                jsonFarmerQuestList.RemoveAt(nr1);
                farmerQuestSpritesList.RemoveAt(nr1);

                switch (currentState)
                {
                    case State.ACTION1:
                        Action1Once = false;
                        FirstAction = 4;
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
        public void SelectQuest2()
        {
            selectedQuest = 1;
            for (int i = 0; i < 2; i++)
                questSelectButtons[i].SetActive(false);
            if (jsonPandaQuestList.Count != 0)
            {
                int nr1 = UnityEngine.Random.Range(0, jsonPandaQuestList.Count);
                playerQuests[turnNumber % playerCount].Add(jsonPandaQuestList[nr1]);
                playerQuestsSprites[turnNumber % playerCount].Add(pandaQuestSpritesList[nr1]);
                jsonPandaQuestList.RemoveAt(nr1);
                pandaQuestSpritesList.RemoveAt(nr1);

                switch (currentState)
                {
                    case State.ACTION1:
                        Action1Once = false;
                        FirstAction = 4;
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

        string fixJson(string value)
        {
            value = "{\"Items\":" + value + "}";
            return value;
        }

    }
}
