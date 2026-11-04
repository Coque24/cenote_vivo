using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;

public static class MejorarCenote
{
    static RectTransform Rect(string nombre,Transform padre)
    {
        var r=new GameObject(nombre,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(padre,false);return r;
    }
    static TMPro.TextMeshProUGUI Texto(string nombre,Transform padre,string contenido,int size)
    {
        var r=Rect(nombre,padre);var t=r.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
        t.font=AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        t.text=contenido;t.fontSize=size;t.color=Color.white;t.alignment=TMPro.TextAlignmentOptions.Midline;t.raycastTarget=false;return t;
    }
    static UnityEngine.UI.Button Boton(string nombre,Transform padre,UnityEngine.Events.UnityAction accion)
    {
        var r=Rect(nombre,padre);var img=r.gameObject.AddComponent<UnityEngine.UI.Image>();img.color=new Color(.1f,.16f,.19f,1);
        var b=r.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=img;
        UnityEventTools.AddPersistentListener(b.onClick,accion);
        var t=Texto("Etiqueta",r,nombre,22);t.alignment=TMPro.TextAlignmentOptions.Center;
        t.rectTransform.anchorMin=Vector2.zero;t.rectTransform.anchorMax=Vector2.one;t.rectTransform.offsetMin=t.rectTransform.offsetMax=Vector2.zero;
        return b;
    }
    public static string CrearUI()
    {
        if(GameObject.Find("ControlesCenote"))return "Ya existe UI";
        var go=new GameObject("ControlesCenote",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster));
        var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scale=go.GetComponent<UnityEngine.UI.CanvasScaler>();scale.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1280,720);scale.matchWidthOrHeight=.5f;
        var control=go.AddComponent<ControlesCenote>();control.cardumen=Object.FindAnyObjectByType<CardumenCenote>();
        var safe=Rect("ZonaSegura",go.transform);safe.anchorMin=Vector2.zero;safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;control.zonaSegura=safe;
        var panel=Rect("PanelEstados",safe);panel.anchorMin=new Vector2(.04f,0);panel.anchorMax=new Vector2(.96f,0);panel.pivot=new Vector2(.5f,0);panel.anchoredPosition=new Vector2(0,20);panel.sizeDelta=new Vector2(0,150);
        panel.gameObject.AddComponent<UnityEngine.UI.Image>().color=new Color(.025f,.065f,.085f,.94f);
        var label=Texto("EstadoActual",panel,"NORMAL · Vida activa",24);control.estado=label;
        label.rectTransform.anchorMin=new Vector2(0,1);label.rectTransform.anchorMax=Vector2.one;label.rectTransform.pivot=new Vector2(.5f,1);label.rectTransform.anchoredPosition=new Vector2(0,-10);label.rectTransform.sizeDelta=new Vector2(-32,32);
        var row=Rect("Botones",panel);row.anchorMin=new Vector2(0,.3f);row.anchorMax=new Vector2(1,.7f);row.offsetMin=new Vector2(16,0);row.offsetMax=new Vector2(-16,0);
        var layout=row.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();layout.spacing=12;layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandWidth=layout.childForceExpandHeight=true;
        control.normal=Boton("Normal",row,control.Normal);control.danado=Boton("Dañado",row,control.Danado);control.contaminado=Boton("Contaminado",row,control.Contaminado);
        var note=Texto("NotaEducativa",panel,"Ejemplo visual: los porcentajes no predicen daños reales.",16);note.color=new Color(.75f,.83f,.84f);
        note.rectTransform.anchorMin=Vector2.zero;note.rectTransform.anchorMax=new Vector2(1,0);note.rectTransform.pivot=new Vector2(.5f,0);note.rectTransform.anchoredPosition=new Vector2(0,8);note.rectTransform.sizeDelta=new Vector2(-32,28);
        if(!Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>())
        {
            var ev=new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem));
            ev.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>().AssignDefaultActions();
        }
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Tres botones conectados y EventSystem listo";
    }
    public static string AjustarEscena()
    {
        var s=Object.FindAnyObjectByType<CardumenCenote>();s.extension=new Vector3(8.2f,2.8f,8.2f);s.velocidad=new Vector2(.3f,.65f);
        var water=GameObject.Find("Water").transform;var p=water.position;p.y=1.8f;water.position=p;water.localScale=new Vector3(2.8f,1,2.8f);
        var rays=GameObject.Find("GodRaysFinal").GetComponent<Renderer>();
        float top=rays.bounds.max.y;var t=rays.transform;var scale=t.localScale;scale.x*=.48f;scale.z*=.48f;t.localScale=scale;
        p=t.position;p.x=-1.5f;p.z=-2.5f;t.position=p;
        rays.sharedMaterial.SetFloat("_RayIntensity",.018f);EditorUtility.SetDirty(rays.sharedMaterial);
        var veg=GameObject.Find("VegetacionSuperior_Puntos").transform;
        for(int i=0;i<veg.childCount;i++){float a=i*Mathf.PI/4;var point=veg.GetChild(i);point.localPosition=new Vector3(Mathf.Cos(a)*6.4f,6.1f,Mathf.Sin(a)*6.4f);}
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();return "Agua y=1.8, volumen elipsoidal y rayos ajustados";
    }
}
