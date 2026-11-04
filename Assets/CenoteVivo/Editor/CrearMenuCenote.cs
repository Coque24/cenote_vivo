using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public static class CrearMenuCenote
{
    static Color ink=new Color(.89f,.96f,.94f), muted=new Color(.59f,.75f,.73f), green=new Color(.24f,.75f,.61f);
    static TMPro.TMP_FontAsset font;
    static readonly List<UnityEngine.UI.Image> logos=new List<UnityEngine.UI.Image>();
    static readonly List<GameObject> placeholders=new List<GameObject>();
    static RectTransform Rect(string n,Transform p){var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);return r;}
    static void Fill(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
    static void Height(RectTransform r,float h){var l=r.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();l.preferredHeight=h;l.minHeight=h;l.flexibleHeight=0;}
    static TMPro.TextMeshProUGUI Text(string n,Transform p,string value,int size,float h=0,bool center=false)
    {
        var r=Rect(n,p);var t=r.gameObject.AddComponent<TMPro.TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=size;t.color=ink;t.raycastTarget=false;
        t.alignment=center?TMPro.TextAlignmentOptions.Center:TMPro.TextAlignmentOptions.TopLeft;
        if(h>0)Height(r,h);return t;
    }
    static UnityEngine.UI.Button Button(string n,Transform p,string label,UnityEngine.Events.UnityAction action,bool primary=false)
    {
        var r=Rect(n,p);Height(r,144);var image=r.gameObject.AddComponent<UnityEngine.UI.Image>();image.color=primary?green:new Color(.055f,.17f,.18f);
        var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=image;UnityEventTools.AddPersistentListener(b.onClick,action);
        var text=Text("Etiqueta",r,label,40,0,true);Fill(text.rectTransform);text.rectTransform.offsetMin=new Vector2(20,12);text.rectTransform.offsetMax=new Vector2(-20,-12);
        text.color=primary?new Color(.025f,.12f,.12f):ink;
        var colors=b.colors;colors.highlightedColor=new Color(.86f,1, .96f);colors.pressedColor=new Color(.65f,.84f,.78f);b.colors=colors;
        return b;
    }
    static Transform Page(string name,Transform safe,out GameObject page)
    {
        var r=Rect(name,safe);Fill(r);page=r.gameObject;
        var scroll=r.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.horizontal=false;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=40;
        var viewport=Rect("Viewport",r);Fill(viewport);viewport.offsetMin=new Vector2(72,40);viewport.offsetMax=new Vector2(-72,-40);
        var bg=viewport.gameObject.AddComponent<UnityEngine.UI.Image>();bg.color=Color.white;
        var mask=viewport.gameObject.AddComponent<UnityEngine.UI.Mask>();mask.showMaskGraphic=false;
        var content=Rect("Contenido",viewport);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;content.sizeDelta=Vector2.zero;
        var layout=content.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();layout.spacing=24;layout.padding=new RectOffset(0,0,32,32);layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;
        var fit=content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();fit.verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport=viewport;scroll.content=content;return content;
    }
    static void Logo(Transform parent)
    {
        var r=Rect("TarjetaLogoUniversidad",parent);Height(r,220);r.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(.94f,.96f,.93f);
        var image=Rect("LogoUniversidad",r);Fill(image);image.offsetMin=new Vector2(28,22);image.offsetMax=new Vector2(-28,-22);
        var img=image.gameObject.AddComponent<UnityEngine.UI.Image>();img.color=Color.white;img.preserveAspect=true;img.raycastTarget=false;img.enabled=false;logos.Add(img);
        var placeholder=Text("EspacioParaLogo",r,"ESPACIO PARA EL LOGO\n<size=24>Universidad Marista de Mérida</size>",30,0,true);Fill(placeholder.rectTransform);placeholder.color=new Color(.24f,.38f,.37f);placeholders.Add(placeholder.gameObject);
    }
    static void Credit(Transform p,string label,string value)
    {
        var t=Text(label,p,"<color=#8FB8B0><size=28>"+label.ToUpperInvariant()+"</size></color>\n"+value,40);t.lineSpacing=8;
        var gap=Rect("Separador",p);Height(gap,2);gap.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(.16f,.3f,.29f);
    }
    public static string Crear()
    {
        var root=GameObject.Find("CanvasPrincipal");if(root.GetComponent<MenuCenote>())return "El menú ya existe";
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/CenoteVivo_AntesMenu.unity",true);
        font=AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");logos.Clear();placeholders.Clear();
        var controls=Object.FindAnyObjectByType<ControlesCenote>(FindObjectsInactive.Include);
        if(controls)controls.transform.SetParent(null,false);
        root.GetComponent<UnityEngine.UI.Image>().enabled=false;
        var canvas=root.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=20;
        var scale=root.AddComponent<UnityEngine.UI.CanvasScaler>();scale.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1080,1920);scale.matchWidthOrHeight=0;
        root.AddComponent<UnityEngine.UI.GraphicRaycaster>();var menu=root.AddComponent<MenuCenote>();
        menu.contenidoCenote=GameObject.Find("CenoteVivoRoot");menu.controlesCenote=controls?controls.gameObject:null;
        var bg=Rect("FondoMenu",root.transform);Fill(bg);bg.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(.018f,.075f,.09f);menu.fondo=bg.gameObject;
        var accent=Rect("LineaSuperior",bg);accent.anchorMin=new Vector2(0,1);accent.anchorMax=Vector2.one;accent.pivot=new Vector2(.5f,1);accent.sizeDelta=new Vector2(0,8);accent.gameObject.AddComponent<UnityEngine.UI.Image>().color=green;
        var safe=Rect("ZonaSeguraMenu",root.transform);Fill(safe);menu.zonaSegura=safe;
        GameObject page;var home=Page("PantallaInicio",safe,out page);menu.inicio=page;
        Text("Lema",home,"EXPLORA · COMPRENDE · CUIDA",28,48,true).color=green;
        var title=Text("Titulo",home,"Cenote Vivo",110,190,true);title.fontStyle=TMPro.FontStyles.Bold;
        Text("Subtitulo",home,"Un universo subterráneo\nque merece ser cuidado.",40,130,true).color=muted;
        Logo(home);
        Text("Universidad",home,"Universidad Marista de Mérida",34,84,true).color=muted;
        Button("AccederAlCenote",home,"Acceder al cenote",menu.AccederAlCenote,true);
        Button("ObjetivoDelProyecto",home,"Objetivo del proyecto",menu.MostrarObjetivo);
        Button("Creditos",home,"Créditos",menu.MostrarCreditos);
        var objective=Page("PantallaObjetivo",safe,out page);menu.objetivo=page;
        Button("VolverInicio",objective,"‹  Volver al inicio",menu.MostrarInicio);
        Text("Titulo",objective,"Objetivo del proyecto",64,170).fontStyle=TMPro.FontStyles.Bold;
        Text("Introduccion",objective,"Conocer los cenotes es el primer paso para cuidarlos.",44).color=green;
        Text("Objetivo",objective,"Cenote Vivo busca crear conciencia sobre el valor de los cenotes: un universo subterráneo que forma parte de la identidad y la vida de la península de Yucatán.\n\nA través de una experiencia visual y de exploración, la aplicación invita a reconocer cómo las construcciones, el turismo sin cuidado, los escombros y la contaminación pueden transformar estos espacios y afectar la vida que albergan.\n\nEl proyecto propone explorar distintos escenarios: un cenote sano, uno contaminado y otro afectado por escombros. Observar sus diferencias nos ayuda a comprender por qué nuestras decisiones importan.\n\nLos cenotes tienen un gran valor en nuestras vidas, de forma directa e indirecta. Protegerlos es una responsabilidad compartida: conocerlos, respetarlos y cuidarlos hoy ayuda a conservarlos para las próximas generaciones.",38);
        Text("Nota",objective,"La experiencia utiliza escenarios visuales simplificados con fines educativos.",30).color=muted;
        var credits=Page("PantallaCreditos",safe,out page);menu.creditos=page;
        Button("VolverInicio",credits,"‹  Volver al inicio",menu.MostrarInicio);
        Text("Titulo",credits,"Créditos",76,130).fontStyle=TMPro.FontStyles.Bold;
        Logo(credits);
        Credit(credits,"Autor","Jorge Carlos Trujeque Díaz");Credit(credits,"Escuela","Universidad Marista de Mérida");Credit(credits,"Semestre","Semestre 5");Credit(credits,"Asignatura","Gráficos por computadora");Credit(credits,"Profesor","Luis de Jesús López Hernández");
        var back=Button("VolverAlMenu",safe,"‹  Inicio",menu.MostrarInicio);menu.botonVolver=back.gameObject;Object.DestroyImmediate(back.GetComponent<UnityEngine.UI.LayoutElement>());
        var br=back.GetComponent<RectTransform>();br.anchorMin=br.anchorMax=new Vector2(0,1);br.pivot=new Vector2(0,1);br.anchoredPosition=new Vector2(28,-28);br.sizeDelta=new Vector2(250,112);
        menu.espaciosLogo=logos.ToArray();menu.etiquetasLogo=placeholders.ToArray();
        if(!Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>()){var es=new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem));es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();}
        menu.MostrarInicio();Canvas.ForceUpdateCanvases();
        EditorUtility.SetDirty(menu);EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();Selection.activeGameObject=root;
        return "Inicio, objetivo y créditos creados; cámara intacta";
    }
}
