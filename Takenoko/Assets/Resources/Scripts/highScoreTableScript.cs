using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using nume;
using Newtonsoft.Json;

public class highScoreTableScript : MonoBehaviour
{
    private Transform entryContainer;
    private Transform entryTemplate;
    private void Awake()
    {
        entryContainer = transform.Find("highscoreEntryContainer");
        entryTemplate = entryContainer.Find("highscoreEntryTemplate");
        
        

        entryTemplate.gameObject.SetActive(false);
        float templateHeight = 80f;
       // GameObject grid = GameObject.Find("Grid");
        //Map map = grid.GetComponent<Map>();
        var playerPoints = new List<(string, int)>();

        string path = "Assets/playerPoints.txt";
        StreamReader sr = new StreamReader(path);
        var converted=sr.ReadToEnd();
        Map.Player[] x= JsonConvert.DeserializeObject<Map.Player[]>(converted);
        for (var id = 0; id < PhotonNetwork.PlayerList.Length; ++id)
        {
            playerPoints.Add((x[id].playerName, x[id].score));
            //var line = sr.ReadLine();
            //while(line!=null)
            //{
            //    string playerName = line;
            //    line = sr.ReadLine();
            //    int score = int.Parse(line);
            //    playerPoints.Add((playerName, score));
            //    line = sr.ReadLine();
                
            //}
            
            //var player = map.Players[id];
            // playerPoints.Add((player.playerName, player.score));
        }
        sr.Close();
        var orderedPlayerPoints = playerPoints.OrderBy(o => -o.Item2).ToList();

        for (var id = 0; id < orderedPlayerPoints.Count; ++id)
        {
            Transform entryTransform = Instantiate(entryTemplate, entryContainer);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight*id);
            entryTransform.gameObject.SetActive(true);

            int rank = id+1;
            string rankString;
            switch (rank)
            {
                case 1:
                    rankString = "1ST"; break;
                case 2:
                    rankString = "2ND"; break;
                case 3:
                    rankString = "3RD"; break;
                default: rankString = rank + "TH"; break;

            }
            entryTransform.Find("posText").GetComponent<Text>().text=rankString;
           
            int score = orderedPlayerPoints[id].Item2;
            entryTransform.Find("scoreText").GetComponent<Text>().text=score.ToString();
            

            string name = orderedPlayerPoints[id].Item1;
            entryTransform.Find("nameText").GetComponent<Text>().text=name;
        }
    }
   
    private class HighscoreEntry
    {
        public int score;
        public string name;
    }
}
