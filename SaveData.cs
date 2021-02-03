using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LevelManagement.Data
{ 
    [Serializable]
    public  class SaveData
    {
        public int carNumber; // carNumber is used to select car from Inventory

        public string hashValue;

        public bool noAds;

        public int[] GameData;  //all levels data is stored in this array

      


        public SaveData()   //this is called construct
        {
            carNumber = 0;
            noAds = false;
            GameData = new int[75];
            GameData[0] = 4;
            hashValue = string.Empty;
            
        }
    }
}
