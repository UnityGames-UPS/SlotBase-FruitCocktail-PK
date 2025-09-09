using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using DG.Tweening;
using System.Linq;
using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

public class SocketIOManager : MonoBehaviour
{
    [SerializeField]
    private SlotBehaviour slotManager;

    internal GameData InitialData = null;
    internal UiData UIData = null;
    internal Root ResultData = null;
    internal Player PlayerData = null;
    [SerializeField]
    internal List<string> bonusdata = null;
    //WebSocket currentSocket = null;
    internal bool isResultdone = false;
    private SocketManager manager;
    [SerializeField] internal UIManager uiManager;
    //HACK: Socket URI
    protected string TestSocketURI = "http://localhost:5000/";
    protected string SocketURI = null;
    // protected string nameSpace="game"; //BackendChanges
    [SerializeField] internal JSFunctCalls JSManager;
    protected string nameSpace = "playground"; //BackendChanges
    private Socket gameSocket; //BackendChanges
    [SerializeField]
    private string TestToken;
    protected string gameID = "SL-FC";//SL-FC
    //protected string gameID = "";

    internal bool isLoaded = false;
    internal bool SetInit = false;
    private const int maxReconnectionAttempts = 6;
    private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);

    private bool isConnected = false; //Back2 Start
    private bool hasEverConnected = false;
    private const int MaxReconnectAttempts = 5;
    private const float ReconnectDelaySeconds = 2f;

    private float lastPongTime = 0f;
    private float pingInterval = 2f;
    private float pongTimeout = 3f;
    private bool waitingForPong = false;
    private int missedPongs = 0;
    private const int MaxMissedPongs = 5;
    private Coroutine PingRoutine; //Back2 end

    [SerializeField] private GameObject RaycastBlocker; //Back2 Start

    private void Start()
    {
        //OpenWebsocket();

        //Open Socket To Connect To The Server
        OpenSocket();
    }

    void ReceiveAuthToken(string jsonData)
    {
        Debug.Log("Received data: " + jsonData);

        // Parse the JSON data
        var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
        SocketURI = data.socketURL;
        myAuth = data.cookie;
        nameSpace = data.nameSpace;
        // Proceed with connecting to the server using myAuth and socketURL
    }
    void CloseGame()
    {
        Debug.Log("Unity: Closing Game");
        StartCoroutine(CloseSocket());
    }

    string myAuth = null;

    private void OpenSocket()
    {
        //Create and setup SocketOptions
        SocketOptions options = new SocketOptions(); //Back2 Start
        options.AutoConnect = false;
        options.Reconnection = false;
        options.Timeout = TimeSpan.FromSeconds(3); //Back2 end
        options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket; //BackendChanges

        //        Application.ExternalCall("window.parent.postMessage", "authToken", "*");

#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("authToken");
        StartCoroutine(WaitForAuthToken(options));
#else
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = TestToken,
                gameId = gameID
            };
        };
        options.Auth = authFunction;
        // Proceed with connecting to the server
        SetupSocketManager(options);
#endif
    }

    private IEnumerator WaitForAuthToken(SocketOptions options)
    {
        // Wait until myAuth is not null
        while (myAuth == null)
        {
            Debug.Log("My Auth is null");
            yield return null;
        }
        while (SocketURI == null)
        {
            Debug.Log("My Socket is null");
            yield return null;
        }

        Debug.Log("My Auth is not null");
        // Once myAuth is set, configure the authFunction
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = myAuth,
            };
        };
        options.Auth = authFunction;

        Debug.Log("Auth function configured with token: " + myAuth);

        // Proceed with connecting to the server
        SetupSocketManager(options);
    }

    private void SetupSocketManager(SocketOptions options)
    {
#if UNITY_EDITOR
        //Create and Setup SocketManager for Testing
        this.manager = new SocketManager(new Uri(TestSocketURI), options);
#else
        // Create and setup SocketManager
        this.manager = new SocketManager(new Uri(SocketURI), options);
#endif

        if (string.IsNullOrEmpty(nameSpace))
        {  //BackendChanges Start
            gameSocket = this.manager.Socket;
        }
        else
        {
            print("nameSpace: " + nameSpace);
            gameSocket = this.manager.GetSocket("/" + nameSpace);
        }
        // Set subscriptions
        gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        gameSocket.On(SocketIOEventTypes.Disconnect, OnDisconnected);
        gameSocket.On<Error>(SocketIOEventTypes.Error, OnError);
        gameSocket.On<string>("game:init", OnListenEvent);
        gameSocket.On<string>("result", OnResult);
        gameSocket.On<bool>("socketState", OnSocketState);
        gameSocket.On<string>("internalError", OnSocketError);
        gameSocket.On<string>("alert", OnSocketAlert);
        gameSocket.On<string>("pong", OnPongReceived);
        gameSocket.On<string>("AnotherDevice", OnSocketOtherDevice); //BackendChanges Finish
        manager.Open();
    }

    // Connected event handler implementation
    void OnConnected(ConnectResponse resp)
    {
        Debug.Log("✅ Connected to server.");

        if (hasEverConnected)
        {
            uiManager.CheckAndClosePopups();
        }

        isConnected = true;
        hasEverConnected = true;
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        SendPing();
    }
    void OnResult(string data)
    {
        ParseResponse(data);
    }
    private void OnDisconnected()
    {
        Debug.LogWarning("⚠️ Disconnected from server.");
        isConnected = false;
        uiManager.DisconnectionPopup();
        ResetPingRoutine();

    }

    private void OnError(Error err)
    {
        Debug.LogError("Socket Error Message: " + err);
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("error");
#endif
    }
    private void OnPongReceived(string data) //Back2 Start
    {
        Debug.Log("✅ Received pong from server.");
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        Debug.Log($"⏱️ Updated last pong time: {lastPongTime}");
        Debug.Log($"📦 Pong payload: {data}");
    } //Back2 end
    private void OnListenEvent(string data)
    {
        Debug.Log("Received some_event with data: " + data);
        ParseResponse(data);
    }

    private void OnSocketState(bool state)
    {
        if (state)
        {
            Debug.Log("my state is " + state);
            //InitRequest("AUTH");
        }
        else
        {

        }
    }
    private void OnSocketError(string data)
    {
        Debug.Log("Received error with data: " + data);
    }
    private void OnSocketAlert(string data)
    {
        Debug.Log("Received alert with data: " + data);
    }

    private void OnSocketOtherDevice(string data)
    {
        Debug.Log("Received Device Error with data: " + data);
        uiManager.ADfunction();
    }

    private void SendPing() //Back2 Start
    {
        ResetPingRoutine();
        PingRoutine = StartCoroutine(PingCheck());
    }

    void ResetPingRoutine()
    {
        if (PingRoutine != null)
        {
            StopCoroutine(PingRoutine);
        }
        PingRoutine = null;
    }

    private IEnumerator PingCheck()
    {
        while (true)
        {
            Debug.Log($"🟡 PingCheck | waitingForPong: {waitingForPong}, missedPongs: {missedPongs}, timeSinceLastPong: {Time.time - lastPongTime}");

            if (missedPongs == 0)
            {
                uiManager.CheckAndClosePopups();
            }

            // If waiting for pong, and timeout passed
            if (waitingForPong)
            {
                if (missedPongs == 2)
                {
                    uiManager.ReconnectionPopup();
                }
                missedPongs++;
                Debug.LogWarning($"⚠️ Pong missed #{missedPongs}/{MaxMissedPongs}");

                if (missedPongs >= MaxMissedPongs)
                {
                    Debug.LogError("❌ Unable to connect to server — 5 consecutive pongs missed.");
                    isConnected = false;
                    uiManager.DisconnectionPopup();
                    yield break;
                }
            }

            // Send next ping
            waitingForPong = true;
            lastPongTime = Time.time;
            Debug.Log("📤 Sending ping...");
            SendDataWithNamespace("ping");
            yield return new WaitForSeconds(pingInterval);
        }
    } //Back2 end
    private void AliveRequest()
    {
        SendDataWithNamespace("YES I AM ALIVE");
    }

    //private void InitRequest(string eventName)
    //{
    //    InitData message = new InitData();
    //    message.Data = new AuthData();
    //    message.Data.GameID = gameID;
    //    message.id = "Auth";
    //    // Serialize message data to JSON
    //    string json = JsonUtility.ToJson(message);
    //    Debug.Log(json);
    //    // Send the message
    //    if (this.manager.Socket != null && this.manager.Socket.IsOpen)
    //    {
    //        this.manager.Socket.Emit(eventName, json);
    //        Debug.Log("JSON data sent: " + json);
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Socket is not connected.");
    //    }
    //}

    internal IEnumerator CloseSocket() //Back2 Start
    {
        RaycastBlocker.SetActive(true);
        ResetPingRoutine();

        Debug.Log("Closing Socket");

        manager?.Close();
        manager = null;

        Debug.Log("Waiting for socket to close");

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Socket Closed");

#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnExit"); //Telling the react platform user wants to quit and go back to homepage
#endif
    }

    private void ParseResponse(string jsonObject)
    {
        Debug.Log(jsonObject);
        Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);

        string id = myData.id;

        switch (id)
        {
            case "initData":
                {
                    InitialData = myData.gameData;
                    UIData = myData.uiData;
                    PlayerData = myData.player;
                    bonusdata = GetBonusData(myData.gameData.spinBonus);

                    if (!SetInit)
                    {
                        Debug.Log(jsonObject);

                        List<string> LinesString = ConvertListListIntToListString(InitialData.lines);
                        // List<string> InitialReels = ConvertListOfListsToStrings(InitialData.Reel);
                        // InitialReels = RemoveQuotes(InitialReels);
                        //Debug.Log(string.Concat("<color=cyan><b>", string.Join(",", LinesString), "</b></color>"));
                        //Debug.Log(string.Concat("<color=cyan><b>", string.Join(",", InitialReels), "</b></color>"));

                        PopulateSlotSocket(LinesString);
                        SetInit = true;
                    }
                    else
                    {
                        RefreshUI();
                    }
                    break;
                }
            case "ResultData":
                {
                    Debug.Log(jsonObject);
                    ResultData = myData;
                    PlayerData = myData.player;
                    isResultdone = true;
                    break;
                }
            case "ExitUser":
                {
                    if (gameSocket != null) //BackendChanges
                    {
                        Debug.Log("Dispose my Socket");
                        this.manager.Close();
                    }
                    // Application.ExternalCall("window.parent.postMessage", "onExit", "*");
#if UNITY_WEBGL && !UNITY_EDITOR
                        JSManager.SendCustomMessage("onExit");
#endif
                    break;
                }
        }
    }

    private void RefreshUI()
    {
        //uiManager.InitialiseUIData(UIData.AbtLogo.link, UIData.AbtLogo.logoSprite, UIData.ToULink, UIData.PopLink, UIData.paylines);
    }
    List<string> GetBonusData(List<int> bonusData)
    {
        List<string> bonusDataString = new List<string>();
        foreach (int data in bonusData)
        {
            bonusDataString.Add(data.ToString());
        }
        return bonusDataString;
    }

    internal void ReactNativeCallOnFailedToConnect() //BackendChanges
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("onExit");
#endif
    }

    private void PopulateSlotSocket(List<string> LineIds)
    {
        slotManager.shuffleInitialMatrix();

        for (int i = 0; i < LineIds.Count; i++)
        {
            slotManager.FetchLines(LineIds[i], i);
        }
        slotManager.SetInitialUI();

        isLoaded = true;
        //Application.ExternalCall("window.parent.postMessage", "OnEnter", "*");
        RaycastBlocker.SetActive(false); 
#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("OnEnter");
#endif

    }

    internal void AccumulateResult(int currBet)
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.type = "SPIN";
        message.payload = new Data();
        message.payload.betIndex = currBet;
        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);
        SendDataWithNamespace("request", json);
    }

    private void SendDataWithNamespace(string eventName, string json = null)
    {
        if (gameSocket != null && gameSocket.IsOpen) //BackendChanges
        {
            if (json != null)
            {
                gameSocket.Emit(eventName, json);
                Debug.Log("JSON data sent: " + json);
            }
            else
            {
                gameSocket.Emit(eventName);
            }
        }
        else
        {
            Debug.LogWarning("Socket is not connected.");
        }
    }

    private List<string> RemoveQuotes(List<string> stringList)
    {
        for (int i = 0; i < stringList.Count; i++)
        {
            stringList[i] = stringList[i].Replace("\"", ""); // Remove inverted commas
        }
        return stringList;
    }

    private List<string> ConvertListListIntToListString(List<List<int>> listOfLists)
    {
        List<string> resultList = new List<string>();

        foreach (List<int> innerList in listOfLists)
        {
            // Convert each integer in the inner list to string
            List<string> stringList = new List<string>();
            foreach (int number in innerList)
            {
                stringList.Add(number.ToString());
            }

            // Join the string representation of integers with ","
            string joinedString = string.Join(",", stringList.ToArray()).Trim();
            resultList.Add(joinedString);
        }

        return resultList;
    }

    private List<string> ConvertListOfListsToStrings(List<List<string>> inputList)
    {
        List<string> outputList = new List<string>();

        foreach (List<string> row in inputList)
        {
            string concatenatedString = string.Join(",", row);
            outputList.Add(concatenatedString);
        }

        return outputList;
    }

    private List<string> TransformAndRemoveRecurring(List<List<string>> originalList)
    {
        // Flattened list
        List<string> flattenedList = new List<string>();
        foreach (List<string> sublist in originalList)
        {
            flattenedList.AddRange(sublist);
        }

        // Remove recurring elements
        HashSet<string> uniqueElements = new HashSet<string>(flattenedList);

        // Transformed list
        List<string> transformedList = new List<string>();
        foreach (string element in uniqueElements)
        {
            transformedList.Add(element.Replace(",", ""));
        }

        return transformedList;
    }
}

[Serializable]
public class MessageData
{
    // public int option;
    // public List<int> index;
    public string type;
    public Data payload;

}
[Serializable]
public class Data
{
    public int betIndex;

}
[Serializable]
public class GameData
{
    public List<List<int>> lines { get; set; }
    public List<double> bets { get; set; }
    public List<int> spinBonus { get; set; }
}

public class BonusResult
{
    public List<List<int>> innerMatrix { get; set; }
    public List<int> outerRingSymbol { get; set; }
    public double totalWinAmount { get; set; }
    public List<double> winings { get; set; }
}



[Serializable]
public class FreeSpins
{
    public int count { get; set; }
    public bool isFreeSpin { get; set; }
}

[Serializable]
public class Root
{
    //Result Data
    public bool success { get; set; }
    public List<List<string>> matrix { get; set; }
    public string name { get; set; }
    public Payload payload { get; set; }
    public Jackpot jackpot { get; set; }
    public Scatter scatter { get; set; }
    public FreeSpins freeSpin { get; set; }
    //Initial Data
    public string id { get; set; }
    public GameData gameData { get; set; }
    public UiData uiData { get; set; }
    public Player player { get; set; }



    public Bonus bonus { get; set; }


}

[Serializable]
public class Bonus
{
    public bool isTriggered { get; set; }
    public Result result { get; set; }
    public double amount { get; set; }
}

[Serializable]
public class Result
{
    public List<int> outerElement { get; set; }
    public List<List<int>> innerElements { get; set; }
    public List<double> winAmount { get; set; }
}

[Serializable]
public class Scatter
{
    public double amount { get; set; }
}
[Serializable]
public class Jackpot
{
    public bool isTriggered { get; set; }
    public double amount { get; set; }
}
[Serializable]
public class Payload
{
    public double winAmount { get; set; }
    public List<Win> wins { get; set; }
}

[Serializable]
public class Win
{
    public int line { get; set; }
    public List<int> positions { get; set; }
    public double amount { get; set; }
}

[Serializable]
public class UiData
{
    public Paylines paylines { get; set; }
}

[Serializable]
public class Paylines
{
    public List<Symbol> symbols { get; set; }

}

[Serializable]
public class Symbol
{
    public int id { get; set; }
    public string name { get; set; }
    public List<int> multiplier { get; set; }
    public string description { get; set; }
}

[Serializable]
public class Player
{
    public double balance { get; set; }
}

[Serializable]
public class AuthTokenData
{
    public string cookie;
    public string socketURL;
    public string nameSpace; //BackendChanges
}