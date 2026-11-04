using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Edición puntual: conserva pantallas, acciones y referencias del menú existente.
[InitializeOnLoad]
public static class MejorarMenuCenote
{
    const string ScenePath = "Assets/Scenes/CenoteVivo_Peces.unity";
    const string Folder = "Assets/CenoteVivo/Resources/UI/";
    const string Pending = "Assets/CenoteVivo/Editor/MenuVisualPendiente.txt";
    static readonly Color Ink = Hex("EAF4EC"), Muted = Hex("AAC8C4"), Accent = Hex("70D9BF");
    static Sprite round;
    static MejorarMenuCenote() { EditorApplication.delayCall += TryPending; }
    static void TryPending()
    {
        if (!File.Exists(Pending)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += TryPending; return; }
        try { Apply(); File.Delete(Pending); AssetDatabase.Refresh(); }
        catch (Exception e) { Debug.LogException(e); File.WriteAllText("MenuVisual_Resultado.txt", e.ToString()); }
    }
    static Color Hex(string h) { ColorUtility.TryParseHtmlString("#"+h,out var c);return c; }
    static RectTransform R(Transform t) { return (RectTransform)t; }
    static void Box(Transform t,float x,float y,float w,float h)
    {
        var r=R(t);r.anchorMin=new Vector2(x,y);r.anchorMax=new Vector2(x+w,y+h);
        r.pivot=new Vector2(.5f,.5f);r.offsetMin=r.offsetMax=Vector2.zero;
    }
    static Transform Child(Transform p,string n) { var t=p.Find(n);if(!t)throw new Exception("Falta elemento: "+n);return t; }
    static Transform New(Transform p,string n)
    {
        var t=p.Find(n);if(t)return t;
        var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);return r;
    }
    static UnityEngine.UI.Image Image(Transform t,Color c,bool rounded=false)
    {
        var i=t.GetComponent<UnityEngine.UI.Image>();if(!i)i=t.gameObject.AddComponent<UnityEngine.UI.Image>();
        i.color=c;i.raycastTarget=false;
        if(rounded){i.sprite=round;i.type=UnityEngine.UI.Image.Type.Sliced;}
        return i;
    }
    static TMPro.TextMeshProUGUI Label(Transform t,string value,float size,Color color,bool bold=false)
    {
        var text=t.GetComponent<TMPro.TextMeshProUGUI>();
        if(!text){text=t.gameObject.AddComponent<TMPro.TextMeshProUGUI>();text.font=AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");}
        text.text=value;text.fontSize=size;text.color=color;text.fontStyle=bold?TMPro.FontStyles.Bold:TMPro.FontStyles.Normal;
        text.alignment=TMPro.TextAlignmentOptions.MidlineLeft;text.raycastTarget=false;
        text.enableAutoSizing=true;text.fontSizeMin=size*.75f;text.fontSizeMax=size;
        return text;
    }
    static void FreeLayout(Transform p)
    {
        foreach(var l in p.GetComponents<UnityEngine.UI.LayoutGroup>())l.enabled=false;
        var fit=p.GetComponent<UnityEngine.UI.ContentSizeFitter>();if(fit)fit.enabled=false;
    }
    static void PrepareSprites()
    {
        var importer=AssetImporter.GetAtPath(Folder+"Marista.png") as TextureImporter;
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        var path=Folder+"PanelRedondeado.png";
        if(!File.Exists(path))
        {
            var tex=new Texture2D(64,64,TextureFormat.RGBA32,false);
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
            {
                float dx=Mathf.Max(15.5f-x,x-47.5f,0),dy=Mathf.Max(15.5f-y,y-47.5f,0);
                tex.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(16-Mathf.Sqrt(dx*dx+dy*dy))));
            }
            tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);
        }
        var ri=(TextureImporter)AssetImporter.GetAtPath(path);ri.textureType=TextureImporterType.Sprite;ri.spriteImportMode=SpriteImportMode.Single;
        ri.spriteBorder=new Vector4(20,20,20,20);ri.mipmapEnabled=false;ri.textureCompression=TextureImporterCompression.Uncompressed;ri.SaveAndReimport();
        round=AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    [MenuItem("Cenote Vivo/Interfaz/Aplicar diseño de inicio y logo Marista")]
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Sal de Play antes de aplicar el diseño.");
        for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
            if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new Exception("Guarda los cambios de escena y ejecuta Cenote Vivo > Interfaz > Aplicar diseño de inicio y logo Marista.");
        string backup=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Respaldos/Menu_"+DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(backup);File.Copy(ScenePath,Path.Combine(backup,"CenoteVivo_Peces.unity"));
        var scene=EditorSceneManager.OpenScene(ScenePath);
        var menu=UnityEngine.Object.FindAnyObjectByType<MenuCenote>(FindObjectsInactive.Include);
        if(!menu)throw new Exception("La escena no contiene MenuCenote.");
        PrepareSprites();
        Undo.RegisterFullObjectHierarchyUndo(menu.gameObject,"Diseño de inicio Cenote Vivo");
        menu.logoUniversidad=AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"Marista.png");
        foreach(var logo in menu.espaciosLogo)if(logo){logo.sprite=menu.logoUniversidad;logo.preserveAspect=true;logo.enabled=true;logo.color=Color.white;logo.raycastTarget=false;Image(logo.transform.parent,Color.white,true);}
        foreach(var p in menu.etiquetasLogo)if(p)p.SetActive(false);
        var bg=menu.fondo.transform;Image(bg,Hex("082C34"));
        var top=bg.Find("LineaSuperior");if(top){Image(top,Accent);R(top).sizeDelta=new Vector2(0,3);}
        // Un panel tonal sin textura pesada ni animación continua.
        var glow=New(bg,"ProfundidadAgua");Box(glow,.56f,0,.44f,1);Image(glow,Hex("0C3941"));
        var safe=menu.zonaSegura;var home=menu.inicio.transform;
        var scroll=home.GetComponent<UnityEngine.UI.ScrollRect>();if(scroll)scroll.enabled=false;
        var view=Child(home,"Viewport");Box(view,.065f,.075f,.87f,.85f);
        var content=Child(view,"Contenido");FreeLayout(content);Box(content,0,0,1,1);
        var identity=Child(content,"Identidad");var nav=Child(content,"Navegacion");
        FreeLayout(identity);FreeLayout(nav);Box(identity,0,0,.52f,1);Box(nav,.59f,.09f,.41f,.82f);
        var kicker=Child(identity,"Lema");Box(kicker,0,.90f,1,.055f);Label(kicker,"YUCATÁN  /  PATRIMONIO NATURAL",24,Accent,true);
        var title=Child(identity,"Titulo");Box(title,0,.47f,1,.39f);var tt=Label(title,"Cenote <color=#70D9BF>Vivo</color>",118,Ink,true);tt.enableAutoSizing=true;tt.fontSizeMin=86;tt.lineSpacing=-12;
        var subtitle=Child(identity,"Subtitulo");Box(subtitle,0,.33f,.94f,.12f);Label(subtitle,"Un universo subterráneo\nque merece ser cuidado.",34,Muted);
        var card=Child(identity,"TarjetaLogoUniversidad");Box(card,0,.055f,.79f,.20f);Image(card,Color.white,true);
        var logoImage=Child(card,"LogoUniversidad");Box(logoImage,.055f,.12f,.89f,.76f);
        var university=Child(identity,"Universidad");Box(university,0,0,1,.04f);Label(university,"UNIVERSIDAD MARISTA DE MÉRIDA",19,Muted);
        var heading=New(nav,"Invitacion");Box(heading,0,.85f,1,.13f);Label(heading,"Tu viaje comienza aquí",37,Ink,true);
        var intro=New(nav,"Ayuda");Box(intro,0,.76f,1,.08f);Label(intro,"Explora. Comprende. Cuida.",25,Muted);
        StyleButton(Child(nav,"AccederAlCenote"),"Acceder al cenote",Accent,Hex("07343A"),0,.47f,1,.235f,36);
        StyleButton(Child(nav,"ObjetivoDelProyecto"),"Objetivo del proyecto",Hex("164A53"),Ink,0,.265f,1,.155f,31);
        StyleButton(Child(nav,"Creditos"),"Créditos",Hex("164A53"),Ink,0,.065f,1,.155f,31);
        var foot=New(content,"NotaConservacion");Box(foot,.59f,0,.41f,.055f);Label(foot,"CONOCER PARA CONSERVAR",20,Accent,true);
        var scaler=menu.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
        menu.MostrarInicio();Canvas.ForceUpdateCanvases();
        EditorUtility.SetDirty(menu);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        var buttons=menu.inicio.GetComponentsInChildren<UnityEngine.UI.Button>(true);
        var report="Diseño aplicado y guardado en "+ScenePath+"\nRespaldo: "+backup+"\nLogo: "+AssetDatabase.GetAssetPath(menu.logoUniversidad)+"\n";
        foreach(var b in buttons)report+=b.name+": "+R(b.transform).rect.size+", acciones="+b.onClick.GetPersistentEventCount()+"\n";
        report+="EventSystem: "+UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsInactive.Include).Length;
        File.WriteAllText("MenuVisual_Resultado.txt",report);Debug.Log(report);Selection.activeGameObject=menu.gameObject;
        EditorApplication.delayCall += CapturePreview;
    }
    static void CapturePreview()
    {
        try
        {
            var gameType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(gameType).Show();EditorWindow.GetWindow(gameType).Repaint();
            EditorApplication.delayCall += () =>
            {
                try
                {
                    var type=Type.GetType("Unity.Pipeline.Editor.Commands.Capture.CaptureCommands, Unity.Pipeline.Editor");
                    var method=type?.GetMethod("EncodeScreenToPng",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
                    if(method!=null)File.WriteAllBytes("MenuVisual_Preview.png",(byte[])method.Invoke(null,new object[]{1280,720}));
                }
                catch(Exception e){Debug.LogWarning("No se pudo capturar la vista: "+e.Message);}
            };
        }
        catch(Exception e){Debug.LogWarning("Vista previa: "+e.Message);}
    }
    static void StyleButton(Transform t,string label,Color bg,Color fg,float x,float y,float w,float h,float size)
    {
        Box(t,x,y,w,h);var image=Image(t,bg,true);image.raycastTarget=true;
        var b=t.GetComponent<UnityEngine.UI.Button>();b.targetGraphic=image;
        var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=Hex("C6EEE4");colors.selectedColor=Hex("C6EEE4");colors.pressedColor=Hex("80BDAF");colors.fadeDuration=.12f;b.colors=colors;
        var text=Child(t,"Etiqueta");Box(text,.07f,.12f,.80f,.76f);Label(text,label,size,fg,true);
        var arrow=New(t,"Indicador");Box(arrow,.87f,.12f,.08f,.76f);Label(arrow,"›",48,fg);
        foreach(var g in t.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))if(g!=image)g.raycastTarget=false;
    }
}
