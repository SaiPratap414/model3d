using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;

[System.Serializable]
public class Thread
{
    public string id;
    public string @object;
    public int created_at;
}

[System.Serializable]
public class RunResponse
{
    public string id;
}

[System.Serializable]
public class RunStatusResponse
{
    public string id;
    public string status;
    public Usage usage;
}

[System.Serializable]
public class TextContent
{
    public string value;
}

[System.Serializable]
public class Content
{
    public string type;
    public TextContent text;
}

[System.Serializable]
public class MessageContent
{
    public string id;
    public string role;
    public Content[] content;
}

[System.Serializable]
public class MessagesList
{
    public string first_id;
    public string last_id;
    public bool has_more;
    public MessageContent[] data;
}

public class OpenAIAssistant : MonoBehaviour
{

    public string OpenAiApi;
    string AssistentID = "asst_NzBdyhyUBslMFBNsE7LEayta";

    public Thread AiThread;
    public MessagesList messageList;

    public UserResponse UResponse;

    private const string CreateThreadUrl = "https://api.openai.com/v1/threads";
    string AddMessageUrl(string ThreadId) => $"https://api.openai.com/v1/threads/{ThreadId}/messages";
    private string CreateRunUrl(string ThreadId) => $"https://api.openai.com/v1/threads/{ThreadId}/runs";
    private string GetRunStatusUrl(string ThreadId, string runId) => $"https://api.openai.com/v1/threads/{ThreadId}/runs/{runId}";
    private string ListMessagesUrl(string ThreadId) => $"https://api.openai.com/v1/threads/{ThreadId}/messages";
    private string DeleteThread(string ThreadId) => $"https://api.openai.com/v1/threads/{ThreadId}";

    public string Message = "";



    public async void TestButton()
    {
        var valur = await AssistantRequest(Message);
        Debug.Log(valur);
    }


    public async UniTask<UserResponse> AssistantRequest(string UMessage)
    {
        if (AiThread.id == string.Empty)
        {
            Debug.Log("Ai thread is Null");
            AiThread = await CreateThreadAsync();
            SaveStats.instance.OpenAiAssistantData.ThreadIDs.Add(AiThread.id);
        }

        string response = await AddMessageToThreadAsync(UMessage, AiThread.id);

        if(response == null)
        {
            Debug.LogError("problem in AddMessageToThread " + AiThread.id);
            return null;
        }

        RunResponse runResponse = await CreateRunAsync(AiThread.id, null);

        if (runResponse == null)
        {
            Debug.LogError("problem in CreateRunAsync " + AiThread.id);
            return null;
        }


        while(true)
        {
            Debug.Log("Running Status request!!");
            RunStatusResponse runStatus = await GetRunStatusAsync(AiThread.id, runResponse.id);
            if(runStatus == null)
            {
                Debug.LogError("problem in GetRunStatusAsync runid: " + runResponse.id);
                return null;
            }
            if (IsTerminalState(runStatus))
            {
                SaveStats.instance.OpenAiAssistantData.TotalTokens += runStatus.usage.total_tokens;
                break;
            }
            // Wait for some time before polling again
            Debug.Log("waiting 2 sec for request!!");
            await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false); // 2 seconds delay
            Debug.Log("waiting Done for request!!");
        }

        messageList = await ListMessagesAsync(AiThread.id);

        string AiAnswer = messageList.data[0].content[0].text.value;
        Debug.Log(AiAnswer);
        UResponse = JsonUtility.FromJson<UserResponse>(AiAnswer);

        return UResponse;
    }



    public async Task<Thread> CreateThreadAsync()
    {
        using (UnityWebRequest request = new UnityWebRequest(CreateThreadUrl, "POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {OpenAiApi}");
            request.SetRequestHeader("OpenAI-Beta", "assistants=v2");
            request.downloadHandler = new DownloadHandlerBuffer();

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("CreateThreadAsync:- " + request.downloadHandler.text);
                return JsonUtility.FromJson<Thread>(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
                return null;
            }
        }
    }

    public async Task<string> AddMessageToThreadAsync(string message, string ThreadID)
    {
        string jsonBody = $"{{\"role\": \"user\", \"content\": \"{message}\"}}";

        

        using (UnityWebRequest request = new UnityWebRequest(AddMessageUrl(ThreadID), "POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {OpenAiApi}");
            request.SetRequestHeader("OpenAI-Beta", "assistants=v2");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonBody));
            request.downloadHandler = new DownloadHandlerBuffer();

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                //Debug.Log("AddMessageToThread:- " + request.downloadHandler.text);
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
                return null;
            }
        }
    }


    public async Task<RunResponse> CreateRunAsync( string ThreadID, string instructions)
    {
        string jsonBody = $"{{\"assistant_id\": \"{AssistentID}\", \"instructions\": \"{instructions}\"}}";

        using (UnityWebRequest request = new UnityWebRequest(CreateRunUrl(ThreadID), "POST"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {OpenAiApi}");
            request.SetRequestHeader("OpenAI-Beta", "assistants=v2");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonBody));
            request.downloadHandler = new DownloadHandlerBuffer();

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                //Debug.Log("CreateRunAsync:- " + request.downloadHandler.text);
                return JsonUtility.FromJson<RunResponse>(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
                return null;
            }
        }
    }

    public async Task<RunStatusResponse> GetRunStatusAsync(string threadID ,string runId)
    {
        using (UnityWebRequest request = new UnityWebRequest(GetRunStatusUrl(threadID, runId), "GET"))
        {
            request.SetRequestHeader("Authorization", $"Bearer {OpenAiApi}");
            request.SetRequestHeader("OpenAI-Beta", "assistants=v2");

            request.downloadHandler = new DownloadHandlerBuffer();

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                //Debug.Log("Run Status:- " + request.downloadHandler.text);
                return JsonUtility.FromJson<RunStatusResponse>(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
                return null;
            }
        }
    }


    public bool IsTerminalState(RunStatusResponse statusResponse)
    {
        string status = statusResponse.status;
        return status == "completed" || status == "failed" || status == "cancelled";
    }

    public async Task<MessagesList> ListMessagesAsync(string ThreadId)
    {
        using (UnityWebRequest request = new UnityWebRequest(ListMessagesUrl(ThreadId), "GET"))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {OpenAiApi}");
            request.SetRequestHeader("OpenAI-Beta", "assistants=v2");
            request.downloadHandler = new DownloadHandlerBuffer();

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                //Debug.Log("ListMessages:- " + request.downloadHandler.text);
                return JsonUtility.FromJson<MessagesList>(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
                return null;
            }
        }
    }
    
    public async Task SendDeleteRequest(string threadID)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Delete(DeleteThread(threadID)))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + OpenAiApi);
            webRequest.SetRequestHeader("OpenAI-Beta", "assistants=v2");

            UnityWebRequestAsyncOperation operation = webRequest.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log("Response: " + webRequest.downloadHandler.text);
            }
        }
    }

    //private async void OnDestroy()
    //{
    //    if (AiThread.id == string.Empty) return;
    //    await SendDeleteRequest(AiThread.id);
    //    Debug.Log("deleted Ai thread :" + AiThread.id);
    //}

}
