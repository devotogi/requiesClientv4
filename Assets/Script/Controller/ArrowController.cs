using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    float speed = 20.0f;
    Vector3 _dir;
    float dist = 0.0f;
    float _damage;
    bool _player = false;

    public void Init(bool player, Vector3 dir, Vector3 euler, float damage) 
    {
        _player = player;
        dir.y = 0;
        _dir = dir;
        transform.eulerAngles = new Vector3(90, euler.y, euler.z);
        _damage = damage;
    }

    void Update()
    {
        transform.position += _dir * Time.deltaTime * speed;
        dist += Time.deltaTime * speed;

        if (dist >= 15) 
        {
            Managers.Resource.Destory(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        int targetLayer = collision.gameObject.layer;

        if (targetLayer == 17)
            return;

        if (_player && targetLayer == 15)
        {
            float x = collision.transform.position.x;
            float y = collision.transform.position.y;
            float z = collision.transform.position.z;

            MonsterController mc = collision.transform.gameObject.GetComponent<MonsterController>();
            Int32 monsterId = mc.MonsterId;

            byte[] bytes = new byte[24];
            MemoryStream ms = new MemoryStream(bytes);
            ms.Position = 0;

            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((Int16)Type.PacketProtocol.C2S_MONSTERATTACKED);
            bw.Write((Int16)24);
            bw.Write((Int32)monsterId);
            bw.Write((float)x);
            bw.Write((float)y);
            bw.Write((float)z);
            bw.Write((Int32)_damage);
            Managers.Data.Network.SendPacket(bytes, 24, Type.ServerPort.NOVICE_PORT);
        }
        Managers.Resource.Destory(gameObject);
    }
}
