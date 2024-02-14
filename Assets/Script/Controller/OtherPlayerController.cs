using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OtherPlayerController : PlayController
{
    private float _xRotateMove;
    private float _rotateSpeed = 200.0f;
    private HpController _hpController = null;
    GameObject bow;
    private TMP_Text _debugText;
    private void LateUpdate()
    {
        if (_hpController != null)
        { 
            _hpController.transform.position = transform.position + Vector3.up * 1.9f;
            _hpController.LookMainCamera();
        }

        if (_talk != null)
        {
            _talk.transform.position = transform.position + (Vector3.up * 2.2f);

            Camera camera = Camera.main;

            if (camera != null)
            {
                Vector3 dir = camera.transform.position;
                dir.y = 0;
                _talk.transform.LookAt(dir);
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

        if (_debugText != null) 
        {
            _debugText.text = $"x:{transform.eulerAngles.x},y:{transform.eulerAngles.y},z:{transform.eulerAngles.z}";
            _debugText.transform.position = new Vector3 (transform.position.x, transform.position.y + 3.0f, transform.position.z);
            _debugText.transform.LookAt(Camera.main.transform);
            _debugText.transform.Rotate(0, 180, 0);
        }
    }

    public override void CInit()
    {
        base.CInit();
        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.enabled = true;

        if (_characterType == Type.CharacterType.Archer)
            bow = FindChildRecursively(transform, "WoodenBow").gameObject;

        _debugText = Managers.Resource.Instantiate("UI/DebugText").transform.GetChild(0).GetComponent<TMP_Text>();
    }

    public void Init(Quaternion cameraLocalRotation, GameObject camera)
    {
        _camera = camera;
        _cameraLocalRotation = cameraLocalRotation;
    }
    public override void KeyBoardMove_Update_IDLE()
    {
        if (_death) return;

        if (_coAttacked)
            return;

        if (_camera != null && _mouseDir != Type.Dir.NONE)
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

        if (_coAttacked)
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

    internal void ExcuteSkill()
    {
        switch (_characterType)
        {
            case Type.CharacterType.Warrior:
                StartCoroutine(CoWarriorQSkill());
                break;

            case Type.CharacterType.Archer:
                StartCoroutine(CoArcherQSkill());
                break;
        }
    }

    IEnumerator CoAttack()
    {
        _coAttack = true;
        yield return new WaitForSeconds(0.6f);
        GameObject arrow = Managers.Resource.Instantiate("Object/Arrow");
        Vector3 dir = transform.localRotation * Vector3.fwd;
        arrow.AddComponent<ArrowController>().Init(false, dir, transform.eulerAngles, 0);
        arrow.transform.position = bow.transform.position;
        yield return new WaitForSeconds(0.4f);
        _coAttack = false;
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
        Vector3 dirVector3 = _target - transform.position;
        transform.position += dirVector3.normalized * Time.deltaTime * _speed;
    }

    public override void UpdateSync(Type.MoveType moveType, Type.State state, Type.Dir dir, Type.Dir mouseDir, Vector3 nowPos, Quaternion quaternion, Vector3 target, Vector3 anagle)
    {
        _moveType = moveType;
        _mouseDir = mouseDir;
        _state = state;

        if (_characterType == Type.CharacterType.Archer && _state == Type.State.ATTACK)
            StartCoroutine(CoAttack());
        
        if (_characterType == Type.CharacterType.Archer && _state == Type.State.ATTACK2)
            StartCoroutine(CoArcherESkill());

        if (_characterType == Type.CharacterType.Warrior && _state == Type.State.ATTACK2)
            StartCoroutine(CoWarriorESkill());

        _dir = dir;
        _target = target;

        if (anagle != Vector3.zero)
            transform.eulerAngles = anagle;

        GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(new Vector3(nowPos.x, nowPos.y, nowPos.z));
        if (_moveType == Type.MoveType.KeyBoard)
        {
            _cameraLocalRotation = quaternion;
            _camera.transform.localRotation = _cameraLocalRotation;
            // transform.localRotation = localRotation;
        }
        
        if (_moveType == Type.MoveType.Mouse && _state == Type.State.MOVE)
        {
            Vector3 dest = new Vector3(_target.x, transform.position.y, _target.z);

            if (dest != Vector3.zero)
                transform.LookAt(dest);
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
    IEnumerator CoArcherQSkill()
    {
        GameObject effect = Managers.Resource.Instantiate("Effect/Meteor");
        effect.transform.position = transform.position;
        yield return new WaitForSeconds(1.0f);
        yield return new WaitForSeconds(1.0f);
        yield return new WaitForSeconds(1.0f);
        yield return new WaitForSeconds(1.0f);
        Managers.Resource.Destory(effect);
    }

    IEnumerator CoWarriorQSkill()
    {
        GameObject effect = Managers.Resource.Instantiate("Effect/FreezeCircle");
        effect.transform.position = transform.position;
        yield return new WaitForSeconds(1.0f);
        yield return new WaitForSeconds(1.0f);
        yield return new WaitForSeconds(1.0f);
        yield return new WaitForSeconds(1.0f);
        Managers.Resource.Destory(effect);
    }
    IEnumerator CoWarriorESkill()
    {
        _coAttack = true;
        yield return new WaitForSeconds(1.0f);
        GameObject effect = Managers.Resource.Instantiate("Effect/Holyhit");
        Vector3 effectD = transform.TransformDirection(Vector3.forward * 2);
        Vector3 pos = transform.position + effectD;
        effect.transform.position = new Vector3(pos.x, pos.y + 1.0f, pos.z);
        yield return new WaitForSeconds(1.0f);
        Managers.Resource.Destory(effect);
        _coAttack = false;
    }

    IEnumerator CoArcherESkill()
    {
        _coAttack = true;
        yield return new WaitForSeconds(0.6f);
        GameObject arrow = Managers.Resource.Instantiate("Effect/MagicArrow");
        Vector3 dir = transform.localRotation * Vector3.fwd;
        arrow.AddComponent<SkillArrowController>().Init(false, dir, transform.eulerAngles, 0);
        arrow.transform.position = bow.transform.position;
        yield return new WaitForSeconds(0.4f);
        _coAttack = false;
        yield return new WaitForSeconds(2.0f);
    }

    IEnumerator CoDeath()
    {
        _animator.Play("death");
        yield return new WaitForSeconds(4.0f);
    }

    public override void Attacked()
    {
        //if (_attackedCoolTime) return;

        //if (_coAttacked) return;

        //_animator.Play("hit");
        //StartCoroutine(CoAttacked());
    }

    public override void Destory()
    {
        base.Destory();
        if (_hpController)
            Managers.Resource.Destory(_hpController.gameObject);

        if (_usernameText)
            Managers.Resource.Destory(_usernameText.transform.parent.gameObject);

        if (_talk)
            Managers.Resource.Destory(_talk.gameObject);

        if (_debugText)
            Managers.Resource.Destory(_debugText.gameObject);
    }

    public override void SetHp(float hp)
    {
        if (_hpController)
            _hpController.SetHp(hp);
    }

    public override void SetHpMax(float hpMax)
    {
        if (_hpController)
            _hpController.SetHPMax(hpMax);
    }

    public override void SetMp(float mp)
    {

    }

    public override void SetMpMax(float mpMax)
    {
 
    }

    public override void Death()
    {
        _death = true;
        StartCoroutine(CoDeath());
    }

    public override void SetExp(int level, float exp, float expMax)
    {
        if (_level != level)
        {
            // ������ ����Ʈ
            _level = level;
            GameObject bufferEffect = Managers.Resource.Instantiate("Effect/Buff");
            BufferEffectController bf = bufferEffect.AddComponent<BufferEffectController>();
            bf.Player = gameObject;
        }
    }
}
