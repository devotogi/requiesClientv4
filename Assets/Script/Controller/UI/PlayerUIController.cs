using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField]
    TMP_Text _skilllQName;
    [SerializeField]
    TMP_Text _skilllEName;
    [SerializeField]
    Image _skill1CollTimeImg;
    [SerializeField]
    Image _skill2CollTimeImg;
    bool skill1CollTime;
    bool skill2CollTime;

    private float skill1CoolTime;
    private float skill2CoolTime;
    public void Init(Type.CharacterType type)
    {
        switch (type) 
        {
            case Type.CharacterType.Warrior:
                _skilllQName.text = "아이스필드(Q)";
                _skilllEName.text = "파워스트라이크(E)";
                break;

            case Type.CharacterType.Archer:
                _skilllQName.text = "메테오(Q)";
                _skilllEName.text = "붐샷(E)";
                break;
        }
    }

    public void SetQCoolTime(float cool) 
    {
        StartCoroutine(QCoolTime(cool));
    }

    public void SetECoolTime(float cool)
    {
        StartCoroutine(ECoolTime(cool));
    }

    IEnumerator QCoolTime(float cool) 
    {
        float coolTimeMax = cool;
        while (cool > 0.0f)
        { 
            cool -= Time.deltaTime;
            _skill1CollTimeImg.fillAmount = cool / coolTimeMax;
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator ECoolTime(float cool)
    {
        float coolTimeMax = cool;
        while (cool > 0.0f)
        {
            cool -= Time.deltaTime;
            _skill2CollTimeImg.fillAmount = cool / coolTimeMax;
            yield return new WaitForFixedUpdate();
        }
    }

}
