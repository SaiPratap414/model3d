using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


[System.Serializable]
public class RequestData
{
    public SystemInstruction system_instruction;
    public Contents contents;
}

[System.Serializable]
public class SystemInstruction
{
    public Parts parts;
}

[System.Serializable]
public class Parts
{
    public string text;
}

[System.Serializable]
public class Contents
{
    public Parts parts;
}


public class GoogleAPIRequestANother : MonoBehaviour
{
    private string apiKey = "AIzaSyCEtw6E9N4aO3qsRN9gvZDfkBWTtVdqGtI";
    private string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=";

    string userText = "Hello How are Doing My Friend"; 

    void Start()
    {
        StartCoroutine(SendPostRequest(userText));
    }

    IEnumerator SendPostRequest(string UserText)
    {
        // Create the JSON data as a C# object
        var requestData = new RequestData
        {
            system_instruction = new SystemInstruction
            {
                parts = new Parts
                {
                    text = "You making Conversations with a User, need a response in Json with response and first expression: The Select appropriate (according to the response) expression from the List [sad, happy, smile ,angry, dissapointed ] and second variable response: here insert the Response"
                }
            },
            contents = new Contents
            {
                parts = new Parts
                {
                    text = UserText
                }
            }
        };

        // Serialize the C# object to JSON
        string jsonData = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(url + apiKey, "POST"))
        {
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("Response: " + request.downloadHandler.text);
            }
        }
    }
}


