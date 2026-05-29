using System.Collections.Generic;
using UnityEngine;

public class ProjectBattleManager : MonoBehaviour
{
    public static ProjectBattleManager Inst {  get; set; }

    private void Awake()
    {
        Inst = this;
    }

    public void RequestResolveCard(ProjectCardModel cardModel, int targetInstanceId, bool isUpgradedCard = false)
    {
        // 1. [원본 매핑] 제공해주신 데이터 테이블 명세에 맞춰 카드 스태틱 데이터를 가져옵니다.
        // (GameDataManager에 등록된 카드 데이터 풀에서 인스턴스의 CardDataId로 조회)
        ProjectCardData cardData = DaniTechGameDataManager.Instance.GetCardData(cardModel.CardDataId);
        if (cardData == null)
        {
            Debug.LogWarning($"[Battle] {cardModel.CardDataId}에 해당하는 카드 명세를 테이블에서 찾을 수 없습니다.");
            return;
        }

        // 2. [원본 매핑] 제공해주신 오브젝트 매니저의 실시간 키 검사 및 객체 반환 함수 활용
        GameObject targetEnemyObj = DaniTechGameObjectManager.Inst.GetEntityObjectCanBeNull(targetInstanceId);
        if (targetEnemyObj == null)
        {
            return;
        }

        // 3. 강화 여부에 따라 엑셀 테이블에 명시된 올바른 수치 리스트(EffectValueList vs UpgradedEffectValueList)를 추출합니다.
        List<int> activeValues = isUpgradedCard ? cardData.UpgradedEffectValueList : cardData.EffectValueList;

        // 4. 내부 전용 판정 함수로 전달 (private 원칙)
        ResolveCardEffect(cardData, activeValues, targetEnemyObj, targetInstanceId);
    }

    private void ResolveCardEffect(ProjectCardData cardData, List<int> effectValues, GameObject targetObj, int targetInstanceId)
    {
        // 테이블 스키마에 정의된 "공격", "스킬", "파워" 분류에 따라 분기 연산합니다.
        switch (cardData.CardType)
        {
            case "공격":
                ApplyAttackCardEffect(cardData, effectValues, targetObj, targetInstanceId);
                break;

            case "스킬":
                ApplySkillCardEffect(cardData, effectValues, targetObj);
                break;

            case "파워":
                ApplyPowerCardEffect(cardData, effectValues);
                break;

            default:
                Debug.LogWarning($"[Battle] 정해지지 않은 카드 타입 분류입니다: {cardData.CardType}");
                break;
        }
    }

    

}
