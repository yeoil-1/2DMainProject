using System.Collections.Generic;
using UnityEngine;

public class ProjectBattleManager : MonoBehaviour
{
    public static ProjectBattleManager Inst {  get; set; }
    private Project_2DPlayer _playerCharacter;

    private void Awake()
    {
        Inst = this;
    }

    public void RequestResolveCard(ProjectCardModel cardModel, int targetInstanceId, bool isUpgradedCard = false)
    {

        ProjectCardData cardData = DaniTechGameDataManager.Instance.GetCardData(cardModel.CardDataId);
        if (cardData == null)
        {
            Debug.LogWarning($"[Battle] {cardModel.CardDataId}에 해당하는 카드 명세를 테이블에서 찾을 수 없습니다.");
            return;
        }

        GameObject targetEnemyObj = DaniTechGameObjectManager.Inst.GetEntityObjectCanBeNull(targetInstanceId);
        if (targetEnemyObj == null)
        {
            return;
        }

        List<int> activeValues = isUpgradedCard ? cardData.UpgradedEffectValueList : cardData.EffectValueList;

        ResolveCardEffectByType(cardData, targetEnemyObj, targetInstanceId);
    }

    private void ResolveCardEffectByType(ProjectCardData cardData, GameObject targetObj, int targetInstanceId)
    {
        switch (cardData.CardType)
        {
            case "공격":
                ApplyAttackCardEffect(cardData, targetObj, targetInstanceId);
                break;

            case "스킬":
                ApplySkillCardEffect(cardData, targetObj);
                break;

            case "파워":
                ApplyPowerCardEffect(cardData);
                break;

            default:
                Debug.LogWarning($"[Battle] 정의되지 않은 CardType 분류입니다: {cardData.CardType}");
                break;
        }
    }

    private void ApplyAttackCardEffect(ProjectCardData cardData, GameObject targetObj, int targetInstanceId)
    {
        Project_2DEnemy enemyComponent = targetObj.GetComponent<Project_2DEnemy>();
        if (enemyComponent == null) return;

        int finalDamage = 0;

        if (cardData.Id == "card_breakthrough_01") // 정면 돌파: [0] 자해 체력, [1] 모든 적 피해량
        {
            int selfDamage = cardData.EffectValueList[0];
            int enemyDamage = cardData.EffectValueList[1];

            finalDamage = DaniTechGameUtil.CalcCharacterFinalDamage(curCharacterLevel: 1, levelPerDamage: enemyDamage, isCritical: false);
        }
        else if (cardData.Id == "card_bash_01" || cardData.Id == "card_thunderclap_01") // 강타, 천둥: [0] 피해량, [1] 디버프 수치
        {
            int targetDamage = cardData.EffectValueList[0];
            int statusValue = cardData.EffectValueList[1];

            finalDamage = DaniTechGameUtil.CalcCharacterFinalDamage(curCharacterLevel: 1, levelPerDamage: targetDamage, isCritical: false);
            // 추후 statusValue(취약 등)를 타겟의 버프/디버프 모델에 주입하는 룰 연산 추가 구간
        }
        else // 일반적인 단일 공격 카드 (타격 등): [0] 피해량
        {
            int targetDamage = cardData.EffectValueList[0];
            finalDamage = DaniTechGameUtil.CalcCharacterFinalDamage(curCharacterLevel: 1, levelPerDamage: targetDamage, isCritical: false);
        }

        enemyComponent.TakeDamage(finalDamage);

        if (enemyComponent.IsDead == true)
        {
            DaniTechGameObjectManager.Inst.RequestDestroyEntityObject(targetInstanceId);
        }
    }

    private void ApplySkillCardEffect(ProjectCardData cardData, GameObject targetObj)
    {
        if (cardData.Id == "card_bloodwall_01") // 피의 벽: [0] 자해 체력, [1] 획득 방어도
        {
            int hpLoss = cardData.EffectValueList[0];
            int shieldValue = cardData.EffectValueList[1];

            Debug.Log($"[Battle] {cardData.Name} 적용 완료. 체력 {hpLoss} 소모 / 방어도 {shieldValue} 획득");
        }
        else if (cardData.Id == "card_defend_01") // 수비: [0] 획득 방어도
        {
            int shieldValue = cardData.EffectValueList[0];

            Debug.Log($"[Battle] {cardData.Name} 적용 완료. 방어도 {shieldValue} 획득");
        }
    }

    private void ApplyPowerCardEffect(ProjectCardData cardData)
    {

        Debug.Log($"[Battle] 파워 카드 영구 룰 활성화: {cardData.Name}");
    }

}
