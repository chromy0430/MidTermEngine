using Unity.Entities;
using Unity.Mathematics;

// 플레이어 컴포넌트
public struct PlayerComponent : IComponentData
{
    public float MoveSpeed;
    public float RotationSpeed;
}

// 플레이어 입력 컴포넌트
public struct PlayerInputComponent : IComponentData
{
    public float2 MovementInput;
    public bool JumpRequested;
}
