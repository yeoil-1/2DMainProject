using UnityEngine;

public class Project_2DEnemy : MonoBehaviour
{
    [SerializeField] private Animator Animator_Enemy;
    
    public int InstanceId { get; private set; }
    public string MonsterDataId { get; private set; }
    public int CurrentHp { get; private set; }
    public int MaxHp { get; private set; }
    public bool IsDead { get; private set; }


    public void InitEnemyInfo(int generatedId, string monsterDataId)
    {
        InstanceId = generatedId;
        MonsterDataId = monsterDataId;

        DNMonsterData staticMonsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(monsterDataId);

        if (staticMonsterData == null)
        {
            Debug.LogError($"[Enemy] {monsterDataId}에 해당하는 몬스터 스태틱 데이터가 존재하지 않습니다.");
            return;
        }

        // 밸런스 데이터 연동 (예시: 엑셀 테이블에 명시된 기본 체력 스탯을 세팅)
        // 만약 DNMonsterData에 Hp 필드가 없다면 기획 스키마에 맞춰 확장하여 사용합니다.
        MaxHp = 100; // 기획서 데이터 기반 매핑 (ex: staticMonsterData.BaseHp)
        CurrentHp = MaxHp;
        IsDead = false;

        gameObject.name = $"{staticMonsterData.Name} (ID: {InstanceId})";
    }

    /// <summary>
    /// BattleManager가 카드의 최종 연산된 대미지를 전달하여 몬스터의 인스턴스 데이터를 수정하는 함수입니다.
    /// </summary>
    public void TakeDamage(int damage)
    {
        if (IsDead == true) return;

        CurrentHp -= damage;
        Debug.Log($"[Enemy] {gameObject.name} 피격! 대미지: {damage} | 남은 체력: {CurrentHp}/{MaxHp}");

        if (Animator_Enemy != null)
        {
            Animator_Enemy.SetTrigger("OnHit");
        }

        // 사망 판정 (스스로 Destroy하지 않고 상태값만 스위칭하여 매니저에게 알림)
        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            IsDead = true;
        }
    }
}
