using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Network : MonoBehaviour
{
    private Dictionary<Type.ServerPort, TCPConnector> _connectors = new Dictionary<Type.ServerPort, TCPConnector>();
    private PacketHandler _packetHandler = new PacketHandler();
    private const int _recvBufferSize = 4096 * 10;
    private byte[] _recvBuffer = new byte[_recvBufferSize];

    public string LocalIp
    {
        get
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }

    void Awake()
    {
        Init();
    }

    public void ServerConnect(Type.ServerPort port)
    {
        if (_connectors[port].ConnectTo(Type.IP, (int)port))
        {
            new Thread(new ThreadStart(() => TCPRecvProc(port))).Start();
        }
    }

    void Init()
    {
        _connectors.Add(Type.ServerPort.LOGIN_PORT, new TCPConnector());
        _connectors.Add(Type.ServerPort.NOVICE_PORT, new TCPConnector());
        ServerConnect(Type.ServerPort.LOGIN_PORT);
    }

    public void SendPacket(byte[] buffer, int sendSize, Type.ServerPort port)
    {
        _connectors[port].ConnectSocket.Send(buffer, sendSize, SocketFlags.None);
    }

    void Update()
    {
        while (true)
        {
            ArraySegment<byte> packet = PacketQueue.Instance.Pop();

            if (packet == null)
                break;

            if (packet != null)
            {
                _packetHandler.Handler(packet);
            }
        }
    }

    private void TCPRecvProc(Type.ServerPort port)
    {
        int recvSize = 0;
        int readPos = 0;
        int writePos = 0;
        try
        {
            while (true)
            {
                recvSize = _connectors[port].ConnectSocket.Receive(_recvBuffer, writePos, _recvBuffer.Length - writePos, SocketFlags.None);

                if (recvSize < 1)
                {
                    _connectors[port].ConnectSocket.Close();
                    break;
                }

                writePos += recvSize;
                // [200][100][200][100]
                while (true)
                {
                    int dataSize = Math.Abs(writePos - readPos);

                    if (dataSize < 4) break;

                    ArraySegment<byte> pktCodeByte = new ArraySegment<byte>(_recvBuffer, readPos, readPos + sizeof(UInt16));
                    ArraySegment<byte> pktSizeByte = new ArraySegment<byte>(_recvBuffer, readPos + sizeof(UInt16), readPos + sizeof(UInt16));

                    Int16 pktCode = BitConverter.ToInt16(pktCodeByte);
                    Int16 pktSize = BitConverter.ToInt16(pktSizeByte);

                    if (pktSize > dataSize)
                        break;

                    ArraySegment<byte> segment = new ArraySegment<byte>(_recvBuffer, readPos, pktSize);
                    byte[] data = new byte[pktSize];

                    Array.Copy(segment.ToArray(), data, pktSize);

                    PacketQueue.Instance.Push(data);

                    // TODO 데이터 처리
                    readPos += pktSize;

                    if (readPos == writePos)
                    {
                        readPos = 0;
                        writePos = 0;
                    }
                    else if (writePos >= 4096 * 4)
                    {
                        Buffer.BlockCopy(_recvBuffer, readPos, _recvBuffer, 0, dataSize);
                        writePos = dataSize;
                    }

                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
