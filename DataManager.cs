using LevelManagement.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelManagement.Data
{
    public class DataManager : MonoBehaviour
    {

        public static DataManager instance;

        private JsonSaver jsonSaver;
        private SaveData saveData;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);


            saveData = new SaveData();
            jsonSaver = new JsonSaver();
        }
        private void Start()
        {
            Load();
        }

        public int carNumber
        {
            get { return saveData.carNumber; }
            set { saveData.carNumber = value; }
        }

        public bool noAds
        {
            get { return saveData.noAds; }
            set { saveData.noAds = value; }
        }

        public int[] GameData
        {
            get { return saveData.GameData; }
            set { saveData.GameData = value; }
        }

        public void Save()
        {
            jsonSaver.SaveData(saveData);
        }

        public void Load()
        {
            jsonSaver.Load(saveData);
        }

    }
}
