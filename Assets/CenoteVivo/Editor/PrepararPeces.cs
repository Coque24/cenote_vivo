using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System.IO;

public static class PrepararPeces
{
    const string Dir = "Assets/CenoteVivo";
    public static string Crear()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (GameObject.Find("VidaCenote")) return "Ya existe VidaCenote; no se duplico.";
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Cenote_AntesDePeces.unity", true);
        Directory.CreateDirectory(Dir + "/Meshes"); Directory.CreateDirectory(Dir + "/Materials"); Directory.CreateDirectory(Dir + "/Prefabs");
        AssetDatabase.Refresh();
        var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Peces.fbx");
        var filters = model.GetComponentsInChildren<MeshFilter>().OrderBy(f=>f.name).ToArray();
        var primary = filters[0];
        var world = primary.sharedMesh.vertices.Select(v=>primary.transform.localToWorldMatrix.MultiplyPoint3x4(v)).ToArray();
        var colors = primary.sharedMesh.colors;
        float eyeX = 0; int eyes = 0;
        for(int i=0;i<colors.Length;i++) if(colors[i].r<.25f && colors[i].g<.25f && colors[i].b<.25f) { eyeX += world[i].x; eyes++; }
        var rotation = Quaternion.FromToRotation(eyeX < 0 ? Vector3.left : Vector3.right, Vector3.forward);
        var bounds = new Bounds(world[0], Vector3.zero); foreach(var v in world) bounds.Encapsulate(v);
        float length = bounds.size.x;
        var mat = new Material(Shader.Find("CenoteVivo/PezVertex")); mat.name = "Pez_Plata";
        AssetDatabase.CreateAsset(mat, Dir + "/Materials/Pez_Plata.mat");
        var root = new GameObject("PezCenote");
        var lods = new LOD[filters.Length];
        for(int n=0;n<filters.Length;n++)
        {
            var f = filters[n]; var mesh = Object.Instantiate(f.sharedMesh); mesh.name = "Pez_LOD" + n;
            mesh.vertices = mesh.vertices.Select(v=>rotation * (f.transform.localToWorldMatrix.MultiplyPoint3x4(v)-bounds.center)/length).ToArray();
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); var b=mesh.bounds; b.Expand(.16f); mesh.bounds=b;
            AssetDatabase.CreateAsset(mesh, Dir+"/Meshes/"+mesh.name+".asset");
            var child = new GameObject("LOD"+n); child.transform.SetParent(root.transform,false);
            child.AddComponent<MeshFilter>().sharedMesh=mesh;
            var r = child.AddComponent<MeshRenderer>(); r.sharedMaterial=mat;
            r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows=false;
            r.lightProbeUsage=UnityEngine.Rendering.LightProbeUsage.Off; r.reflectionProbeUsage=UnityEngine.Rendering.ReflectionProbeUsage.Off;
            lods[n] = new LOD(n==0 ? .025f : .006f, new Renderer[]{r});
        }
        var group = root.AddComponent<LODGroup>(); group.SetLODs(lods); group.RecalculateBounds();
        var prefab=PrefabUtility.SaveAsPrefabAsset(root,Dir+"/Prefabs/PezCenote.prefab"); Object.DestroyImmediate(root);
        var vida = new GameObject("VidaCenote"); vida.transform.position=new Vector3(-2,0,-3);
        var volume=new GameObject("VolumenPeces"); volume.transform.SetParent(vida.transform,false); volume.transform.localPosition=new Vector3(0,-2,0);
        var school=volume.AddComponent<CardumenCenote>(); school.prefabPez=prefab; school.extension=new Vector3(3,.8f,3.5f);
        var old = GameObject.Find("Peces"); if(old) old.SetActive(false);
        var vegetation=new GameObject("VegetacionSuperior_Puntos"); vegetation.transform.SetParent(vida.transform,false);
        for(int i=0;i<8;i++) { float a=i*Mathf.PI/4; var p=new GameObject("Raices_"+(i+1).ToString("00")); p.transform.SetParent(vegetation.transform,false); p.transform.localPosition=new Vector3(Mathf.Cos(a)*5.8f,6.1f,Mathf.Sin(a)*5.8f); }
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/CenoteVivo_Peces.unity"); AssetDatabase.SaveAssets();
        Selection.activeGameObject=volume;
        return "Escena guardada; " + filters.Length + " LOD; longitud original="+length+" cabezaX="+eyeX+" ojos="+eyes;
    }
}
