using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ChatMessage
{
    public string Message;
    public TMP_Text UI_text;
}

public enum ChatBy
{
    Bot, User
}

public class ChatBoxManager : MonoBehaviour
{
    public static ChatBoxManager instance;
    public GameObject chatPanel, textObject;

    [SerializeField] List<ChatMessage> messages = new List<ChatMessage>();

    [SerializeField] TMP_InputField field;
    [SerializeField] Button SubmitButton;

    public TMP_Text expressionText;

    [SerializeField] MainHandler _mainHandler;

    StringBuilder MasterText = new StringBuilder();

    [SerializeField] string botHeader = "<color=#FF6060>AI: </color>";
    [SerializeField] string userHeader = "<color=#FF6060>User: </color>";

    //int count = 0;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if(_mainHandler == null) _mainHandler = GetComponent<MainHandler>();

        SubmitButton.onClick.AddListener(HandleInput);

    }

    void Update()
    {
//#if UNITY_EDITOR
//        if (Input.GetKeyDown(KeyCode.Space) && !field.isFocused)
//        {
//            SendMessageToChat("Teri maa Randi " + count++, ChatBy.User);
//            SendMessageToChat("Bot ki maa Randi " + count++, ChatBy.Bot);
//        }
//#endif

        if (Input.GetKeyDown(KeyCode.Return) && !field.isFocused)
        {
            if(String.IsNullOrWhiteSpace(field.text)) //simple
                return;
            HandleInput();
        }
    }

    void HandleInput()
    {
        if (!_mainHandler.isFree) return;
        _mainHandler.TakeInputFromUser(field.text);
        field.text = string.Empty;
    }



    public void SendMessageToChat(string Message, ChatBy chatBy)
    {
        switch (chatBy)
        {
            case ChatBy.Bot:
                MasterText.Append(botHeader);
                break;
            case ChatBy.User:
                MasterText.Append(userHeader);
                break;
            default:
                break;
        }

        MasterText.Append(Message);

        ChatMessage newMessage = new ChatMessage();
        newMessage.Message = MasterText.ToString();

        GameObject newTextObj = Instantiate(textObject, chatPanel.transform);
        newMessage.UI_text = newTextObj.GetComponentInChildren<TMP_Text>();

        newMessage.UI_text.text = newMessage.Message;
        messages.Add(newMessage);

        MasterText.Clear();
    }
}
