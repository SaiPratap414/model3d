using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class OpenAIPayload
{
    public string model;
    public List<Message> messages;
}

[Serializable]
public class OpenAIResponse
{
    public string id;
    public string _object;
    public long created;
    public string model;
    public string system_fingerprint;
    public List<Choice> choices;
    public Usage usage;
}

[Serializable]
public class Choice
{
    public int index;
    public Message message;
    public object logprobs;
    public string finish_reason;
}

[Serializable]
public class Message
{
    public string role;
    public string content;
}

[Serializable]
public class Usage
{
    public int prompt_tokens;
    public int completion_tokens;
    public int total_tokens;
}

[System.Serializable]
public class UserResponse
{
    public string Expression;
    public string Response;
}

public class OpenAiApi : MonoBehaviour
{
    public string apiKey;
    private string url = "https://api.openai.com/v1/chat/completions";

    public OpenAIResponse aIResponse;
    public UserResponse myResponse;

    public string CustomCharacteristics = "";

    private void Start()
    {
    }

    public async Task<UserResponse> SendOpenAIRequest(string text)
    {
        OpenAIPayload payload = new OpenAIPayload
        {
            model = "gpt-3.5-turbo",
            messages = new List<Message>
            {
                new Message{ role = "system", content = "You are conversational Bot "+ CustomCharacteristics.Trim() +" ,The response should only be in JSON format which consits of two objects first is Expression(which should be selected depending on the response from only this list [Happy,Sad,Surprised,Angry,Confused]) and second Response of Bot (keep the response Short in lenght)" },
                new Message{ role = "user", content = text }
            }
        };

        string json = JsonUtility.ToJson(payload);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);

            UnityWebRequestAsyncOperation operation = webRequest.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
                return null;
            }
            else
            {
                //Debug.Log("Response: " + webRequest.downloadHandler.text);

                aIResponse = JsonUtility.FromJson<OpenAIResponse>(webRequest.downloadHandler.text);

                myResponse = JsonUtility.FromJson<UserResponse>(aIResponse.choices[0].message.content);
                Debug.LogWarning("Expression: " + myResponse.Expression + "\n Content: " + myResponse.Response);
                return myResponse;
            }
        }
    }
}
