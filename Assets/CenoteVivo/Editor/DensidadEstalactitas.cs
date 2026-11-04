using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Collections.Generic;

public static class DensidadEstalactitas
{
    const string Dir="Assets/CenoteVivo/Estalactitas";
    public static string Crear()
    {
        if(GameObject.Find("Estalactitas_Densidad"))return "Ya existe el grupo; no se duplicó.";
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/CenoteVivo_AntesEstalactitas.unity",true);
        System.IO.Directory.CreateDirectory(Dir);AssetDatabase.Refresh();
        var originals=Object.FindObjectsByType<MeshFilter>().Where(f=>f.name.StartsWith("Estalactita")).ToArray();
        var source=originals.First();var bounds=source.GetComponent<Renderer>().bounds;
        var matrix=source.transform.localToWorldMatrix;float height=bounds.size.y,width=Mathf.Max(bounds.size.x,bounds.size.z);
        var normalized=Object.Instantiate(source.sharedMesh);normalized.name="Estalactita_Normalizada";
        normalized.vertices=source.sharedMesh.vertices.Select(v=>{var p=matrix.MultiplyPoint3x4(v);return new Vector3((p.x-bounds.center.x)/width,(p.y-bounds.max.y)/height,(p.z-bounds.center.z)/width);}).ToArray();
        normalized.RecalculateNormals();normalized.RecalculateTangents();normalized.RecalculateBounds();AssetDatabase.CreateAsset(normalized,Dir+"/Estalactita_Normalizada.asset");
        var material=new Material(source.GetComponent<Renderer>().sharedMaterial);material.name="Caliza_Estalactitas";
        if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",.22f);
        if(material.HasProperty("_Metallic"))material.SetFloat("_Metallic",0);
        AssetDatabase.CreateAsset(material,Dir+"/Caliza_Estalactitas.mat");
        var body=Object.FindObjectsByType<MeshFilter>().First(f=>f.name=="CenoteBody");
        var temp=new GameObject("MedicionTechoTemporal");temp.transform.SetParent(body.transform,false);
        var collider=temp.AddComponent<MeshCollider>();collider.sharedMesh=body.sharedMesh;
        bool back=Physics.queriesHitBackfaces;var random=Random.state;
        var root=new GameObject("Estalactitas_Densidad");root.transform.SetParent(GameObject.Find("CenoteVivoRoot").transform,false);
        int created=0,aligned=0;
        try
        {
            Physics.queriesHitBackfaces=true;Random.InitState(73109);
            foreach(var f in originals)
            {
                var r=f.GetComponent<Renderer>();var b=r.bounds;RaycastHit hit;
                if(collider.Raycast(new Ray(new Vector3(b.center.x,-2,b.center.z),Vector3.up),out hit,12)&&hit.point.y>2.3f)
                {Undo.RecordObject(f.transform,"Encajar estalactita en techo");f.transform.position+=Vector3.up*(hit.point.y+.12f-b.max.y);aligned++;}
                Undo.RecordObject(r,"Material caliza");r.sharedMaterial=material;
            }
            var positions=new List<Vector3>();
            for(int sector=0;sector<12;sector++)
            {
                var group=new GameObject("Racimo_"+(sector+1).ToString("00"));group.transform.SetParent(root.transform,false);
                float angle=sector*Mathf.PI*2/12+Random.Range(-.1f,.1f);
                float radius=Random.Range(8.4f,10.3f);
                Vector3 center=new Vector3(-2+Mathf.Cos(angle)*radius,0,-3+Mathf.Sin(angle)*radius);
                group.transform.position=center;
                var sources=new GameObject("Fuentes_Editables");sources.transform.SetParent(group.transform,false);
                int target=Random.Range(5,9),count=0;
                for(int attempt=0;attempt<80&&count<target;attempt++)
                {
                    Vector2 offset=Random.insideUnitCircle*1.7f;var from=center+new Vector3(offset.x,-2,offset.y);RaycastHit hit;
                    if(!collider.Raycast(new Ray(from,Vector3.up),out hit,12)||hit.point.y<2.6f||hit.point.y>6.3f)continue;
                    if(positions.Any(p=>new Vector2(p.x-hit.point.x,p.z-hit.point.z).sqrMagnitude<.15f))continue;
                    float length=Random.value<.24f?Random.Range(1.5f,2.4f):Random.Range(.45f,1.45f);
                    float thick=length*Random.Range(.18f,.30f);
                    var obj=new GameObject("Estalactita_"+(count+1).ToString("00"));obj.transform.SetParent(sources.transform,false);
                    obj.transform.position=hit.point+Vector3.up*.16f;
                    obj.transform.rotation=Quaternion.Euler(Random.Range(-4f,4f),Random.Range(0,360),Random.Range(-4f,4f));
                    obj.transform.localScale=new Vector3(thick,length,thick*Random.Range(.85f,1.15f));
                    obj.AddComponent<MeshFilter>().sharedMesh=normalized;var r=obj.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                    positions.Add(hit.point);count++;created++;
                }
                if(count==0){Object.DestroyImmediate(group);continue;}
                Combinar(group,material);sources.SetActive(false);
            }
        }
        finally{Physics.queriesHitBackfaces=back;Random.state=random;Object.DestroyImmediate(temp);}
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Selection.activeGameObject=root;
        return "Nuevas="+created+" originales="+originals.Length+" bases ajustadas="+aligned+" sectores="+root.transform.childCount+" triangulos nuevos="+created*normalized.triangles.Length/3;
    }
    static void Combinar(GameObject group,Material material)
    {
        var sources=group.transform.Find("Fuentes_Editables");var filters=sources.GetComponentsInChildren<MeshFilter>(true);
        var mesh=new Mesh();mesh.name=group.name+"_Combinado";
        mesh.CombineMeshes(filters.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=group.transform.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray(),true,true);
        var path=Dir+"/"+mesh.name+".asset";var previous=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(previous){EditorUtility.CopySerialized(mesh,previous);Object.DestroyImmediate(mesh);mesh=previous;}else AssetDatabase.CreateAsset(mesh,path);
        var existing=group.transform.Find("Render");var render=existing?existing.gameObject:new GameObject("Render");render.transform.SetParent(group.transform,false);render.SetActive(true);
        var mf=render.GetComponent<MeshFilter>();if(!mf)mf=render.AddComponent<MeshFilter>();mf.sharedMesh=mesh;
        var mr=render.GetComponent<MeshRenderer>();if(!mr)mr=render.AddComponent<MeshRenderer>();mr.sharedMaterial=material;mr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.lightProbeUsage=UnityEngine.Rendering.LightProbeUsage.Off;mr.reflectionProbeUsage=UnityEngine.Rendering.ReflectionProbeUsage.Off;
        var lod=group.GetComponent<LODGroup>();if(!lod)lod=group.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.008f,new Renderer[]{mr})});lod.RecalculateBounds();
    }
    [MenuItem("Cenote Vivo/Estalactitas/Reconstruir grupos editados")]
    public static void Reconstruir()
    {
        var root=GameObject.Find("Estalactitas_Densidad");if(!root)return;
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/CenoteVivo/Remodelado/Caliza_EstalactitasNueva.mat");if(!material)material=AssetDatabase.LoadAssetAtPath<Material>(Dir+"/Caliza_Estalactitas.mat");
        foreach(Transform group in root.transform){Combinar(group.gameObject,material);group.Find("Fuentes_Editables").gameObject.SetActive(false);}
        EditorSceneManager.MarkSceneDirty(root.scene);AssetDatabase.SaveAssets();
    }
}
