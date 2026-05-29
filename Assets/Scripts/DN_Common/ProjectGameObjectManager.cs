using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class ProjectGameObjectManager : MonoBehaviour
{
    [SerializeField] private Transform Root_Enemy;

    public static DaniTechGameObjectManager Inst { get; set; }

    // 생성된 오브젝트의 키가 됨
    private int _objectInstanceKeyGenerator = 0;

    // 생성된 오브젝트의 생명을 보관 (기존 자료구조 완벽 유지)
    private Dictionary<int, GameObject> _createdGameObjectContainer = new Dictionary<int, GameObject>();
    private Dictionary<int, DaniTech_2DFieldObject> _fieldObjectContainer = new Dictionary<int, DaniTech_2DFieldObject>();

    private void Awake()
    {
        Inst = this;
    }

    /// <summary>
    /// [데이터 드리븐 완벽 교정] 몬스터 데이터 ID를 받아 어드레서블 비동기로 적을 동적 생성합니다.
    /// </summary>
    public async UniTaskVoid CreateEnemyObject(string monsterDataId, Transform spawnSpot)
    {
        // 1. [원본 활용] 데이터 매니저에서 기획자가 작성한 몬스터 테이블 데이터를 조회합니다.
        DNMonsterData monsterData = DaniTechGameDataManager.Instance.GetDNMonsterData(monsterDataId);
        if (monsterData == null)
        {
            Debug.LogError($"[GameObjectManager] {monsterDataId} 몬스터 스태틱 데이터가 존재하지 않습니다.");
            return;
        }

        // 2. [원본 활용] 리소스 매니저를 통해 엑셀에 적힌 PrefabPath 경로로 비동기 인스턴스화를 진행합니다.
        GameObject gObj = await DaniTechResourceManager.Inst.InstantiateAsync(monsterData.PrefabPath, Root_Enemy, true);
        if (gObj == null)
        {
            Debug.LogWarning($"[GameObjectManager] 프리팹 생성에 실패했습니다: {monsterData.PrefabPath}");
            return;
        }

        // 3. 위치 동적 세팅
        gObj.transform.position = spawnSpot.position;

        // 4. 고유 Key 발급 및 컨테이너 보관 (기존 검증 로직 구조 유지)
        _objectInstanceKeyGenerator++;
        int generatedInstanceId = _objectInstanceKeyGenerator;

        if (_createdGameObjectContainer.ContainsKey(generatedInstanceId) == true)
        {
            Debug.LogWarning("이미 동일한 키가 발급된 게임 오브젝트가 존재합니다");
            Destroy(gObj);
            return;
        }

        _createdGameObjectContainer.Add(generatedInstanceId, gObj);

        // 5. 몬스터 정보 초기화 함수로 위임
        InitGeneratedEntityObject(generatedInstanceId, monsterDataId, gObj);

        Debug.Log($"키: {generatedInstanceId}의 몬스터 {monsterData.Name}이 데이터 기반으로 동적 생성되었습니다.");
    }

    /// <summary>
    /// 생성된 적 오브젝트에 실시간 인스턴스 ID와 스태틱 ID를 주입하여 데이터 드리븐 초기화를 수행합니다.
    /// </summary>
    private void InitGeneratedEntityObject(int generatedId, string monsterDataId, GameObject gObj)
    {
        DaniTech_2DEnemy gameEntity = gObj.GetComponent<DaniTech_2DEnemy>();
        if (gameEntity == null)
        {
            Debug.LogWarning($"생성된 {gObj.name}의 InstanceId를 대입할 수 있는 컴포넌트를 가져올 수 없습니다!");
            return;
        }

        // [데이터 드리븐 교정] 몬스터 컴포넌트가 스태틱 테이블 정보를 참조할 수 있도록 데이터를 함께 주입합니다.
        gameEntity.InitEnemyInfo(generatedId, monsterDataId);
    }

    public GameObject GetEntityObjectCanBeNull(int instanceId)
    {
        if (_createdGameObjectContainer.ContainsKey(instanceId) == false)
        {
            Debug.LogWarning($"{instanceId}는 존재하지 않습니다.");
            return null;
        }

        return _createdGameObjectContainer[instanceId];
    }

    public void RequestDestroyEntityObject(int instanceId)
    {
        var gObj = GetEntityObjectCanBeNull(instanceId);
        if (gObj == null)
        {
            return;
        }

        _createdGameObjectContainer.Remove(instanceId);
        Destroy(gObj);
    }


    //[필드 오브젝트] ====================================================================================================

    public async UniTaskVoid CreateFieldObject(string fieldObjectDataId, Transform spawnSpot)
    {
        var fieldObject = DaniTechGameDataManager.Instance.GetDNFieldObjectData(fieldObjectDataId);
        if (fieldObject != null)
        {
            var createdObj = await DaniTechResourceManager.Inst.InstantiateAsync(fieldObject.PrefabPath, Root_Enemy, true);
            createdObj.transform.position = spawnSpot.position;
            AddFieldObjectOnCreate(createdObj, fieldObjectDataId);
        }
    }

    private void AddFieldObjectOnCreate(GameObject createdObject, string fieldObjectDataId)
    {
        _objectInstanceKeyGenerator++;
        var generatedInstanceId = _objectInstanceKeyGenerator;
        var fieldObject = createdObject.GetComponent<DaniTech_2DFieldObject>();

        if (fieldObject != null)
        {
            _fieldObjectContainer.Add(generatedInstanceId, fieldObject);
            fieldObject.InitFieldObjectInfoOnCreated(generatedInstanceId, fieldObjectDataId);
        }
    }

    public void RequestDestroyFieldObject(int instanceId)
    {
        var fieldObjectComponent = GetFieldObjectByInstanceId(instanceId);
        if (fieldObjectComponent == null)
        {
            return;
        }

        _fieldObjectContainer.Remove(instanceId);
        Destroy(fieldObjectComponent.gameObject);
    }

    public DaniTech_2DFieldObject GetFieldObjectByInstanceId(int fieldObjectInstanceId)
    {
        if (_fieldObjectContainer.ContainsKey(fieldObjectInstanceId) == false)
        {
            Debug.LogError($"{fieldObjectInstanceId} 찾으려는 필드 오브젝트가 유효하지 않습니다");
            return null;
        }

        return _fieldObjectContainer[fieldObjectInstanceId];
    }
}
