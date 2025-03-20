using UnityEngine;
using UnityEngine.AI;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;    // NavMeshAgent�� ���Ե� �� ������
    [SerializeField] private Transform player;          // �÷��̾� transform
    [SerializeField] private float moveSpeed = 5f;      // �̵� �ӵ� (NavMeshAgent�� �ݿ�)

    private GameObject[] enemyInstances;
    private NavMeshAgent[] enemyAgents;
    private NativeArray<Vector3> enemyPositions;        // �� ��ġ
    private NativeArray<Vector3> targetDirections;      // Job���� ���� ����
    private const int ENEMY_COUNT = 200;

    void Start()
    {
        enemyInstances = new GameObject[ENEMY_COUNT];
        enemyAgents = new NavMeshAgent[ENEMY_COUNT];
        enemyPositions = new NativeArray<Vector3>(ENEMY_COUNT, Allocator.Persistent);
        targetDirections = new NativeArray<Vector3>(ENEMY_COUNT, Allocator.Persistent);

        // �� ��ȯ
        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(-50f, 50f),
                0f,
                Random.Range(-50f, 50f)
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPosition, out hit, 10f, NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
            }

            enemyInstances[i] = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemyAgents[i] = enemyInstances[i].GetComponent<NavMeshAgent>();
            enemyAgents[i].speed = moveSpeed;
            enemyPositions[i] = spawnPosition;
        }
    }

    void Update()
    {
        // �� ��ġ ������Ʈ
        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            enemyPositions[i] = enemyInstances[i].transform.position;
        }

        // Job ����: �÷��̾ ���� ���� ���
        ChaseDirectionJob chaseJob = new ChaseDirectionJob
        {
            playerPosition = player.position,
            enemyPositions = enemyPositions,
            targetDirections = targetDirections
        };

        JobHandle jobHandle = chaseJob.Schedule(ENEMY_COUNT, 64);
        jobHandle.Complete();

        // NavMeshAgent�� ��� ����
        for (int i = 0; i < ENEMY_COUNT; i++)
        {
            Vector3 targetPosition = enemyPositions[i] + targetDirections[i];
            enemyAgents[i].SetDestination(targetPosition);
        }
    }

    void OnDestroy()
    {
        enemyPositions.Dispose();
        targetDirections.Dispose();
    }
}

[BurstCompile]
public struct ChaseDirectionJob : IJobParallelFor
{
    [ReadOnly] public Vector3 playerPosition;
    [ReadOnly] public NativeArray<Vector3> enemyPositions;
    [WriteOnly] public NativeArray<Vector3> targetDirections;

    public void Execute(int index)
    {
        // �÷��̾ ���� ���� ���
        Vector3 direction = (playerPosition - enemyPositions[index]).normalized;
        targetDirections[index] = direction * 5f; // �̵� �Ÿ� ���� (���� ��)
    }
}