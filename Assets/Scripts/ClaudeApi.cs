using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Text;
using System;

[System.Serializable]
public class Content
{
    public string text;
    public string type;
}

[System.Serializable]
public class ResposneClaude
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
    private const string MODEL = "claude-3-5-sonnet-20240624";

    [SerializeField] private string apiKey;

    [SerializeField] ResposneClaude claudeResponse;

    [System.Serializable]
    private class RequestBody
    {
        public string model;
        public int max_tokens;
        public Message[] messages;
    }

    [System.Serializable]
    private class Message
    {
        public string role;
        public string content;
    }

    public async Task<ResposneClaude> SendMessageAsync(string userMessage)
    {
        RequestBody requestBody = new RequestBody
        {
            model = MODEL,
            max_tokens = 1024,
            messages = new Message[]
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

            try
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    claudeResponse = JsonUtility.FromJson<ResposneClaude>(request.downloadHandler.text);

                    return claudeResponse;
                }
                else
                {
                    Debug.LogError($"Error: {request.error}");
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception: {e.Message}");
                return null;
            }
        }
    }
}