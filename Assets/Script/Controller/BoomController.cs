using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BoomController : MonoBehaviour
{
    float _damage;
    bool _player = false;
    public void Init(bool player, float damage)
    {
        _player = player;
        _damage = damage;
        StartCoroutine(Boom());
    }

    IEnumerator Boom() 
    {
        if (_player)
        {
            int monsterLayerMask = 1 << LayerMask.NameToLayer("Enemy");
            Collider[] colliders = Physics.OverlapSphere(transform.position, 3.5f, monsterLayerMask);
            foreach (Collider collider in colliders)
            {
                int targetLayer = collider.transform.gameObject.layer;
                if (targetLayer == 15)
                {
                    if (collider.gameObject != gameObject)
                    {
                        float x = collider.transform.position.x;
                        float y = collider.transform.position.y;
                        float z = collider.transform.position.z;

                        MonsterController mc = collider.transform.gameObject.GetComponent<MonsterController>();
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
                }
            }
        }
        yield return new WaitForSeconds(1.0f);
        Managers.Resource.Destory(gameObject);
    }
}
