using System.Collections.Generic;
using UnityEngine;

public interface IProjectCardEffect
{
    void Apply(int targetInstanceId, List<int> activeValues);
}

public enum DaniTechCardEffectType
{
    SingleDamage, 
    GiveShield,    
    AreaDamage,   
    DrawCard,       
    SelfDamage,
    GiveStatusEffect,
    ExhaustCard
}

public class ProjectCardManager : MonoBehaviour
{
    public static ProjectCardManager Inst { get; set; }

    [SerializeField] private GameObject Prefab_CardVisual;
    [SerializeField] private Transform Transform_HandRoot;

    private List<ProjectCardModel> _deckPile = new List<ProjectCardModel>();
    private List<ProjectCardModel> _handPile = new List<ProjectCardModel>();
    private List<ProjectCardModel> _gravePile = new List<ProjectCardModel>();
    private List<ProjectCardModel> _exhaustPile = new List<ProjectCardModel>();

    private Dictionary<int, GameObject> _spawnedCardVisuals = new Dictionary<int, GameObject>();
    private int _cardVisualKeyGenerator = 0;


    private Dictionary<string, IProjectCardEffect> _effectLibrary = new Dictionary<string, IProjectCardEffect>();


    private ProjectCardModel _currentProcessingCard;
    private int _currentProcessingVisualId;
    private bool _isCurrentCardExhausted = false;

    public int DeckPileCount => _deckPile.Count;
    public int HandPileCount => _handPile.Count;
    public int ExhaustPileCount => _exhaustPile.Count;

    private void Awake()
    {
        Inst = this;

        _effectLibrary.Add("SingleDamage", new Effect_SingleDamage());
        _effectLibrary.Add("GiveShield", new Effect_GiveShield());
        _effectLibrary.Add("AreaDamage", new Effect_AreaDamage());
        _effectLibrary.Add("DrawCard", new Effect_DrawCard());
        _effectLibrary.Add("SelfDamage", new Effect_SelfDamage());
        _effectLibrary.Add("GiveStatusEffect", new Effect_GiveStatusEffect(ProjectStatusEffectType.Weaken));
        _effectLibrary.Add("ExhaustCard", new Effect_ExhaustCard());
    }


    public void InitBattleDeck(List<ProjectCardModel> playerTotalCards)
    {
        _deckPile.Clear();
        _handPile.Clear();
        _gravePile.Clear();
        _exhaustPile.Clear();
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


    public void ExecuteCardEffect(ProjectCardModel cardModel, int targetInstanceId, bool isUpgradedCard = false, int cardVisualInstanceId = 0)
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
            return;
        }

        player.ConsumeEnergy(requiredCost);

        string[] effectKeys = cardData.EffectType.Split(',');
        List<int> activeValues = isUpgradedCard ? cardData.UpgradedEffectValueList : cardData.EffectValueList;

        foreach (string key in effectKeys)
        {
            // 텍스트 공백 제거 안전장치
            string trimmedKey = key.Trim();

            if (_effectLibrary.TryGetValue(trimmedKey, out IProjectCardEffect processor))
            {
                processor.Apply(targetInstanceId, activeValues);
            }
        }

        if (_isCurrentCardExhausted == true)
        {
            if (_handPile.Contains(_currentProcessingCard)) _handPile.Remove(_currentProcessingCard);
            _exhaustPile.Add(_currentProcessingCard);
            DestroyCardVisual(_currentProcessingVisualId);
            Debug.Log($"[소멸] {_currentProcessingCard.CardDataId} 카드가 소멸되었습니다.");
        }
        else
        {

            RemoveCardFromHandToGrave(_currentProcessingVisualId, _currentProcessingCard);
        }
    }

    public void RequestExhaustCurrentCard()
    {
        _isCurrentCardExhausted = true;
    }

    public void RemoveCardFromHandToGrave(int cardVisualInstanceId, ProjectCardModel cardModel)
    {
        if (_handPile.Contains(cardModel))
        {
            _handPile.Remove(cardModel);
            _gravePile.Add(cardModel);
        }
        DestroyCardVisual(cardVisualInstanceId);
    }

    private void DestroyCardVisual(int cardVisualInstanceId)
    {
        if (_spawnedCardVisuals.TryGetValue(cardVisualInstanceId, out var cardObj))
        {
            if (cardObj != null) Destroy(cardObj);
            _spawnedCardVisuals.Remove(cardVisualInstanceId);
        }
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

// 1. 단일 대미지
public class Effect_SingleDamage : IProjectCardEffect
{
    public void Apply(int targetInstanceId, List<int> activeValues)
    {
        GameObject targetEnemyObj = DaniTechGameObjectManager.Inst.GetEntityObjectCanBeNull(targetInstanceId);
        if (targetEnemyObj == null) return;

        Project_2DEnemy enemyComponent = targetEnemyObj.GetComponent<Project_2DEnemy>();
        if (enemyComponent == null) return;

        int finalDamage = DaniTechGameUtil.CalcCharacterFinalDamage(curCharacterLevel: 1, levelPerDamage: activeValues[0], isCritical: false);
        enemyComponent.TakeDamage(finalDamage);

        if (enemyComponent.IsDead == true)
        {
            DaniTechGameObjectManager.Inst.RequestDestroyEntityObject(targetInstanceId);
        }
    }
}

// 2. 방어도
public class Effect_GiveShield : IProjectCardEffect
{
    public void Apply(int targetInstanceId, List<int> activeValues)
    {
        var player = DaniTechGameObjectManager.Inst.BattlePlayerTarget;
        if (player != null)
        {
            player.AddStatusEffect(ProjectStatusEffectType.ShieldBuff, activeValues[0]);
        }
    }
}

// 3. 광역 대미지
public class Effect_AreaDamage : IProjectCardEffect
{
    public void Apply(int targetInstanceId, List<int> activeValues)
    {
        int baseAreaDamage = activeValues.Count > 1 ? activeValues[1] : activeValues[0]; // 정면돌파 조립형 인덱스 방어용 예외 연산
        int finalAreaDamage = DaniTechGameUtil.CalcCharacterFinalDamage(curCharacterLevel: 1, levelPerDamage: baseAreaDamage, isCritical: false);

        List<GameObject> activeEnemies = DaniTechGameObjectManager.Inst.GetAllLivingEnemiesCanBeNull();
        if (activeEnemies == null) return;

        foreach (var enemyObj in activeEnemies)
        {
            if (enemyObj == null) continue;
            var enemyComponent = enemyObj.GetComponent<Project_2DEnemy>();
            if (enemyComponent != null)
            {
                enemyComponent.TakeDamage(finalAreaDamage);
                if (enemyComponent.IsDead == true)
                {
                    // 실시간 순회 중 리스트 터짐 방지를 위해 오브젝트 매니저의 예약 디스트로이 기능 호출 선호
                    DaniTechGameObjectManager.Inst.RequestDestroyEntityObject(enemyComponent.InstanceId);
                }
            }
        }
    }
}

// 4. 드로우
public class Effect_DrawCard : IProjectCardEffect
{
    public void Apply(int targetInstanceId, List<int> activeValues)
    {
        ProjectCardManager.Inst.DrawCards(activeValues[0]);
    }
}

// 5. 자해 대미지
public class Effect_SelfDamage : IProjectCardEffect
{
    public void Apply(int targetInstanceId, List<int> activeValues)
    {
        var player = DaniTechGameObjectManager.Inst.BattlePlayerTarget;
        if (player != null)
        {
            player.TakeDamage(activeValues[0]);
        }
    }
}

// 6. 버프 및 디버프 상태 효과
public class Effect_GiveStatusEffect : IProjectCardEffect
{
    private ProjectStatusEffectType _effectType;
    public Effect_GiveStatusEffect(ProjectStatusEffectType effectType) => _effectType = effectType;

    public void Apply(int targetInstanceId, List<int> activeValues)
    {
        int statusValue = activeValues.Count > 1 ? activeValues[1] : activeValues[0];

        GameObject targetEnemyObj = DaniTechGameObjectManager.Inst.GetEntityObjectCanBeNull(targetInstanceId);
        if (targetEnemyObj == null) return;

        Project_2DEnemy enemyComponent = targetEnemyObj.GetComponent<Project_2DEnemy>();
        if (enemyComponent != null)
        {
            enemyComponent.AddStatusEffect(_effectType, statusValue);
        }
    }
}

// 7. 카드 소멸
public class Effect_ExhaustCard : IProjectCardEffect
{
    public void Apply(int targetInstanceId, List<int> activeValues)
    {
        ProjectCardManager.Inst.RequestExhaustCurrentCard();
    }
}