using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PotalController : MonoBehaviour
{
    [SerializeField]
    private Type.ServerPort go;

    void Start()
    {

    }

    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != Managers.Data.PlayerController.gameObject) return;

        if (go == Type.ServerPort.NOVICE_PORT)
            GoNoviceFiled();
        else if (go == Type.ServerPort.VILLAGE_PORT)
            GoVillage();
    }

    public void GoNoviceFiled()
    {
        Managers.Data.Clear();  
        Managers.Data.Network.ServerDisConnect();
        int userSQ = Managers.Data.userSQ;
        int playerSQ = Managers.Data.playerSQ;
        byte[] bytes = new byte[1000];
        MemoryStream ms = new MemoryStream(bytes);
        ms.Position = 0;

        int pktSize = 14;

        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write((Int16)Type.PacketProtocol.C2S_SERVER_MOVE);
        bw.Write((Int16)pktSize);
        bw.Write(userSQ);
        bw.Write(playerSQ);
        bw.Write((Int16)Type.ServerPort.NOVICE_PORT);
        Managers.Data.Network.SendPacket(bytes, pktSize, Type.ServerPort.LOGIN_PORT);
        // Managers.Data.Network.ServerConnect(Type.ServerPort.NOVICE_PORT);
    }

    public void GoVillage() 
    {
        Managers.Data.Clear();
        Managers.Data.Network.ServerDisConnect();
        int userSQ = Managers.Data.userSQ;
        int playerSQ = Managers.Data.playerSQ;
        byte[] bytes = new byte[1000];
        MemoryStream ms = new MemoryStream(bytes);
        ms.Position = 0;

        int pktSize = 14;

        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write((Int16)Type.PacketProtocol.C2S_SERVER_MOVE);
        bw.Write((Int16)pktSize);
        bw.Write(userSQ);
        bw.Write(playerSQ);
        bw.Write((Int16)Type.ServerPort.VILLAGE_PORT);
        Managers.Data.Network.SendPacket(bytes, pktSize, Type.ServerPort.LOGIN_PORT);
    }
}
