using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillArrowController : MonoBehaviour
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
            Boom();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        int targetLayer = collision.gameObject.layer;
        if (targetLayer == 17)
            return;
        Boom();
    }

    private void Boom() 
    {
        GameObject explosion = Managers.Resource.Instantiate("Effect/Explosion");
        explosion.transform.position = transform.position;
        BoomController bc = explosion.AddComponent<BoomController>();
        bc.Init(_player, _damage);
        Managers.Resource.Destory(gameObject);
    }
}
