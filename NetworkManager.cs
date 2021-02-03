using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Connection Status")]
    public Text connectionStatusText;

    [Header("Login UI Panel")]
    public InputField playerNameInput;
    public GameObject Login_UI_Panel;

    [Header("GameOptions Panel")]
    public GameObject GameOptions_UI_Panel;

    [Header("Create Room Panel")]
    public GameObject CreateRoom_UI_Panel;
    public InputField roomNameInputField;
    public InputField MaxPlayersInputField;
    private string gameMode; //either dm or ctf
   

    [Header("Inside Room Panel")]
    public GameObject InsideRoom_Panel;
    public Text roomInfoText;
    public GameObject playerListPrefab;
    public GameObject playerListPrefabParent;
    public GameObject StartGameButton;
    private bool isPlayerReady = false; //initially isPlayerReady is set false
    [SerializeField] Button PlayerReadyButton;
    private Dictionary<int, GameObject> playerListGameObjects;  //for instantiating playerlist prefab inside room,int-actor num
    public Text gameModeDisplay;
    public Text gameTimeGO;
    public GameObject TotalTime;
    public GameObject TimeLeftButton, TimeRightButton;
    public int teamSelectionNumber;
    public GameObject[] teamType;
    public GameObject leftTeamButton, rightTeamButton,SettingButton,TeamColors, InsideSettingOKButton;
    float gameTime ;



    [Header("Room List Panel")]
    public GameObject RoomList_Panel;
    public GameObject roomListEntryPrebaf;
    public GameObject roomListParentGameObject;


    [Header("Join RandomRoom Panel")]
    public GameObject JoinRandomRoom_Panel;
    public GameObject RandomRoomOptions;

    [Header("Loading Bar")]
    public GameObject LoadingBar_Panel;
    public GameObject Bar_Fill;



    private Dictionary<string, RoomInfo> catchedInfo;
    private Dictionary<string, GameObject> roomListGameObjects;

    #region UnityMethods

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    // Start is called before the first frame update
    void Start()
    {
      //if(PlayerPrefs.GetString("PlayerName") != null)
      //  {
      //      Debug.Log(PlayerPrefs.GetString("PlayerName"));
      //      string PlayerName = PlayerPrefs.GetString("PlayerName");
      //      PhotonNetwork.LocalPlayer.NickName = PlayerName;
      //      PhotonNetwork.ConnectUsingSettings();  //this method connect player to photon server
      //  }


        if (PhotonNetwork.IsConnected)  //coming back to lobby after completing the game
        {
            Debug.Log("InsideRoom panel reloading");
            ActivatePanel(InsideRoom_Panel);
            UpdateInsideRoomTexts(); //called to update roomName,totalPlayer/max.Player,dm/ctf,time
        }
        else
        {
            Debug.Log("No Inside Room panel");
            ActivatePanel(Login_UI_Panel);
            catchedInfo = new Dictionary<string, RoomInfo>();
            roomListGameObjects = new Dictionary<string, GameObject>();
            teamSelectionNumber = 0;
        }
        

      


     
        //ActivateTeamColor(teamSelectionNumber);




    }

    // Update is called once per frame
    void Update()
    {
        
        connectionStatusText.text = "Connection Status : " + PhotonNetwork.NetworkClientState;  //gives client status


        //showing progress when level is loading
        if (PhotonNetwork.LevelLoadingProgress > 0 && PhotonNetwork.LevelLoadingProgress < 1)
        {
            if (!LoadingBar_Panel.active)
            {
                ActivatePanel(LoadingBar_Panel);
            }
           // Debug.Log(PhotonNetwork.LevelLoadingProgress/0.9f);
            float abc = PhotonNetwork.LevelLoadingProgress ;
            Bar_Fill.GetComponent<Image>().fillAmount = abc;
        }

    }
    #endregion

    #region UI Buttons and Callbacks

    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PlayerPrefs.SetString("PlayerName", playerName);
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();  //this method connect player to photon server
        }
        else if (string.IsNullOrEmpty(playerName))
        {
            Debug.Log("Please enter playerName or invalid");
        }
    }

    public void OnCreateRoomButtonClicked()
    {
       

        if (gameMode != null)
        {
            string roomName = roomNameInputField.text;

            if (string.IsNullOrEmpty(roomName))
            {
                roomName = "Room : " + Random.Range(1000, 10000);
            }
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = (byte)int.Parse(MaxPlayersInputField.text);
            roomOptions.IsOpen = true;
            roomOptions.IsVisible = true;

            string[] roomPropsInLobby = { "gm" }; //gm-gamemode
            roomOptions.CustomRoomPropertiesForLobby = roomPropsInLobby;

            gameTime = 5f;  //game ka total countdown time
            ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "gm", gameMode } , {"gt",gameTime} };

            


            roomOptions.CustomRoomProperties = customRoomProperties;

            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
    }

    public void OnShowRoomButtonClicked()
    {
        print("OnShowRoomButtonClicked ");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        ActivatePanel(RoomList_Panel);
    }

    public void OnBackButtonClicked()  //back button in show list panel
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivatePanel(GameOptions_UI_Panel);
    }


    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnJoinRandomRoomButtonClicked(string _gameMode)
    {
        gameMode = _gameMode;

        ActivatePanel(JoinRandomRoom_Panel);
        float gameTime = 5f;  //game ka total countdown time
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = 
            new ExitGames.Client.Photon.Hashtable() { { "gm", _gameMode } };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        { 


            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gm"))  //gm-gameMode
          {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("DM"))
            {
                    //DM scene
                    PhotonNetwork.CurrentRoom.IsOpen = false;  //closing room entry after game starts 
                    //PhotonNetwork.LoadLevel("SampleScene");
                    StartCoroutine("LoadLevelAsync");
                    
                }
            else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("CTF"))
            {
                    //load CTF scene
                    Debug.Log("CTF is under construction");
            }
          }


        }
    }

    public void CreateRoomButtonClickedInGameOptionPanel()
    {
        ActivatePanel(CreateRoom_UI_Panel);
        roomNameInputField.text = null;
        MaxPlayersInputField.text = "";
    }
   
    #endregion

    #region Photon Callbacks
    public override void OnConnected()
    {
        Debug.Log("Connected to internet");
    }

  

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Connected to photon Server");
        ActivatePanel(GameOptions_UI_Panel);


       
  
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created.");
    }

    public override void OnJoinedRoom()  //is called only 1 time
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has joined " + PhotonNetwork.CurrentRoom.Name);

       teamSelectionNumber = 0;  //by default value
        ExitGames.Client.Photon.Hashtable defaultteamType =
    new ExitGames.Client.Photon.Hashtable() { { "teamType",teamSelectionNumber } };  //0 means SOLO teamType
        PhotonNetwork.LocalPlayer.SetCustomProperties(defaultteamType);

        isPlayerReady = false; //by default value
        ExitGames.Client.Photon.Hashtable intialProps =
            new ExitGames.Client.Photon.Hashtable() { { "isPlayerReady", isPlayerReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(intialProps);




        ActivatePanel(InsideRoom_Panel);

        //custom room properties
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gm"))
        {

            object gameModeName;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gm", out gameModeName))
            {
                Debug.Log(gameModeName.ToString());
            }

        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("DM"))
        {
            //DM game mode
            gameModeDisplay.GetComponent<Text>().text = "DM";
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("CTF"))

        {
            //CTF game mode
            gameModeDisplay.GetComponent<Text>().text = "CTF";
            
        }

        //display total time of game
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gt"))
        {
            object time;
            if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gt",out time))
            {
                gameTimeGO.GetComponent<Text>().text = "TIME- " + time.ToString() + " MIN";
            }
        }




        //update roomInfo
        roomInfoText.text = "RoomName: " + PhotonNetwork.CurrentRoom.Name +
            "           Players/Max.Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        if (playerListGameObjects == null)
        {
            Debug.Log("playerListGameObjects dictionary is null");
            playerListGameObjects = new Dictionary<int, GameObject>();
        }

        //Instantiating player list gameobjects
        foreach (Player player in PhotonNetwork.PlayerList)
        {

            GameObject playerListGameObject = Instantiate(playerListPrefab);
            playerListGameObject.transform.SetParent(playerListPrefabParent.transform);
            playerListGameObject.transform.localScale = Vector3.one;

            playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

            if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);  //you 

                readyOrNot();
          

            }
            else
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);  //not you
            }

            if (player.IsMasterClient)
            {
                playerListGameObject.transform.Find("HostOrNot").gameObject.SetActive(true);  //host
            }
            else
            {
                playerListGameObject.transform.Find("HostOrNot").gameObject.SetActive(false);  //not host
            }

            if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                //updating teamType for playerListGameObjects
                //matlab jab hmne join kiya usse pehele jo log already room me the,unki custom properties ke hisaab se unke playerListPrefab me changes
                if (player.CustomProperties.ContainsKey("teamType"))
                {
                    Debug.Log("Contains teamType" + player.NickName);

                    object teamType;
                    if (player.CustomProperties.TryGetValue("teamType", out teamType))
                    {

                        
                        if (int.Parse(teamType.ToString()) == 1)
                        {
                            Debug.Log("1 teamtype " + player.NickName);
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().text = "TEAM RED";
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.red;

                        }
                        else if (int.Parse(teamType.ToString()) == 2)
                        {
                            Debug.Log("2 teamtype " + player.NickName);
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().text = "TEAM GREEN";
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.green;
                        }
                        else if (int.Parse(teamType.ToString()) == 0)
                        {
                            Debug.Log("0 teamtype " + player.NickName);
                            //teamType =0... SOLO player
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().text = "SOLO";
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.white;
                        }
                        else
                        {
                            Debug.Log("BKL teamtype " + player.NickName);
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().text = "BKL";
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.white;
                        }
                    }
                }
                else
                {
                    Debug.Log("no teamType " + player.NickName);
                }
                //readyORnot update
                //matlab jab hmne join kiya usse pehele jo log already room me the,unki custom properties ke hisaab se unke playerListPrefab me changes

                if (player.CustomProperties.ContainsKey("isPlayerReady"))
                {
                    Debug.Log("player contains isPlayer Ready custom properties" + player.NickName);

                    object isPlayerReady;
                    if (player.CustomProperties.TryGetValue("isPlayerReady", out isPlayerReady))
                    {
                        if ((bool)isPlayerReady)
                        {
                            playerListGameObject.transform.Find("readyIndicator").GetComponentInChildren<Text>().text = "READY";
                            playerListGameObject.transform.Find("readyIndicator").GetComponentInChildren<Text>().color = Color.green;


                        }
                        else
                        {
                            playerListGameObject.transform.Find("readyIndicator").GetComponentInChildren<Text>().text = "NOT READY";
                            playerListGameObject.transform.Find("readyIndicator").GetComponentInChildren<Text>().color = Color.red;

                        }


                    }

                }
                else
                {
                    Debug.Log("NO isPlayer Ready" + player.NickName);
                }
            }
            playerListGameObjects.Add(player.ActorNumber, playerListGameObject); //adding playerListGameObject in playerListGameObjects dictionary


            
        }





        //Enabling or disabling StartGameButton if user is masterclient or not
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            StartGameButton.SetActive(true);

        }
        else
        {
            StartGameButton.SetActive(false);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {

        //update roomInfo
        roomInfoText.text = "RoomName: " + PhotonNetwork.CurrentRoom.Name +
            "           Players/Max.Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;


        GameObject playerListGameObject = Instantiate(playerListPrefab);
        playerListGameObject.transform.SetParent(playerListPrefabParent.transform);
        playerListGameObject.transform.localScale = Vector3.one;

        playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;

        // for making the playerIndicator false for the Player Enterd remotely in room 
        if (newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
        }
        else
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
        }
        if (newPlayer.IsMasterClient)
        {
            playerListGameObject.transform.Find("HostOrNot").gameObject.SetActive(true);
        }
        else
        {
            playerListGameObject.transform.Find("HostOrNot").gameObject.SetActive(false);
        }

        playerListGameObjects.Add(newPlayer.ActorNumber, playerListGameObject);

    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        //update roomInfo
        roomInfoText.text = "RoomName: " + PhotonNetwork.CurrentRoom.Name +
            "           Players/Max.Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;



        Destroy(playerListGameObjects[otherPlayer.ActorNumber]);
        playerListGameObjects.Remove(otherPlayer.ActorNumber);


        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            StartGameButton.SetActive(true);
            playerListGameObjects[PhotonNetwork.LocalPlayer.ActorNumber].transform.Find("HostOrNot").gameObject.SetActive(true);
        }


    }
    //called when we left(local player) left the room
    public override void OnLeftRoom()
    {
        ActivatePanel(GameOptions_UI_Panel);
        foreach (GameObject playerListGameObject in playerListGameObjects.Values)
        {
            Destroy(playerListGameObject);
        }
        playerListGameObjects.Clear();
        playerListGameObjects = null;


    }



    //called when we use PhotonNetwork.JoinLobby();,called whenever there is any update in the lobby
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        foreach (RoomInfo room in roomList)
        {
            Debug.Log(room.Name);

            if (!room.IsVisible || !room.IsOpen || room.RemovedFromList)
            {
                if (catchedInfo.ContainsKey(room.Name))
                {
                    catchedInfo.Remove(room.Name); //removing roomName and roomInfo in the dictionary
                }
            }

            else
            {
                if (catchedInfo.ContainsKey(room.Name))
                {
                    catchedInfo[room.Name] = room;  //updating info of the room
                }
                else
                {
                    catchedInfo.Add(room.Name, room);  //adding roomName and roomInfo in the dictionary
                }




            }
        }

        foreach (var room in catchedInfo.Values)  //showing rooms list in show rooms panel
        {
            GameObject roomListEntryGameObject = Instantiate(roomListEntryPrebaf);
            roomListEntryGameObject.transform.SetParent(roomListParentGameObject.transform); //setting content as parent
            roomListEntryGameObject.transform.localScale = Vector3.one;

            roomListEntryGameObject.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
            roomListEntryGameObject.transform.Find("RoomPlayersText").GetComponent<Text>().text =
                                                   room.PlayerCount + "/" + room.MaxPlayers;
            roomListEntryGameObject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() =>
                                                     OnJoinRoomButtonClicked(room.Name));
            roomListGameObjects.Add(room.Name, roomListEntryGameObject); //adding above gameObjects to delete for next time

        }
        
    }

    public override void OnLeftLobby()
    {
        ClearRoomListView();
        catchedInfo.Clear();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);  //debugging the fail message
        if (gameMode != null)
        {

            string roomName = "Room : " + Random.Range(1000, 10000);

            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 12;
            roomOptions.IsOpen = true;
            roomOptions.IsVisible = true;

            string[] roomPropsInLobby = { "gm" }; //gm-gamemode
            roomOptions.CustomRoomPropertiesForLobby = roomPropsInLobby;
            float gameTime = 1f;
            ExitGames.Client.Photon.Hashtable customRoomProperties = 
                new ExitGames.Client.Photon.Hashtable() { { "gm", gameMode },{ "gt",gameTime} };
            roomOptions.CustomRoomProperties = customRoomProperties;

            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log(cause);
    }


    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log("properties have changed for " + targetPlayer.NickName);
        GameObject playerListPrefab;
        if (playerListGameObjects.TryGetValue(targetPlayer.ActorNumber,out playerListPrefab))
        {
            //readyORnot update
            object isPlayerReady;
            if(changedProps.TryGetValue("isPlayerReady",out isPlayerReady))
            {
                if ((bool)isPlayerReady)
                {
                    playerListPrefab.transform.Find("readyIndicator").GetComponentInChildren<Text>().text = "READY";
                    playerListPrefab.transform.Find("readyIndicator").GetComponentInChildren<Text>().color = Color.green;


                }
                else
                {
                    playerListPrefab.transform.Find("readyIndicator").GetComponentInChildren<Text>().text = "NOT READY";
                    playerListPrefab.transform.Find("readyIndicator").GetComponentInChildren<Text>().color = Color.red;

                }


            }
            //teamType update
            object teamType;
            if(changedProps.TryGetValue("teamType",out teamType))
            {
                if (int.Parse(teamType.ToString()) == 1){
                    playerListPrefab.transform.Find("teamType").GetComponentInChildren<Text>().text = "TEAM RED";
                    playerListPrefab.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.red;
                    
                }
                else if(int.Parse(teamType.ToString()) == 2)
                {
                    playerListPrefab.transform.Find("teamType").GetComponentInChildren<Text>().text = "TEAM GREEN";
                    playerListPrefab.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.green;
                }
                else
                {
                    playerListPrefab.transform.Find("teamType").GetComponentInChildren<Text>().text = "SOLO";
                    playerListPrefab.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.white;
                }
            }
        }

    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("OnROOMProperties have updated");
        object abc;
        if(propertiesThatChanged.TryGetValue("gt",out abc))  //that means abc = total game time
        {
            gameTimeGO.GetComponent<Text>().text = "TIME- " + abc.ToString() +" MIN";
        }
         
    }
    #endregion

    #region Public Methods
    public void ActivatePanel(GameObject panelToBeActivated)
    {
                       
        Login_UI_Panel.SetActive(Login_UI_Panel.Equals(panelToBeActivated));
        GameOptions_UI_Panel.SetActive(GameOptions_UI_Panel.Equals(panelToBeActivated));
        CreateRoom_UI_Panel.SetActive(CreateRoom_UI_Panel.Equals(panelToBeActivated));
        InsideRoom_Panel.SetActive(InsideRoom_Panel.Equals(panelToBeActivated));
        RoomList_Panel.SetActive(RoomList_Panel.Equals(panelToBeActivated));
        JoinRandomRoom_Panel.SetActive(JoinRandomRoom_Panel.Equals(panelToBeActivated));
        RandomRoomOptions.SetActive(RandomRoomOptions.Equals(panelToBeActivated));
        LoadingBar_Panel.SetActive(LoadingBar_Panel.Equals(panelToBeActivated));

    }


    #region InsideRoomPanel

    public void LeftTeamButton()
    {
        teamSelectionNumber -= 1;
        if (teamSelectionNumber <0)
        {
            teamSelectionNumber = 2;
        }
        ActivateTeamColor(teamSelectionNumber);
    }
    public void RightTeamButton()
    {
        teamSelectionNumber += 1;
            if(teamSelectionNumber >= teamType.Length)
        {
            teamSelectionNumber = 0;
        }
        ActivateTeamColor(teamSelectionNumber);
    }

    public void OnSettingButtonClicked()
    {
        
        ExitGames.Client.Photon.Hashtable intialteamType =
           new ExitGames.Client.Photon.Hashtable() { { "teamType", teamSelectionNumber } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(intialteamType);


        leftTeamButton.SetActive(true);     //leftcolorbutton on
        rightTeamButton.SetActive(true);    //right color button on
         TeamColors.SetActive(true);
        InsideSettingOKButton.SetActive(true);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)  //start game button off
        {
            StartGameButton.SetActive(false);
            TimeLeftButton.SetActive(true);
            TimeRightButton.SetActive(true);
            TotalTime.SetActive(true);
            object gameTime;
            if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gt",out gameTime))
            {
                TotalTime.GetComponent<TextMeshProUGUI>().text = gameTime.ToString() + " MIN";
            }
           
        }
       
        PlayerReadyButton.gameObject.SetActive(false);  // ready button of
       SettingButton.SetActive(false);    // setting button itself off


    }
    public void OnOKButtonInsideSettingClicked()
    {
        ExitGames.Client.Photon.Hashtable finalteamType =
           new ExitGames.Client.Photon.Hashtable() { { "teamType", teamSelectionNumber } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(finalteamType) ;

        leftTeamButton.SetActive(false);     //leftcolorbutton on
        rightTeamButton.SetActive(false);    //right color button on
        TeamColors.SetActive(false);
       InsideSettingOKButton.SetActive(false);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)  //start game button off
        {
            StartGameButton.SetActive(true);
            TimeLeftButton.SetActive(false);
            TimeRightButton.SetActive(false);
            TotalTime.SetActive(false);
        }

        PlayerReadyButton.gameObject.SetActive(true);  // ready button of
        SettingButton.SetActive(true);    // setting button itself off
    }


    //used by toggles(DM or CTF)in create room panel
    public void gameModeSelection(string _gameMode)
    {
        gameMode = _gameMode;
    }



    public void MinusTimeButton()
    {
        if (gameTime != 1)
        {
            gameTime -= 1;
        }
        if(gameTime <= 0)
        {
            gameTime = 1;
        }

        TotalTime.GetComponent<TextMeshProUGUI>().text =   gameTime + " MIN";
        ExitGames.Client.Photon.Hashtable changedRoomProperties = 
            new ExitGames.Client.Photon.Hashtable() {  { "gt", gameTime } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(changedRoomProperties);

    }

    public void PlusTimeButton()
    {
        if (gameTime != 10)
        {
            gameTime += 1;
        }
        if(gameTime > 10)
        {
            gameTime = 10;
        }

        TotalTime.GetComponent<TextMeshProUGUI>().text =  gameTime + " MIN";
        ExitGames.Client.Photon.Hashtable changedRoomProperties =
            new ExitGames.Client.Photon.Hashtable() { { "gt", gameTime } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(changedRoomProperties);


    }
    #endregion


    #endregion

    #region Private Methods

    IEnumerator LoadLevelAsync()  //called in line number 184
    {
        PhotonNetwork.LoadLevel("Level");
        
        //while (PhotonNetwork.LevelLoadingProgress < 1)
        //{
        //    Debug.Log("Loading: %" + (int)(PhotonNetwork.LevelLoadingProgress * 100));
        //    //loadAmount = async.progress;
        //   // progressBar.fillAmount = PhotonNetwork.LevelLoadingProgress;
         yield return new WaitForEndOfFrame();
        //}
    }




    void OnJoinRoomButtonClicked(string _roomName)
        {
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby(); //as we are joining room,so get out of lobby


            }
            
            PhotonNetwork.JoinRoom(_roomName);

        }

        void ClearRoomListView()
        {
            foreach (var roomListGameObject in roomListGameObjects.Values)
            {
                Destroy(roomListGameObject);
            }
            roomListGameObjects.Clear();
        }



    void readyOrNot()
    {
        ExitGames.Client.Photon.Hashtable intialProps =
            new ExitGames.Client.Photon.Hashtable() { { "isPlayerReady", isPlayerReady } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(intialProps);


        PlayerReadyButton.onClick.AddListener(() =>
        {

            isPlayerReady = !isPlayerReady; //change false to true and vice-versa
            DisplayReadyorNotinplayerListPrefab();  //right down button text change


            ExitGames.Client.Photon.Hashtable newProps =
             new ExitGames.Client.Photon.Hashtable() { { "isPlayerReady", isPlayerReady } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(newProps);

            SettingButton.SetActive(!isPlayerReady);
        }
           );
    }

    void DisplayReadyorNotinplayerListPrefab()
    {

        
        GameObject playerListPrefab;
        playerListGameObjects.TryGetValue(PhotonNetwork.LocalPlayer.ActorNumber, out playerListPrefab);
        if (isPlayerReady)
        {
          playerListPrefab.transform.Find("readyIndicator").GetComponentInChildren<Text>().text = "READY";
            playerListPrefab.transform.Find("readyIndicator").GetComponentInChildren<Text>().color = Color.green;
            PlayerReadyButton.GetComponentInChildren<Text>().text = "Not Ready";

        }
        else
        {
            playerListPrefab.transform.Find("readyIndicator").GetComponentInChildren<Text>().text = "NOT READY";
            playerListPrefab.transform.Find("readyIndicator").GetComponentInChildren<Text>().color = Color.red; ;
            PlayerReadyButton.GetComponentInChildren<Text>().text = "Ready??";
        }


    }

    void ActivateTeamColor(int numb)
    {
        teamType[0].SetActive(numb.Equals(0));
        teamType[1].SetActive(numb.Equals(1));
        teamType[2].SetActive(numb.Equals(2));

        if(playerListGameObjects.TryGetValue(PhotonNetwork.LocalPlayer.ActorNumber,out GameObject playerListPrefab))
        {
            if (numb == 1)
            {
                playerListPrefab.transform.Find("teamType").GetComponentInChildren<Text>().text = "TEAM RED";
                playerListPrefab.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.red;

            }
            else if (numb == 2)
            {
                playerListPrefab.transform.Find("teamType").GetComponentInChildren<Text>().text = "TEAM GREEN";
                playerListPrefab.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.green;
            }
            else
            {
                playerListPrefab.transform.Find("teamType").GetComponentInChildren<Text>().text = "SOLO";
                playerListPrefab.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.white;
            }
        }

            

    }

    void UpdateInsideRoomTexts()
    {

        //update roomInfo
        roomInfoText.text = "RoomName: " + PhotonNetwork.CurrentRoom.Name +
            "           Players/Max.Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("DM"))
        {
            //DM game mode
            gameModeDisplay.GetComponent<Text>().text = "DM";
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsValue("CTF"))

        {
            //CTF game mode
            gameModeDisplay.GetComponent<Text>().text = "CTF";

        }

        //display total time of game
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gt"))
        {
            object time;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gt", out time))
            {
                gameTimeGO.GetComponent<Text>().text = "TIME- " + time.ToString() + " MIN";
            }
        }





        
       
       //      Transform[] allChildren = playerListPrefabParent.GetComponentsInChildren<Transform>();
       //foreach(Transform abc in allChildren)
       // {
       //     Destroy(abc.gameObject);
       // }
        playerListGameObjects = null;

            if (playerListGameObjects == null)
        {
            Debug.Log("playerListGameObjects dictionary is null");
            playerListGameObjects = new Dictionary<int, GameObject>();
        }

        //Instantiating player list gameobjects
        foreach (Player player in PhotonNetwork.PlayerList)
        {

            GameObject playerListGameObject = Instantiate(playerListPrefab);
            playerListGameObject.transform.SetParent(playerListPrefabParent.transform);
            playerListGameObject.transform.localScale = Vector3.one;

            playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

            if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);  //you 

                readyOrNot();


            }
            else
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);  //not you
            }


            if (player.IsMasterClient)
            {
                playerListGameObject.transform.Find("HostOrNot").gameObject.SetActive(true);  //host
            }
            else
            {
                playerListGameObject.transform.Find("HostOrNot").gameObject.SetActive(false);  //not host
            }


            if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
            {
                //updating teamType for playerListGameObjects
                //matlab jab hmne join kiya usse pehele jo log already room me the,unki custom properties ke hisaab se unke playerListPrefab me changes
                if (player.CustomProperties.ContainsKey("teamType"))
                {
                    Debug.Log("Contains teamType" + player.NickName);

                    object teamType;
                    if (player.CustomProperties.TryGetValue("teamType", out teamType))
                    {


                        if (int.Parse(teamType.ToString()) == 1)
                        {
                            Debug.Log("1 teamtype " + player.NickName);
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().text = "TEAM RED";
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.red;

                        }
                        else if (int.Parse(teamType.ToString()) == 2)
                        {
                            Debug.Log("2 teamtype " + player.NickName);
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().text = "TEAM GREEN";
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.green;
                        }
                        else if (int.Parse(teamType.ToString()) == 0)
                        {
                            Debug.Log("0 teamtype " + player.NickName);
                            //teamType =0... SOLO player
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().text = "SOLO";
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.white;
                        }
                        else
                        {
                            Debug.Log("BKL teamtype " + player.NickName);
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().text = "BKL";
                            playerListGameObject.transform.Find("teamType").GetComponentInChildren<Text>().color = Color.white;
                        }
                    }
                }
                else
                {
                    Debug.Log("no teamType " + player.NickName);
                }
                //readyORnot update
                //matlab jab hmne join kiya usse pehele jo log already room me the,unki custom properties ke hisaab se unke playerListPrefab me changes

                if (player.CustomProperties.ContainsKey("isPlayerReady"))
                {
                    Debug.Log("player contains isPlayer Ready custom properties" + player.NickName);

                    object isPlayerReady;
                    if (player.CustomProperties.TryGetValue("isPlayerReady", out isPlayerReady))
                    {
                        if ((bool)isPlayerReady)
                        {
                            playerListGameObject.transform.Find("readyIndicator").GetComponentInChildren<Text>().text = "READY";
                            playerListGameObject.transform.Find("readyIndicator").GetComponentInChildren<Text>().color = Color.green;


                        }
                        else
                        {
                            playerListGameObject.transform.Find("readyIndicator").GetComponentInChildren<Text>().text = "NOT READY";
                            playerListGameObject.transform.Find("readyIndicator").GetComponentInChildren<Text>().color = Color.red;

                        }


                    }

                }
                else
                {
                    Debug.Log("NO isPlayer Ready" + player.NickName);
                }
            }
            playerListGameObjects.Add(player.ActorNumber, playerListGameObject); //adding playerListGameObject in playerListGameObjects dictionary


            //making room visible again for new players to join
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = true;  //making the room open again so that new players can join
                PhotonNetwork.CurrentRoom.IsVisible = true;
            }



            //Enabling or disabling StartGameButton if user is masterclient or not
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                StartGameButton.SetActive(true);

            }
            else
            {
                StartGameButton.SetActive(false);
            }

        }
    }




    #endregion

}
