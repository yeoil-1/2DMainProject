using System.Collections.Generic;
using UnityEngine;

public class Project_2DPlayer : MonoBehaviour
{
    public int InstanceId { get; private set; }
    public int CurrentHp { get; private set; }
    public int MaxHp { get; private set; }
    public bool IsDead { get; private set; }

    // 기수 규칙: 멤버변수는 _ 소문자 시작, 참조 객체는 대문자 시작
    private List<ProjectStatusInstanceModel> _activeStatusList = new List<ProjectStatusInstanceModel>();
    [SerializeField] private Animator Animator_Player;


    public void InitPlayerCharacterInfo(int generatedId)
    {
        InstanceId = generatedId;
        IsDead = false;

        // [데이터 드리븐 연동] 

        MaxHp = 80;
        CurrentHp = MaxHp;

        _activeStatusList.Clear();
    }


    public List<ProjectStatusInstanceModel> GetActiveStatusList()
    {
        return _activeStatusList;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead == true) return;

        CurrentHp -= damage;
        Debug.Log($"[Player] 플레이어가 {damage}의 피해를 입음. 실시간 HP: {CurrentHp}/{MaxHp}");

        if (Animator_Player != null)
        {
            Animator_Player.SetTrigger("OnHit");
        }

        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            IsDead = true;
        }
    }

    public void SetHp(int newHp)
    {
        CurrentHp = newHp;
    }
}
