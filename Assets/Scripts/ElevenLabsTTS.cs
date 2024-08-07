using System.Collections;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ElevenLabsTTS : MonoBehaviour
{
    [SerializeField] MainHandler mainHandler;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource audioSource_3d;

    private const string url = "https://api.elevenlabs.io/v1/text-to-speech/9mKHRoYKB5GLFj04MY0i";
    private const string apiKey = "d2c1e4ce6ac029710adcf80484f60dcb";
    public string model_id = "eleven_monolingual_v1";
    public float stability = 0.5f;
    public float similarity_boost = 0.5f;

    [DllImport("__Internal")]
    private static extern bool CloseSampling(string name);

    private void Awake()
    {
        mainHandler = GetComponent<MainHandler>();
    }

    IEnumerator DestroyAudioFile(AudioClip audioClip, float time)
    {
        yield return new WaitForSeconds(time);
#if UNITY_WEBGL && !UNITY_EDITOR
        CloseSampling(audioClip.name);
#endif
        Destroy(audioClip);
    }
    
    public async Task GetTextToSpeech(string textToConvert)
    {
        if (string.IsNullOrWhiteSpace(textToConvert))
        {
            Debug.LogError("Text Is Empty");
            return;
        }

        string jsonData = "{\"text\": \"" + textToConvert + "\", \"model_id\": \"eleven_monolingual_v1\", \"voice_settings\": {\"stability\": 0.5, \"similarity_boost\": 0.5}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", apiKey);

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while(!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("Response received. Status code: " + request.responseCode);
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);
                audioClip.name = Random.Range(500, 500000).ToString();
                if (audioClip == null)
                {
                    Debug.LogError("Failed to load audio clip.");
                    return;
                }
                while (!audioClip.loadState.Equals(AudioDataLoadState.Loaded))
                {
                    Debug.Log("is Loading : " + audioClip.loadState);
                    await Task.Yield();
                }
                audioClip.LoadAudioData();
                if (mainHandler.is2D)
                {
                    audioSource.clip = audioClip;
                    audioSource.Play();
                }
                else 
                {
                    audioSource_3d.clip = audioClip;
                    audioSource_3d.Play();
                }
                
                ChatBoxManager.instance.SendMessageToChat(textToConvert, ChatBy.Bot);

                // Fucking avoiding memory leak still should test...... Might replace with a saneObj rather than creating new one
                Debug.Log("AudioClipLength: " + audioClip.length);
                Debug.Log("Plus 1 " + (audioClip.length + 1));
                //Destroy(audioClip, audioClip.length + 1);
                StartCoroutine(DestroyAudioFile(audioClip,audioClip.length + 1));
            }
        }
    }
}
