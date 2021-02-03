using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LevelManagement.Data;
using System;
using System.Text;
using System.Security.Cryptography;

public class JsonSaver
{
    private static readonly string _filename = "saveData1.mat";

    public static string GetSaveFileName()
    {
        return Application.persistentDataPath + "/" + _filename;
    }

    public void SaveData(SaveData data)
    {
        data.hashValue = string.Empty;

        string json = JsonUtility.ToJson(data);

        data.hashValue = GetSHA256(json);
        json = JsonUtility.ToJson(data);

        string saveFilename = GetSaveFileName();
       // Debug.Log(GetSaveFileName());

        FileStream fileStream = new FileStream(saveFilename,FileMode.Create);

        using(StreamWriter writer = new StreamWriter(fileStream))
        {
            writer.Write(json);
        }
    }

    public bool Load(SaveData data)
    {
        string loadFilename = GetSaveFileName();

        if (File.Exists(loadFilename))
        {
            using(StreamReader reader = new StreamReader(loadFilename))
            {
                string json = reader.ReadToEnd();

                //check hash before reading
                if (CheckData(json))
                {
                    
                    JsonUtility.FromJsonOverwrite(json, data);
                }
                else
                {

                    Debug.LogWarning("Game Data has been altered");
                }

                

            }
            return true;
        }
        return false;
    }
    private bool CheckData(string json)
    {
        SaveData tempSaveData = new SaveData();
        JsonUtility.FromJsonOverwrite(json, tempSaveData);

        string oldHash = tempSaveData.hashValue;
        tempSaveData.hashValue = String.Empty;

        string tempJson = JsonUtility.ToJson(tempSaveData);
        string newHash = GetSHA256(tempJson);

        return (oldHash == newHash);
    }

    public void Delete()
    {
        if (File.Exists(GetSaveFileName()))
        {
            File.Delete(GetSaveFileName());
        }
    }
    public string GetHexStringFromHash(byte[] hash)
    {
        string hexString = String.Empty;

        foreach(byte b in hash)
        {
            hexString += b.ToString("x2");
        }
        return hexString;
    }
    private string GetSHA256(string text)
    {
        byte[] textToBytes = Encoding.UTF8.GetBytes(text);
        SHA256Managed mySHA256 = new SHA256Managed();

        byte[] hashValue = mySHA256.ComputeHash(textToBytes);

        return GetHexStringFromHash(hashValue);
    }
}
