using UnityEngine;
[CreateAssetMenu(fileName="NavMeshConfig",menuName="PR19/NavMesh Config",order=2)]
public class NavMeshConfig : ScriptableObject
{
    public string configName="Основная конфигурация";
    public DifficultyLevel difficulty=DifficultyLevel.Normal;
    public float agentRadius=.5f, agentHeight=2, agentStepHeight=.4f, agentMaxSlope=45;
    public float cellSize=.2f, cellHeight=.1f;
    public int maxSlope=45, minRegionArea=2;
    public float globalSpeedMultiplier=1, globalDetectionMultiplier=1, globalAttackMultiplier=1;
    public string GetFullInfo()=> $"{configName} | {difficulty} | Радиус: {agentRadius} | Высота: {agentHeight} | Ячейка: {cellSize} | Скорость ×{globalSpeedMultiplier}";
    public void CycleDifficulty() { difficulty=(DifficultyLevel)(((int)difficulty+1)%4); float m=new[]{.5f,1,1.5f,2}[(int)difficulty]; globalSpeedMultiplier=globalDetectionMultiplier=globalAttackMultiplier=m; }
}
public enum DifficultyLevel { Easy, Normal, Hard, Nightmare }
