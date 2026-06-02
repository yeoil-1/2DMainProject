using System.Collections.Generic;
using UnityEngine;

public class Project_2DEnemy : MonoBehaviour
{
    [SerializeField] private Animator Animator_Enemy;
    
    public int InstanceId { get; private set; }
    public string MonsterDataId { get; private set; }
    public int CurrentHp { get; private set; }
    public int MaxHp { get; private set; }
    public bool IsDead { get; private set; }

    private List<ProjectStatusInstanceModel> _activeStatusList = new List<ProjectStatusInstanceModel>();

    public void InitEnemyInfo(int generatedId, string monsterDataId)
    {
        InstanceId = generatedId;
        MonsterDataId = monsterDataId;

        ProjectMonsterData staticMonsterData = DaniTechGameDataManager.Instance.GetProjectMonsterData(monsterDataId);

        if (staticMonsterData == null)
        {
            Debug.LogError($"[Enemy] {monsterDataId}에 해당하는 몬스터 스태틱 데이터가 존재하지 않습니다.");
            return;
        }

        MaxHp = staticMonsterData.MaxHp; 
        CurrentHp = MaxHp;
        IsDead = false;

        gameObject.name = $"{staticMonsterData.Name} (ID: {InstanceId})";
        _activeStatusList.Clear();
    }


    public void TakeDamage(int damage)
    {
        if (IsDead == true) return;

        CurrentHp -= damage;
        Debug.Log($"[Enemy] {gameObject.name} 피격! 대미지: {damage} | 남은 체력: {CurrentHp}/{MaxHp}");

        if (Animator_Enemy != null)
        {
            Animator_Enemy.SetTrigger("OnHit");
        }

        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            IsDead = true;
        }
    }
}
