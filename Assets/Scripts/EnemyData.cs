using UnityEngine;
[CreateAssetMenu(fileName="EnemyData", menuName="PR19/Enemy Data", order=1)]
public class EnemyData : ScriptableObject
{
    public string enemyName="Новый враг";
    [TextArea(3,5)] public string description="Описание врага";
    public float walkSpeed=2, runSpeed=6, angularSpeed=120, acceleration=8, stoppingDistance=.5f;
    public bool autoBraking=false;
    public float sightRange=15, attackRange=2;
    public LayerMask playerLayer, groundLayer;
    public float patrolRange=10, patrolWaitTime=2, attackCooldown=1.5f, attackDamage=10, maxHealth=100;
    public GameObject projectilePrefab;
    public Transform attackPoint;
    public EnemyCategory category;
    public EnemyRarity rarity;
    public string GetFullInfo() => $"<b>{enemyName}</b>\n<i>{description}</i>\nКатегория: {category}\nРедкость: {rarity}\nСкорость патруля: {walkSpeed:F1}\nСкорость погони: {runSpeed:F1}\nРадиус обзора: {sightRange:F1}\nДистанция атаки: {attackRange:F1}\nУрон: {attackDamage:F0}\nЗдоровье: {maxHealth:F0}";
}
public enum EnemyCategory { Melee, Ranged, Tank, Scout }
public enum EnemyRarity { Common, Rare, Epic, Legendary }
