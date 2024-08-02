using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleAPIRequest : MonoBehaviour
{
    private string apiKey = "AIzaSyCEtw6E9N4aO3qsRN9gvZDfkBWTtVdqGtI";
    private string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=";

    string userText = string.Empty;

    void Start()
    {
        StartCoroutine(SendPostRequest(userText));
    }

    IEnumerator SendPostRequest(string UserText)
    {
        string jsonData = @"
        {
            ""system_instruction"": {
                ""parts"": {
                    ""text"": ""need a response in Json with response and first expression: The Select appropriate (according to the response) expression from the List [sad, happy, smile ,angry, dissapointed ] and second variable response: here insert the Response from the promt via user""
                }
            },
            ""contents"": {
                ""parts"": {
                    ""text"": """ + UserText + @"""
                }
            }
        }";

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
