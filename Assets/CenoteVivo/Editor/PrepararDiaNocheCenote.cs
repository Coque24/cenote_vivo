using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class PrepararDiaNocheCenote
{
    const string Dir = "Assets/CenoteVivo/DiaNoche/";
    static Material Mat(string name, Shader shader, Color color)
    {
        var path = Dir + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
        if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);
        else if(m.HasProperty("_Color"))m.SetColor("_Color",color);
        return m;
    }
    static GameObject Part(PrimitiveType type,string name,Transform parent,Vector3 position,Vector3 scale,Material material)
    {
        var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;
        UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=material;return g;
    }
    static void Combine(Transform root,Material material,string name)
    {
        var filters=root.GetComponentsInChildren<MeshFilter>();var ci=new List<CombineInstance>();
        foreach(var f in filters)ci.Add(new CombineInstance{mesh=f.sharedMesh,transform=root.worldToLocalMatrix*f.transform.localToWorldMatrix});
        var mesh=new Mesh{name=name};mesh.CombineMeshes(ci.ToArray());AssetDatabase.CreateAsset(mesh,Dir+name+".asset");
        foreach(var f in filters)UnityEngine.Object.DestroyImmediate(f.gameObject);
        var g=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(root,false);g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=material;
    }
    [MenuItem("Cenote Vivo/Iluminación/Preparar día y noche")]
    public static string Crear()
    {
        if(EditorApplication.isPlaying)throw new Exception("Salir de Play primero.");
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!="Assets/Scenes/CenoteVivo_Peces.unity")throw new Exception("Abrir la escena móvil CenoteVivo_Peces.");
        if(scene.isDirty)throw new Exception("Guardar cambios antes de preparar iluminación.");
        if(UnityEngine.Object.FindAnyObjectByType<CicloDiaNocheCenote>(FindObjectsInactive.Include))return "El ciclo ya existe";
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/CenoteVivo_AntesDiaNoche.unity",true);
        var menu=UnityEngine.Object.FindAnyObjectByType<MenuCenote>();var controles=UnityEngine.Object.FindAnyObjectByType<ControlesCenote>(FindObjectsInactive.Include);
        var ambiente=UnityEngine.Object.FindAnyObjectByType<AmbienteCenote>(FindObjectsInactive.Include);
        var root=new GameObject("IluminacionDiaNoche");root.transform.SetParent(menu.contenidoCenote.transform,false);
        var lit=Shader.Find("Universal Render Pipeline/Lit");
        var metal=Mat("MetalBronce",lit,new Color(.17f,.10f,.045f));metal.SetFloat("_Metallic",.65f);metal.SetFloat("_Smoothness",.35f);
        var cuerda=Mat("Cuerda",lit,new Color(.10f,.075f,.04f));cuerda.SetFloat("_Smoothness",.05f);
        var graph=AssetDatabase.LoadAssetAtPath<Shader>(Dir+"FarolEmision.shadergraph");if(!graph || ShaderUtil.ShaderHasError(graph))throw new Exception("Revisar Shader Graph de emisión.");
        var emission=Mat("FarolCalido",graph,Color.white);emission.SetColor("_LuzCalida",new Color(.04f,.02f,.006f,1));
        var sky=Mat("CieloDiaNoche",Shader.Find("CenoteVivo/CieloDiaNoche"),Color.white);sky.SetFloat("_Noche",0);
        var lights=new List<Light>();var bulbs=new List<Renderer>();
        Vector3[] lugares={new Vector3(-7.5f,2.7f,-7),new Vector3(3.3f,2.9f,-5),new Vector3(-7,3.4f,2),new Vector3(2.2f,3.2f,3.2f)};
        for(int i=0;i<lugares.Length;i++)
        {
            var lamp=new GameObject("FarolColgante_"+(i+1));lamp.transform.SetParent(root.transform,false);lamp.transform.position=lugares[i];
            var frame=new GameObject("Estructura").transform;frame.SetParent(lamp.transform,false);
            Part(PrimitiveType.Cylinder,"Base",frame,new Vector3(0,-.34f,0),new Vector3(.63f,.07f,.63f),metal);
            Part(PrimitiveType.Cylinder,"Sombrero",frame,new Vector3(0,.39f,0),new Vector3(.78f,.065f,.78f),metal);
            Part(PrimitiveType.Sphere,"Corona",frame,new Vector3(0,.46f,0),new Vector3(.43f,.22f,.43f),metal);
            for(int j=0;j<4;j++){float a=j*Mathf.PI*.5f+.785f;Part(PrimitiveType.Cylinder,"Marco",frame,new Vector3(Mathf.Cos(a)*.23f,0,Mathf.Sin(a)*.23f),new Vector3(.04f,.35f,.04f),metal);}
            Combine(frame,metal,"MarcoFarol_"+i);
            float largo=6.1f-lugares[i].y-.55f;
            Part(PrimitiveType.Cylinder,"CableColgante",lamp.transform,new Vector3(0,.55f+largo*.5f,0),new Vector3(.025f,largo*.5f,.025f),cuerda);
            var bulb=Part(PrimitiveType.Capsule,"DifusorCalido",lamp.transform,Vector3.zero,new Vector3(.39f,.32f,.39f),emission);
            bulb.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;bulbs.Add(bulb.GetComponent<Renderer>());
            var lg=new GameObject("LuzCalida");lg.transform.SetParent(lamp.transform,false);lg.transform.localPosition=new Vector3(0,-.4f,0);
            var l=lg.AddComponent<Light>();l.type=LightType.Point;l.color=new Color(1,.56f,.27f);l.intensity=0;l.range=17;l.shadows=LightShadows.None;l.renderMode=LightRenderMode.ForcePixel;l.enabled=false;lights.Add(l);
        }
        // Suelo exterior ligero, con hueco central y sin tapar el cenote.
        var ground=new GameObject("TerrenoExteriorExtendido",typeof(MeshFilter),typeof(MeshRenderer));ground.transform.SetParent(root.transform,false);
        var vertices=new List<Vector3>();var triangles=new List<int>();int seg=96;
        for(int ring=0;ring<3;ring++)for(int j=0;j<=seg;j++)
        {
            float a=j*2*Mathf.PI/seg;float r=ring==0?22: ring==1?65:220;
            float y=6.05f+(ring==0?-.2f:Mathf.PerlinNoise(Mathf.Cos(a)*2+3,Mathf.Sin(a)*2+4)*1.5f);
            vertices.Add(new Vector3(Mathf.Cos(a)*r,y,Mathf.Sin(a)*r));
        }
        for(int ring=0;ring<2;ring++)for(int j=0;j<seg;j++){int a=ring*(seg+1)+j,b=a+seg+1;triangles.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
        var gm=new Mesh{name="SueloExterior"};gm.SetVertices(vertices);gm.SetTriangles(triangles,0);gm.RecalculateNormals();gm.RecalculateBounds();AssetDatabase.CreateAsset(gm,Dir+"SueloExterior.asset");ground.GetComponent<MeshFilter>().sharedMesh=gm;
        ground.GetComponent<MeshRenderer>().sharedMaterial=ambiente.agua.transform.parent.Find("BordeTerreno").GetComponent<Renderer>().sharedMaterial;
        var ciclo=controles.gameObject.AddComponent<CicloDiaNocheCenote>();
        foreach(var luz in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include))if(luz.type==LightType.Directional){ciclo.sol=luz;break;}
        if(!ciclo.sol)throw new Exception("Falta la luz solar direccional.");ciclo.faroles=lights.ToArray();ciclo.bombillas=bulbs.ToArray();ciclo.cieloPlantilla=sky;ciclo.emisionPlantilla=emission;ciclo.ambiente=ambiente;ambiente.cicloDiaNoche=ciclo;
        // Un botón sencillo, debajo de Inicio y fuera del selector de cámaras.
        var br=new GameObject("CambiarDiaNoche",typeof(RectTransform)).GetComponent<RectTransform>();br.SetParent(controles.zonaSegura,false);
        br.anchorMin=br.anchorMax=new Vector2(0,1);br.pivot=new Vector2(0,1);br.anchoredPosition=new Vector2(28,-126);br.sizeDelta=new Vector2(300,76);
        var image=br.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=new Color(.08f,.19f,.22f,.94f);
        var button=br.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=image;ciclo.boton=button;
        var label=new GameObject("Etiqueta",typeof(RectTransform)).GetComponent<RectTransform>();label.SetParent(br,false);label.anchorMin=Vector2.zero;label.anchorMax=Vector2.one;label.offsetMin=new Vector2(8,4);label.offsetMax=new Vector2(-8,-4);
        var text=label.gameObject.AddComponent<TMPro.TextMeshProUGUI>();text.font=controles.estado.font;text.text="Cambiar a noche";text.fontSize=29;text.color=new Color(1,.86f,.62f);text.alignment=TMPro.TextAlignmentOptions.Center;text.raycastTarget=false;ciclo.etiqueta=text;
        var profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Dir+"AtmosferaFaroles.asset");
        var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.24f);bloom.threshold.Override(1.1f);bloom.scatter.Override(.55f);bloom.maxIterations.Override(4);bloom.highQualityFiltering.Override(false);
        var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.Neutral);
        foreach(var c in profile.components)AssetDatabase.AddObjectToAsset(c,profile);
        var vgo=new GameObject("AtmosferaFaroles");vgo.transform.SetParent(root.transform,false);var volume=vgo.AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=profile;
        var camera=Camera.main;camera.clearFlags=CameraClearFlags.Skybox;camera.allowHDR=true;camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
        RenderSettings.skybox=sky;RenderSettings.sun=ciclo.sol;
        EditorUtility.SetDirty(ambiente);EditorUtility.SetDirty(ciclo);EditorUtility.SetDirty(profile);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "4 faroles, Shader Graph emisivo, cielo procedural, bloom y ciclo de 6 segundos guardados";
    }
}
