using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class PacketHandler
{
    internal void Handler(ArraySegment<byte> segment)
    {
        ArraySegment<byte> pktCodeByte = new ArraySegment<byte>(segment.Array, 0, sizeof(Int16)); // 2byte 타입 추출
        ArraySegment<byte> pktSizeByte = new ArraySegment<byte>(segment.Array, sizeof(Int16), sizeof(Int16)); // 2byte 사이즈 추출

        Type.PacketProtocol pktCode = (Type.PacketProtocol)BitConverter.ToInt16(pktCodeByte);
        Int16 pktSize = BitConverter.ToInt16(pktSizeByte);
        int dataSize = pktSize - sizeof(Int32);
        ArraySegment<byte> dataPtr = new ArraySegment<byte>(segment.Array, sizeof(Int32), dataSize);

        switch (pktCode)
        {
            case Type.PacketProtocol.S2C_PLAYERINIT:
                PacketHandler_SC2_PLAYERINIT(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_PLAYERSYNC:
                PacketHandler_S2C_PLAYERSYNC(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_PLAYEROUT:
                PacketHandler_S2C_PLAYEROUT(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_PLAYERNEW:
                PacketHandler_S2C_PLAYERNEW(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_PLAYERDESTORY:
                PacketHandler_S2C_PLAYERDESTORY(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_PLAYERLIST:
                PacketHandler_S2C_PLAYERLIST(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_PLAYERREMOVELIST:
                PacketHandler_PLAYERREMOVELIST(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_PLAYERATTACKED:
                PacketHandler_S2C_PLAYERATTACKED(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_PLAYERCHAT:
                PacketHandler_S2C_PLAYERCHAT(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_PLAYERDETH:
                PacketHandler_S2C_PLAYERDETH(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_PLAYERESPAWN:
                PacketHandler_S2C_PLAYERESPAWN(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_MONSTERSPAWN:
                PacketHandler_S2C_MONSTERSPAWN(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_MONSTERREMOVELIST:
                PacketHandler_S2C_MONSTERREMOVELIST(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_MONSTERRENEWLIST:
                PacketHandler_S2C_MONSTERRENEWLIST(dataPtr, dataSize);
                break;
            
            case Type.PacketProtocol.S2C_MONSTERATTACKED:
                PacketHandler_S2C_MONSTERATTACKED(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_MONSTERDEAD:
                PacketHandler_S2C_MONSTERDEAD(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_MONSTERSYNC:
                PacketHandler_S2C_MONSTERSYNC(dataPtr, dataSize);
                break;

            //case Type.PacketProtocol.S2C_NEWMONSTER:
            //    PacketHandler_S2C_NEWMONSTER(dataPtr, dataSize);
            //    break;

            case Type.PacketProtocol.S2C_DELETEMONSTER:
                PacketHandler_S2C_DELETEMONSTER(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_MONSTERDEADCLIENT:
                PacketHandler_S2C_MONSTERDEADCLIENT(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_MONSTERINFO:
                PacketHandler_S2C_MONSTERINFO(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_PLAYEREXP:
                PacketHandler_S2C_PLAYEREXP(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_PLAYERSTATINFO:
                PacketHandler_S2C_PLAYERSTATINFO(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_LOGIN:
                PacketHandler_S2C_LOGIN(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_CHARACTERLIST:
                PacketHandler_S2C_CHARACTERLIST(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_CREATECHARACTER:
                PacketHandler_S2C_CREATECHARACTER(dataPtr, dataSize);
                break;

            case Type.PacketProtocol.S2C_SERVERMOVE:
                PacketHandler_S2C_SERVERMOVE(dataPtr, dataSize);
                break;
        }
    }

    private void PacketHandler_S2C_SERVERMOVE(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);
        Type.ServerPort port = (Type.ServerPort)br.ReadInt16();
        int playerSQ = br.ReadInt32();

        Managers.Data.playerSQ = playerSQ;

        if (port == Type.ServerPort.NOVICE_PORT)
        {
            LoadingSceneController.Instance.LoadScene("NoviceFieldScene");

        }
        else if (port == Type.ServerPort.VILLAGE_PORT)
        {
            LoadingSceneController.Instance.LoadScene("VillageScene");
        }
    }

    private void PacketHandler_S2C_CHARACTERLIST(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        int playerCnt = br.ReadInt32();
        List<Type.PlayerSelectInfo> playerInfos = new List<Type.PlayerSelectInfo>();
        for (int i = 0; i < playerCnt; i++)
        {
            Type.CharacterType playerType = (Type.CharacterType)br.ReadInt32();
            int playerNameLen = br.ReadInt32();
            byte[] playerNameBytes = br.ReadBytes(playerNameLen);
            string playerName = UnicodeEncoding.Unicode.GetString(playerNameBytes);
            int level = br.ReadInt32();
            Type.PlayerSelectInfo playerSelectInfo = new Type.PlayerSelectInfo();
            playerSelectInfo.playerName = playerName;
            playerSelectInfo.playerType = playerType;
            playerSelectInfo.index = i;
            playerSelectInfo.level = level;
            playerInfos.Add(playerSelectInfo);
        }
        GameObject.FindObjectOfType<SelectCharacterController>().PlayerListBind(playerInfos);
    }

    private void PacketHandler_S2C_LOGIN(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);
        int check = br.ReadInt32();
        int userSQ = br.ReadInt32();
        int serverPort = br.ReadInt32();

        GameObject login = GameObject.Find("Login");
        LoginController lc = login.GetComponent<LoginController>();

        if (check == 9999)
        {
            //lc.SetAlert(true);
            //lc.SetAlertMsg("로그인 성공");
            try
            {
                Managers.Data.userSQ = userSQ;
                LoadingSceneController.Instance.LoadScene("SelectCharacterScene");
                // Managers.Data.Network.ServerConnect((Type.ServerPort)serverPort);
                // UnityEngine.SceneManagement.SceneManager.LoadScene("MapScene");
            }
            catch (Exception e) { Debug.LogError(e); }
        }
        else 
        {
            // 로그인 실패
            lc.SetAlert(true);
            lc.SetAlertMsg("아이디 또는 패스워드가 일치하지 않습니다");
        }
    }

    private void PacketHandler_S2C_PLAYERSTATINFO(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);
        float damage = br.ReadSingle();
        float speed = br.ReadSingle();
        float defense = br.ReadSingle();
        int statPoint = br.ReadInt32();
        Managers.Data.PlayerController.StatInfoOpen(damage,speed,defense, statPoint);
    }

    private void PacketHandler_S2C_PLAYEREXP(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);
        int playerId = br.ReadInt32();
        int level = br.ReadByte();
        float exp = br.ReadSingle();
        float expMax = br.ReadSingle();
        float hp = br.ReadSingle();

        if (playerId == Managers.Data.PlayerController.PlayerID)
        {
            Managers.Data.PlayerController.SetExp(level, exp, expMax);
            Managers.Data.PlayerController.SetHp(hp);
            Managers.Data.PlayerController.SetHpMax(hp);
        }
        else
        {
            Managers.Data.PlayerDic.TryGetValue(playerId, out var otherPlayer);
            otherPlayer.SetExp(level, exp, expMax);
            otherPlayer.SetHp(hp);
            otherPlayer.SetHpMax(hp);
        }
    }

    private void PacketHandler_S2C_MONSTERINFO(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        int monsterId =   br.ReadInt32();
        Type.MonsterType monsterType = (Type.MonsterType)br.ReadByte();
        float monsterHp = br.ReadSingle();
        int hitDamage = br.ReadInt32();
        Managers.Data.PlayerController.SetMonsterInfo(monsterId, (int)monsterType, monsterHp);
        Managers.Data.MonsterDic.TryGetValue(monsterId, out var otherMonster);  
        Vector3 damgeSpawnPos = new Vector3(otherMonster.transform.position.x, otherMonster.transform.position.y + 2.0f, otherMonster.transform.position.z);
        Damage damageText = Managers.Resource.Instantiate("UI/DamageText").GetComponent<Damage>();
        damageText.Init(damgeSpawnPos, hitDamage);

        //Managers.Data.MonsterDic.TryGetValue(monsterId, out var monster);
        //Managers.Data.MonsterDic.Remove(monsterId);
        //monster.GetComponent<MonsterController>().Dead();
    }

    private void PacketHandler_S2C_MONSTERDEADCLIENT(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        int monsterId = br.ReadInt32();
        Managers.Data.MonsterDic.TryGetValue(monsterId, out var monster);
        Managers.Data.MonsterDic.Remove(monsterId);
        monster.GetComponent<MonsterController>().Dead();
        Managers.Data.PlayerController.MonsterInfoIsUnActive(monsterId);
    }

    private void PacketHandler_S2C_DELETEMONSTER(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Type.State monsterState = (Type.State)br.ReadInt16();
        int monsterId = br.ReadInt32();
        int monsterType = br.ReadInt32();
        float x = br.ReadSingle();
        float y = br.ReadSingle();
        float z = br.ReadSingle();
        float hp = br.ReadSingle();
        float lx = br.ReadSingle();
        float ly = br.ReadSingle();
        float lz = br.ReadSingle();
        Vector3 pos = new Vector3(x, y, z);
        Vector3 look = new Vector3(lx, ly, lz);

        Managers.Data.MonsterDic.TryGetValue(monsterId, out var monster);
        Managers.Resource.Destory(monster.gameObject);
        Managers.Data.MonsterDic.Remove(monsterId);
    }

    //private void PacketHandler_S2C_NEWMONSTER(ArraySegment<byte> dataPtr, int dataSize)
    //{
    //    MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
    //    BinaryReader br = new BinaryReader(ms);

    //    Type.State monsterState = (Type.State)br.ReadInt16();
    //    int monsterId = br.ReadInt32();
    //    int monsterType = br.ReadInt32();
    //    float x = br.ReadSingle();
    //    float y = br.ReadSingle();
    //    float z = br.ReadSingle();
    //    float hp = br.ReadSingle();
    //    float lx = br.ReadSingle();
    //    float ly = br.ReadSingle();
    //    float lz = br.ReadSingle();
    //    Vector3 pos = new Vector3(x, y, z);
    //    Vector3 look = new Vector3(lx, ly, lz);

    //    GameObject mo = MonsterSpawnType((Type.MonsterType)monsterType);
    //    MonsterController mc = mo.AddComponent<MonsterController>();
    //    GameObject hpObject = Managers.Resource.Instantiate("UI/HP");
    //    HpController hpc = hpObject.GetComponent<HpController>();
    //    mc.HPC = hpc;

    //    mc.SetHp(hp);
    //    mc.MonsterId = monsterId;
    //    mc.MonsterType = (Type.MonsterType)monsterType;
    //    mo.transform.position = pos;

    //    Managers.Data.MonsterDic.Add(monsterId, mo);
    //    mc.GetComponent<MonsterController>().Sync(monsterState, pos, hp, look, Vector3.zero, null);
    //}

    private GameObject MonsterSpawnType(Type.MonsterType type)
    {
        switch (type) 
        {
            case Type.MonsterType.Bear:
                return Managers.Resource.Instantiate("Monster/Bear"); 

            case Type.MonsterType.Skeleton:
                return Managers.Resource.Instantiate("Monster/Skleeton");

            case Type.MonsterType.Thief:
                return Managers.Resource.Instantiate("Monster/Thief");

            default:
                return null;
        }
    }
    private void PacketHandler_S2C_MONSTERSYNC(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);
        int monsterId = 0;

        try
        {
            Type.State monsterState = (Type.State)br.ReadByte();
            monsterId = br.ReadInt32();
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            float hp = br.ReadSingle();
            float lx = br.ReadSingle();
            float ly = br.ReadSingle();
            float lz = br.ReadSingle();
            float dx = br.ReadSingle();
            float dy = br.ReadSingle();
            float dz = br.ReadSingle();

            int connerSize = br.ReadInt32();

            List<Vector2Int> conner = new List<Vector2Int>();

            for (int i = 0; i < connerSize; i++)
            {
                int _x = br.ReadInt32();
                int _z = br.ReadInt32();
                conner.Add(new Vector2Int(_x, _z));
            }

            Vector3 pos = new Vector3(x, y, z);
            Vector3 look = new Vector3(lx, ly, lz);
            Vector3 dest = new Vector3(dx, dy, dz);

            Managers.Data.MonsterDic.TryGetValue(monsterId, out var monster);
            monster.GetComponent<MonsterController>().Sync(monsterState, pos, hp, look, dest, conner);

            if (monsterState == Type.State.ATTACKED) 
            {
                Managers.Data.PlayerController.MonsterInfoIsAttacked(monsterId, hp);
            }
   
        }
        catch (Exception e)
        {
            Debug.Log(monsterId);
        }
    }

    private void PacketHandler_S2C_MONSTERDEAD(ArraySegment<byte> dataPtr, int dataSize)
    {
        try
        {
            MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
            BinaryReader br = new BinaryReader(ms);

            int monsterId = br.ReadInt32();

            Managers.Data.MonsterDic.TryGetValue(monsterId, out var monster);
            Managers.Resource.Destory(monster.gameObject);
            Managers.Data.MonsterDic.Remove(monsterId);
        }
        catch (Exception e) 
        {
            
        }
    }

    private void PacketHandler_S2C_MONSTERATTACKED(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Type.State monsterState = (Type.State)br.ReadInt16();
        Type.MonsterType type = (Type.MonsterType)br.ReadInt32();
        int monsterId = br.ReadInt32();
        float x = br.ReadSingle();
        float y = br.ReadSingle();
        float z = br.ReadSingle();
        Vector3 pos = new Vector3(x, y, z);
        float hp = br.ReadSingle();
        float lx = br.ReadSingle();
        float ly = br.ReadSingle();
        float lz = br.ReadSingle();
        Vector3 look = new Vector3(lx, ly, lz);

        Managers.Data.MonsterDic.TryGetValue(monsterId, out var monster);
        MonsterController mc = monster.GetComponent<MonsterController>();
        mc.Sync(monsterState, pos, hp, look, Vector3.zero, null);   
    }

    private void PacketHandler_S2C_MONSTERRENEWLIST(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 cnt = br.ReadInt32();

        for (int i = 0; i < cnt; i++)
        {
            Type.State monsterState = (Type.State)br.ReadByte();
            Type.MonsterType monsterType = (Type.MonsterType)br.ReadByte();
            Int32 monsterId = br.ReadInt32();
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            float hp = br.ReadSingle();
            float lx = br.ReadSingle(); 
            float ly = br.ReadSingle();
            float lz = br.ReadSingle();
            float dx = br.ReadSingle();
            float dy = br.ReadSingle();
            float dz = br.ReadSingle();

            int connerSize = br.ReadInt32();
            List<Vector2Int> conner = new List<Vector2Int>();

            for (int j = 0; j < connerSize; j++)
            {
                int _x = br.ReadInt32();
                int _z = br.ReadInt32();
                conner.Add(new Vector2Int(_x, _z));
            }

            Vector3 pos = new Vector3(x, y, z);
            Vector3 look = new Vector3(lx, ly, lz);
            Vector3 dest = new Vector3(dx, dy, dz);

            GameObject mo = MonsterSpawnType((Type.MonsterType)monsterType);
            MonsterController mc = mo.AddComponent<MonsterController>();
            //GameObject hpObject = Managers.Resource.Instantiate("UI/HP");
            //HpController hpc = hpObject.GetComponent<HpController>();

            mc.MonsterId = monsterId;
            mc.MonsterType = (Type.MonsterType)monsterType;
            mo.transform.position = pos;
            //mc.HPC = hpc;

            Managers.Data.MonsterDic.Add(monsterId, mo);
            mc.GetComponent<MonsterController>().Sync(monsterState, pos, hp, look, dest, conner);
        }
    }

    private void PacketHandler_S2C_MONSTERREMOVELIST(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 cnt = br.ReadInt32();

        for (int i = 0; i < cnt; i++)
        {
            int monsterId = br.ReadInt32();
            Managers.Data.MonsterDic.TryGetValue(monsterId, out var monster);
            Managers.Resource.Destory(monster);
            Managers.Data.MonsterDic.Remove(monsterId);
        }
    }

    private void PacketHandler_S2C_MONSTERSPAWN(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Type.State monsterState = (Type.State)br.ReadByte();
        Int32 monsterId = br.ReadInt32();
        Type.MonsterType monsterType = (Type.MonsterType)br.ReadByte();
        float x = br.ReadSingle();
        float y = br.ReadSingle();
        float z = br.ReadSingle();
        float hp = br.ReadSingle();
        float lx = br.ReadSingle();
        float ly = br.ReadSingle();
        float lz = br.ReadSingle();
        float dx = br.ReadSingle();
        float dy = br.ReadSingle();
        float dz = br.ReadSingle();

        int connerSize = br.ReadInt32();
        List<Vector2Int> conner = new List<Vector2Int>();

        for (int i = 0; i < connerSize; i++)
        {
            int _x = br.ReadInt32();
            int _z = br.ReadInt32();
            conner.Add(new Vector2Int(_x, _z));
        }

        Vector3 pos = new Vector3(x, y, z);
        Vector3 look = new Vector3(lx, ly, lz);
        Vector3 dest = new Vector3(dx, dy, dz);

        GameObject mo = MonsterSpawnType((Type.MonsterType)monsterType);
        MonsterController mc = mo.AddComponent<MonsterController>();
        //GameObject hpObject = Managers.Resource.Instantiate("UI/HP");
        //HpController hpc = hpObject.GetComponent<HpController>();

      
        mc.MonsterId = monsterId;
        mc.MonsterType = (Type.MonsterType) monsterType;
        mo.transform.position = pos;

        Managers.Data.MonsterDic.Add(monsterId, mo);
        mc.GetComponent<MonsterController>().Sync(monsterState, pos, hp, look, dest, conner);
        //mc.HPC = hpc;
    }

    private void PacketHandler_S2C_PLAYERESPAWN(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 playerId = br.ReadInt32();
        Type.State state = (Type.State)br.ReadUInt16();
        Type.Dir dir = (Type.Dir)br.ReadUInt16();
        Type.Dir mouseDir = (Type.Dir)br.ReadUInt16();
        float x = br.ReadSingle();
        float y = br.ReadSingle();
        float z = br.ReadSingle();
        Vector3 nowPos = new Vector3(x, y, z);
        float qx = br.ReadSingle();
        float qy = br.ReadSingle();
        float qz = br.ReadSingle();
        float qw = br.ReadSingle();
        Quaternion quaternion = new Quaternion(qx, qy, qz, qw);
        float hp = br.ReadSingle();
        float mp = br.ReadSingle();

        Managers.Data.PlayerController.Respwan(nowPos, state, dir, mouseDir, quaternion, hp, mp);
    }

    private void PacketHandler_S2C_PLAYERDETH(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        int playerId = br.ReadInt32();

        if (Managers.Data.PlayerController.PlayerID == playerId)
        {
            Managers.Data.PlayerController.Death();
        }
        else
        {
            Managers.Data.PlayerDic.TryGetValue(playerId, out var opc);
            opc.Death();
        }
    }

    private void PacketHandler_S2C_PLAYERCHAT(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        int chatType = br.ReadInt32();
        int playerId = br.ReadInt32();
        int msgSize = br.ReadInt32();
        byte[] msgBytes = br.ReadBytes(msgSize);
        string msg = Encoding.Unicode.GetString(msgBytes);
        int userNameIndex = msg.IndexOf(":");

        if (Managers.Data.PlayerController.PlayerID == playerId)
        {
            Managers.Data.PlayerController.Talk(msg.Substring(userNameIndex + 1));
        }
        else
        {
            Managers.Data.PlayerDic.TryGetValue(playerId, out var opc);
            if (opc != null)
                opc.Talk(msg.Substring(userNameIndex + 1));
        }


        GameObject chatInput = GameObject.FindGameObjectWithTag("ChatContent");
        chatInput.GetComponent<ChatViewController>().Push(msg, chatType);
    }

    private void PacketHandler_S2C_PLAYERATTACKED(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 attackedPlayerId = br.ReadInt32();
        float attackedHp = br.ReadSingle();
        float attackedMp = br.ReadSingle();
        int damage = br.ReadInt32();

        if (Managers.Data.PlayerController.PlayerID == attackedPlayerId)
        {
            Managers.Data.PlayerController.Attacked();

            Managers.Data.PlayerController.SetHp(attackedHp);
            Managers.Data.PlayerController.SetMp(attackedMp);

            Damage damageText = Managers.Resource.Instantiate("UI/DamageText").GetComponent<Damage>();
            damageText.Init(new Vector3(Managers.Data.PlayerController.transform.position.x, Managers.Data.PlayerController.transform.position.y + 1.7f, Managers.Data.PlayerController.transform.position.z), damage);
        }
        else
        {
            Managers.Data.PlayerDic.TryGetValue(attackedPlayerId, out var opc);
            opc.Attacked();
            opc.SetHp(attackedHp);
            opc.SetMp(attackedMp);
        }
    }

    private void PacketHandler_PLAYERREMOVELIST(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 userCnt = br.ReadInt32();
        for (int i = 0; i < userCnt; i++)
        {
            int playerId = br.ReadInt32();

            if (playerId == Managers.Data.PlayerController.PlayerID)
                continue;

            Managers.Data.PlayerDic.TryGetValue(playerId, out var opc);
            Managers.Data.PlayerDic.Remove(playerId);
            opc.Destory();
        }
    }

    private void PacketHandler_S2C_PLAYERDESTORY(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        int playerId = br.ReadInt32();
        Managers.Data.PlayerDic.TryGetValue(playerId, out var opc);
        Managers.Data.PlayerDic.Remove(playerId);
        opc.Destory();
    }

    private void PacketHandler_S2C_PLAYERNEW(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 playerId = br.ReadInt32();

        if (playerId == Managers.Data.PlayerController.PlayerID)
        {
            return;
        }

        try
        {
            Type.State state = (Type.State)br.ReadByte();
            Type.Dir dir = (Type.Dir)br.ReadByte();
            Type.Dir mouseDir = (Type.Dir)br.ReadByte();
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            Vector3 nowPos = new Vector3(x, y, z);
            float qx = br.ReadSingle();
            float qy = br.ReadSingle();
            float qz = br.ReadSingle();
            float qw = br.ReadSingle();
            Quaternion quaternion = new Quaternion(qx, qy, qz, qw);
            float tx = br.ReadSingle();
            float ty = br.ReadSingle();
            float tz = br.ReadSingle();
            Vector3 target = new Vector3(tx, ty, tz);
            Type.MoveType moveType = (Type.MoveType)br.ReadByte();
            float hp = br.ReadSingle();
            float mp = br.ReadSingle();
            int level = br.ReadByte();
            int usernameSIze = br.ReadByte();
            byte[] usernameBytes = br.ReadBytes(usernameSIze);
            string username = Encoding.Unicode.GetString(usernameBytes);
            Type.CharacterType playerType = (Type.CharacterType)br.ReadByte();

            GameObject playerGo;

            if (playerType == Type.CharacterType.Warrior)
            {
                playerGo = Managers.Resource.Instantiate("Player/Warrior");
            }
            else
            {
                playerGo = Managers.Resource.Instantiate("Player/Archer");
            }

            playerGo.name = $"otherPlayer{playerId}";

            OtherPlayerController opc = playerGo.AddComponent<OtherPlayerController>();
            opc.PlayerID = playerId;

            Managers.Data.PlayerDic.Add(opc.PlayerID, opc);

            GameObject cameraPosGo = Managers.Resource.Instantiate("Camera/FakeCameraPos");
            cameraPosGo.GetComponent<FakeCameraPos>().Init(playerGo);

            opc.Init(quaternion, cameraPosGo.transform.GetChild(0).gameObject);
            opc.UpdateSync(moveType, state, dir, mouseDir, nowPos, quaternion, target,Vector3.zero);
            opc.SetHpMax(hp);
            opc.SetHpMax(mp);
            opc.SetHp(hp);
            opc.SetMp(mp);
            opc.SetLevel(level);
            opc.SetUserName(username);
            opc.SetCharacterType(playerType);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Debug.Log(playerId);
        }
    }

    private void PacketHandler_S2C_PLAYEROUT(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);
        int playerId = br.ReadInt32();
        try
        {
            Managers.Data.PlayerDic.TryGetValue(playerId, out var opc);
            Managers.Data.PlayerDic.Remove(playerId);
            opc.Destory();
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    private void PacketHandler_S2C_PLAYERLIST(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 userCnt = br.ReadInt32();
        for (int i = 0; i < userCnt; i++) 
        {
            int playerId = br.ReadInt32();
            Type.State state = (Type.State)br.ReadByte();
            Type.Dir dir = (Type.Dir)br.ReadByte();
            Type.Dir mouseDir = (Type.Dir)br.ReadByte();
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            Vector3 startPos = new Vector3(x,y,z);
            float qx = br.ReadSingle();
            float qy = br.ReadSingle();
            float qz = br.ReadSingle();
            float qw = br.ReadSingle();
            Quaternion cameraLocalRotation = new Quaternion(qx, qy, qz, qw);
            float tx = br.ReadSingle();
            float ty = br.ReadSingle();
            float tz = br.ReadSingle();
            Vector3 target = new Vector3(tx, ty, tz);
            Type.MoveType moveType = (Type.MoveType)br.ReadByte();
            float hp = br.ReadSingle();
            float mp = br.ReadSingle();
            int playerLevel = br.ReadByte();
            int usernameSIze = br.ReadByte();
            byte[] usernameBytes = br.ReadBytes(usernameSIze);
            string username = Encoding.Unicode.GetString(usernameBytes);
            Type.CharacterType type = (Type.CharacterType) br.ReadByte();
            if (playerId == Managers.Data.PlayerController.PlayerID)
                continue;

            GameObject playerGo;

            if (type == Type.CharacterType.Warrior)
            {
                playerGo = Managers.Resource.Instantiate("Player/Warrior");
            }
            else
            {
                playerGo = Managers.Resource.Instantiate("Player/Archer");
            }

            playerGo.name = $"otherPlayer{playerId}";
            
            OtherPlayerController opc = playerGo.AddComponent<OtherPlayerController>();
            opc.PlayerID = playerId;
            try
            {
                Managers.Data.PlayerDic.Add(opc.PlayerID, opc);
                opc.SetLevel(playerLevel);
            }
            catch (Exception e) {
                Debug.LogException(e);
            }

            GameObject hpObject = Managers.Resource.Instantiate("UI/HP");

            GameObject cameraPosGo = Managers.Resource.Instantiate("Camera/FakeCameraPos");
            cameraPosGo.GetComponent<FakeCameraPos>().Init(playerGo);;

            opc.Init(cameraLocalRotation, cameraPosGo.transform.GetChild(0).gameObject);
            opc.UpdateSync(moveType, state, dir, mouseDir,startPos, cameraLocalRotation, target,Vector3.zero);
            opc.SetHpMax(hp);
            opc.SetHpMax(mp);
            opc.SetHp(hp);
            opc.SetMp(mp);
            opc.SetUserName(username);
            opc.SetCharacterType(type);
        }
    }

    private void PacketHandler_S2C_PLAYERSYNC(ArraySegment<byte> dataPtr, int dataSize)
    {

        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);

        Int32 playerId = br.ReadInt32();

        if (playerId == Managers.Data.PlayerController.PlayerID)
        {
            return;
        }

        try
        {
            Type.State state = (Type.State)br.ReadByte();
            Debug.Log(state);
            Type.Dir dir = (Type.Dir)br.ReadByte();
            Type.Dir mouseDir = (Type.Dir)br.ReadByte();
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();
            Vector3 nowPos = new Vector3(x, y, z);
            float qx = br.ReadSingle();
            float qy = br.ReadSingle();
            float qz = br.ReadSingle();
            float qw = br.ReadSingle();
            Quaternion quaternion = new Quaternion(qx, qy, qz, qw);
            float tx = br.ReadSingle();
            float ty = br.ReadSingle();
            float tz = br.ReadSingle();
            Vector3 target = new Vector3(tx, ty, tz);
            float ex = br.ReadSingle();
            float ey = br.ReadSingle();
            float ez = br.ReadSingle();
            Vector3 anagle = new Vector3(ex, ey, ez);
            Type.MoveType moveType = (Type.MoveType)br.ReadByte();
            
            Managers.Data.PlayerDic.TryGetValue(playerId, out var opc);
            if (opc != null)
                opc.UpdateSync(moveType, state, dir, mouseDir, nowPos, quaternion,target, anagle);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Debug.Log(playerId);
        }
    }

    private void PacketHandler_SC2_PLAYERINIT(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);
        
        Int32 playerId = br.ReadInt32();
        Type.State state = (Type.State)br.ReadByte();
        Type.Dir dir = (Type.Dir)br.ReadByte();
        Type.Dir mouseDir = (Type.Dir)br.ReadByte();
        float x = br.ReadSingle();
        float y = br.ReadSingle();
        float z = br.ReadSingle();
        Vector3 nowPos = new Vector3(x, y, z);
        float qx = br.ReadSingle();
        float qy = br.ReadSingle();
        float qz = br.ReadSingle();
        float qw = br.ReadSingle(); 
        Quaternion quaternion = new Quaternion(qx, qy, qz, qw);
        float hp = br.ReadSingle();
        float mp = br.ReadSingle();
        int level = br.ReadByte();
        float speed = br.ReadSingle();
        float damage = br.ReadSingle();
        int userNameSize = br.ReadByte();
        byte[] userNameBytes = br.ReadBytes(userNameSize);
        string username = Encoding.Unicode.GetString(userNameBytes);
        Type.CharacterType type = (Type.CharacterType)br.ReadByte();
        int exp = br.ReadInt32();

        GameObject playerGo;
        if (type == Type.CharacterType.Warrior)
        {
            playerGo = Managers.Resource.Instantiate("Player/Warrior");
        }
        else
        {
            playerGo = Managers.Resource.Instantiate("Player/Archer");
        }

        playerGo.name = "player";
        PlayerController pc = playerGo.AddComponent<PlayerController>();
        pc.SetAgentPos(new Vector3(x, y, z));
        pc.PlayerID = playerId;
        pc.SetMp(mp);
        pc.SetHp(hp);
        pc.SetLevel(level);
        pc.SetSpeed(speed);
        pc.SetDamage(damage);
        pc.SetUserName(username);
        pc.SetExp(exp);
        pc.SetCharacterType(type);
        Managers.Data.PlayerController = pc;

        if (!Managers.Data.PlayerDic.ContainsKey(pc.PlayerID))
            Managers.Data.PlayerDic.Add(pc.PlayerID, pc);

        GameObject cameraPosGo = Managers.Resource.Instantiate("Camera/CameraPos");
        cameraPosGo.GetComponent<CameraPos>().Init(playerGo);
        cameraPosGo.transform.GetChild(0).gameObject.AddComponent<CameraController>().Init(playerGo);
        pc.Init(quaternion, cameraPosGo.transform.GetChild(0).gameObject);
    }
    private void PacketHandler_S2C_CREATECHARACTER(ArraySegment<byte> dataPtr, int dataSize)
    {
        MemoryStream ms = new MemoryStream(dataPtr.Array, dataPtr.Offset, dataPtr.Count);
        BinaryReader br = new BinaryReader(ms);
        int code = br.ReadInt32();
        SelectCharacterController sc = GameObject.FindObjectOfType<SelectCharacterController>();
        if (code == 1)
        {
            sc.UserNameAlertMsg("이미 존재하는 닉네임 입니다.");
        }
        else if (code == 4)
        {
            sc.UserNameAlertMsg("캐릭터는 최대 3개까지 생성 가능합니다.");
        }
        else
        {
            sc.HideCreateCharaterModal();
        }
    }
}
