using NUnit.Framework;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class ProjectCardManager : MonoBehaviour
{
    public static ProjectCardManager Inst { get; set; }


    [SerializeField] private GameObject Prefab_CardVisual; // 씬에 생성할 카드 UI 프리팹
    [SerializeField] private Transform Transform_HandRoot;  // 카드가 정렬될 손패 부모 위치


    // 인게임 전투 중에 실시간으로 관리되는 3대 카드 더미 (인스턴스 데이터)
    private List<ProjectCardModel> _deckPile = new List<ProjectCardModel>();
    private List<ProjectCardModel> _handPile = new List<ProjectCardModel>();
    private List<ProjectCardModel> _gravePile = new List<ProjectCardModel>();

    // 화면에 동적 생성된 카드 Visual 게임 오브젝트들을 관리하기 위한 컨테이너와 키 생성기
    private Dictionary<int, GameObject> _spawnedCardVisuals = new Dictionary<int, GameObject>();
    private int _cardVisualKeyGenerator = 0;

    private void Awake()
    {
        Inst = this;
    }

    // 전투가 시작될 때 BattleManager 등에서 플레이어의 보유 카드 리스트를 받아와 세팅하는 함수
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

        // 전체 카드 중 현재 '덱에 포함된 카드(IsInDeck == true)'만 선별하여 전투용 덱 파일 구축
        foreach (var cardModel in playerTotalCards)
        {
            if (cardModel.IsInDeck == true)
            {
                // 전투 중 원본 유저 데이터가 오염되지 않도록 새 객체로 복사하여 추가
                var inGameCard = new ProjectCardModel
                {
                    CardUniqueId = cardModel.CardUniqueId,
                    CardDataId = cardModel.CardDataId,
                    IsInDeck = cardModel.IsInDeck
                };
                _deckPile.Add(inGameCard);
            }
        }

        // 전투용 첫 덱을 무작위로 섞음
        ShuffleDeck();
    }

    // 지정된 장수만큼 카드를 뽑는 핵심 드로우 함수
    public void DrawCards(int drawCount)
    {
        for (int i = 0; i < drawCount; i++)
        {
            // 덱에 카드가 없다면 무덤에 쌓인 카드를 다시 덱으로 복사해서 섞음
            if (_deckPile.Count == 0)
            {
                if (_gravePile.Count == 0)
                {
                    Debug.LogWarning("덱과 무덤 모두에 카드가 없어 드로우를 종료합니다.");
                    break;
                }
                ReshuffleGraveToDeck();
            }

            // 덱의 맨 위(마지막 인덱스)에서 카드를 한 장 추출
            int topIndex = _deckPile.Count - 1;
            ProjectCardModel drawnCard = _deckPile[topIndex];
            _deckPile.RemoveAt(topIndex);

            // 손패(Hand) 더미에 데이터 추가
            _handPile.Add(drawnCard);

            // 데이터 이동이 완료되었으니 눈에 보이는 실제 카드 오브젝트를 화면에 물리적 생성
            CreateCardVisualOnHand(drawnCard);
        }
    }

    // 손패에 있는 특정 카드를 사용했을 때 호출되는 함수
    public void RemoveCardFromHandToGrave(int cardVisualInstanceId, ProjectCardModel cardModel)
    {
        // 1. 손패 데이터 리스트에서 제거 후 무덤 더미로 이동
        if (_handPile.Contains(cardModel))
        {
            _handPile.Remove(cardModel);
            _gravePile.Add(cardModel);
        }

        // 2. 화면에 배치되어 있던 해당 카드 오브젝트 물리적 파괴
        if (_spawnedCardVisuals.TryGetValue(cardVisualInstanceId, out var cardObj))
        {
            if (cardObj != null)
            {
                Destroy(cardObj);
            }
            _spawnedCardVisuals.Remove(cardVisualInstanceId);
        }

        Debug.Log($"카드가 무덤으로 이동했습니다. 현재 무덤 카드 수: {_gravePile.Count}");
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
        Debug.Log("인게임 덱이 완전히 무작위로 뒤섞였습니다.");
    }

    private void ReshuffleGraveToDeck()
    {
        foreach (var card in _gravePile)
        {
            _deckPile.Add(card);
        }
        _gravePile.Clear();
        ShuffleDeck();
    }

    private void CreateCardVisualOnHand(ProjectCardModel cardModel)
    {
        if (Prefab_CardVisual == null) return;

        // 손패 레이아웃 루트 자식으로 카드 프리팹 동적 생성
        var cardObj = Instantiate(Prefab_CardVisual, Transform_HandRoot);
        if (cardObj == null) return;

        _cardVisualKeyGenerator++;

        // 카드 비주얼 오브젝트에 붙어있을 UI 컴포넌트를 가져와서 데이터 바인딩 연동
        var cardVisualScript = cardObj.GetComponent<Project_CardVisual>();
        if (cardVisualScript != null)
        {
            // 발급된 인스턴스 ID와 기획 데이터 ID, 그리고 실시간 인스턴스 모델 주입
            cardVisualScript.InitCardVisualInfo(_cardVisualKeyGenerator, cardModel.CardDataId, cardModel);
        }

        _spawnedCardVisuals.Add(_cardVisualKeyGenerator, cardObj);
    }

    private void ClearAllSpawnedVisuals()
    {
        foreach (var kv in _spawnedCardVisuals)
        {
            if (kv.Value != null)
            {
                Destroy(kv.Value);
            }
        }
        _spawnedCardVisuals.Clear();
        _cardVisualKeyGenerator = 0;
    }

}
