using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class ProjectGameObjectManager : MonoBehaviour
{
    [SerializeField] private Transform Root_Enemy;

    public static ProjectGameObjectManager Inst { get; set; }

    private int _objectInstanceKeyGenerator = 0;

    private Dictionary<int, GameObject> _createdGameObjectContainer = new Dictionary<int, GameObject>();
    private Dictionary<int, DaniTech_2DFieldObject> _fieldObjectContainer = new Dictionary<int, DaniTech_2DFieldObject>();

    private void Awake()
    {
        Inst = this;
    }


    public async UniTaskVoid CreateEnemyObject(string monsterDataId, Transform spawnSpot)
    {
        ProjectMonsterData monsterData = DaniTechGameDataManager.Instance.GetProjectMonsterData(monsterDataId);
        if (monsterData == null)
        {
            Debug.LogError($"[GameObjectManager] {monsterDataId} 몬스터 스태틱 데이터가 존재하지 않습니다.");
            return;
        }

        GameObject gObj = await DaniTechResourceManager.Inst.InstantiateAsync(monsterData.PrefabPath, Root_Enemy, true);
        if (gObj == null)
        {
            Debug.LogWarning($"[GameObjectManager] 프리팹 생성에 실패했습니다: {monsterData.PrefabPath}");
            return;
        }

        gObj.transform.position = spawnSpot.position;

        _objectInstanceKeyGenerator++;
        int generatedInstanceId = _objectInstanceKeyGenerator;

        if (_createdGameObjectContainer.ContainsKey(generatedInstanceId) == true)
        {
            Debug.LogWarning("이미 동일한 키가 발급된 게임 오브젝트가 존재합니다");
            Destroy(gObj);
            return;
        }

        _createdGameObjectContainer.Add(generatedInstanceId, gObj);

        InitGeneratedEntityObject(generatedInstanceId, monsterDataId, gObj);

        Debug.Log($"키: {generatedInstanceId}의 몬스터 {monsterData.Name}이 데이터 기반으로 동적 생성되었습니다.");
    }


    private void InitGeneratedEntityObject(int generatedId, string monsterDataId, GameObject gObj)
    {
        Project_2DEnemy gameEntity = gObj.GetComponent<Project_2DEnemy>();
        if (gameEntity == null)
        {
            Debug.LogWarning($"생성된 {gObj.name}의 InstanceId를 대입할 수 있는 컴포넌트를 가져올 수 없습니다!");
            return;
        }

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
