using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.AI.Navigation;
using TMPro;
public static class BuildPractice
{
    static TMP_FontAsset font;
    [MenuItem("PR19/Создать сцену по инструкции")]
    public static void Create()
    {
        foreach(var p in new[]{"Scenes","Prefabs","ScriptableObjects/Enemies","ScriptableObjects/Configs","Fonts"})Directory.CreateDirectory("Assets/"+p);
        AssetDatabase.Refresh();
        if(!File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset")) AssetDatabase.ImportPackage("Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage",false);
        var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var list=tags.FindProperty("tags"); bool found=false; for(int i=0;i<list.arraySize;i++)if(list.GetArrayElementAtIndex(i).stringValue=="Enemy")found=true;
        if(!found){list.InsertArrayElementAtIndex(list.arraySize);list.GetArrayElementAtIndex(list.arraySize-1).stringValue="Enemy";}
        tags.FindProperty("layers").GetArrayElementAtIndex(8).stringValue="Player"; tags.FindProperty("layers").GetArrayElementAtIndex(9).stringValue="Enemy"; tags.ApplyModifiedProperties();
        PlayerSettings.defaultScreenWidth=1920; PlayerSettings.defaultScreenHeight=1080;
        var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]); var input=settings.FindProperty("activeInputHandler"); if(input!=null){input.intValue=0;settings.ApplyModifiedProperties();}
        File.Copy("C:/Windows/Fonts/arial.ttf","Assets/Fonts/Arial.ttf",true); AssetDatabase.Refresh();
        font=TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Arial.ttf")); font.atlasPopulationMode=AtlasPopulationMode.Dynamic;
        AssetDatabase.CreateAsset(font,"Assets/Fonts/Arial SDF.asset"); AssetDatabase.AddObjectToAsset(font.material,font); foreach(var t in font.atlasTextures)AssetDatabase.AddObjectToAsset(t,font);
        var config=ScriptableObject.CreateInstance<NavMeshConfig>(); AssetDatabase.CreateAsset(config,"Assets/ScriptableObjects/Configs/MainNavMeshConfig.asset");
        var db=ScriptableObject.CreateInstance<EnemyDatabase>();
        string[] names={"Воин ближнего боя","Стрелок","Тяжёлый танк","Разведчик"}, ids={"MeleeEnemy","RangedEnemy","TankEnemy","ScoutEnemy"}, descriptions={"Агрессивный враг, атакующий в ближнем бою.","Атакует на расстоянии.","Медленный, но очень живучий.","Быстрый и юркий."};
        float[] walk={2,1.5f,1,3},run={5,4,2.5f,8},sight={12,20,10,18},attack={2,10,3,1.5f},damage={15,10,25,5},health={100,100,300,50};
        for(int i=0;i<4;i++){var d=ScriptableObject.CreateInstance<EnemyData>();d.enemyName=names[i];d.description=descriptions[i];d.walkSpeed=walk[i];d.runSpeed=run[i];d.sightRange=sight[i];d.attackRange=attack[i];d.attackDamage=damage[i];d.maxHealth=health[i];d.category=(EnemyCategory)i;d.rarity=i==1?EnemyRarity.Rare:i==2?EnemyRarity.Epic:EnemyRarity.Common;d.patrolRange=i==0?8:10;d.playerLayer=1<<8;d.groundLayer=1;AssetDatabase.CreateAsset(d,$"Assets/ScriptableObjects/Enemies/{ids[i]}.asset");db.allEnemies.Add(d);}
        AssetDatabase.CreateAsset(db,"Assets/ScriptableObjects/MainEnemyDatabase.asset");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);ground.name="Ground";ground.transform.localScale=new Vector3(5,1,5);
        Vector3[] obstacles={new Vector3(5,.5f,5),new Vector3(-5,.5f,-5),new Vector3(0,.5f,-8)};
        for(int i=0;i<3;i++){var c=GameObject.CreatePrimitive(PrimitiveType.Cube);c.name="Obstacle"+(i+1);c.transform.position=obstacles[i];c.transform.localScale=i==2?new Vector3(3,1,3):new Vector3(2,1,2);}
        var surface=ground.AddComponent<NavMeshSurface>();surface.layerMask=1;surface.useGeometry=NavMeshCollectGeometry.RenderMeshes;surface.overrideVoxelSize=true;surface.voxelSize=.2f;
        surface.BuildNavMesh();
        AssetDatabase.CreateAsset(surface.navMeshData,"Assets/Scenes/MainScene-NavMesh.asset");
        var player=GameObject.CreatePrimitive(PrimitiveType.Capsule);player.name="Player";player.tag="Player";player.layer=8;player.transform.position=Vector3.up;player.GetComponent<Renderer>().sharedMaterial=Material("Player",Color.cyan);player.AddComponent<Rigidbody>().constraints=RigidbodyConstraints.FreezeRotation;
        var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.position=new Vector3(0,32,-29);camera.transform.LookAt(Vector3.zero);camera.GetComponent<Camera>().fieldOfView=60;
        var light=new GameObject("Directional Light",typeof(Light));light.GetComponent<Light>().type=LightType.Directional;light.transform.rotation=Quaternion.Euler(50,-30,0);RenderSettings.ambientLight=Color.gray;
        var canvas=new GameObject("Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);
        new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
        // The opaque full-screen background in the handout hides the 3D level.
        var background=Panel("Background",canvas.transform,new Vector2(1920,1080),Vector2.zero,new Vector2(.5f,.5f));background.GetComponent<Image>().color=new Color(.1f,.1f,.1f,.10f);background.GetComponent<Image>().raycastTarget=false;
        Text("TitleText",canvas.transform,"NavMesh: Навигация и ИИ",42,new Vector2(900,80),new Vector2(0,-60),new Vector2(.5f,1));
        var status=Text("StatusText",canvas.transform,"Выберите врага для создания",28,new Vector2(1200,60),new Vector2(0,-150),new Vector2(.5f,1));
        var infoPanel=Panel("EnemyInfoPanel",canvas.transform,new Vector2(500,400),new Vector2(250,0),new Vector2(0,.5f));
        var info=Text("EnemyInfoText",infoPanel.transform,"Информация о враге",24,new Vector2(460,360),Vector2.zero,new Vector2(.5f,.5f));info.alignment=TextAlignmentOptions.TopLeft;
        var listPanel=Panel("EnemyListPanel",canvas.transform,new Vector2(500,400),new Vector2(-250,0),new Vector2(1,.5f));
        Text("ListTitleText",listPanel.transform,"Доступные враги",28,new Vector2(460,40),new Vector2(0,-30),new Vector2(.5f,1)).color=new Color(1,.42f,.42f);
        var container=Panel("EnemiesContainer",listPanel.transform,new Vector2(460,310),new Vector2(0,-25),new Vector2(.5f,.5f));container.GetComponent<Image>().enabled=false;var layout=container.AddComponent<VerticalLayoutGroup>();layout.spacing=10;layout.childForceExpandHeight=false;layout.childControlHeight=true;layout.childForceExpandWidth=true;container.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        var control=Panel("ControlPanel",canvas.transform,new Vector2(1200,120),new Vector2(0,80),new Vector2(.5f,0));
        var create=Button("CreateEnemyButton",control.transform,"Создать врага",-420);var toggle=Button("ToggleNavMeshButton",control.transform,"Показать NavMesh",-140);var state=Button("ToggleStateButton",control.transform,"Состояние",140);var clear=Button("ClearButton",control.transform,"Очистить",420);clear.GetComponent<Image>().color=new Color(.55f,0,0);
        var ct=Text("ConfigText",canvas.transform,config.GetFullInfo(),22,new Vector2(1600,70),new Vector2(0,220),new Vector2(.5f,0));ct.color=new Color(.56f,.93f,.56f);
        var prefabButton=Button("EnemyButtonPrefab",canvas.transform,"Враг",0);prefabButton.GetComponent<RectTransform>().sizeDelta=new Vector2(440,50);var le=prefabButton.gameObject.AddComponent<LayoutElement>();le.preferredHeight=50;
        var prefab=PrefabUtility.SaveAsPrefabAsset(prefabButton.gameObject,"Assets/Prefabs/EnemyButtonPrefab.prefab");Object.DestroyImmediate(prefabButton.gameObject);
        var obj=new GameObject("NavMeshController");var factory=obj.AddComponent<EnemyFactory>();factory.enemyDatabase=db;factory.navMeshConfig=config;
        var controller=obj.AddComponent<NavMeshController>();controller.enemyDatabase=db;controller.navMeshConfig=config;controller.enemyFactory=factory;controller.player=player.transform;controller.navMeshSurface=surface;controller.statusText=status;controller.enemyInfoText=info;controller.configText=ct;controller.enemiesContainer=container.transform;controller.enemyButtonPrefab=prefab;controller.createEnemyButton=create;controller.toggleNavMeshButton=toggle;controller.toggleStateButton=state;controller.clearButton=clear;
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/MainScene.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/MainScene.unity",true)};AssetDatabase.SaveAssets();
        Debug.Log("PR19_SETUP_OK");
    }
    static Material Material(string name,Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;AssetDatabase.CreateAsset(m,$"Assets/Prefabs/{name}.mat");return m;}
    static GameObject Panel(string name,Transform parent,Vector2 size,Vector2 pos,Vector2 anchor){var g=new GameObject(name,typeof(RectTransform),typeof(Image));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.sizeDelta=size;r.anchoredPosition=pos;g.GetComponent<Image>().color=new Color(0,0,0,.5f);return g;}
    static TextMeshProUGUI Text(string name,Transform parent,string value,int size,Vector2 dimensions,Vector2 pos,Vector2 anchor){var g=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.sizeDelta=dimensions;r.anchoredPosition=pos;var t=g.GetComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;}
    static Button Button(string name,Transform parent,string value,float x){var g=Panel(name,parent,new Vector2(240,70),new Vector2(x,0),new Vector2(.5f,.5f));g.GetComponent<Image>().color=new Color(.2f,.25f,.3f);var b=g.AddComponent<Button>();b.targetGraphic=g.GetComponent<Image>();Text("Text (TMP)",g.transform,value,22,new Vector2(230,65),Vector2.zero,new Vector2(.5f,.5f));return b;}
}


