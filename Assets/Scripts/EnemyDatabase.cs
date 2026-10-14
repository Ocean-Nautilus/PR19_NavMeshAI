using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName="EnemyDatabase",menuName="PR19/Enemy Database",order=3)]
public class EnemyDatabase : ScriptableObject
{
    public List<EnemyData> allEnemies=new List<EnemyData>();
    public EnemyData GetEnemyByName(string name)=>allEnemies.Find(e=>e.enemyName==name);
    public List<EnemyData> GetEnemiesByCategory(EnemyCategory category)=>allEnemies.FindAll(e=>e.category==category);
    public EnemyData GetRandomEnemy()=>allEnemies.Count==0?null:allEnemies[Random.Range(0,allEnemies.Count)];
    public int GetTotalEnemiesCount()=>allEnemies.Count;
}
