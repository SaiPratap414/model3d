using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
public class OpenAiAssistantData
{
    public List<string> ThreadIDs = new List<string>();
    public int TotalTokens;
}


public class SaveStats : MonoBehaviour
{
    public static SaveStats instance;
    public OpenAiAssistantData OpenAiAssistantData = new OpenAiAssistantData();
    string filePath = "Assets/Resources/OpenAIData.json";

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
#if UNITY_EDITOR
        LoadFromJson();
#endif
    }


    public void SaveToJson()
    {
#if UNITY_EDITOR
        string data = JsonUtility.ToJson(OpenAiAssistantData);
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(data);
            }
        }


        UnityEditor.AssetDatabase.Refresh();
#endif
        Debug.Log("saveCreated");
    }

    public void LoadFromJson()
    {
#if UNITY_EDITOR
        string Data = System.IO.File.ReadAllText(filePath);

        OpenAiAssistantData = JsonUtility.FromJson<OpenAiAssistantData>(Data);
#endif
        Debug.Log("Retrived Data succeffuly");
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        SaveToJson();
#endif
    }
}
