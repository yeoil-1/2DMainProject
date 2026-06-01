using System;
using System.Collections.Generic;

[System.Serializable]
public class GameDataBase
{
    public string Id;
}

// C# 때와 약간 달라진 점
    // Syste.Text.Json대신 유니티 내장 JsonUtility를 사용
    // 따라서 프로퍼티말고 그냥 일반 public 멤버변수로 변경함
    // [System.Serializable]가 없다면 JsonUtility는 데이터를 무시

[System.Serializable]
public class DNCharacterData : GameDataBase
{
    public string Name;
    public string SkillList;
    public string UseWeaponId;
    public string BasicCostumeId;
}

[System.Serializable]
public class DNSkillData : GameDataBase
{
    public string Name;
    public string Description;
}

[System.Serializable]
public class DNWeaponData : GameDataBase
{
    public string Name;
    public string Description;
}

[System.Serializable] 
public class DNCostumeData : GameDataBase
{
    public string Name;
    public string Description;
}

[System.Serializable]
public class DNItemData : GameDataBase
{
    public string Name;
    public string Description;
    public string ItemType;
    public string Grade;
    public string MaxStackCount;
    public string SellingPrice;
    public string IconPath;
}

[System.Serializable]
public class DNDialogueGroupData : GameDataBase
{
    public List<string> DialogueIdList;
}

[System.Serializable]
public class DNDialogueData : GameDataBase
{
    public string CharacterDataId;
    public string Description;
    public string NextDialogueId;
    public List<string> SelectionNameList;
    public List<string> SelectionDialogueIdList;
    public string TexturePath;
    public string VoicePath;
}

[System.Serializable]
public class DNFieldObjectData : GameDataBase
{
    public string Name;
    public string Description;
    public string FieldObjectType;
    public List<int> DropCountRange;
    public string DropItemDataId;
    public string IconPath;
    public string PrefabPath;
}

[System.Serializable]
public class DNMonsterData : GameDataBase
{
    public string Name;
    public string Description;
    public string IconPath;
    public string PrefabPath;
}

[System.Serializable]
public class ProjectCardData: GameDataBase
{
    public string Name;
    public string Description;
    public int Cost;
    public string CardType;                // 공격, 스킬, 파워
    public List<int> EffectValueList;      
    public string Grade;                   // 시작, 일반, 고급, 희귀, 고대의존재, 멀티고급, 멀티희귀
    public int UpgradedCost;
    public List<int> UpgradedEffectValueList;
    public string TargetType;
    public string PrefabPath;
}

[System.Serializable]
public class ProjectMonsterData : GameDataBase
{
    public string Name;
    public string Description;
    public string EnemyType;
    public string SpawnZone;

    public int MinHp;
    public int MaxHp;
    public int UpgradedMinHp;
    public int UpgradedMaxHp;

    public List<int> EffectValueList;
    public List<int> UpgradedEffectValueList;
    public string PrefabPath;
}


[System.Serializable]
public class ProjectStatusData : GameDataBase
{
    public string Name;
    public string Description;
    public string EffectType;
    public string IconPath;
}

[System.Serializable]
public class RelicData
{
    public string Id;
    public string Name;
    public string Description;
    public string SpawnCharacter;
    public List<int> EffectValueList;
}

public class RuntimeRelicData
{
    public string Id;
    public string Name;
    public string Description;
    public string SpawnCharacter;
    public List<int> EffectValueList;

    public int Counter;       // 턴을 카운팅할 변수
    public bool IsActive;     // 아귀 저금통의 파괴 여부, 붉은 해골의 활성화 여부 제어

    public RuntimeRelicData(RelicData data)
    {
        this.Id = data.Id;
        this.Name = data.Name;
        this.Description = data.Description;
        this.SpawnCharacter = data.SpawnCharacter;
        this.EffectValueList = new List<int>(data.EffectValueList);

        this.Counter = 0;
        this.IsActive = true;
    }
}

