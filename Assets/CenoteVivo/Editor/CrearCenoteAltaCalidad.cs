using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public static class CrearCenoteAltaCalidad
{
 const string Dir="Assets/CenoteVivo/AltaCalidad";
 class Geo
 {
  public List<Vector3> v=new List<Vector3>();public List<Color> c=new List<Color>();public List<Vector2> uv=new List<Vector2>();public List<int> t=new List<int>();
  public int Add(Vector3 p,Color col,Vector2 q=default){v.Add(p);c.Add(col);uv.Add(q);return v.Count-1;}
  public void Tri(int a,int b,int d,Vector3 n){if(Vector3.Dot(Vector3.Cross(v[b]-v[a],v[d]-v[a]),n)<0){int x=b;b=d;d=x;}t.Add(a);t.Add(b);t.Add(d);}
  public Mesh Mesh(string name){var m=new Mesh();m.name=name;m.indexFormat=IndexFormat.UInt32;m.SetVertices(v);m.SetColors(c);m.SetUVs(0,uv);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
 }
 static float Noise(Vector3 p){return Mathf.Sin(p.x*1.9f+Mathf.Sin(p.z*1.4f))*Mathf.Cos(p.y*2.8f+p.z*.9f)+.35f*Mathf.Sin(p.x*7+p.y*5+p.z*6);}
 static Mesh Refine(Mesh src,int levels)
 {
  var v=src.vertices.ToList();var cols=src.colors.Length==v.Count?src.colors.ToList():v.Select(p=>Color.white).ToList();var tris=src.triangles.ToList();
  for(int step=0;step<levels;step++)
  {
   var edges=new Dictionary<long,int>();var output=new List<int>();
   System.Func<int,int,int> mid=(a,b)=>{long key=((long)Mathf.Min(a,b)<<32)|(uint)Mathf.Max(a,b);int idx;if(edges.TryGetValue(key,out idx))return idx;idx=v.Count;v.Add((v[a]+v[b])*.5f);cols.Add((cols[a]+cols[b])*.5f);edges[key]=idx;return idx;};
   for(int i=0;i<tris.Count;i+=3){int a=tris[i],b=tris[i+1],c=tris[i+2],ab=mid(a,b),bc=mid(b,c),ca=mid(c,a);output.AddRange(new[]{a,ab,ca,ab,b,bc,ca,bc,c,ab,bc,ca});}tris=output;
  }
  for(int i=0;i<v.Count;i++){var p=v[i];var radial=new Vector3(p.x,0,p.z).normalized;v[i]=p+radial*Noise(p)*.13f+Vector3.up*Noise(p*.73f+Vector3.one)*.055f;}
  var m=new Mesh();m.indexFormat=IndexFormat.UInt32;m.SetVertices(v);m.SetColors(cols);m.SetTriangles(tris,0);m.RecalculateNormals();m.RecalculateBounds();return m;
 }
 static GameObject Render(string name,Mesh mesh,Material material,Transform parent,bool shadows=true)
 {
  mesh.name=name;AssetDatabase.CreateAsset(mesh,Dir+"/"+name+".asset");var g=new GameObject(name);g.transform.SetParent(parent,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=shadows?ShadowCastingMode.On:ShadowCastingMode.Off;return g;
 }
 static Material Mat(string name,Texture baseTex,Texture normal,Texture moss,Color tint,float vegetation=0)
 {
  var m=new Material(Shader.Find("CenoteVivo/HD/MaterialNatural"));m.name=name;m.SetTexture("_BaseMap",baseTex);m.SetTexture("_NormalMap",normal);m.SetTexture("_MossMap",moss);m.SetColor("_Tint",tint);m.SetFloat("_Vegetation",vegetation);m.SetFloat("_Cull",vegetation>0?0:2);AssetDatabase.CreateAsset(m,Dir+"/"+name+".mat");return m;
 }
 static void Tube(Geo g,List<Vector3> path,float radius,Color col,int sides=10)
 {
  int start=g.v.Count;
  for(int j=0;j<path.Count;j++){var tangent=(path[Mathf.Min(j+1,path.Count-1)]-path[Mathf.Max(0,j-1)]).normalized;var x=Vector3.Cross(tangent,Vector3.forward).normalized;if(x.sqrMagnitude<.01f)x=Vector3.right;var z=Vector3.Cross(tangent,x);float taper=Mathf.Lerp(1,.13f,(float)j/(path.Count-1));for(int i=0;i<sides;i++){float a=i*2*Mathf.PI/sides;float r=radius*taper*(1+.08f*Mathf.Sin(i*2.5f+j*.6f));g.Add(path[j]+(x*Mathf.Cos(a)+z*Mathf.Sin(a))*r,col,new Vector2((float)i/sides,j*.3f));}}
  for(int j=0;j<path.Count-1;j++)for(int i=0;i<sides;i++){int a=start+j*sides+i,b=start+j*sides+(i+1)%sides,c=b+sides,d=a+sides;g.Tri(a,b,c,g.v[a]-path[j]);g.Tri(a,c,d,g.v[a]-path[j]);}
 }
 static void Leaf(Geo g,Vector3 origin,Vector3 dir,float len,float width,Color color)
 {
  dir.Normalize();var side=Vector3.Cross(dir,Vector3.up).normalized;if(side.sqrMagnitude<.01f)side=Vector3.right;int first=g.v.Count;const int rows=9;
  for(int j=0;j<rows;j++){float t=j/(rows-1f),w=Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*Mathf.PI)),.8f)*width;var p=origin+dir*len*t+Vector3.up*(Mathf.Sin(t*Mathf.PI)*len*.12f-t*t*len*.18f);for(int i=0;i<3;i++){float x=i-1;var c=color*Mathf.Lerp(.75f,1.12f,Mathf.Sin(t*Mathf.PI));c.a=1;g.Add(p+side*x*w-Vector3.up*Mathf.Abs(x)*w*.18f,c,new Vector2(i*.5f,t));}}
  for(int j=0;j<rows-1;j++)for(int i=0;i<2;i++){int a=first+j*3+i,b=a+1,c=b+3,d=a+3;g.Tri(a,b,c,Vector3.up);g.Tri(a,c,d,Vector3.up);}
 }
 static void Formation(Geo g,Vector3 basePos,float length,float width,float seed,bool hanging=true)
 {
  const int sides=18,rings=20;int start=g.v.Count;float sign=hanging?-1:1;
  for(int j=0;j<rings;j++){float t=j/(rings-1f);float r=width*Mathf.Pow(1-t,1.4f)*(1+.06f*Mathf.Sin(t*45+seed));if(j==rings-1)r=.005f;var center=basePos+new Vector3(Mathf.Sin(t*2+seed)*t*t*length*.04f,sign*t*length,Mathf.Cos(t*2+seed)*t*t*length*.04f);for(int i=0;i<sides;i++){float a=i*2*Mathf.PI/sides;float rr=r*(1+.1f*Mathf.Sin(a*5+seed+t*2)+.05f*Mathf.Sin(a*9+seed));var c=new Color(.77f,.75f,.65f,0);g.Add(center+new Vector3(Mathf.Cos(a)*rr,0,Mathf.Sin(a)*rr),c);}}
  for(int j=0;j<rings-1;j++)for(int i=0;i<sides;i++){int a=start+j*sides+i,b=start+j*sides+(i+1)%sides,c=b+sides,d=a+sides;var facing=g.v[a]-(basePos+Vector3.up*sign*j/(rings-1f)*length);g.Tri(a,b,c,facing);g.Tri(a,c,d,facing);}
 }
 public static string Crear()
 {
  string scenePath="Assets/Scenes/CenoteVivo_AltaCalidad.unity";if(File.Exists(scenePath))return "La escena HD ya existe; no se sobrescribió";
  EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());Directory.CreateDirectory(Dir);AssetDatabase.Refresh();
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var root=new GameObject("Cenote_AltaCalidad");
  var normalPath=Dir+"/Caliza_Normal.exr";AssetDatabase.CopyAsset("Assets/Textures/marble_cliff_05_nor_gl_1k.exr",normalPath);var importer=(TextureImporter)AssetImporter.GetAtPath(normalPath);importer.textureType=TextureImporterType.NormalMap;importer.SaveAndReimport();
  var normal=AssetDatabase.LoadAssetAtPath<Texture>(normalPath);var rockTex=AssetDatabase.LoadAssetAtPath<Texture>("Assets/Textures/marble_cliff_05_diff_1k.jpg");var moss=AssetDatabase.LoadAssetAtPath<Texture>("Assets/Textures/mossy_rock_diff_1k.jpg");
  var rock=Mat("Caliza_HD",rockTex,normal,moss,new Color(.95f,.93f,.82f));var wood=Mat("RaicesMadera_HD",rockTex,normal,moss,new Color(.23f,.15f,.075f));wood.SetFloat("_Scale",1.3f);wood.SetFloat("_Wet",.15f);
  var leafMat=Mat("Follaje_HD",rockTex,normal,moss,Color.white,1);
  GameObject cave=null;
  foreach(var name in new[]{"CavidadCaliza","BordeTerreno","ExteriorCaliza","Tunel_01","Tunel_02","Tunel_03"})
  {var src=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/CenoteVivo/Remodelado/"+name+".asset");if(!src)continue;var obj=Render(name+"_HD",Refine(src,2),rock,root.transform);if(name=="CavidadCaliza")cave=obj;}
  var collider=cave.AddComponent<MeshCollider>();collider.sharedMesh=cave.GetComponent<MeshFilter>().sharedMesh;Physics.SyncTransforms();bool oldBack=Physics.queriesHitBackfaces;Physics.queriesHitBackfaces=true;var previousRandom=Random.state;Random.InitState(616322);
  int formations=0,leaves=0,trees=0,roots=0;
  var mineralRoot=new GameObject("FormacionesCalcareas");mineralRoot.transform.SetParent(root.transform,false);
  for(int sector=0;sector<24;sector++)
  {
   var geo=new Geo();float a=sector*2*Mathf.PI/24;
   for(int k=0;k<25;k++){float angle=a+Random.Range(-.15f,.15f);float r=Random.Range(7.7f,12.5f);var p=new Vector3(Mathf.Cos(angle)*r,-1,Mathf.Sin(angle)*r);RaycastHit hit;if(!collider.Raycast(new Ray(p,Vector3.up),out hit,12)||hit.point.y<2.4f)continue;float l=k<7?Random.Range(1.8f,4.3f):Random.Range(.3f,1.7f);float width=l*Random.Range(.13f,.27f);Formation(geo,hit.point+Vector3.up*.12f,l,width,Random.value*20);formations++;}
   for(int k=0;k<3;k++){float angle=a+Random.Range(-.15f,.15f),r=Random.Range(10,12);var p=new Vector3(Mathf.Cos(angle)*r,1,Mathf.Sin(angle)*r);RaycastHit hit;if(collider.Raycast(new Ray(p,Vector3.down),out hit,15)){Formation(geo,hit.point-Vector3.up*.08f,Random.Range(.4f,1.8f),Random.Range(.18f,.4f),Random.value*20,false);formations++;}}
   if(geo.v.Count>0)Render("Minerales_"+sector,geo.Mesh("Minerales"+sector),rock,mineralRoot.transform);
  }
  var plantsRoot=new GameObject("VegetacionDensa");plantsRoot.transform.SetParent(root.transform,false);
  for(int sector=0;sector<16;sector++)
  {
   var branches=new Geo();var foliage=new Geo();float angle=(sector+.3f)*2*Mathf.PI/16;var radial=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));var side=Vector3.Cross(Vector3.up,radial);
   // Árbol con tronco inclinado y ramas alrededor del borde.
   Vector3 treeBase=radial*Random.Range(8.1f,10.5f)+Vector3.up*6.45f;float h=Random.Range(3.5f,6.5f),lean=Random.Range(.7f,1.6f);var trunk=new List<Vector3>();
   for(int j=0;j<=16;j++){float t=j/16f;trunk.Add(treeBase+Vector3.up*h*t-radial*(t*t*lean)+side*Mathf.Sin(t*2)*.3f);}Tube(branches,trunk,Random.Range(.18f,.33f),new Color(.9f,.8f,.6f,0),14);trees++;
   for(int b=0;b<11;b++)
   {
    var start=trunk[Random.Range(7,16)];var dir=Quaternion.AngleAxis(Random.Range(-85f,85f),Vector3.up)*(-radial);float length=Random.Range(1.5f,3.5f);var path=new List<Vector3>();
    for(int j=0;j<=8;j++){float t=j/8f;path.Add(start+dir*length*t+Vector3.up*(Mathf.Sin(t*Mathf.PI)*.35f+t*.5f));}Tube(branches,path,Random.Range(.05f,.09f),new Color(.9f,.8f,.6f,0),9);
    for(int k=0;k<85;k++){float t=Random.Range(.3f,1);var p=path[Mathf.RoundToInt(t*8)]+Random.insideUnitSphere*.85f;Leaf(foliage,p,new Vector3(Random.Range(-1f,1),Random.Range(-.25f,.2f),Random.Range(-1f,1)),Random.Range(.45f,.9f),Random.Range(.14f,.25f),Color.Lerp(new Color(.045f,.13f,.018f),new Color(.16f,.34f,.055f),Random.value));leaves++;}
   }
   // Raíces y lianas de longitud variable, con ramificaciones y hojas alternas.
   for(int vine=0;vine<7;vine++)
   {
    float aa=angle+(vine-3)*.045f;var rr=new Vector3(Mathf.Cos(aa),0,Mathf.Sin(aa));var top=rr*(7.3f+.2f*Mathf.Sin(aa*3))+Vector3.up*6.6f;var path=new List<Vector3>();float length=Random.Range(2.1f,7.5f);
    for(int j=0;j<=22;j++){float t=j/22f;path.Add(top-rr*(.2f+.65f*Mathf.Sin(t*Mathf.PI))+Vector3.down*length*t+side*Mathf.Sin(t*8+vine)*.15f);}
    Tube(branches,path,vine%2==0?.055f:.022f,new Color(.9f,.8f,.6f,0),8);roots++;
    if(vine%2==1)for(int j=1;j<20;j++){if(Random.value<.18f)continue;var dir=side*(j%2==0?1:-1)-rr*Random.Range(.15f,.65f)+Vector3.down*Random.Range(-.25f,.5f);Leaf(foliage,path[j]+Random.insideUnitSphere*.07f,dir,Random.Range(.3f,.65f),Random.Range(.09f,.17f),Color.Lerp(new Color(.035f,.11f,.025f),new Color(.12f,.25f,.05f),Random.value));leaves++;}
   }
   // Helechos en el labio de la abertura.
   for(int f=0;f<8;f++)
   {
    float aa=angle+Random.Range(-.13f,.13f);var at=new Vector3(Mathf.Cos(aa),0,Mathf.Sin(aa))*Random.Range(7.4f,8.3f)+Vector3.up*6.6f;var dir=Quaternion.AngleAxis(Random.Range(-90f,90f),Vector3.up)*(-radial);var across=Vector3.Cross(dir,Vector3.up);
    for(int j=1;j<=10;j++){float t=j/11f;var p=at+dir*t*1.1f+Vector3.up*Mathf.Sin(t*Mathf.PI)*.45f;for(int sign=-1;sign<=1;sign+=2){Leaf(foliage,p,across*sign+dir*.35f,(1-t)*.38f+.05f,.045f,new Color(.06f,.17f,.025f));leaves++;}}
   }
   Render("TroncosRaices_"+sector,branches.Mesh("TroncosRaices"+sector),wood,plantsRoot.transform);
   Render("HojasHelechos_"+sector,foliage.Mesh("HojasHelechos"+sector),leafMat,plantsRoot.transform);
  }
  Physics.queriesHitBackfaces=oldBack;Object.DestroyImmediate(collider);Random.state=previousRandom;
  // Agua densa en vértices con refracción y reflejos de sonda.
  var wg=new Geo();const int sides=192,rings=28;wg.Add(new Vector3(0,1.8f,0),Color.white);
  for(int j=1;j<=rings;j++)for(int i=0;i<sides;i++){float a=i*2*Mathf.PI/sides,r=j/(float)rings*(13.1f+.48f*Mathf.Sin(a*3+.4f));wg.Add(new Vector3(Mathf.Cos(a)*r,1.8f,Mathf.Sin(a)*r*1.07f),Color.white);}
  for(int i=0;i<sides;i++)wg.Tri(0,1+i,1+(i+1)%sides,Vector3.up);
  for(int j=0;j<rings-1;j++)for(int i=0;i<sides;i++){int a=1+j*sides+i,b=1+j*sides+(i+1)%sides,c=b+sides,d=a+sides;wg.Tri(a,b,c,Vector3.up);wg.Tri(a,c,d,Vector3.up);}
  var wm=new Material(Shader.Find("CenoteVivo/HD/AguaCristalina"));wm.name="Agua_HD";wm.SetTexture("_RippleMap",AssetDatabase.LoadAssetAtPath<Texture>("Assets/CenoteVivo/Remodelado/Ondulaciones.png"));AssetDatabase.CreateAsset(wm,Dir+"/Agua_HD.mat");var water=Render("EspejoAgua_HD",wg.Mesh("EspejoAgua"),wm,root.transform,false);water.layer=4;
  var sun=new GameObject("SolEntreElDosel").AddComponent<Light>();sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(62,-32,0);sun.color=new Color(1,.91f,.73f);sun.intensity=2.2f;sun.shadows=LightShadows.Soft;sun.shadowBias=.025f;sun.shadowNormalBias=.15f;RenderSettings.sun=sun;
  var fill=new GameObject("LuzIndirectaAgua").AddComponent<Light>();fill.type=LightType.Point;fill.transform.position=new Vector3(0,3,-2);fill.range=26;fill.intensity=16;fill.color=new Color(.58f,.83f,.77f);fill.shadows=LightShadows.None;
  var sky=new Material(Shader.Find("Skybox/Procedural"));sky.SetColor("_SkyTint",new Color(.46f,.58f,.62f));sky.SetFloat("_Exposure",.85f);sky.SetFloat("_AtmosphereThickness",.85f);AssetDatabase.CreateAsset(sky,Dir+"/Cielo_HD.mat");RenderSettings.skybox=sky;
  RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.34f,.4f,.35f);RenderSettings.ambientEquatorColor=new Color(.17f,.22f,.18f);RenderSettings.ambientGroundColor=new Color(.08f,.11f,.09f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.13f,.25f,.22f);RenderSettings.fogDensity=.012f;
  var camera=new GameObject("Main Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.transform.position=new Vector3(-.8f,3.9f,-9.4f);camera.transform.LookAt(new Vector3(.5f,3,-.5f));camera.fieldOfView=66;camera.nearClipPlane=.08f;camera.farClipPlane=140;camera.allowHDR=true;camera.allowMSAA=true;camera.gameObject.AddComponent<AudioListener>();var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.requiresDepthTexture=true;data.requiresColorTexture=true;data.volumeLayerMask=1;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
  var probe=new GameObject("ReflejosDelCenote").AddComponent<ReflectionProbe>();probe.transform.position=new Vector3(0,3,0);probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.IndividualFaces;probe.resolution=512;probe.hdr=true;probe.size=new Vector3(36,30,36);probe.boxProjection=true;probe.cullingMask=~(1<<4);probe.backgroundColor=RenderSettings.fogColor;
  var profile=ScriptableObject.CreateInstance<VolumeProfile>();var tone=profile.Add<Tonemapping>(true);tone.mode.value=TonemappingMode.ACES;var bloom=profile.Add<Bloom>(true);bloom.intensity.value=.20f;bloom.threshold.value=1.15f;bloom.scatter.value=.6f;bloom.highQualityFiltering.value=true;var colors=profile.Add<ColorAdjustments>(true);colors.postExposure.value=.5f;colors.contrast.value=8;colors.saturation.value=7;var vignette=profile.Add<Vignette>(true);vignette.intensity.value=.13f;vignette.smoothness.value=.6f;AssetDatabase.CreateAsset(profile,Dir+"/ColorCinematico_HD.asset");var vol=new GameObject("TratamientoCinematico").AddComponent<Volume>();vol.isGlobal=true;vol.sharedProfile=profile;
  var source=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset");var rp=Object.Instantiate(source);rp.name="Cenote_AltaCalidad_RP";rp.supportsHDR=true;rp.msaaSampleCount=4;rp.renderScale=1.15f;rp.mainLightShadowmapResolution=4096;rp.shadowDistance=75;rp.shadowCascadeCount=4;var serializedRp=new SerializedObject(rp);serializedRp.FindProperty("m_AdditionalLightsRenderingMode").intValue=(int)LightRenderingMode.PerPixel;serializedRp.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.CreateAsset(rp,Dir+"/Cenote_AltaCalidad_RP.asset");var graphics=new GameObject("PerfilGraficoSoloEstaEscena").AddComponent<PerfilGraficoHD>();graphics.perfil=rp;
  var schoolObj=new GameObject("VidaAcuatica_HD");schoolObj.transform.SetParent(root.transform,false);schoolObj.transform.localPosition=new Vector3(0,-1.5f,0);var school=schoolObj.AddComponent<CardumenCenote>();school.prefabPez=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CenoteVivo/Prefabs/PezCenote.prefab");school.materialesVariantes=new[]{"Plata","Oliva","Arena","Azulado"}.Select(n=>AssetDatabase.LoadAssetAtPath<Material>("Assets/CenoteVivo/Materials/Pez_Var_"+n+".mat")).ToArray();school.cantidadMaxima=60;school.extension=new Vector3(9,2.2f,9);school.longitud=new Vector2(.24f,.45f);school.velocidad=new Vector2(.25f,.6f);
  DynamicGI.UpdateEnvironment();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),scenePath);AssetDatabase.SaveAssets();Selection.activeGameObject=root;
  int triangles=root.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);
  return "Escena HD creada: "+triangles+" triángulos estáticos; "+formations+" formaciones; "+trees+" árboles; "+roots+" raíces/lianas; "+leaves+" hojas/frondas";
 }

 static void ActualizarMesh(string name,Mesh mesh){var asset=AssetDatabase.LoadAssetAtPath<Mesh>(Dir+"/"+name+".asset");mesh.name=name;asset.Clear();asset.indexFormat=mesh.indexFormat;asset.vertices=mesh.vertices;asset.colors=mesh.colors;asset.uv=mesh.uv;asset.triangles=mesh.triangles;asset.RecalculateNormals();asset.RecalculateBounds();asset.UploadMeshData(false);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(asset);}
 public static string RepararFollaje(){var previous=Random.state;Random.InitState(616766);int trees=0,roots=0,leaves=0;
  for(int sector=0;sector<16;sector++)
  {
   var branches=new Geo();var foliage=new Geo();float angle=(sector+.3f)*2*Mathf.PI/16;var radial=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));var side=Vector3.Cross(Vector3.up,radial);
   // Árbol con tronco inclinado y ramas alrededor del borde.
   Vector3 treeBase=radial*Random.Range(8.1f,10.5f)+Vector3.up*6.45f;float h=Random.Range(3.5f,6.5f),lean=Random.Range(.7f,1.6f);var trunk=new List<Vector3>();
   for(int j=0;j<=16;j++){float t=j/16f;trunk.Add(treeBase+Vector3.up*h*t-radial*(t*t*lean)+side*Mathf.Sin(t*2)*.3f);}Tube(branches,trunk,Random.Range(.18f,.33f),new Color(.9f,.8f,.6f,0),14);trees++;
   for(int b=0;b<11;b++)
   {
    var start=trunk[Random.Range(7,16)];var dir=Quaternion.AngleAxis(Random.Range(-85f,85f),Vector3.up)*(-radial);float length=Random.Range(1.5f,3.5f);var path=new List<Vector3>();
    for(int j=0;j<=8;j++){float t=j/8f;path.Add(start+dir*length*t+Vector3.up*(Mathf.Sin(t*Mathf.PI)*.35f+t*.5f));}Tube(branches,path,Random.Range(.05f,.09f),new Color(.9f,.8f,.6f,0),9);
    for(int k=0;k<85;k++){float t=Random.Range(.3f,1);var p=path[Mathf.RoundToInt(t*8)]+Random.insideUnitSphere*.85f;Leaf(foliage,p,new Vector3(Random.Range(-1f,1),Random.Range(-.25f,.2f),Random.Range(-1f,1)),Random.Range(.45f,.9f),Random.Range(.14f,.25f),Color.Lerp(new Color(.045f,.13f,.018f),new Color(.16f,.34f,.055f),Random.value));leaves++;}
   }
   // Raíces y lianas de longitud variable, con ramificaciones y hojas alternas.
   for(int vine=0;vine<7;vine++)
   {
    float aa=angle+(vine-3)*.045f;var rr=new Vector3(Mathf.Cos(aa),0,Mathf.Sin(aa));var top=rr*(7.3f+.2f*Mathf.Sin(aa*3))+Vector3.up*6.6f;var path=new List<Vector3>();float length=Random.Range(2.1f,7.5f);
    for(int j=0;j<=22;j++){float t=j/22f;path.Add(top-rr*(.2f+.65f*Mathf.Sin(t*Mathf.PI))+Vector3.down*length*t+side*Mathf.Sin(t*8+vine)*.15f);}
    Tube(branches,path,vine%2==0?.055f:.022f,new Color(.9f,.8f,.6f,0),8);roots++;
    if(vine%2==1)for(int j=1;j<20;j++){if(Random.value<.18f)continue;var dir=side*(j%2==0?1:-1)-rr*Random.Range(.15f,.65f)+Vector3.down*Random.Range(-.25f,.5f);Leaf(foliage,path[j]+Random.insideUnitSphere*.07f,dir,Random.Range(.3f,.65f),Random.Range(.09f,.17f),Color.Lerp(new Color(.035f,.11f,.025f),new Color(.12f,.25f,.05f),Random.value));leaves++;}
   }
   // Helechos en el labio de la abertura.
   for(int f=0;f<8;f++)
   {
    float aa=angle+Random.Range(-.13f,.13f);var at=new Vector3(Mathf.Cos(aa),0,Mathf.Sin(aa))*Random.Range(7.4f,8.3f)+Vector3.up*6.6f;var dir=Quaternion.AngleAxis(Random.Range(-90f,90f),Vector3.up)*(-radial);var across=Vector3.Cross(dir,Vector3.up);
    for(int j=1;j<=10;j++){float t=j/11f;var p=at+dir*t*1.1f+Vector3.up*Mathf.Sin(t*Mathf.PI)*.45f;for(int sign=-1;sign<=1;sign+=2){Leaf(foliage,p,across*sign+dir*.35f,(1-t)*.38f+.05f,.045f,new Color(.06f,.17f,.025f));leaves++;}}
   }
   ActualizarMesh("TroncosRaices_"+sector,branches.Mesh("TroncosRaices"+sector));
   ActualizarMesh("HojasHelechos_"+sector,foliage.Mesh("HojasHelechos"+sector));
  }
Random.state=previous;AssetDatabase.SaveAssets();return "Vegetación regenerada: "+leaves+" hojas";}
}
