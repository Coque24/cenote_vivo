using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public static class PrepararAmbienteCenote
{
 const string Dir="Assets/CenoteVivo/Ambiente";
 class Geo
 {
  public List<Vector3> v=new List<Vector3>();public List<int> t=new List<int>();public List<Color> c=new List<Color>();
  public int Add(Vector3 p,Color col){v.Add(p);c.Add(col);return v.Count-1;}
  public void Tri(int a,int b,int d,Vector3 n){if(Vector3.Dot(Vector3.Cross(v[b]-v[a],v[d]-v[a]),n)<0){int x=b;b=d;d=x;}t.Add(a);t.Add(b);t.Add(d);}
  public void Quad(Vector3 a,Vector3 b,Vector3 d,Vector3 e,Color col){int s=Add(a,col);Add(b,col);Add(d,col);Add(e,col);t.AddRange(new[]{s,s+1,s+2,s,s+2,s+3});}
  public Mesh Mesh(string name){var m=new Mesh();m.name=name;m.SetVertices(v);m.SetColors(c);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
 }
 static void Leaf(Geo g,Vector3 p,Vector3 dir,float len,float width,Color col)
 {
  dir.Normalize();var side=Vector3.Cross(dir,Vector3.up).normalized;if(side.sqrMagnitude<.1f)side=Vector3.right;
  var mid=p+dir*len*.45f+Vector3.up*.07f;int a=g.Add(p,col),b=g.Add(mid-side*width,col*.85f),c=g.Add(p+dir*len,col),d=g.Add(mid+side*width,col*1.06f),e=g.Add(mid+Vector3.up*.025f,col);
  g.Tri(a,b,e,Vector3.up);g.Tri(b,c,e,Vector3.up);g.Tri(c,d,e,Vector3.up);g.Tri(d,a,e,Vector3.up);
 }
 static void Tube(Geo g,List<Vector3> path,float radius,Color col,int sides=5)
 {
  int start=g.v.Count;
  for(int j=0;j<path.Count;j++){var dir=(path[Mathf.Min(path.Count-1,j+1)]-path[Mathf.Max(0,j-1)]).normalized;var x=Vector3.Cross(dir,Vector3.forward).normalized;if(x.sqrMagnitude<.1f)x=Vector3.right;var z=Vector3.Cross(dir,x);float r=radius*Mathf.Lerp(1,.15f,(float)j/(path.Count-1));for(int i=0;i<sides;i++){float a=i*2*Mathf.PI/sides;g.Add(path[j]+(x*Mathf.Cos(a)+z*Mathf.Sin(a))*r,col);}}
  for(int j=0;j<path.Count-1;j++)for(int i=0;i<sides;i++){int a=start+j*sides+i,b=start+j*sides+(i+1)%sides,c=b+sides,d=a+sides;g.Tri(a,b,c,g.v[a]-path[j]);g.Tri(a,c,d,g.v[a]-path[j]);}
 }
 static void Box(Geo g,Vector3 center,Vector3 size,Quaternion rotation,Color col)
 {
  var p=new Vector3[8];for(int i=0;i<8;i++)p[i]=center+rotation*new Vector3((i%2==0?-1:1)*size.x*.5f,((i/2)%2==0?-1:1)*size.y*.5f,(i/4==0?-1:1)*size.z*.5f);
  int[][] faces={new[]{0,1,3,2},new[]{4,6,7,5},new[]{0,4,5,1},new[]{2,3,7,6},new[]{0,2,6,4},new[]{1,5,7,3}};
  foreach(var f in faces)g.Quad(p[f[0]],p[f[1]],p[f[2]],p[f[3]],col);
 }
 static GameObject Render(string name,Geo geo,Material mat,Transform parent)
 {
  var mesh=geo.Mesh(name);AssetDatabase.CreateAsset(mesh,Dir+"/"+name+".asset");var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;var r=obj.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.lightProbeUsage=UnityEngine.Rendering.LightProbeUsage.Off;r.reflectionProbeUsage=UnityEngine.Rendering.ReflectionProbeUsage.Off;return obj;
 }
 public static string Crear()
 {
  var anchor=Resources.FindObjectsOfTypeAll<GameObject>().First(g=>g.scene.IsValid()&&g.name=="CenoteVivoRoot");
  var model=Resources.FindObjectsOfTypeAll<GameObject>().First(g=>g.scene.IsValid()&&g.name=="Cenote_Remodelado");
  if(model.GetComponent<AmbienteCenote>())return "Ambiente ya instalado";
  EditorSceneManager.SaveScene(model.scene,"Assets/Scenes/CenoteVivo_AntesEstadosAmbiente.unity",true);
  Directory.CreateDirectory(Dir);AssetDatabase.Refresh();anchor.SetActive(true);
  var random=Random.state;Random.InitState(219433);
  var sourceWater=GameObject.Find("AguaRemodelada").GetComponent<Renderer>();var water=new Material[3];
  string[] names={"Normal","Danado","Contaminado"};
  for(int i=0;i<3;i++){water[i]=new Material(sourceWater.sharedMaterial);water[i].name="Agua_"+names[i];AssetDatabase.CreateAsset(water[i],Dir+"/Agua_"+names[i]+".mat");}
  water[0].SetFloat("_UnderOpacity",.13f);
  water[1].SetColor("_Shallow",new Color(.69f,.71f,.67f));water[1].SetColor("_Deep",new Color(.32f,.35f,.33f));water[1].SetFloat("_Opacity",.88f);water[1].SetFloat("_UnderOpacity",.28f);water[1].SetFloat("_WaveStrength",.045f);
  water[2].SetColor("_Shallow",new Color(.38f,.27f,.12f));water[2].SetColor("_Deep",new Color(.13f,.085f,.035f));water[2].SetFloat("_Opacity",.96f);water[2].SetFloat("_UnderOpacity",.38f);water[2].SetFloat("_WaveStrength",.025f);
  var plantSource=AssetDatabase.LoadAssetAtPath<Material>("Assets/CenoteVivo/Remodelado/Raices_Follaje.mat");var vegetationMaterials=new Material[3];
  for(int i=0;i<3;i++){vegetationMaterials[i]=new Material(plantSource);vegetationMaterials[i].name="Vegetacion_"+names[i];vegetationMaterials[i].SetColor("_Tint",i==0?Color.white:i==1?new Color(.8f,.8f,.65f):new Color(.55f,.38f,.18f));AssetDatabase.CreateAsset(vegetationMaterials[i],Dir+"/Vegetacion_"+names[i]+".mat");}
  var vinesRoot=new GameObject("LianasYDoselVisible");vinesRoot.transform.SetParent(model.transform,false);
  var reducible=new List<GameObject>();int leaves=0,vines=0;
  for(int sector=0;sector<8;sector++)
  {
   var geo=new Geo();float angle=(sector+.5f)*Mathf.PI/4;
   for(int vine=0;vine<4;vine++)
   {
    float a=angle+(vine-1.5f)*.11f;var radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));float radius=7.3f+.26f*Mathf.Sin(a*3);var top=radial*radius+Vector3.up*6.65f;float length=Random.Range(2.5f,6.3f);var path=new List<Vector3>();
    for(int k=0;k<=14;k++){float t=k/14f;path.Add(top+Vector3.down*length*t-radial*(.25f+Mathf.Sin(t*Mathf.PI)*.7f)+new Vector3(Mathf.Sin(t*6+a)*.12f,0,Mathf.Cos(t*5+a)*.12f));}
    Tube(geo,path,.023f,new Color(.21f,.24f,.075f));vines++;
    for(int k=1;k<13;k++){var pos=path[k];float sign=k%2==0?1:-1;var direction=new Vector3(-radial.z*sign,-.18f,radial.x*sign)-radial*.25f;Leaf(geo,pos,direction,Random.Range(.25f,.55f),.11f,Color.Lerp(new Color(.09f,.27f,.055f),new Color(.25f,.44f,.11f),Random.value));leaves++;}
   }
   // Solo ramas y follaje que asoman a la abertura; no hay árboles completos fuera de vista.
   var axis=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));var tangent=Vector3.Cross(Vector3.up,axis);var branch=new List<Vector3>();
   for(int k=0;k<=8;k++){float t=k/8f;branch.Add(axis*(8.5f-t*1.9f)+Vector3.up*(6.7f+Mathf.Sin(t*Mathf.PI)*.85f)+tangent*Mathf.Sin(t*2)*.5f);}
   Tube(geo,branch,.11f,new Color(.25f,.18f,.085f));
   for(int k=0;k<50;k++){float t=Random.Range(.2f,1f);var p=branch[Mathf.RoundToInt(t*8)]+tangent*Random.Range(-.9f,.9f)+axis*Random.Range(-.4f,.4f)+Vector3.up*Random.Range(-.15f,.3f);Leaf(geo,p,new Vector3(Random.Range(-1f,1),Random.Range(-.15f,.25f),Random.Range(-1f,1)),Random.Range(.35f,.65f),Random.Range(.10f,.18f),Color.Lerp(new Color(.065f,.22f,.04f),new Color(.25f,.42f,.095f),Random.value));leaves++;}
   var obj=Render("LianasDosel_"+sector,geo,vegetationMaterials[0],vinesRoot.transform);if(sector%2==1)reducible.Add(obj);
  }
  var debrisRoot=new GameObject("EscombrosEstadoDanado");debrisRoot.transform.SetParent(model.transform,false);
  var debrisMaterial=new Material(plantSource);debrisMaterial.name="EscombrosMate";debrisMaterial.SetColor("_Tint",Color.white);AssetDatabase.CreateAsset(debrisMaterial,Dir+"/EscombrosMate.mat");
  var cave=GameObject.Find("CavidadCaliza");var collider=cave.AddComponent<MeshCollider>();collider.sharedMesh=cave.GetComponent<MeshFilter>().sharedMesh;Physics.SyncTransforms();bool back=Physics.queriesHitBackfaces;Physics.queriesHitBackfaces=true;int chunks=0;
  var centers=new[]{new Vector3(-5,0,9),new Vector3(3,0,11),new Vector3(-2,0,8),new Vector3(6,0,9)};
  for(int patch=0;patch<centers.Length;patch++)
  {
   var geo=new Geo();
   for(int k=0;k<15;k++)
   {
    var p=centers[patch]+new Vector3(Random.Range(-2.1f,2.1f),0,Random.Range(-2.1f,2.1f));var from=model.transform.TransformPoint(p+Vector3.up);RaycastHit hit;if(!collider.Raycast(new Ray(from,Vector3.down),out hit,15))continue;
    var at=model.transform.InverseTransformPoint(hit.point);var normal=model.transform.InverseTransformDirection(hit.normal);if(normal.y<0)normal=-normal;var q=Quaternion.FromToRotation(Vector3.up,normal)*Quaternion.Euler(Random.Range(-7f,7f),Random.Range(0,360),Random.Range(-7f,7f));
    float h=Random.Range(.18f,.5f);var size=k<12?new Vector3(Random.Range(.65f,1.4f),h,Random.Range(.45f,.95f)):new Vector3(Random.Range(1.4f,2.2f),.13f,.17f);
    Box(geo,at+normal*h*.42f,size,q,k<12?Color.Lerp(new Color(.43f,.44f,.41f),new Color(.68f,.65f,.58f),Random.value):new Color(.33f,.24f,.13f));chunks++;
   }
   Render("EscombrosSector_"+patch,geo,debrisMaterial,debrisRoot.transform);
  }
  Physics.queriesHitBackfaces=back;Object.DestroyImmediate(collider);debrisRoot.SetActive(false);
  var env=model.AddComponent<AmbienteCenote>();env.cardumen=Object.FindAnyObjectByType<CardumenCenote>();env.agua=sourceWater;env.materialesAgua=water;env.materialesVegetacion=vegetationMaterials;
  env.vegetacion=model.GetComponentsInChildren<Renderer>(true).Where(r=>r.name.StartsWith("VegetacionBorde_")||r.name.StartsWith("LianasDosel_")).ToArray();env.escombros=debrisRoot;env.vegetacionReducible=reducible.ToArray();env.colorNormal=RenderSettings.fogColor;env.densidadNormal=RenderSettings.fogDensity;
  sourceWater.sharedMaterial=water[0];foreach(var r in env.vegetacion)r.sharedMaterial=vegetationMaterials[0];
  Random.state=random;Object.FindAnyObjectByType<MenuCenote>().MostrarInicio();EditorSceneManager.SaveScene(model.scene);AssetDatabase.SaveAssets();Selection.activeGameObject=model;
  int triangles=vinesRoot.GetComponentsInChildren<MeshFilter>(true).Sum(f=>f.sharedMesh.triangles.Length/3)+debrisRoot.GetComponentsInChildren<MeshFilter>(true).Sum(f=>f.sharedMesh.triangles.Length/3);
  return "Estados preparados; "+vines+" lianas; "+leaves+" hojas nuevas; "+chunks+" escombros; "+triangles+" triangulos adicionales";
 }
}
