using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ProjectStatusEffectType
{
    None = 0,

    // [버프계열]
    AttackBuff,     // 공격력 증가
    ShieldBuff,     // 매 턴 방어도 생성

    // [디버프계열]
    Poison,         // 턴 시작 시 대미지
    Weaken          // 공격력 감소
}

// 실시간으로 유저에게 적용되는 버프/디버프 인스턴스 모델
public class ProjectStatusEffectInstance
{
    public ProjectStatusEffectType EffectType { get; set; }
    public int StackCount { get; set; } // 중첩 수 또는 지속 시간
}


public class Project_BattlePlayer : MonoBehaviour
{
    [SerializeField] private Text Text_CharacterName;
    [SerializeField] private Text Text_HpDisplay;
    [SerializeField] private Text Text_ManaDisplay;
    [SerializeField] private Text Text_StatusEffectDisplay; // 버프/디버프 통합 출력 UI

    // [기수 규칙] 멤버 변수는 _aaa 이고 소문자로 시작
    private int _currentHp;
    private int _maxHp;
    private int _currentMana;

    private int _instanceId;

    // 기획 스태틱 데이터 참조
    private ProjectCharacterData _characterStaticData;

    // 현재 플레이어에게 적용된 모든 상태 효과(버프/디버프) 컨테이너
    private Dictionary<ProjectStatusEffectType, ProjectStatusEffectInstance> _activeEffects =
        new Dictionary<ProjectStatusEffectType, ProjectStatusEffectInstance>();

    // [기수 규칙] 외부 객체가 참조할 수 있도록 프로퍼티로 개방
    public int CurrentHp
    {
        get => _currentHp;
        private set => _currentHp = value;
    }

    public int InstanceId
    {
        get => _instanceId;
        private set => _instanceId = value;
    }

    // [기수 규칙] 함수는 동사로 시작할 것
    public void InitBattlePlayer(string characterDataId, int maxHp)
    {
        _characterStaticData = DaniTechGameDataManager.Instance.GetProjectCharacterData(characterDataId);

        if (_characterStaticData == null)
        {
            Debug.LogError($"[Error] {characterDataId} 데이터를 찾을 수 없습니다.");
            return;
        }

        _maxHp = maxHp;
        CurrentHp = maxHp;
        _currentMana = 3;

        _activeEffects.Clear();

        RefreshPlayerUI();
    }

    public void InitPlayerInstanceInfo(int generatedId)
    {
        InstanceId = generatedId;
    }

    public void TakeDamage(int damage)
    {
        CurrentHp -= damage;
        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            Die();
        }

        RefreshPlayerUI();
    }

    // 외부(카드 사용, 몬스터 공격 등)에서 플레이어에게 버프 또는 디버프를 부여할 때 호출
    public void AddStatusEffect(ProjectStatusEffectType type, int stack)
    {
        if (_activeEffects.ContainsKey(type))
        {
            _activeEffects[type].StackCount += stack;
        }
        else
        {
            var newEffect = new ProjectStatusEffectInstance { EffectType = type, StackCount = stack };
            _activeEffects.Add(type, newEffect);
        }

        Debug.Log($"플레이어에게 효과 {type} 적용됨 (현재 스택: {_activeEffects[type].StackCount})");
        RefreshPlayerUI();
    }

    // BattleManager가 플레이어 턴 시작을 알릴 때 호출 (예: 독 대미지, 실드 획득 등)
    public void ProcessTurnStartEffects()
    {
        // 1. [디버프] 독 처리
        if (_activeEffects.TryGetValue(ProjectStatusEffectType.Poison, out var poisonEffect))
        {
            if (poisonEffect.StackCount > 0)
            {
                Debug.Log($"독 발동! 대미지: {poisonEffect.StackCount}");
                TakeDamage(poisonEffect.StackCount);

                poisonEffect.StackCount--; // 턴 경과로 스택 감소
                CheckAndRemoveExpiredEffect(ProjectStatusEffectType.Poison);
            }
        }

        // 2. [버프] 매 턴 실드 생성 버프 처리
        if (_activeEffects.TryGetValue(ProjectStatusEffectType.ShieldBuff, out var shieldEffect))
        {
            if (shieldEffect.StackCount > 0)
            {
                Debug.Log($"실드 버프 발동! 방어도 {shieldEffect.StackCount} 획득");
                // TODO: 방어도 인스턴스 변수가 추가된다면 여기에 누적 연산 진행
            }
        }

        RefreshPlayerUI();
    }

    // BattleManager가 플레이어 턴 종료를 알릴 때 호출 (예: 버프/디버프 지속시간 감소)
    public void ProcessTurnEndEffects()
    {
        // 지속시간형 효과(약화, 공격력 버프 등)의 턴 소모 연산
        ProjectStatusEffectType[] effectsToTick = {
            ProjectStatusEffectType.Weaken,
            ProjectStatusEffectType.AttackBuff
        };

        foreach (var effectType in effectsToTick)
        {
            if (_activeEffects.TryGetValue(effectType, out var effect))
            {
                effect.StackCount--;
                CheckAndRemoveExpiredEffect(effectType);
            }
        }

        RefreshPlayerUI();
    }

    // 필요한 경우 외부에서 특정 효과의 수치를 안전하게 받아갈 수 있도록 Get 함수 제공 원칙
    public int GetStatusEffectStackCount(ProjectStatusEffectType type)
    {
        if (_activeEffects.TryGetValue(type, out var effect))
        {
            return effect.StackCount;
        }
        return 0;
    }

    // [기수 규칙] 클래스 내부에서만 사용하는 헬퍼 메서드는 private 원칙
    private void CheckAndRemoveExpiredEffect(ProjectStatusEffectType type)
    {
        if (_activeEffects.TryGetValue(type, out var effect))
        {
            if (effect.StackCount <= 0)
            {
                _activeEffects.Remove(type);
            }
        }
    }

    private void RefreshPlayerUI()
    {
        if (_characterStaticData != null && Text_CharacterName != null)
        {
            Text_CharacterName.text = _characterStaticData.Name;
        }

        if (Text_HpDisplay != null)
        {
            Text_HpDisplay.text = $"HP: {CurrentHp} / {_maxHp}";
        }

        if (Text_ManaDisplay != null)
        {
            Text_ManaDisplay.text = $"MANA: {_currentMana}";
        }

        // 버프와 디버프의 현재 상태를 문자열로 예쁘게 결합하여 출력
        if (Text_StatusEffectDisplay != null)
        {
            string effectText = "";
            foreach (var kv in _activeEffects)
            {
                effectText += $"[{kv.Key}({kv.Value.StackCount})] ";
            }
            Text_StatusEffectDisplay.text = string.IsNullOrEmpty(effectText) ? "상태: 정상" : effectText;
        }
    }

    private void Die()
    {
        Debug.Log("플레이어가 쓰러졌습니다.");
    }
}
