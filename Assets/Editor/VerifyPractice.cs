using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
public static class VerifyPractice
{
    static double start;
    static int stage;
    static NavMeshController controller;
    static EnemyAI enemy;
    public static void Run() { EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity"); EditorApplication.playModeStateChanged+=Changed; EditorApplication.isPaused=false; EditorApplication.isPlaying=true; }
    static void Changed(PlayModeStateChange state) { if(state!=PlayModeStateChange.EnteredPlayMode)return; start=Time.time;EditorApplication.update+=Tick; }
    static void Check(bool value,string message) { if(!value)throw new Exception(message); }
    static void Tick()
    {
        EditorApplication.isPaused=false; EditorApplication.QueuePlayerLoopUpdate(); if(Time.time-start<1)return;
        start=Time.time;
        try {
            if(stage==0) { controller=UnityEngine.Object.FindAnyObjectByType<NavMeshController>();Check(controller.enemiesContainer.childCount==4,"Enemy buttons");Check(NavMesh.CalculateTriangulation().indices.Length>0,"Baked NavMesh");controller.player.position=new Vector3(20,1,20);controller.SelectEnemy(controller.enemyDatabase.allEnemies[0]);controller.CreateRandomEnemy(); }
            if(stage==1) { enemy=UnityEngine.Object.FindAnyObjectByType<EnemyAI>();Check(enemy && enemy.GetComponent<NavMeshAgent>().isOnNavMesh,"Spawn");Check(enemy.GetAIInfo().Contains("Patrol"),"Patrol");controller.player.position=enemy.transform.position+new Vector3(10,1,0); }
            if(stage==2) { Check(enemy.GetAIInfo().Contains("Chase"),"Chase: "+enemy.GetAIInfo());controller.player.position=enemy.transform.position+new Vector3(.5f,1,0); }
            if(stage==3) { Check(enemy.GetAIInfo().Contains("Attack"),"Attack");controller.ToggleEnemyState();Check(enemy.GetAIInfo().Contains("Patrol"),"Manual state");controller.ToggleNavMeshDisplay();Check(GameObject.Find("NavMeshDisplay"),"NavMesh display");controller.ClearAllEnemies(); }
            if(stage==4) { Check(!UnityEngine.Object.FindAnyObjectByType<EnemyAI>(),"Clear");Debug.Log("PR19_TESTS_OK: buttons, bake, spawn, patrol, chase, attack, manual state, visualization, clear");EditorApplication.Exit(0); }
            stage++;
        }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
}




