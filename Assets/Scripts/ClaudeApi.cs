using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

[System.Serializable]
public class Content
{
    public string text;
    public string type;
}

[System.Serializable]
public class ResponseClaude
{
    public List<Content> content;
    public string id;
    public string model;
    public string role;
    public string stop_reason;
    public object stop_sequence;
    public string type;
    public Usage usage;
}

[System.Serializable]
public class Usage
{
    public int input_tokens;
    public int output_tokens;
}

public class AnthropicAPIClient : MonoBehaviour
{
    private const string API_URL = "https://api.anthropic.com/v1/messages";
    private const string API_VERSION = "2023-06-01";
    private const string MODEL = "claude-3-sonnet-20240229";
    [SerializeField] private string apiKey;

    [System.Serializable]
    private class RequestBody
    {
        public string model;
        public int max_tokens;
        public List<Message> messages;
    }

    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;
    }

    public void SendMessage(string userMessage, Action<ResponseClaude> onComplete, Action<string> onError)
    {
        StartCoroutine(SendMessageCoroutine(userMessage, onComplete, onError));
    }

    private IEnumerator SendMessageCoroutine(string userMessage, Action<ResponseClaude> onComplete, Action<string> onError)
    {
        RequestBody requestBody = new RequestBody
        {
            model = MODEL,
            max_tokens = 1024,
            messages = new List<Message>
            {
                new Message { role = "user", content = userMessage }
            }
        };

        string jsonBody = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("x-api-key", apiKey);
            request.SetRequestHeader("anthropic-version", API_VERSION);
            request.SetRequestHeader("content-type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ResponseClaude response = JsonUtility.FromJson<ResponseClaude>(request.downloadHandler.text);
                onComplete?.Invoke(response);
            }
            else
            {
                string errorMessage = $"Error: {request.error}";
                Debug.LogError(errorMessage);
                onError?.Invoke(errorMessage);
            }
        }
    }
}