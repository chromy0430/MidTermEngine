using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

// 플레이어 오써링 컴포넌트
public class PlayerAuthoring : MonoBehaviour
{
    public float MoveSpeed = 5f;
    public float RotationSpeed = 10f;
}

// 플레이어 컴포넌트 베이크 시스템
public class PlayerBaker : Baker<PlayerAuthoring>
{
    public override void Bake(PlayerAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        
        AddComponent(entity, new PlayerComponent
        {
            MoveSpeed = authoring.MoveSpeed,
            RotationSpeed = authoring.RotationSpeed
        });
        
        AddComponent(entity, new PlayerInputComponent
        {
            MovementInput = float2.zero,
            JumpRequested = false
        });
    }
}
