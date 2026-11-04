using UnityEngine.Rendering.Universal;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public static class RemodelarCenote
{
 const string Dir="Assets/CenoteVivo/Remodelado";
 const int N=96;
 static float[] ys={-8,-7.3f,-6.2f,-4.8f,-3,-1,1.8f,3.5f,4.8f,5.7f,6.35f,6.6f};
 static float[] rs={2.5f,6.5f,10,12.3f,13.6f,13.8f,13,11.8f,10.1f,8.6f,7.35f,7.25f};
 class Geo
 {
  public List<Vector3> v=new List<Vector3>();public List<Color> c=new List<Color>();public List<int> t=new List<int>();
  public int Add(Vector3 p,Color color){v.Add(p);c.Add(color);return v.Count-1;}
  public void Tri(int a,int b,int d,Vector3 facing){if(Vector3.Dot(Vector3.Cross(v[b]-v[a],v[d]-v[a]),facing)<0){int q=b;b=d;d=q;}t.Add(a);t.Add(b);t.Add(d);}
  public void Quad(Vector3 a,Vector3 b,Vector3 d,Vector3 e,Vector3 facing,Color color){int s=Add(a,color);Add(b,color);Add(d,color);Add(e,color);Tri(s,s+1,s+2,facing);Tri(s,s+2,s+3,facing);}
  public Mesh Mesh(string name){var m=new Mesh();m.name=name;if(v.Count>65000)m.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;m.SetVertices(v);m.SetColors(c);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
 }
 struct Portal{public int center,low,high;public Portal(int c,int l,int h){center=c;low=l;high=h;}}
 static Portal[] portals={new Portal(8,4,7),new Portal(30,3,6),new Portal(58,4,7)};
 static float Radius(float a,int j){return rs[j]*(1+.045f*Mathf.Sin(a*3+.4f)+.025f*Mathf.Sin(a*7+ys[j]*.35f))+.18f*Mathf.Sin(a*13+ys[j]);}
 static Vector3 Point(int j,int i)
 {
  int ii=(i+N)%N;float a=ii*Mathf.PI*2/N;float y=ys[j]+.13f*Mathf.Sin(a*5+ys[j]*.4f);
  foreach(var p in portals)if(Mathf.Abs(ii-p.center)<=3 && j>=p.low && j<=p.high){float u=(ii-p.center)/3f;float top=ys[p.high]-1.8f+1.8f*Mathf.Sqrt(Mathf.Max(0,1-u*u));y=Mathf.Lerp(ys[p.low],top,(float)(j-p.low)/(p.high-p.low));}
  float r=Radius(a,j);return new Vector3(Mathf.Cos(a)*r,y,Mathf.Sin(a)*r*1.07f);
 }
 static Color RockColor(float y){return Color.Lerp(new Color(.46f,.56f,.51f),new Color(.86f,.83f,.68f),Mathf.InverseLerp(-5,6.6f,y));}
 static GameObject Render(string name,Mesh mesh,Material mat,Transform parent)
 {
  AssetDatabase.CreateAsset(mesh,Dir+"/"+name+".asset");var g=new GameObject(name);g.transform.SetParent(parent,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.lightProbeUsage=UnityEngine.Rendering.LightProbeUsage.Off;r.reflectionProbeUsage=UnityEngine.Rendering.ReflectionProbeUsage.Off;return g;
 }
 static Material Material(string shader,string name){var m=new Material(Shader.Find(shader));m.name=name;AssetDatabase.CreateAsset(m,Dir+"/"+name+".mat");return m;}
 static void Leaf(Geo geo,Vector3 p,Vector3 dir,float length,float width,Color col)
 {
  dir.Normalize();var side=Vector3.Cross(dir,Vector3.up);if(side.sqrMagnitude<.01f)side=Vector3.right;side.Normalize();var mid=p+dir*length*.48f+Vector3.up*.07f;int a=geo.Add(p,col),b=geo.Add(mid-side*width,col*.9f),c=geo.Add(p+dir*length,col),d=geo.Add(mid+side*width,col*1.05f);geo.Tri(a,b,c,Vector3.up);geo.Tri(a,c,d,Vector3.up);
 }
 static void Tube(Geo geo,List<Vector3> path,float radius,Color col)
 {
  int start=geo.v.Count,sides=5;
  for(int j=0;j<path.Count;j++){var tangent=(path[Mathf.Min(j+1,path.Count-1)]-path[Mathf.Max(0,j-1)]).normalized;var x=Vector3.Cross(tangent,Vector3.forward).normalized;if(x.sqrMagnitude<.01f)x=Vector3.right;var z=Vector3.Cross(tangent,x).normalized;float r=radius*Mathf.Lerp(1,.08f,(float)j/(path.Count-1));for(int i=0;i<sides;i++){float a=i*2*Mathf.PI/sides;geo.Add(path[j]+(x*Mathf.Cos(a)+z*Mathf.Sin(a))*r,col);}}
  for(int j=0;j<path.Count-1;j++)for(int i=0;i<sides;i++){int a=start+j*sides+i,b=start+j*sides+(i+1)%sides,c=b+sides,d=a+sides;Vector3 normal=(geo.v[a]+geo.v[b])*.5f-path[j];geo.Tri(a,b,c,normal);geo.Tri(a,c,d,normal);}
 }
 public static string Crear()
 {
  var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(GameObject.Find("Cenote_Remodelado"))return "Ya existe remodelado";
  EditorSceneManager.SaveScene(scene,"Assets/Scenes/CenoteVivo_AntesRemodelado.unity",true);Directory.CreateDirectory(Dir);AssetDatabase.Refresh();
  var anchor=GameObject.Find("CenoteVivoRoot");if(!anchor)anchor=Resources.FindObjectsOfTypeAll<GameObject>().First(g=>g.scene.IsValid()&&g.name=="CenoteVivoRoot");
  anchor.SetActive(true);var root=new GameObject("Cenote_Remodelado");root.transform.SetParent(anchor.transform,false);
  var rock=Material("CenoteVivo/RocaNatural","Roca_Caliza");rock.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture>("Assets/Textures/marble_cliff_05_diff_1k.jpg"));
  var plants=Material("CenoteVivo/VegetacionNatural","Raices_Follaje");
  var wall=new Geo();
  for(int j=0;j<ys.Length-1;j++)for(int i=0;i<N;i++)
  {
   bool hole=portals.Any(p=>j>=p.low&&j<p.high&&i>=p.center-3&&i<p.center+3);if(hole)continue;
   var a=Point(j,i);var b=Point(j,i+1);var c=Point(j+1,i+1);var d=Point(j+1,i);wall.Quad(a,b,c,d,new Vector3(-a.x,0,-a.z),RockColor((a.y+d.y)*.5f));
  }
  int center=wall.Add(new Vector3(0,-8.4f,0),RockColor(-8));for(int i=0;i<N;i++){int a=wall.Add(Point(0,i),RockColor(-8)),b=wall.Add(Point(0,i+1),RockColor(-8));wall.Tri(center,a,b,Vector3.up);}
  var cave=Render("CavidadCaliza",wall.Mesh("CavidadCaliza"),rock,root.transform);
  var ground=new Geo();
  for(int band=0;band<5;band++)for(int i=0;i<N;i++)
  {
   float aa=i*2*Mathf.PI/N,ab=(i+1)*2*Mathf.PI/N;Vector3 a=Point(11,i),b=Point(11,i+1);var oa=new Vector3(Mathf.Cos(aa)*23,6.1f+Mathf.Sin(aa*5)*.15f,Mathf.Sin(aa)*24);var ob=new Vector3(Mathf.Cos(ab)*23,6.1f+Mathf.Sin(ab*5)*.15f,Mathf.Sin(ab)*24);
   ground.Quad(Vector3.Lerp(a,oa,band/5f),Vector3.Lerp(b,ob,band/5f),Vector3.Lerp(b,ob,(band+1)/5f),Vector3.Lerp(a,oa,(band+1)/5f),Vector3.up,new Color(.59f,.65f,.4f));
  }
  Render("BordeTerreno",ground.Mesh("BordeTerreno"),rock,root.transform);
  var outside=new Geo();for(int j=0;j<6;j++)for(int i=0;i<N;i++)
  {
   float a=i*2*Mathf.PI/N,b=(i+1)*2*Mathf.PI/N;float y0=Mathf.Lerp(-8.4f,6.1f,j/6f),y1=Mathf.Lerp(-8.4f,6.1f,(j+1)/6f);float r0=Mathf.Lerp(3,23,Mathf.Sqrt(j/6f)),r1=Mathf.Lerp(3,23,Mathf.Sqrt((j+1)/6f));
   outside.Quad(new Vector3(Mathf.Cos(a)*r0,y0,Mathf.Sin(a)*r0*24/23),new Vector3(Mathf.Cos(b)*r0,y0,Mathf.Sin(b)*r0*24/23),new Vector3(Mathf.Cos(b)*r1,y1,Mathf.Sin(b)*r1*24/23),new Vector3(Mathf.Cos(a)*r1,y1,Mathf.Sin(a)*r1*24/23),new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)),new Color(.64f,.65f,.5f));
  }
  Render("ExteriorCaliza",outside.Mesh("ExteriorCaliza"),rock,root.transform);
  int tunnelIndex=0;
  foreach(var p in portals)
  {
   var boundary=new List<Vector3>();for(int i=p.center-3;i<=p.center+3;i++)boundary.Add(Point(p.low,i));for(int j=p.low+1;j<=p.high;j++)boundary.Add(Point(j,p.center+3));for(int i=p.center+2;i>=p.center-3;i--)boundary.Add(Point(p.high,i));for(int j=p.high-1;j>p.low;j--)boundary.Add(Point(j,p.center-3));
   Vector3 origin=boundary.Aggregate(Vector3.zero,(s,v)=>s+v)/boundary.Count;float angle=p.center*2*Mathf.PI/N;var dir=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));var side=Vector3.Cross(Vector3.up,dir);var geo=new Geo();int count=boundary.Count;
   for(int k=0;k<=8;k++){float d=k*1.8f;var cc=origin+dir*d+side*Mathf.Sin(d*.12f)*1.8f+Vector3.down*d*.035f;var q=Quaternion.AngleAxis(k*1.2f,Vector3.up);for(int i=0;i<count;i++)geo.Add(k==0?boundary[i]:cc+q*(boundary[i]-origin)*Mathf.Lerp(1,.72f,k/8f),RockColor(cc.y)*Mathf.Lerp(.94f,.10f,k/8f));}
   for(int k=0;k<8;k++)for(int i=0;i<count;i++){int a=k*count+i,b=k*count+(i+1)%count,c=b+count,d=a+count;var mid=(geo.v[a]+geo.v[b])*.5f;var axis=origin+dir*(k*1.8f);var facing=axis-mid;facing-=dir*Vector3.Dot(facing,dir);geo.Tri(a,b,c,facing);geo.Tri(a,c,d,facing);}
   int cap=geo.Add(geo.v.Skip(8*count).Take(count).Aggregate(Vector3.zero,(s,v)=>s+v)/count,new Color(.018f,.025f,.022f));for(int i=0;i<count;i++)geo.Tri(cap,8*count+i,8*count+(i+1)%count,-dir);
   Render("Tunel_"+(++tunnelIndex).ToString("00"),geo.Mesh("Tunel"+tunnelIndex),rock,root.transform);
   var marker=new GameObject("EntradaTunel_"+tunnelIndex);marker.transform.SetParent(root.transform,false);marker.transform.localPosition=origin;marker.transform.localRotation=Quaternion.LookRotation(dir);
  }
  // Disco de agua que llega a la pared en su cota exacta.
  var waterGeo=new Geo();int wc=waterGeo.Add(new Vector3(0,1.8f,0),Color.white);for(int i=0;i<N;i++){var wp=Point(6,i);wp.y=1.8f;waterGeo.Add(wp,Color.white);}for(int i=0;i<N;i++)waterGeo.Tri(wc,1+i,1+(i+1)%N,Vector3.up);
  var waterMat=Material("CenoteVivo/AguaNatural","Agua_Turquesa");
  const int texSize=128;var tex=new Texture2D(texSize,texSize,TextureFormat.RGBA32,true,true);
  for(int y=0;y<texSize;y++)for(int x=0;x<texSize;x++){float a=x*2*Mathf.PI/texSize,b=y*2*Mathf.PI/texSize;float dx=.48f*Mathf.Cos(a*3+b*2)+.26f*Mathf.Cos(a*7-b*4)+.16f*Mathf.Sin(a*11+b*5);float dz=.48f*Mathf.Sin(b*3-a*2)+.26f*Mathf.Sin(b*7+a*4)+.16f*Mathf.Cos(b*11-a*5);tex.SetPixel(x,y,new Color(dx*.5f+.5f,dz*.5f+.5f,1,1));}tex.Apply();File.WriteAllBytes(Dir+"/Ondulaciones.png",tex.EncodeToPNG());Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(Dir+"/Ondulaciones.png");var importer=(TextureImporter)AssetImporter.GetAtPath(Dir+"/Ondulaciones.png");importer.sRGBTexture=false;importer.wrapMode=TextureWrapMode.Repeat;importer.mipmapEnabled=true;importer.SaveAndReimport();waterMat.SetTexture("_RippleMap",AssetDatabase.LoadAssetAtPath<Texture>(Dir+"/Ondulaciones.png"));
  Render("AguaRemodelada",waterGeo.Mesh("AguaRemodelada"),waterMat,root.transform);
  var oldRandom=Random.state;Random.InitState(9421);int leaves=0,roots=0;
  for(int sector=0;sector<8;sector++)
  {
   var geo=new Geo();
   for(int r=0;r<5;r++)
   {
    float a=(sector+(r+.4f)/5)*2*Mathf.PI/8;Vector3 top=Point(11,Mathf.RoundToInt(a*N/(2*Mathf.PI)));var radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
    float length=Random.Range(2.2f,6.8f);var path=new List<Vector3>();for(int j=0;j<=12;j++){float t=j/12f;path.Add(top-radial*(.12f+t*.6f)+Vector3.down*(length*t)+new Vector3(Mathf.Sin(t*7+a)*.12f,0,Mathf.Cos(t*8+a)*.12f));}
    Tube(geo,path,Random.Range(.035f,.075f),new Color(.27f,.20f,.105f));roots++;
    var branch=new List<Vector3>();for(int j=0;j<=6;j++)branch.Add(path[5]+new Vector3(Mathf.Sin(a+1)*j*.10f,-j*.25f,Mathf.Cos(a+1)*j*.10f));Tube(geo,branch,.025f,new Color(.25f,.19f,.1f));
    for(int l=0;l<Random.Range(5,10);l++){float t=Random.Range(.02f,.65f);var pos=path[Mathf.RoundToInt(t*12)];Leaf(geo,pos,new Vector3(Random.Range(-1f,1),Random.Range(-.4f,.3f),Random.Range(-1f,1)),Random.Range(.25f,.55f),Random.Range(.06f,.13f),Color.Lerp(new Color(.09f,.27f,.09f),new Color(.26f,.43f,.12f),Random.value));leaves++;}
    for(int l=0;l<15;l++){var pos=top+radial*Random.Range(0,.8f)+new Vector3(Random.Range(-.45f,.45f),.08f,Random.Range(-.45f,.45f));Leaf(geo,pos,new Vector3(Random.Range(-1f,1),Random.Range(.05f,.4f),Random.Range(-1f,1)),Random.Range(.35f,.85f),Random.Range(.08f,.2f),Color.Lerp(new Color(.10f,.28f,.075f),new Color(.28f,.46f,.13f),Random.value));leaves++;}
   }
   Render("VegetacionBorde_"+sector,geo.Mesh("VegetacionBorde"+sector),plants,root.transform);
  }
  Random.state=oldRandom;
  // Encaja de nuevo las estalactitas existentes en el techo remodelado.
  var col=cave.AddComponent<MeshCollider>();col.sharedMesh=cave.GetComponent<MeshFilter>().sharedMesh;Physics.SyncTransforms();bool oldBack=Physics.queriesHitBackfaces;Physics.queriesHitBackfaces=true;int attached=0;
  var density=Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g=>g.scene.IsValid()&&g.name=="Estalactitas_Densidad");
  if(density){foreach(var f in density.GetComponentsInChildren<MeshFilter>(true).Where(f=>f.name.StartsWith("Estalactita_"))){var renderer=f.GetComponent<Renderer>();var b=renderer.bounds;RaycastHit hit;if(col.Raycast(new Ray(new Vector3(b.center.x,-1.5f,b.center.z),Vector3.up),out hit,12)){f.transform.position+=Vector3.up*(hit.point.y+.16f-b.max.y);attached++;}}DensidadEstalactitas.Reconstruir();}
  foreach(var f in Resources.FindObjectsOfTypeAll<MeshFilter>().Where(f=>f.gameObject.scene.IsValid()&&f.name.StartsWith("Estalactita")&&!f.name.StartsWith("Estalactita_")&&f.transform.IsChildOf(anchor.transform))){var b=f.GetComponent<Renderer>().bounds;RaycastHit hit;if(col.Raycast(new Ray(new Vector3(b.center.x,-2,b.center.z),Vector3.up),out hit,12))f.transform.position+=Vector3.up*(hit.point.y+.12f-b.max.y);}
  Physics.queriesHitBackfaces=oldBack;Object.DestroyImmediate(col);
  foreach(var r in Resources.FindObjectsOfTypeAll<MeshRenderer>().Where(r=>r.gameObject.scene.IsValid()&&new[]{"CenoteBody","ExteriorFloor","Water","RayosSubacuaticos","GodRaysFinal"}.Contains(r.name)))r.gameObject.SetActive(false);
  var camera=Camera.main;camera.GetUniversalAdditionalCameraData().requiresDepthTexture=true;
  var menu=Object.FindAnyObjectByType<MenuCenote>();if(menu)menu.MostrarInicio();
  EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Selection.activeGameObject=root;
  int tris=root.GetComponentsInChildren<MeshFilter>(true).Sum(f=>f.sharedMesh.triangles.Length/3);
  return "Remodelado: "+tris+" triangulos, 3 túneles de 14.4 unidades, "+roots+" raíces, "+leaves+" hojas, "+attached+" estalactitas reajustadas";
 }
}
