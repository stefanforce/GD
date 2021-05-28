using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nume;
using System;

namespace cameraManager
{
    public class cameraManager : MonoBehaviour
    {
        public Camera camera;

        public void resizeCamera()
        {
            GameObject grid = GameObject.Find("Grid");
            Map map = grid.GetComponent<Map>();
            int mini = 1000;
            int maxi = -1000;
            int x;
            int y;
            for (int i = 0; i < 100; i++)
                for (int j = 0; j < 100; j++)
                    if (map.board[i, j] != null)
                    {
                        mini = Math.Min(mini, i);
                        maxi = Math.Max(maxi, i);
                        
                    }
            int len = 2;
            len = maxi - mini;
           

            camera.orthographicSize = len+1;
            Vector3 pos = new Vector3(5, 4.5f, -11.8f);
            camera.transform.position = pos;
            Debug.Log(len);
        }
    }
}