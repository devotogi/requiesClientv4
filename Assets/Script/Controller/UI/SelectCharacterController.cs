using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectCharacterController : MonoBehaviour
{
    [SerializeField]
    GameObject createCharaterModal;

    [SerializeField]
    GameObject warriorSelectBtn;

    [SerializeField]
    GameObject archerSelectBtn;

    [SerializeField]
    Image ProfileImg;

    [SerializeField]
    TMP_Text TypeText;

    [SerializeField]
    TMP_Text ExplanationText;

    [SerializeField]
    TMP_InputField UsernameInput;

    [SerializeField]
    GameObject playerSelect1;
    [SerializeField]
    GameObject playerSelect2;
    [SerializeField]
    GameObject playerSelect3;
    [SerializeField]
    GameObject highlight;
    [SerializeField]
    TMP_Text alertText;

    List<GameObject> playerList = new List<GameObject>();
    Type.CharacterType _selectType;
    private Color selectBtnOriginalColor;
    PlayerSelectObject selectObject = null;

    private void Start()
    {
        selectBtnOriginalColor = warriorSelectBtn.GetComponent<Image>().color;
        SelectCharaterType(Type.CharacterType.Warrior);
    }

    public void ShowCreateCharaterModal() 
    {
        createCharaterModal.SetActive(true);
    }

    public void HideCreateCharaterModal() 
    {
        SelectCharaterType(Type.CharacterType.Warrior);
        createCharaterModal.SetActive(false);
    }

    public void SelectWarrior() 
    {
        SelectCharaterType(Type.CharacterType.Warrior);
    }

    public void SelectArcher()
    {
        SelectCharaterType(Type.CharacterType.Archer);
    }

    public void SelectCharaterType(Type.CharacterType characterType) 
    {
        warriorSelectBtn.GetComponent<Image>().color = selectBtnOriginalColor;
        archerSelectBtn.GetComponent<Image>().color = selectBtnOriginalColor;
        _selectType = characterType;
        UsernameInput.text = "";

        switch (characterType)
        {
            case Type.CharacterType.Warrior:
                warriorSelectBtn.GetComponent<Image>().color = Color.red;
                TypeText.text = "타입:전사";
                ExplanationText.text = "그림자의 속삭임이 떨어지는 황야의 땅에서 태어난, 힘과 용기의 전령, 나는 라이나 블레이드하트. 검은 갑옷에 싸여진 몸은 마치 밤 그 자체, 그 중에서도 내 검은 눈은 불타오르는 전쟁의 불길을 닮고 있다.";
                ProfileImg.sprite = Resources.Load<Sprite>("Img/Warrior");
                break;

            case Type.CharacterType.Archer:
                archerSelectBtn.GetComponent<Image>().color = Color.red;
                ExplanationText.text = "아리사는 그린우드의 마법사로 태어났으며, 어린 시절부터 숲의 수호자로서 훈련받았다. 어둠의 세력이 그린우드를 위협하자, 그녀는 고대 마법의 힘을 찾아 나서 어둠을 물리치고 숲을 지키기 위한 여정에 나섰다. 그녀의 모험은 그린우드의 미래를 결정짓고, 그곳에 희망의 빛을 되찾게 할 것이다.";
                TypeText.text = "타입:아처";
                ProfileImg.sprite = Resources.Load<Sprite>("Img/Archer");
                break;
        }
    }

    public void DeleteCharater() 
    {
        if (selectObject == null) return;

        PlayerSelectObject pl = selectObject.GetComponent<PlayerSelectObject>();
        byte[] playerNameBytes = Encoding.Unicode.GetBytes(pl.playerName);
        int playerNameLen = playerNameBytes.Length;
        int userSQ = Managers.Data.userSQ;
        byte playerType = (byte)pl.characterType;
        byte[] bytes = new byte[1000];
        MemoryStream ms = new MemoryStream(bytes);
        ms.Position = 0;

        int pktSize = 4 + 4 + playerNameLen + 1 + 4;

        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write((Int16)Type.PacketProtocol.C2S_DELETECHARACTER);
        bw.Write((Int16)pktSize);
        bw.Write((Int32)playerNameLen);
        bw.Write(playerNameBytes);
        bw.Write(playerType);
        bw.Write((Int32)userSQ);
        Managers.Data.Network.SendPacket(bytes, pktSize, Type.ServerPort.LOGIN_PORT);
    }

    GameObject GetIndexToGameObject(int index)
    {
        if (index == 0)
            return playerSelect1;
        else if (index == 1)
            return playerSelect2;
        else if (index == 2)
            return playerSelect3;
        return null;
    }


    internal void PlayerListBind(List<Type.PlayerSelectInfo> list)
    {
        playerSelect1.SetActive(true);
        playerSelect2.SetActive(true);
        playerSelect3.SetActive(true);

        for (int i = 0; i < playerList.Count; i++)
        {
            Managers.Resource.Destory(playerList[i]);
        }
        playerList.Clear();

        foreach (var item in list)
        {
            GameObject selectGo = GetIndexToGameObject(item.index);
            selectGo.SetActive(false);
            GameObject player;
            if (item.playerType == Type.CharacterType.Warrior)
                player = Managers.Resource.Instantiate("Player/Warrior");
            else
                player = Managers.Resource.Instantiate("Player/Archer");

            player.transform.position = selectGo.transform.position;
            player.transform.eulerAngles = new Vector3(0, -180, 0);
            GameObject userNameGo = Managers.Resource.Instantiate("UI/Username_Player");
            userNameGo.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 2.0f, player.transform.position.z);
            userNameGo.transform.parent = player.transform;
            userNameGo.transform.GetChild(0).GetComponent<TMP_Text>().text = $"LV.{item.level} {item.playerName}";
            userNameGo.transform.eulerAngles = new Vector3(0, 360, 0);
            PlayerSelectObject pl = player.AddComponent<PlayerSelectObject>();
            pl.playerName = item.playerName;
            pl.characterType = item.playerType;
            pl.level = item.level;
            playerList.Add(player);
        }
    }

    private void Update()
    {
        if (selectObject != null)
        {
            if (highlight.active == false)
                highlight.SetActive(true);

            highlight.transform.position = new Vector3 (selectObject.transform.position.x, highlight.transform.position.y, selectObject.transform.position.z);
        }
        else
        {
            if (highlight.active == true)
                highlight.SetActive(false);
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = 1 << LayerMask.NameToLayer("Player");
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (EventSystem.current.IsPointerOverGameObject() == false)
                {
                    selectObject = hit.transform.gameObject.GetComponent<PlayerSelectObject>();
                }
            }
        }
    }

    public void CreateCharacter()
    {
        alertText.gameObject.SetActive(false);
        string username = UsernameInput.text.Trim();

        if (username == "")
        {
            alertText.gameObject.SetActive(true);
            alertText.text = "닉네임을 입력하세요";
            return;
        }

        byte[] bytes = new byte[1000];
        MemoryStream ms = new MemoryStream(bytes);
        ms.Position = 0;

        byte[] userNameBytes = Encoding.Unicode.GetBytes(username);
        int userNameLen = userNameBytes.Length;
        int pktSize = userNameLen + 4 + 4 + 4 + 4;
        int userSQ = Managers.Data.userSQ;
        int playerType = (int)_selectType;

        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write((Int16)Type.PacketProtocol.C2S_CREATECHARACTER);
        bw.Write((Int16)pktSize);
        bw.Write((Int32)userSQ);
        bw.Write((Int32)playerType);
        bw.Write((Int32)userNameLen);
        bw.Write(userNameBytes);
        Managers.Data.Network.SendPacket(bytes, pktSize, Type.ServerPort.LOGIN_PORT);
        alertText.text = "";
    }

    internal void UserNameAlertMsg(string v)
    {
        alertText.text = v;
        alertText.gameObject.SetActive(true);
    }

    public void GamePlay()
    {
        if (selectObject == null) return;

        PlayerSelectObject pl = selectObject.GetComponent<PlayerSelectObject>();
        byte[] playerNameBytes = Encoding.Unicode.GetBytes(pl.playerName);
        int playerNameLen = playerNameBytes.Length;
        int userSQ = Managers.Data.userSQ;

        byte[] bytes = new byte[1000];
        MemoryStream ms = new MemoryStream(bytes);
        ms.Position = 0;

        int pktSize = playerNameLen + 4 + 4 + 4;

        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write((Int16)Type.PacketProtocol.C2S_GAMEPLAY);
        bw.Write((Int16)pktSize);
        bw.Write((Int32)playerNameLen);
        bw.Write(playerNameBytes);
        bw.Write((Int32)userSQ);
        Managers.Data.Network.SendPacket(bytes, pktSize, Type.ServerPort.LOGIN_PORT);
    }
}
