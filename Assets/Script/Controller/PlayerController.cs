using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : PlayController
{
    [SerializeField]
    private float _rotateSpeed = 200.0f;
    [SerializeField]
    private float _xRotateMove;
    [SerializeField]
    private float _damage = 300f;

    private int _movePacketCnt = 0;
    private Text _text;
    private Quaternion _prevCameraLocalRotation;
    HpMpController _hpMpController;
    MonsterInfoController _monsterInfoController = null;
    ExpController _expController = null;
    TMP_Text _levelUi = null;
    StatInfoController _statInfoController = null;
    TMP_InputField _chatInput;
    float _exp;
    GameObject bow;
    private bool skill1CoolTime = false;
    private bool skill2CoolTime = false;
    private void LateUpdate()
    {
        if (_talk != null)
        {
            _talk.transform.position = transform.position + (Vector3.up * 2.2f);
            Camera camera = Camera.main;

            if (camera != null)
            { 
                _talk.transform.LookAt(camera.transform);
            }
        }

        if (_usernameText != null)
        {
            _usernameText.transform.position = transform.position + (Vector3.up * 2.0f);
            Camera camera = Camera.main;

            if (camera != null)
            {
                _usernameText.transform.LookAt(camera.transform);
                _usernameText.transform.Rotate(0, 180, 0);
            }
        }
    }

    void OnDrawGizmos()
    {
        //float maxDistance = 2;
        //RaycastHit hit;
        //int layerMask = 1 << LayerMask.NameToLayer("Enemy");
        //// Physics.SphereCast (레이저를 발사할 위치, 구의 반경, 발사 방향, 충돌 결과, 최대 거리)
        //bool isHit = Physics.SphereCast(transform.position + Vector3.up, transform.lossyScale.x / 2, transform.forward, out hit, maxDistance, layerMask);
        //Gizmos.color = Color.red;
        //if (isHit)
        //{
        //    Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * hit.distance);
        //    Gizmos.DrawWireSphere((transform.position + Vector3.up) + (transform.forward * hit.distance), transform.lossyScale.x / 2);
        //}
        //else
        //{
        //    Gizmos.DrawRay(transform.position + Vector3.up, transform.forward * maxDistance);
        //}

        float sphereRadius = 0.5f;
        Vector3 attackPos = transform.position + transform.TransformDirection(Vector3.forward) + Vector3.up;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPos, sphereRadius);
        Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(transform.position, 4.5f);

        Vector3 effectD = transform.TransformDirection(Vector3.forward * 2);
        Vector3 pos = transform.position + effectD;
        Vector3 effectPos = new Vector3(pos.x, pos.y + 1.0f, pos.z);
        sphereRadius = 3.5f;
        Gizmos.DrawWireSphere(effectPos, sphereRadius);
    }

    IEnumerator movePacketCount()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.0f);
            // Debug.Log($"movePacket Cnt:{_movePacketCnt}");
            _movePacketCnt = 0;
        }
    }

    public override void CInit()
    {
        base.CInit();
        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.enabled = true;
        GameObject playerUi = GameObject.FindGameObjectWithTag("Finish");
        _hpMpController = GameObject.FindGameObjectWithTag("HpMp").GetComponent<HpMpController>();
        _expController = playerUi.transform.GetChild(1).GetComponent<ExpController>();
        _expController.Init();
        _monsterInfoController = playerUi.transform.GetChild(4).GetComponent<MonsterInfoController>();
        _monsterInfoController.Init();
        _levelUi = playerUi.transform.GetChild(5).GetChild(0).GetComponent<TMP_Text>();
        _statInfoController = playerUi.transform.GetChild(6).GetComponent<StatInfoController>();
        _statInfoController.Init();
        _chatInput = GameObject.FindWithTag("ChatInput").GetComponent<TMP_InputField>();
        SetExp(_level, _exp, 1000);

        if (_characterType == Type.CharacterType.Archer)
            bow = FindChildRecursively(transform, "WoodenBow").gameObject;
    }

    public void Init(Quaternion cameraLocalRotation, GameObject camera)
    {
        StartCoroutine(movePacketCount());
        _camera = camera;
        _cameraLocalRotation = cameraLocalRotation;
        GameObject text_ui = GameObject.FindGameObjectWithTag("Respawn");
    }

    public void Respwan(Vector3 pos, Type.State state, Type.Dir dir, Type.Dir mouseDir, Quaternion q, float hp, float mp) 
    {
        _agent.Warp(pos);
        _state = state;
        _dir = dir;
        _mouseDir = mouseDir;
        _hpMpController.SetHp(hp);
        _hpMpController.SetMp(mp);
        _death = false;
    }

    public void SetAgentPos(Vector3 pos) 
    {
        GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(pos);
    }

    public override void KeyBoardMove_Update_Input()
    {
        if (_chatInput.isFocused) 
        {
            _dir = Type.Dir.NONE;
            _state = Type.State.IDLE;
            _mouseDir = Type.Dir.NONE;
            return;
        }

        if (_death) return;;

        if (_coAttacked == true)
            return;

        if (_coAttack == false)
            _state = Type.State.IDLE;

        if (_state == Type.State.ATTACK || _state == Type.State.ATTACK2)
            return;

        if (_movePacketCnt > 8)
        {
            _dir = Type.Dir.NONE;
            _state = Type.State.IDLE;
            _mouseDir = Type.Dir.NONE;
            return;
        }

        if (Input.GetKeyDown(KeyCode.I)) 
        {
            if (_statInfoController.gameObject.active == false)
            {
                RequestStatInfo();
                // _statInfoController.On();
            }
            else
                _statInfoController.Off();
        }

        if (Input.GetMouseButtonUp(1))
        {
            _state = Type.State.ATTACK;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = LayerMask.GetMask("Player", "MainGround", "WALL", "Enemy");
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (EventSystem.current.IsPointerOverGameObject() == false)
                {
                    Vector3 target = new Vector3(hit.point.x, 0, hit.point.z);
                    Vector3 dir = target - new Vector3(transform.position.x, 0, transform.position.z);
                    transform.rotation = Quaternion.LookRotation(dir.normalized);
                }
            }
            return;
        }

        if (skill1CoolTime == false && Input.GetKeyDown(KeyCode.Q))
        {
            QSkill();
            return;
        }
        else if (skill2CoolTime == false && Input.GetKeyDown(KeyCode.E))
        {
            ESkill();
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _mouseDir = Type.Dir.NONE;
        }

        if (Input.GetKeyUp(KeyCode.W) && (_dir & (Type.Dir.UP)) != 0)
        {
            _dir = (Type.Dir)((UInt16)_dir ^ (0x01 << 1));
        }

        if (Input.GetKeyUp(KeyCode.D) && (_dir & (Type.Dir.RIGHT)) != 0)
        {
            _dir = (Type.Dir)((UInt16)_dir ^ (0x01 << 2));
        }

        if (Input.GetKeyUp(KeyCode.S) && (_dir & (Type.Dir.DOWN)) != 0)
        {
            _dir = (Type.Dir)((UInt16)_dir ^ (0x01 << 3));
        }

        if (Input.GetKeyUp(KeyCode.A) && (_dir & (Type.Dir.LEFT)) != 0)
        {
            _dir = (Type.Dir)((UInt16)_dir ^ (0x01 << 4));
        }

        if (_dir == Type.Dir.NONE)
        {
            _dir = Type.Dir.NONE;
            _state = Type.State.IDLE;
        }

        if (Input.GetMouseButton(0))
        {
            _prevCameraLocalRotation = _cameraLocalRotation;

            _xRotateMove = Input.GetAxis("Mouse X"); /** Time.deltaTime * _rotateSpeed;*/

            if (_xRotateMove < 0)
            {
                _mouseDir = Type.Dir.LEFT;
            }
            else if (_xRotateMove > 0)
            {
                _mouseDir = Type.Dir.RIGHT;
            }
        }

        if (Input.GetKey(KeyCode.W))
        {
            _dir = (Type.Dir)((UInt16)_dir | (0x01 << 1));

            if ((_dir & (Type.Dir.DOWN)) != 0)
                _dir = (Type.Dir)((UInt16)_dir ^ (0x01 << 3));

            _state = Type.State.MOVE;
        }
        if (Input.GetKey(KeyCode.D))
        {
            _dir = (Type.Dir)((UInt16)_dir | (0x01 << 2));
            _state = Type.State.MOVE;
        }

        if (Input.GetKey(KeyCode.S))
        {
            _dir = (Type.Dir)((UInt16)_dir | (0x01 << 3));

            if ((_dir & (Type.Dir.UP)) != 0)
                _dir = (Type.Dir)((UInt16)_dir ^ (0x01 << 1));

            _state = Type.State.MOVE;
        }

        if (Input.GetKey(KeyCode.A))
        {
            _dir = (Type.Dir)((UInt16)_dir | (0x01 << 4));
            _state = Type.State.MOVE;
        }
    }

    internal void StatInfoOpen(float damage, float speed, float defense, int statPoint)
    {
        _statInfoController.On(damage, speed, defense, statPoint);
    }

    private void RequestStatInfo()
    {
        byte[] bytes = new byte[100];
        MemoryStream ms = new MemoryStream(bytes);
        ms.Position = 0;

        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write((Int16)Type.PacketProtocol.C2S_PLAYERSTATINFO);
        bw.Write((Int16)4);
        Managers.Data.Network.SendPacket(bytes, 4, Type.ServerPort.NOVICE_PORT);
    }

    public override void SetExp(int level, float exp, float expMax)
    {
        if (_level != level) 
        {
            // 레벨업 이펙트
            _level = level;
            GameObject bufferEffect = Managers.Resource.Instantiate("Effect/Buff");
            BufferEffectController bf = bufferEffect.AddComponent<BufferEffectController>();
            bf.Player = gameObject;
        }
        _levelUi.text = $"Lv.{_level}";
       _expController.SetExp(level, exp, expMax);
    }

    internal void MonsterInfoIsUnActive(int monsterId)
    {
        _monsterInfoController.MonsterInfoIsUnActive(monsterId);
    }

    internal void SetMonsterInfo(int monsterId, int monsterType, float monsterHp)
    {
        _monsterInfoController.gameObject.SetActive(true);
        _monsterInfoController.SetMonsterInfo(monsterId, monsterType, monsterHp);        
    }

    public override void KeyBoardMove_Update_IDLE()
    {
        if (_death) return;

        if (_coAttacked == true)
            return;

        if (_mouseDir != Type.Dir.NONE)
        {
            _xRotateMove = _mouseDir == Type.Dir.LEFT ? -1 : 1;

            _xRotateMove = _xRotateMove * Time.deltaTime * _rotateSpeed;

            _camera.transform.RotateAround(transform.position, Vector3.up, _xRotateMove);

            _cameraLocalRotation = _camera.transform.localRotation;
        }
    }

    public override void KeyBoardMove_Update_MOVE()
    {
        if (_death) return;

        if (_coAttacked == true)
            return;

        if (_mouseDir != Type.Dir.NONE)
        {
            _xRotateMove = _mouseDir == Type.Dir.LEFT ? -1 : 1;

            _xRotateMove = _xRotateMove * Time.deltaTime * _rotateSpeed;

            _camera.transform.RotateAround(transform.position, Vector3.up, _xRotateMove);

            _cameraLocalRotation = _camera.transform.localRotation;
        }

        Vector3 cameraFVector = _cameraLocalRotation * Vector3.forward;
        cameraFVector.y = 0;
        cameraFVector = cameraFVector.normalized;

        Vector3 cameraRVector = _cameraLocalRotation * Vector3.right;
        cameraRVector.y = 0;
        cameraRVector = cameraRVector.normalized;

        Vector3 cameraBVector = _cameraLocalRotation * Vector3.back;
        cameraBVector.y = 0;
        cameraBVector = cameraBVector.normalized;

        Vector3 cameraLVector = _cameraLocalRotation * Vector3.left;
        cameraLVector.y = 0;
        cameraLVector = cameraLVector.normalized;

        Vector3 cameraFRVector = (cameraFVector + cameraRVector).normalized;
        Vector3 cameraRBVector = (cameraBVector + cameraRVector).normalized;
        Vector3 cameraLBVector = (cameraLVector + cameraBVector).normalized;
        Vector3 cameraLFVector = (cameraLVector + cameraFVector).normalized;

        switch (_dir)
        {
            case Type.Dir.UP:
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(cameraFVector), 0.2f);
                transform.position += cameraFVector * Time.deltaTime * _speed;
                _dirVector3 = cameraFVector;
                break;
            case Type.Dir.UPRIGHT:
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(cameraFRVector), 0.2f);
                transform.position += cameraFRVector * Time.deltaTime * _speed;
                _dirVector3 = cameraFRVector;
                break;
            case Type.Dir.RIGHT:
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(cameraRVector), 0.2f);
                transform.position += cameraRVector * Time.deltaTime * _speed;
                _dirVector3 = cameraRVector;
                break;
            case Type.Dir.RIGHTDOWN:
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(cameraRBVector), 0.2f);
                transform.position += cameraRBVector * Time.deltaTime * _speed;
                _dirVector3 = cameraRBVector;
                break;
            case Type.Dir.DOWN:
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(cameraBVector), 0.2f);
                transform.position += cameraBVector * Time.deltaTime * _speed;
                _dirVector3 = cameraBVector;
                break;
            case Type.Dir.LEFTDOWN:
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(cameraLBVector), 0.2f);
                transform.position += cameraLBVector * Time.deltaTime * _speed;
                _dirVector3 = cameraLBVector;
                break;
            case Type.Dir.LEFT:
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(cameraLVector), 0.2f);
                transform.position += cameraLVector * Time.deltaTime * _speed;
                _dirVector3 = cameraLVector;
                break;
            case Type.Dir.LEFTUP:
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(cameraLFVector), 0.2f);
                transform.position += cameraLFVector * Time.deltaTime * _speed;
                _dirVector3 = cameraLFVector;
                break;
        }
    }

    internal void MonsterInfoIsAttacked(int monsterId, float hp)
    {
        _monsterInfoController.MonsterInfoIsAttacked(monsterId, hp);
    }

    public override void KeyBoardMove_Update_ATTACK()
    {
        if (_coAttack == false)
            StartCoroutine(CoAttack());
    }

    public override void KeyBoardMove_Update_ATTACKED()
    {
        if (_coAttacked == false)
            StartCoroutine(CoAttacked());
    }

    public override void UpdateAnimation()
    {
        if (_death) return;

        if (_coAttacked == true)
            return;

        switch (_state)
        {
            case Type.State.IDLE:
                _animator.Play("idle");
                break;

            case Type.State.MOVE:
                _animator.Play("run");
                break;

            case Type.State.ATTACK:
                _animator.Play("attack");
                break;
            case Type.State.ATTACK2:
                _animator.Play("skillAttack");
                break;
        }
    }

    public override void SendSyncPlayer()
    {
        byte[] bytes = new byte[74];
        MemoryStream ms = new MemoryStream(bytes);
        ms.Position = 0;

        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write((Int16)Type.PacketProtocol.C2S_PLAYERSYNC);
        bw.Write((Int16)64);
        bw.Write((Int32)PlayerID); // 4
        bw.Write((byte)_state); // 1
        bw.Write((byte)_dir); // 2
        bw.Write((byte)_mouseDir); // 3

        bw.Write((float)transform.position.x); // 
        bw.Write((float)transform.position.y); // 
        bw.Write((float)transform.position.z); // 15

        bw.Write((float)_cameraLocalRotation.x); // 4
        bw.Write((float)_cameraLocalRotation.y); // 4
        bw.Write((float)_cameraLocalRotation.z); // 4
        bw.Write((float)_cameraLocalRotation.w); // 31

        bw.Write((float)_target.x); // 4
        bw.Write((float)_target.y); // 4
        bw.Write((float)_target.z); // 43

        bw.Write((float)transform.eulerAngles.x);
        bw.Write((float)transform.eulerAngles.y);
        bw.Write((float)transform.eulerAngles.z);

        bw.Write((byte)_moveType); // 44 

        Managers.Data.Network.SendPacket(bytes, 64, Type.ServerPort.NOVICE_PORT);   
        _movePacketCnt++;
    }

    public override void SendSyncMap()
    {
        byte[] bytes = new byte[74];
        MemoryStream ms = new MemoryStream(bytes);
        ms.Position = 0;

        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write((Int16)Type.PacketProtocol.C2S_MAPSYNC);
        bw.Write((Int16)64);
        bw.Write((Int32)PlayerID);
        bw.Write((byte)_state); // 2
        bw.Write((byte)_dir); // 2
        bw.Write((byte)_mouseDir); // 2

        bw.Write((float)transform.position.x); // 4
        bw.Write((float)transform.position.y); // 4
        bw.Write((float)transform.position.z); // 4

        bw.Write((float)_cameraLocalRotation.x); // 4
        bw.Write((float)_cameraLocalRotation.y); // 4
        bw.Write((float)_cameraLocalRotation.z); // 4
        bw.Write((float)_cameraLocalRotation.w); // 4

        bw.Write((float)_target.x); // 4
        bw.Write((float)_target.y); // 4
        bw.Write((float)_target.z); // 4

        bw.Write((float)transform.eulerAngles.x);
        bw.Write((float)transform.eulerAngles.y);
        bw.Write((float)transform.eulerAngles.z);

        bw.Write((byte)_moveType); // 4

        Managers.Data.Network.SendPacket(bytes, 64, Type.ServerPort.NOVICE_PORT);
    }
    public override void MouseMove_Update_Input()
    {
        if (Input.GetMouseButtonUp(0))
        {
            _mouseDir = Type.Dir.NONE;
        }

        if (Input.GetMouseButton(0))
        {
            _prevCameraLocalRotation = _cameraLocalRotation;

            _xRotateMove = Input.GetAxis("Mouse X"); /** Time.deltaTime * _rotateSpeed;*/

            if (_xRotateMove < 0)
            {
                _mouseDir = Type.Dir.LEFT;
            }
            else if (_xRotateMove > 0)
            {
                _mouseDir = Type.Dir.RIGHT;
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //레이캐스트 충돌 발생 시
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                //충돌한 오브젝트를 태그로 구분해서 OK일 경우
                _agent.SetDestination(hit.point);
                _state = Type.State.MOVE;
            }
            return;
        }
    }

    public override void MouseMove_Update_IDLE()
    {
        if (_mouseDir != Type.Dir.NONE)
        {
            _xRotateMove = _mouseDir == Type.Dir.LEFT ? -1 : 1;

            _xRotateMove = _xRotateMove * Time.deltaTime * _rotateSpeed;

            _camera.transform.RotateAround(transform.position, Vector3.up, _xRotateMove);

            _cameraLocalRotation = _camera.transform.localRotation;
        }
    }

    public override void MouseMove_Update_MOVE()
    {
        NavMeshPath navMesh = _agent.path;

        for (int i = 0; i < navMesh.corners.Length - 1; i++)
        {
            Debug.DrawLine(navMesh.corners[i], navMesh.corners[i + 1], Color.red);
        }

        if (navMesh.corners.Length >= 2)
        {
            Vector3 dest = new Vector3(navMesh.corners[1].x, transform.position.y, navMesh.corners[1].z);
            transform.LookAt(dest);
            _target = navMesh.corners[1];
        }

        if (_mouseDir != Type.Dir.NONE)
        {
            _xRotateMove = _mouseDir == Type.Dir.LEFT ? -1 : 1;

            _xRotateMove = _xRotateMove * Time.deltaTime * _rotateSpeed;

            _camera.transform.RotateAround(transform.position, Vector3.up, _xRotateMove);

            _cameraLocalRotation = _camera.transform.localRotation;
        }

        if (_agent.velocity.sqrMagnitude >= 0.2f * 0.2f && _agent.remainingDistance <= 0.1f)
        {
            _state = Type.State.IDLE;
        }
    }

    IEnumerator CoAttack() 
    {
        if (_characterType == Type.CharacterType.Warrior)
        {
            _coAttack = true;
            yield return new WaitForSeconds(0.2f);

            RaycastHit hit;
            int playerLayerMask = 1 << LayerMask.NameToLayer("Player");
            int monsterLayerMask = 1 << LayerMask.NameToLayer("Enemy");

            float sphereRadius = 0.5f;
            Vector3 attackPos = transform.position + transform.TransformDirection(Vector3.forward) + Vector3.up;

            Collider[] colliders = Physics.OverlapSphere(attackPos, sphereRadius, (monsterLayerMask | playerLayerMask));

            foreach (Collider collider in colliders)
            {
                int targetLayer = collider.transform.gameObject.layer;

                if (targetLayer == 17)
                {
                    if (collider.gameObject != gameObject)
                    {
                        PlayController pc = collider.transform.gameObject.GetComponent<PlayController>();
                        Int32 otherPlayerId = pc.PlayerID;

                        byte[] bytes = new byte[12];
                        MemoryStream ms = new MemoryStream(bytes);
                        ms.Position = 0;

                        BinaryWriter bw = new BinaryWriter(ms);
                        bw.Write((Int16)Type.PacketProtocol.C2S_PLAYERATTACK);
                        bw.Write((Int16)12);
                        bw.Write((Int32)otherPlayerId);
                        bw.Write((Int32)_damage);
                        Managers.Data.Network.SendPacket(bytes, 12, Type.ServerPort.NOVICE_PORT);
                    }
                }
                else if (targetLayer == 15)
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

            yield return new WaitForSeconds(0.8f);
            _coAttack = false;
            _dir = Type.Dir.NONE;
        }
        else
        {
            _coAttack = true;
            yield return new WaitForSeconds(0.6f);
            GameObject arrow = Managers.Resource.Instantiate("Object/Arrow");
            Vector3 dir = transform.localRotation * Vector3.fwd;
            arrow.AddComponent<ArrowController>().Init(true, dir,transform.eulerAngles, _damage);
            arrow.transform.position = bow.transform.position;
            yield return new WaitForSeconds(0.4f);
            _coAttack = false;
            _dir = Type.Dir.NONE;
        }
    }

    IEnumerator CoAttacked()
    {
        _attackedCoolTime = true;
        _coAttacked = true;
        yield return new WaitForSeconds(1.2f);
        _dir = Type.Dir.NONE;
        _coAttacked = false;
        yield return new WaitForSeconds(3f);
        _attackedCoolTime = false;

    }

    IEnumerator CoDeath()
    {
        _animator.Play("death");
        yield return new WaitForSeconds(4.0f);
        // TODO 리스폰 패킷
        byte[] bytes = new byte[12];
        MemoryStream ms = new MemoryStream(bytes);
        ms.Position = 0;
        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write((Int16)Type.PacketProtocol.C2S_PLAYERESPAWN);
        bw.Write((Int16)4);
        Managers.Data.Network.SendPacket(bytes, 4, Type.ServerPort.NOVICE_PORT);
    }

    private void SyncQSkill() 
    {
        byte[] bytes = new byte[100];
        MemoryStream ms = new MemoryStream(bytes);
        ms.Position = 0;
        BinaryWriter bw = new BinaryWriter(ms);
        bw.Write((Int16)Type.PacketProtocol.C2S_PLAYERSKILLSYNC);
        bw.Write((Int16)8);
        bw.Write((Int32)Managers.Data.PlayerController.PlayerID);
        Managers.Data.Network.SendPacket(bytes, 8, Type.ServerPort.NOVICE_PORT);
    }

    private void QSkill() 
    {
        switch (_characterType)
        {
            case Type.CharacterType.Warrior:
                SyncQSkill();
                StartCoroutine(CoWarriorQSkill());
                GameObject.FindGameObjectWithTag("Finish").GetComponent<PlayerUIController>().SetQCoolTime(14);
                break;

            case Type.CharacterType.Archer:
                SyncQSkill();
                StartCoroutine(CoArcherQSkill());
                GameObject.FindGameObjectWithTag("Finish").GetComponent<PlayerUIController>().SetQCoolTime(14);
                break;
        }
    }

    private void ESkill() 
    {
        switch (_characterType)
        {
            case Type.CharacterType.Warrior:
                StartCoroutine(CoWarriorESkill());
                GameObject.FindGameObjectWithTag("Finish").GetComponent<PlayerUIController>().SetECoolTime(12);
                break;

            case Type.CharacterType.Archer:
                StartCoroutine(CoArcherESkill());
                GameObject.FindGameObjectWithTag("Finish").GetComponent<PlayerUIController>().SetECoolTime(11);
                break;
        }
    }

    IEnumerator CoWarriorQSkill() 
    {
        skill1CoolTime = true;
        GameObject effect = Managers.Resource.Instantiate("Effect/FreezeCircle");
        effect.transform.position = transform.position;
        yield return new WaitForSeconds(1.0f);
        QSkillAttack(effect.transform.position);
        yield return new WaitForSeconds(1.0f);
        QSkillAttack(effect.transform.position);
        yield return new WaitForSeconds(1.0f);
        QSkillAttack(effect.transform.position);
        yield return new WaitForSeconds(1.0f);
        Managers.Resource.Destory(effect);
        yield return new WaitForSeconds(10.0f);
        skill1CoolTime = false;
    }

    void QSkillAttack(Vector3 skillPos) 
    {
        int monsterLayerMask = 1 << LayerMask.NameToLayer("Enemy");
        Collider[] colliders = Physics.OverlapSphere(skillPos, 4.5f, monsterLayerMask);

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

    IEnumerator CoArcherQSkill()
    {
        skill1CoolTime = true;
        GameObject effect = Managers.Resource.Instantiate("Effect/Meteor");
        effect.transform.position = transform.position;
        yield return new WaitForSeconds(1.0f);
        QSkillAttack(effect.transform.position);
        yield return new WaitForSeconds(1.0f);
        QSkillAttack(effect.transform.position);
        yield return new WaitForSeconds(1.0f);
        QSkillAttack(effect.transform.position);
        yield return new WaitForSeconds(1.0f);
        Managers.Resource.Destory(effect);
        yield return new WaitForSeconds(10.0f);
        skill1CoolTime = false;
    }

    IEnumerator CoWarriorESkill()
    {
        _state = Type.State.ATTACK2;
        _coAttack = true;
        skill2CoolTime = true;
        yield return new WaitForSeconds(1.0f);
        GameObject effect = Managers.Resource.Instantiate("Effect/Holyhit");
        Vector3 effectD = transform.TransformDirection(Vector3.forward*2);
        Vector3 pos = transform.position + effectD;
        effect.transform.position = new Vector3(pos.x,pos.y + 1.0f,pos.z);
        ESkillAttack(effect.transform.position);
        yield return new WaitForSeconds(1.0f);
        Managers.Resource.Destory(effect);
        _dir = Type.Dir.NONE;
        _coAttack = false;
        yield return new WaitForSeconds(10.0f);
        skill2CoolTime = false;
    }

    IEnumerator CoArcherESkill()
    {
        _state = Type.State.ATTACK2;
        _coAttack = true;
        skill2CoolTime = true;
        yield return new WaitForSeconds(0.6f);
        GameObject arrow = Managers.Resource.Instantiate("Effect/MagicArrow");
        Vector3 dir = transform.localRotation * Vector3.fwd;
        arrow.AddComponent<SkillArrowController>().Init(true, dir, transform.eulerAngles, _damage);
        arrow.transform.position = bow.transform.position;
        yield return new WaitForSeconds(0.4f);
        _coAttack = false;
        _dir = Type.Dir.NONE;
        yield return new WaitForSeconds(10.0f);
        skill2CoolTime = false;
    }

    void ESkillAttack(Vector3 skillPos)
    {
        int monsterLayerMask = 1 << LayerMask.NameToLayer("Enemy");
        Collider[] colliders = Physics.OverlapSphere(skillPos, 2.0f, monsterLayerMask);

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


    public override void Attacked() 
    {
        //if (_attackedCoolTime) return;

        //if (_coAttacked) return;

        //_animator.Play("hit");
        //StartCoroutine(CoAttacked());
    }

    public override void SetHpMax(float hpMax)
    {
        GameObject.FindGameObjectWithTag("HpMp").GetComponent<HpMpController>().SetHPMax(hpMax);
    }

    public override void SetMpMax(float mpMax)
    {
        GameObject.FindGameObjectWithTag("HpMp").GetComponent<HpMpController>().SetMPMax(mpMax);
    }

    public override void SetHp(float hp)
    {
        GameObject.FindGameObjectWithTag("HpMp").GetComponent<HpMpController>().SetHp(hp);
    }

    public override void SetMp(float mp)
    {
        GameObject.FindGameObjectWithTag("HpMp").GetComponent<HpMpController>().SetMp(mp);
    }

    public override void Death()
    {
        _death = true;
        StartCoroutine(CoDeath());
    }

    internal void SetSpeed(float speed)
    {
        _speed = speed;
    }

    internal void SetDamage(float damage)
    {
        _damage = damage;
    }

    internal void SetExp(int exp)
    {
        _exp = exp;
    }
}
