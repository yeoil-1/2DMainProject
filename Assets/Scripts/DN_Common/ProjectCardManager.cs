using System.Collections.Generic;
using UnityEngine;

public class ProjectCardManager : MonoBehaviour
{
    public static ProjectCardManager Inst { get; set; }


    [SerializeField] private GameObject Prefab_CardVisual;
    [SerializeField] private Transform Transform_HandRoot;


    private List<ProjectCardModel> _deckPile = new List<ProjectCardModel>();
    private List<ProjectCardModel> _handPile = new List<ProjectCardModel>();
    private List<ProjectCardModel> _gravePile = new List<ProjectCardModel>();

    private Dictionary<int, GameObject> _spawnedCardVisuals = new Dictionary<int, GameObject>();
    private int _cardVisualKeyGenerator = 0;

    public int DeckPileCount => _deckPile.Count;
    public int HandPileCount => _handPile.Count;

    private void Awake()
    {
        Inst = this;
    }

    public void InitBattleDeck(List<ProjectCardModel> playerTotalCards)
    {
        _deckPile.Clear();
        _handPile.Clear();
        _gravePile.Clear();
        ClearAllSpawnedVisuals();

        if (playerTotalCards == null || playerTotalCards.Count == 0)
        {
            Debug.LogWarning("보유한 카드가 없어 인게임 덱을 구성할 수 없습니다.");
            return;
        }

        foreach (var cardModel in playerTotalCards)
        {
            if (cardModel.IsInDeck == true)
            {
                var inGameCard = new ProjectCardModel
                {
                    CardUniqueId = cardModel.CardUniqueId,
                    CardDataId = cardModel.CardDataId,
                    IsInDeck = cardModel.IsInDeck
                };
                _deckPile.Add(inGameCard);
            }
        }

        ShuffleDeck();
    }

    public void DrawCards(int drawCount)
    {
        for (int i = 0; i < drawCount; i++)
        {
            if (_deckPile.Count == 0)
            {
                if (_gravePile.Count == 0) break;
                ReshuffleGraveToDeck();
            }

            int topIndex = _deckPile.Count - 1;
            ProjectCardModel drawnCard = _deckPile[topIndex];
            _deckPile.RemoveAt(topIndex);

            _handPile.Add(drawnCard);
            CreateCardVisualOnHand(drawnCard);
        }
    }

    public void ExecuteCardEffect(ProjectCardModel cardModel, int targetInstanceId, bool isUpgradedCard = false)
    {
        ProjectCardData cardData = DaniTechGameDataManager.Instance.GetProjectCardData(cardModel.CardDataId);
        if (cardData == null)
        {
            Debug.LogWarning($"[CardManager] {cardModel.CardDataId}에 해당하는 카드를 찾을 수 없습니다.");
            return;
        }

        int requiredCost = isUpgradedCard ? cardData.UpgradedCost : cardData.Cost;
        
        var player = DaniTechGameObjectManager.Inst.BattlePlayerTarget;
        if (player == null) return;
        if (player.CheckCanUseEnergy(requiredCost) == false)
        {
            Debug.LogWarning($"[CardManager] 에너지가 부족합니다! 필요: {requiredCost}, 현재: {player.CurrentMana}");
            // TODO: UIManager.Instance.OpenSimplePopup("에너지가 부족합니다!"); 같은 경고창 연동 구간
            return;
        }

        player.ConsumeEnergy(requiredCost);

        List<int> activeValues = isUpgradedCard ? cardData.UpgradedEffectValueList : cardData.EffectValueList;

        switch (cardData.CardType)
        {
            case "공격":
                ProcessAttackCardEffect(cardData, targetInstanceId, activeValues);
                break;

            case "스킬":
                ProcessSkillCardEffect(cardData, activeValues);
                break;

            case "파워":
                ProcessPowerCardEffect(cardData, activeValues);
                break;

            default:
                Debug.LogWarning($"[CardManager] 정의되지 않은 CardType 분류입니다: {cardData.CardType}");
                break;
        }
    }

    public void RemoveCardFromHandToGrave(int cardVisualInstanceId, ProjectCardModel cardModel)
    {
        if (_handPile.Contains(cardModel))
        {
            _handPile.Remove(cardModel);
            _gravePile.Add(cardModel);
        }

        if (_spawnedCardVisuals.TryGetValue(cardVisualInstanceId, out var cardObj))
        {
            if (cardObj != null) Destroy(cardObj);
            _spawnedCardVisuals.Remove(cardVisualInstanceId);
        }
    }

    private void ProcessAttackCardEffect(ProjectCardData cardData, int targetInstanceId, List<int> activeValues)
    {
        GameObject targetEnemyObj = DaniTechGameObjectManager.Inst.GetEntityObjectCanBeNull(targetInstanceId);
        if (targetEnemyObj == null) return;

        Project_2DEnemy enemyComponent = targetEnemyObj.GetComponent<Project_2DEnemy>();
        if (enemyComponent == null) return;

        int finalDamage = 0;

        // 공용 계산식 활용 연산 분기
        if (cardData.Id == "card_breakthrough_01")
        {
            int selfDamage = activeValues[0];
            int enemyDamage = activeValues[1];

            var player = DaniTechGameObjectManager.Inst.BattlePlayerTarget;
            if (player != null) player.TakeDamage(selfDamage);

            finalDamage = DaniTechGameUtil.CalcCharacterFinalDamage(curCharacterLevel: 1, levelPerDamage: enemyDamage, isCritical: false);
        }
        else if (cardData.Id == "card_bash_01" || cardData.Id == "card_thunderclap_01")
        {
            int targetDamage = activeValues[0];
            int statusValue = activeValues[1];

            finalDamage = DaniTechGameUtil.CalcCharacterFinalDamage(curCharacterLevel: 1, levelPerDamage: targetDamage, isCritical: false);

            // enemyComponent.AddStatusEffect(DaniTechStatusEffectType.Weaken, statusValue);
        }
        else
        {
            int targetDamage = activeValues[0];
            finalDamage = DaniTechGameUtil.CalcCharacterFinalDamage(curCharacterLevel: 1, levelPerDamage: targetDamage, isCritical: false);
        }

        enemyComponent.TakeDamage(finalDamage);

        if (enemyComponent.IsDead == true)
        {
            DaniTechGameObjectManager.Inst.RequestDestroyEntityObject(targetInstanceId);
        }
    }

    private void ProcessSkillCardEffect(ProjectCardData cardData, List<int> activeValues)
    {
        var player = DaniTechGameObjectManager.Inst.BattlePlayerTarget;

        if (cardData.Id == "card_bloodwall_01")
        {
            int hpLoss = activeValues[0];
            int shieldValue = activeValues[1];

            if (player != null)
            {
                player.TakeDamage(hpLoss);
                player.AddStatusEffect(ProjectStatusEffectType.ShieldBuff, shieldValue);
            }
            Debug.Log($"[CardManager] {cardData.Name} 발동. 체력 {hpLoss} 소모 / 방어도 {shieldValue} 획득");
        }
        else if (cardData.Id == "card_defend_01")
        {
            int shieldValue = activeValues[0];

            if (player != null)
            {
                player.AddStatusEffect(ProjectStatusEffectType.ShieldBuff, shieldValue);
            }
            Debug.Log($"[CardManager] {cardData.Name} 발동. 방어도 {shieldValue} 획득");
        }
    }

    private void ProcessPowerCardEffect(ProjectCardData cardData, List<int> activeValues)
    {
        int powerValue = activeValues[0];
        var player = DaniTechGameObjectManager.Inst.BattlePlayerTarget;

        if (player != null)
        {
            player.AddStatusEffect(ProjectStatusEffectType.AttackBuff, powerValue);
        }
        Debug.Log($"[CardManager] 파워 카드 작동 완료: {cardData.Name} (공격버프 {powerValue} 중첩)");
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < _deckPile.Count; i++)
        {
            int rnd = Random.Range(i, _deckPile.Count);
            var temp = _deckPile[i];
            _deckPile[i] = _deckPile[rnd];
            _deckPile[rnd] = temp;
        }
    }

    private void ReshuffleGraveToDeck()
    {
        foreach (var card in _gravePile) _deckPile.Add(card);
        _gravePile.Clear();
        ShuffleDeck();
    }

    private void CreateCardVisualOnHand(ProjectCardModel cardModel)
    {
        if (Prefab_CardVisual == null) return;

        var cardObj = Instantiate(Prefab_CardVisual, Transform_HandRoot);
        if (cardObj == null) return;

        _cardVisualKeyGenerator++;
        var cardVisualScript = cardObj.GetComponent<Project_CardVisual>();
        if (cardVisualScript != null)
        {
            cardVisualScript.InitCardVisualInfo(_cardVisualKeyGenerator, cardModel.CardDataId, cardModel);
        }

        _spawnedCardVisuals.Add(_cardVisualKeyGenerator, cardObj);
    }

    private void ClearAllSpawnedVisuals()
    {
        foreach (var kv in _spawnedCardVisuals)
        {
            if (kv.Value != null) Destroy(kv.Value);
        }
        _spawnedCardVisuals.Clear();
        _cardVisualKeyGenerator = 0;
    }
}