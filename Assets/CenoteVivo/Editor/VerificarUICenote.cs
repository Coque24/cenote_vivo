using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

// Utilidades de QA en el Editor. El juego no incluye este código.
public static class VerificarUICenote
{
    static BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance;
    static Type Type(string n)=>typeof(EditorWindow).Assembly.GetType("UnityEditor."+n);
    static object Group()
    {
        var size=Type("GameViewSizes");var singleton=typeof(ScriptableSingleton<>).MakeGenericType(size);
        var obj=singleton.GetProperty("instance",F).GetValue(null);return size.GetProperty("currentGroup",F).GetValue(obj);
    }
    public static string Resolucion(int width,int height)
    {
        var group=Group();var gt=group.GetType();int total=(int)gt.GetMethod("GetTotalCount",F).Invoke(group,null);
        string name="QA Cenote "+width+"x"+height;int index=-1;
        for(int i=0;i<total;i++){var x=gt.GetMethod("GetGameViewSize",F).Invoke(group,new object[]{i});if((string)x.GetType().GetProperty("baseText",F).GetValue(x)==name){index=i;break;}}
        if(index<0){var type=Type("GameViewSize");var st=Type("GameViewSizeType");var x=Activator.CreateInstance(type,F,null,new[]{Enum.Parse(st,"FixedResolution"),(object)width,height,name},null);gt.GetMethod("AddCustomSize",F).Invoke(group,new[]{x});index=total;}
        var viewType=Type("GameView");var view=EditorWindow.GetWindow(viewType);viewType.GetProperty("selectedSizeIndex",F).SetValue(view,index);view.Repaint();
        return "Seleccionado "+name+" index="+index;
    }
    public static string Restaurar(int original)
    {
        var viewType=Type("GameView");var view=EditorWindow.GetWindow(viewType);viewType.GetProperty("selectedSizeIndex",F).SetValue(view,original);
        var group=Group();var gt=group.GetType();int total=(int)gt.GetMethod("GetTotalCount",F).Invoke(group,null);int removed=0;
        for(int i=total-1;i>=0;i--){var x=gt.GetMethod("GetGameViewSize",F).Invoke(group,new object[]{i});var name=(string)x.GetType().GetProperty("baseText",F).GetValue(x);if(name.StartsWith("QA Cenote ")){gt.GetMethod("RemoveCustomSize",F).Invoke(group,new object[]{i});removed++;}}
        return "Game View restaurada; perfiles temporales eliminados="+removed;
    }
    public static string Click(UnityEngine.UI.Button button)
    {
        var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
        var eventData=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,(corners[0]+corners[2])*.5f),button=PointerEventData.InputButton.Left};
        var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(eventData,hits);
        if(hits.Count==0)return "SIN OBJETIVO: "+button.name;
        var receiver=hits[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>();
        if(receiver!=button)return "OBSTRUIDO: "+button.name+" por "+hits[0].gameObject.name;
        ExecuteEvents.Execute(receiver.gameObject,eventData,ExecuteEvents.pointerClickHandler);return "OK: "+button.name;
    }
    public static string Inspeccionar(Transform root)
    {
        Canvas.ForceUpdateCanvases();var outp=new System.Text.StringBuilder();
        outp.AppendLine("Screen="+Screen.width+"x"+Screen.height+" Safe="+Screen.safeArea);
        foreach(var b in root.GetComponentsInChildren<UnityEngine.UI.Button>(false))
        {
            var r=(RectTransform)b.transform;var c=new Vector3[4];r.GetWorldCorners(c);var a=RectTransformUtility.WorldToScreenPoint(null,c[0]);var z=RectTransformUtility.WorldToScreenPoint(null,c[2]);
            bool fuera=a.x<-1||a.y<-1||z.x>Screen.width+1||z.y>Screen.height+1;
            outp.AppendLine(b.name+" " + (z-a)+" px"+(fuera?" FUERA":""));
        }
        foreach(var t in root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(false))
        {
            if(!t.enabled||!t.gameObject.activeInHierarchy)continue;t.ForceMeshUpdate();
            if(t.isTextOverflowing)outp.AppendLine("TEXT OVERFLOW: "+t.name+" = "+t.text.Replace("\n"," "));
        }
        return outp.ToString();
    }
}
