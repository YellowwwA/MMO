using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    [SerializeField]
    Transform _hpBar = null;
    
    public void SetHpBar(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1); //Clamp�Լ� : ���� 0���Ϸ� �������� 0���� ������, 1�̻����� �ö󰡵� 1�� ������
        _hpBar.localScale = new Vector3(ratio, 1, 1);
    }
}
