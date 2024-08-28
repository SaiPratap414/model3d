using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Text;

public class ElevenLabsTTS : MonoBehaviour
{
    [SerializeField] private string apiKey;
    [SerializeField] private string voiceId;
    private const string API_URL = "https://api.elevenlabs.io/v1/text-to-speech/";

    public void GetTextToSpeech(string text, Action<AudioClip> onComplete, Action<string> onError)
    {
        StartCoroutine(GetTextToSpeechCoroutine(text, onComplete, onError));
    }

    private IEnumerator GetTextToSpeechCoroutine(string text, Action<AudioClip> onComplete, Action<string> onError)
    {
        string url = API_URL + voiceId;

        WWWForm form = new WWWForm();
        form.AddField("text", text);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);
            www.SetRequestHeader("Content-Type", "application/json");

            string jsonBody = JsonUtility.ToJson(new { text = text });
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Error: {www.error}");
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    onComplete?.Invoke(clip);
                }
                else
                {
                    onError?.Invoke("Failed to generate audio clip");
                }
            }
        }
    }
}