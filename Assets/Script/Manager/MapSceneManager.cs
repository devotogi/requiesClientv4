using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapSceneManager : MonoBehaviour
{

    void Awake()
    {
        GameObject playerUi = Managers.Resource.Instantiate("UI/PlayerUI");

        byte[] bytes = new byte[1000];
        MemoryStream ms = new MemoryStream(bytes);
        ms.Position = 0;

        Int32 SQ = Managers.Data.userSQ;

        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write((Int16)Type.PacketProtocol.C2S_PLAYERINIT);
        bw.Write((Int16)8);
        bw.Write((Int32)SQ);
        Managers.Data.Network.SendPacket(bytes, 8, Type.ServerPort.NOVICE_PORT);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
