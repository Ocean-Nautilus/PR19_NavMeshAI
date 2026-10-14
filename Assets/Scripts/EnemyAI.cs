using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public EnemyData enemyData;
    public NavMeshConfig config;
    public Transform player;
    public Transform[] patrolPoints;
    public bool showDebugInfo=true, showGizmos=true;
    NavMeshAgent agent;
    EnemyState currentState=EnemyState.Patrol;
    Vector3[] points;
    int currentPatrolIndex;
    float waitTimer, attackTimer, currentHealth, manualUntil;
    bool playerInSightRange, playerInAttackRange;
    float SpeedMultiplier=>config?config.globalSpeedMultiplier:1;
    void Awake() { agent=GetComponent<NavMeshAgent>(); if(!player) { var p=GameObject.FindGameObjectWithTag("Player"); if(p) player=p.transform; } }
    void Start()
    {
        if(!enemyData) { enabled=false; return; }
        agent.angularSpeed=enemyData.angularSpeed; agent.acceleration=enemyData.acceleration;
        agent.stoppingDistance=enemyData.stoppingDistance; agent.autoBraking=enemyData.autoBraking;
        currentHealth=enemyData.maxHealth;
        points=new Vector3[patrolPoints!=null && patrolPoints.Length>0?patrolPoints.Length:4];
        for(int i=0;i<points.Length;i++) {
            Vector3 p=patrolPoints!=null && patrolPoints.Length>0?patrolPoints[i].position:transform.position+new Vector3(Mathf.Cos(i*Mathf.PI/2),0,Mathf.Sin(i*Mathf.PI/2))*enemyData.patrolRange;
            points[i]=NavMesh.SamplePosition(p,out var hit,5,agent.areaMask)?hit.position:transform.position;
        }
        GoToNextPatrolPoint();
    }
    void Update()
    {
        if(!enemyData || !player || !agent.isOnNavMesh) return;
        playerInSightRange=Physics.CheckSphere(transform.position,enemyData.sightRange*(config?config.globalDetectionMultiplier:1),enemyData.playerLayer);
        playerInAttackRange=Physics.CheckSphere(transform.position,enemyData.attackRange,enemyData.playerLayer);
        attackTimer-=Time.deltaTime;
        bool auto=Time.time>=manualUntil;
        switch(currentState) {
            case EnemyState.Patrol:
                if(auto && playerInSightRange) { ChangeState(EnemyState.Chase); return; }
                if(!agent.pathPending && agent.remainingDistance<=Mathf.Max(.5f,agent.stoppingDistance)) { waitTimer+=Time.deltaTime; if(waitTimer>=enemyData.patrolWaitTime) GoToNextPatrolPoint(); }
                break;
            case EnemyState.Chase:
                if(auto && playerInAttackRange) { ChangeState(EnemyState.Attack); return; }
                if(auto && !playerInSightRange) { ChangeState(EnemyState.Patrol); return; }
                agent.speed=enemyData.runSpeed*SpeedMultiplier; agent.SetDestination(player.position); break;
            case EnemyState.Attack:
                Vector3 direction=player.position-transform.position; direction.y=0;
                if(direction.sqrMagnitude>.001f) transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(direction),Time.deltaTime*10);
                if(playerInAttackRange && attackTimer<=0) { Debug.Log($"[Атака] {enemyData.enemyName} атакует игрока! Урон: {enemyData.attackDamage*(config?config.globalAttackMultiplier:1)}"); attackTimer=enemyData.attackCooldown; }
                if(auto && !playerInAttackRange) ChangeState(EnemyState.Chase); break;
        }
    }
    void GoToNextPatrolPoint() { if(points==null || points.Length==0 || !agent.isOnNavMesh)return; agent.speed=enemyData.walkSpeed*SpeedMultiplier; agent.isStopped=false; agent.SetDestination(points[currentPatrolIndex]); currentPatrolIndex=(currentPatrolIndex+1)%points.Length; waitTimer=0; }
    void ChangeState(EnemyState state) { currentState=state; if(agent.isOnNavMesh) { agent.isStopped=state==EnemyState.Attack; if(state==EnemyState.Patrol)GoToNextPatrolPoint(); } if(showDebugInfo)Debug.Log($"{enemyData.enemyName}: {state}"); }
    public void CycleState() { manualUntil=Time.time+3; ChangeState((EnemyState)(((int)currentState+1)%3)); }
    public string GetAIInfo()=> $"<b>СОСТОЯНИЕ ВРАГА</b>\nВраг: {enemyData.enemyName}\nСостояние: {currentState}\nЗдоровье: {currentHealth}/{enemyData.maxHealth}\nПозиция: {transform.position:F1}\nСкорость: {agent.speed:F1}\nОсталось до цели: {(agent.isOnNavMesh?agent.remainingDistance:0):F2}\nИгрок в зоне обзора: {(playerInSightRange?"Да":"Нет")}\nИгрок в зоне атаки: {(playerInAttackRange?"Да":"Нет")}";
    void OnDrawGizmosSelected() { if(!showGizmos || !enemyData)return; Gizmos.color=Color.yellow; Gizmos.DrawWireSphere(transform.position,enemyData.sightRange); Gizmos.color=Color.red; Gizmos.DrawWireSphere(transform.position,enemyData.attackRange); Gizmos.color=Color.blue; Gizmos.DrawWireSphere(transform.position,enemyData.patrolRange); }
}
public enum EnemyState { Patrol, Chase, Attack }
