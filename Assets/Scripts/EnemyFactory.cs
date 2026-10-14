using UnityEngine;
using UnityEngine.AI;
public class EnemyFactory : MonoBehaviour
{
    public EnemyDatabase enemyDatabase;
    public NavMeshConfig navMeshConfig;
    public GameObject CreateEnemy(EnemyData data,Vector3 position)
    {
        if(!data || !NavMesh.SamplePosition(position,out var hit,5,NavMesh.AllAreas))return null;
        var obj=GameObject.CreatePrimitive(PrimitiveType.Capsule); obj.name=$"Enemy_{data.enemyName}";
        obj.transform.position=hit.position; obj.tag="Enemy"; obj.layer=9;
        var collider=obj.GetComponent<CapsuleCollider>(); collider.center=Vector3.up; collider.radius=.4f;
        // Capsule pivot is at its centre; move the mesh so the agent pivot stays on the floor.
        obj.GetComponent<MeshFilter>().mesh=OffsetCapsule(obj.GetComponent<MeshFilter>().sharedMesh);
        var rb=obj.AddComponent<Rigidbody>(); rb.isKinematic=true;
        var agent=obj.AddComponent<NavMeshAgent>(); agent.radius=navMeshConfig.agentRadius; agent.height=navMeshConfig.agentHeight; agent.baseOffset=0;
        var ai=obj.AddComponent<EnemyAI>(); ai.enemyData=data; ai.config=navMeshConfig;
        obj.GetComponent<Renderer>().material.color=new[]{Color.red,Color.blue,Color.green,Color.yellow}[(int)data.category];
        return obj;
    }
    static Mesh OffsetCapsule(Mesh source) { var mesh=Instantiate(source); var vertices=mesh.vertices; for(int i=0;i<vertices.Length;i++)vertices[i]+=Vector3.up; mesh.vertices=vertices; mesh.RecalculateBounds(); return mesh; }
    public void ApplyConfigToEnemy(NavMeshAgent agent) { if(agent && navMeshConfig) agent.speed*=navMeshConfig.globalSpeedMultiplier; }
}
