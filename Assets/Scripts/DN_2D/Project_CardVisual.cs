using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class Project_CardVisual : MonoBehaviour
{
    [SerializeField] private Text Text_CardName;
    [SerializeField] private Text Text_Description;
    [SerializeField] private Text Text_Cost;
    [SerializeField] private Text Text_CardType;
    [SerializeField] private Text Text_Grade;
    [SerializeField] private Image Image_CardIcon; // 추후 비동기 스프라이트 로드용 칸
    [SerializeField] private DaniTechUIButton Button_CardSelect; // 카드 선택용 래핑 버튼

    private int _visualInstanceId;
    private ProjectCardData _cardStaticData;
    private ProjectCardModel _cardInstanceModel;

    public int VisualInstanceId => _visualInstanceId;

    private void OnEnable()
    {
        Button_CardSelect.BindOnClickButtonEvent(OnClick_UseCardRequest);
    }

    // 카드 매니저가 드로우하면서 스폰할 때 데이터를 채워주기 위해 호출하는 함수
    public void InitCardVisualInfo(int generatedInstanceId, string cardDataId, ProjectCardModel model)
    {
        _visualInstanceId = generatedInstanceId;
        _cardInstanceModel = model;

        _cardStaticData = DaniTechGameDataManager.Instance.GetProjectCardData(cardDataId);

        if (_cardStaticData == null)
        {
            Debug.LogWarning($"[CardVisual] {cardDataId}번에 매핑되는 카드의 Static 기획 데이터를 찾을 수 없습니다.");
            return;
        }

        RefreshCardUI();
    }

    public void OnClick_UseCardRequest()
    {
        if (_cardStaticData == null) return;
        Debug.Log($"[{_cardStaticData.Name}] 카드 클릭됨! 비용: {_cardStaticData.Cost}, 타겟: {_cardStaticData.TargetType}");

        // TODO: 에너지 확인

        ProjectCardManager.Inst.RemoveCardFromHandToGrave(_visualInstanceId, _cardInstanceModel);
    }

    private void RefreshCardUI()
    {
        if (_cardStaticData == null) return;

        if (Text_CardName != null) Text_CardName.text = _cardStaticData.Name;
        if (Text_Description != null) Text_Description.text = _cardStaticData.Description;
        if (Text_Cost != null) Text_Cost.text = _cardStaticData.Cost.ToString();
        if (Text_CardType != null) Text_CardType.text = $"[{_cardStaticData.CardType}]";
        if (Text_Grade != null) Text_Grade.text = _cardStaticData.Grade;

        if (Image_CardIcon != null && string.IsNullOrEmpty(_cardStaticData.PrefabPath) == false)
        {
            DaniTechGameUtil.LoadAndSetSpriteImage(Image_CardIcon, _cardStaticData.PrefabPath).Forget();
        }
        else
        {
            Debug.LogWarning($"[{_cardStaticData.Name}] 카드의 Image 컴포넌트가 누락되었거나 이미지 경로가 비어있습니다.");
        }
    }
}
