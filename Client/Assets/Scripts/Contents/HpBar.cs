using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    [SerializeField]
    Transform _hpBar = null;
    
    public void SetHpBar(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1); //Clamp함수 : 값이 0이하로 떨어지면 0으로 맞춰줌, 1이상으로 올라가도 1로 맞춰줌
        _hpBar.localScale = new Vector3(ratio, 1, 1);
    }
}
