using System;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MainHandler : MonoBehaviour
{
    [Header("Camers")]
    [SerializeField] GameObject Camera2d;
    [SerializeField] GameObject Camera3d;
    [Header("Charaters")]
    [SerializeField] GameObject Charater2d;
    [SerializeField] GameObject Charater3d;
    public bool is2D = true;
    
    [Header("")]
    [SerializeField] OpenAiApi _OpenAiApi;
    [SerializeField] OpenAIAssistant _OpenAIAssistant;
    [SerializeField] ElevenLabsTTS _ElevenLabsTTS;

    [SerializeField] TMP_InputField Settings_Input;
    [SerializeField] TMP_Text Settings_SubHeader;

    [DllImport("__Internal")]
    private static extern bool CloseSampling(string name);

    public bool isFree = true;

    void Start()
    {
        if(_OpenAiApi == null) _OpenAiApi = GetComponent<OpenAiApi>();
        if (_ElevenLabsTTS == null) _ElevenLabsTTS = GetComponent<ElevenLabsTTS>();
        if(_OpenAIAssistant == null) _OpenAIAssistant = GetComponent<OpenAIAssistant>();
        is2D = false;
    }


    public async void TakeInputFromUser(string input)
    {
        isFree = false;
        if (string.IsNullOrWhiteSpace(input))
        {
            isFree = true;
            return;
        }

        ChatBoxManager.instance.SendMessageToChat(input,ChatBy.User);

        UserResponse GivenResponse = await _OpenAiApi.SendOpenAIRequest(input);
        if (GivenResponse == null)
        {
            Debug.LogError("Failed to get a response.");
            isFree = true;
            return;
        }

        if (Enum.TryParse<Expressions>(GivenResponse.Expression, out Expressions _expression))
        {
            Debug.Log("Parsed Succusfully " + _expression);
            CubismExpression.instance.ChangeExpression(_expression);
        }
        Debug.Log("Sending to Voice");
        await _ElevenLabsTTS.GetTextToSpeech(GivenResponse.Response);

        Debug.Log("Done Func");
        isFree = true;

        
    }

    public void SetCustomProperties()
    {
        _OpenAiApi.CustomCharacteristics = Settings_Input.text;
        Settings_SubHeader.text = "Done";
    }

    
    public void SwitchCharaters()
    {
        if (is2D)
        {
            Charater2d.gameObject.SetActive(false);
            Camera2d.gameObject.SetActive(false);
            Camera3d.gameObject.SetActive(true);
            Charater3d.gameObject.SetActive(true);
            is2D = false;
        }
        else 
        {
            Charater2d.gameObject.SetActive(true);
            Camera2d.gameObject.SetActive(true);
            Camera3d.gameObject.SetActive(false);
            Charater3d.gameObject.SetActive(false);
            is2D = true;
        }
    }


}
