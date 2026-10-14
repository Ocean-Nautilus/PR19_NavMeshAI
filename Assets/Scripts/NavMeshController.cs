using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.UI;
using TMPro;
public class NavMeshController : MonoBehaviour
{
    public TextMeshProUGUI statusText, enemyInfoText, configText;
    public Transform enemiesContainer;
    public Button createEnemyButton, toggleNavMeshButton, toggleStateButton, clearButton;
    public GameObject enemyButtonPrefab;
    public EnemyDatabase enemyDatabase;
    public NavMeshConfig navMeshConfig;
    public EnemyFactory enemyFactory;
    public Transform player;
    public NavMeshSurface navMeshSurface;
    public float spawnRadius=5;
    readonly List<GameObject> spawnedEnemies=new List<GameObject>();
    EnemyData selectedEnemy;
    GameObject navMeshDisplay;
    void Start()
    {
        createEnemyButton.onClick.AddListener(CreateRandomEnemy); toggleNavMeshButton.onClick.AddListener(ToggleNavMeshDisplay);
        toggleStateButton.onClick.AddListener(ToggleEnemyState); clearButton.onClick.AddListener(ClearAllEnemies);
        if (!enemyDatabase || enemyDatabase.allEnemies == null || !enemyButtonPrefab || !enemiesContainer)
        {
            Debug.LogError("NavMeshController: назначьте EnemyDatabase, EnemyButtonPrefab и EnemiesContainer в Inspector.", this);
            enabled = false;
            return;
        }
        if (!enemyButtonPrefab.GetComponent<Button>() || !enemyButtonPrefab.GetComponentInChildren<TextMeshProUGUI>(true))
        {
            Debug.LogError("EnemyButtonPrefab должен содержать Button и дочерний TextMeshProUGUI.", enemyButtonPrefab);
            enabled = false;
            return;
        }
        foreach (var enemy in enemyDatabase.allEnemies)
        {
            if (!enemy)
            {
                Debug.LogWarning("В EnemyDatabase найдена пустая ссылка на врага. Проверьте AllEnemies.", enemyDatabase);
                continue;
            }
            var obj = Instantiate(enemyButtonPrefab, enemiesContainer);
            obj.GetComponentInChildren<TextMeshProUGUI>(true).text = enemy.enemyName;
            var captured = enemy;
            obj.GetComponent<Button>().onClick.AddListener(() => SelectEnemy(captured));
        }
        statusText.text="Выберите врага для создания"; configText.text=navMeshConfig.GetFullInfo();
        var tri=NavMesh.CalculateTriangulation(); var mesh=new Mesh(); mesh.vertices=tri.vertices; mesh.triangles=tri.indices; mesh.RecalculateNormals();
        navMeshDisplay=new GameObject("NavMeshDisplay",typeof(MeshFilter),typeof(MeshRenderer)); navMeshDisplay.layer=9;
        navMeshDisplay.transform.position=Vector3.up*.03f; navMeshDisplay.GetComponent<MeshFilter>().sharedMesh=mesh;
        var material=new Material(Shader.Find("Universal Render Pipeline/Unlit")); material.color=new Color(.1f,.5f,.8f); navMeshDisplay.GetComponent<Renderer>().sharedMaterial=material;
        navMeshDisplay.SetActive(false);
    }
    public void SelectEnemy(EnemyData enemy) { selectedEnemy=enemy; enemyInfoText.text=enemy.GetFullInfo(); statusText.text=$"Выбран: {enemy.enemyName}"; }
    public void CreateRandomEnemy()
    {
        var data=selectedEnemy?selectedEnemy:enemyDatabase.GetRandomEnemy();
        var p=new Vector3(Random.Range(-spawnRadius,spawnRadius),0,Random.Range(-spawnRadius,spawnRadius));
        var obj=enemyFactory.CreateEnemy(data,p); if(!obj) { statusText.text="Не найдена точка на NavMesh"; return; }
        obj.GetComponent<EnemyAI>().player=player; spawnedEnemies.Add(obj); selectedEnemy=data; statusText.text=$"Создан враг: {data.enemyName}. Всего: {spawnedEnemies.Count}";
    }
    public void ToggleNavMeshDisplay() { navMeshDisplay.SetActive(!navMeshDisplay.activeSelf); statusText.text=$"NavMesh: {(navMeshDisplay.activeSelf?"Показан":"Скрыт")}"; }
    public void ToggleEnemyState() { foreach(var obj in spawnedEnemies)if(obj)obj.GetComponent<EnemyAI>().CycleState(); statusText.text="Состояние врагов переключено"; }
    public void ClearAllEnemies() { foreach(var obj in spawnedEnemies)if(obj)Destroy(obj); spawnedEnemies.Clear(); selectedEnemy=null; enemyInfoText.text="Информация о враге"; statusText.text="Всё очищено. Выберите врага"; }
    void Update()
    {
        if(selectedEnemy) foreach(var obj in spawnedEnemies)if(obj && obj.GetComponent<EnemyAI>().enemyData==selectedEnemy) { enemyInfoText.text=obj.GetComponent<EnemyAI>().GetAIInfo(); break; }
        if(Input.GetKeyDown(KeyCode.Space))CreateRandomEnemy(); if(Input.GetKeyDown(KeyCode.N))ToggleNavMeshDisplay(); if(Input.GetKeyDown(KeyCode.T))ToggleEnemyState(); if(Input.GetKeyDown(KeyCode.Delete))ClearAllEnemies();
    }
    void OnDestroy() { if(navMeshDisplay)Destroy(navMeshDisplay); }
}
