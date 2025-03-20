using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;

// 플레이어 입력 시스템
public partial class PlayerInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // 실제 게임에서는 UnityEngine.Input 대신 Unity의 새 Input System을 사용하는 것이 좋습니다
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool jump = Input.GetKeyDown(KeyCode.Space);

        // 모든 플레이어 입력 컴포넌트에 입력 값을 할당
        foreach (var inputComponent in 
                 SystemAPI.Query<RefRW<PlayerInputComponent>>())
        {
            inputComponent.ValueRW.MovementInput = new float2(horizontal, vertical);
            inputComponent.ValueRW.JumpRequested = jump;
        }
    }
}

// 플레이어 이동 시스템
[BurstCompile]
public partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerComponent>();
        state.RequireForUpdate<PlayerInputComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        // 플레이어 움직임 처리
        foreach (var (transform, playerComponent, inputComponent) in 
                 SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerComponent>, RefRO<PlayerInputComponent>>())
        {
            // 입력 값 가져오기
            float2 movementInput = inputComponent.ValueRO.MovementInput;
            
            // 입력이 있는 경우에만 이동 처리
            if (math.lengthsq(movementInput) > 0.001f)
            {
                // 이동 방향 계산
                float3 moveDirection = new float3(movementInput.x, 0, movementInput.y);
                
                // 실제 이동 적용
                transform.ValueRW.Position += moveDirection * playerComponent.ValueRO.MoveSpeed * deltaTime;
                
                // 회전 처리 (이동 방향을 바라보도록)
                if (math.lengthsq(moveDirection) > 0.001f)
                {
                    quaternion targetRotation = quaternion.LookRotation(moveDirection, math.up());
                    transform.ValueRW.Rotation = math.slerp(
                        transform.ValueRO.Rotation,
                        targetRotation,
                        playerComponent.ValueRO.RotationSpeed * deltaTime
                    );
                }
            }
        }
    }
}
