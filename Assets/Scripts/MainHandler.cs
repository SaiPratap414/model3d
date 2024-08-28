using System;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using System.Collections;

public class MainHandler : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] GameObject Camera2d;
    [SerializeField] GameObject Camera3d;
    [Header("Characters")]
    [SerializeField] GameObject Character2d;
    [SerializeField] GameObject Character3d;
    public bool is2D = true;

    [Header("")]
    [SerializeField] AnthropicAPIClient _AnthropicAPIClient;
    [SerializeField] ElevenLabsTTS _ElevenLabsTTS;
    [SerializeField] TMP_InputField Settings_Input;
    [SerializeField] TMP_Text Settings_SubHeader;
    [SerializeField] AudioSource audioSource;

    [DllImport("__Internal")]
    private static extern bool CloseSampling(string name);

    public bool isFree = true;

    void Start()
    {
        if (_AnthropicAPIClient == null) _AnthropicAPIClient = GetComponent<AnthropicAPIClient>();
        if (_ElevenLabsTTS == null) _ElevenLabsTTS = GetComponent<ElevenLabsTTS>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    public void TakeInputFromUser(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || !isFree)
        {
            return;
        }

        isFree = false;
        ChatBoxManager.instance.SendMessageToChat(input, ChatBy.User);

        _AnthropicAPIClient.SendMessage(
            input,
            onComplete: (claudeResponse) => ProcessClaudeResponse(claudeResponse),
            onError: (errorMessage) =>
            {
                Debug.LogError($"Failed to get a response from Claude: {errorMessage}");
                isFree = true;
            }
        );
    }

    private void ProcessClaudeResponse(ResponseClaude claudeResponse)
    {
        if (claudeResponse.content == null || claudeResponse.content.Count == 0)
        {
            Debug.LogError("Claude response content is empty.");
            isFree = true;
            return;
        }

        string responseText = claudeResponse.content[0].text;

        // TODO: Implement expression parsing for Claude's response
        // For now, we'll skip the expression parsing part

        Debug.Log("Sending to Voice");
        _ElevenLabsTTS.GetTextToSpeech(
            responseText,
            onComplete: (audioClip) =>
            {
                audioSource.clip = audioClip;
                audioSource.Play();
                Debug.Log("Done Func");
                isFree = true;
            },
            onError: (errorMessage) =>
            {
                Debug.LogError($"Text-to-speech error: {errorMessage}");
                isFree = true;
            }
        );
    }

    public void SetCustomProperties()
    {
        // TODO: Implement custom properties for Claude if needed
        Settings_SubHeader.text = "Done";
    }

    public void SwitchCharacters()
    {
        if (is2D)
        {
            Character2d.gameObject.SetActive(false);
            Camera2d.gameObject.SetActive(false);
            Camera3d.gameObject.SetActive(true);
            Character3d.gameObject.SetActive(true);
            is2D = false;
        }
        else
        {
            Character2d.gameObject.SetActive(true);
            Camera2d.gameObject.SetActive(true);
            Camera3d.gameObject.SetActive(false);
            Character3d.gameObject.SetActive(false);
            is2D = true;
        }
    }
}