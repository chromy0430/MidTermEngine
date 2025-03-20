using UnityEngine;
using TMPro;
using Unity.Entities;

public class SpawnerUIBridge : MonoBehaviour
{
    public TextMeshProUGUI spawnCountText;
    private Entity spawnerEntity; // public 제거, 런타임에 설정
    private EntityManager entityManager;
    private World subsceneWorld;
    private int lastSpawnCount = 0;

    void Start()
    {
        // 서브씬 월드 찾기
        foreach (var world in World.All)
        {
            if (world.IsCreated && world.Name.Contains("Sub Scene"))
            {
                subsceneWorld = world;
                break;
            }
        }

        if (subsceneWorld != null)
        {
            entityManager = subsceneWorld.EntityManager;
            Debug.Log($"서브씬 월드 발견: {subsceneWorld.Name}");

            // Spawner 컴포넌트를 가진 엔티티 찾기
            var entities = entityManager.CreateEntityQuery(typeof(Spawner)).ToEntityArray(Unity.Collections.Allocator.Temp);
            if (entities.Length > 0)
            {
                spawnerEntity = entities[0]; // 첫 번째 Spawner 엔티티 사용
                Debug.Log($"Spawner 엔티티 발견: {spawnerEntity}, Exists: {entityManager.Exists(spawnerEntity)}");
            }
            else
            {
                Debug.LogError("Spawner 컴포넌트를 가진 엔티티를 찾을 수 없습니다.");
            }
            entities.Dispose();
        }
        else
        {
            Debug.LogError("서브씬 월드를 찾을 수 없습니다.");
        }

        UpdateCountText(0);
    }

    void Update()
    {
        if (entityManager.Exists(spawnerEntity))
        {
            var spawner = entityManager.GetComponentData<Spawner>(spawnerEntity);
            Debug.Log($"현재 Spawner a 값: {spawner.a}, lastSpawnCount: {lastSpawnCount}");
            if (spawner.a != lastSpawnCount)
            {
                lastSpawnCount = spawner.a++;
                UpdateCountText(lastSpawnCount);
            }
        }
        else
        {
            Debug.LogError($"spawnerEntity가 유효하지 않습니다. 값: {spawnerEntity}");
        }
    }

    private void UpdateCountText(int count)
    {
        if (spawnCountText != null)
        {
            spawnCountText.text = $"SpawnNum: {count}";
        }
    }
}