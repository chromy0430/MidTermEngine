using Unity.Entities;
using Unity.Mathematics;

public struct NavAgentComponent : IComponentData
{
    /*public Entity targetEntity;
    public bool pathCalculated;
    public int currentWaypoint;
    public float moveSpeed;
    public float nextPathCalculateTime;*/
    
    public Entity targetEntity;
    public float moveSpeed;
    public bool pathCalculated;
    public int currentWaypoint;
    public double nextPathCalculateTime;
}

public struct WaypointBuffer : IBufferElementData
{
    public float3 wayPoint;
}
