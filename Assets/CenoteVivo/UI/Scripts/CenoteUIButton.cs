using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public sealed class CenoteUIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum Accent { Primary, Damage, Pollution }
    public CenoteUITheme theme;
    public Accent accent;
    public bool selected;
    public TMPro.TextMeshProUGUI label;
    public UnityEngine.UI.Image icon;
    public UnityEngine.UI.Image secondaryIcon;
    public bool primaryAction;
    UnityEngine.UI.Image surface;
    Coroutine animacion;
    Vector3 escala;
    void Awake(){ surface=GetComponent<UnityEngine.UI.Image>(); escala=transform.localScale; }
    void OnEnable(){ if(!surface) surface=GetComponent<UnityEngine.UI.Image>(); Apply(); }
    public void SetSelected(bool value){selected=value;Apply();}
    public void Apply()
    {
        if(!theme)return;
        if(!surface)surface=GetComponent<UnityEngine.UI.Image>();
        bool activo=selected || primaryAction;
        Color destacado=accent==Accent.Damage?theme.damage:accent==Accent.Pollution?theme.pollution:theme.primary;
        if(surface){surface.sprite=theme.rounded;surface.type=UnityEngine.UI.Image.Type.Sliced;surface.color=activo?destacado:theme.glass;}
        Color texto=activo?theme.background:theme.text;
        if(label){label.color=texto;label.fontStyle=activo?TMPro.FontStyles.Bold:TMPro.FontStyles.Normal;}
        if(icon)icon.color=texto;
        if(secondaryIcon)secondaryIcon.color=activo?theme.background:theme.secondaryText;
    }
    public void OnPointerDown(PointerEventData e)
    {
        if(!isActiveAndEnabled || !GetComponent<UnityEngine.UI.Button>().IsInteractable())return;
        Animate(.97f);
        if(surface && theme)surface.color=(selected||primaryAction) && accent==Accent.Primary
            ? theme.pressed : (selected?surface.color*.88f:theme.surface);
    }
    public void OnPointerUp(PointerEventData e){if(isActiveAndEnabled){Animate(1);Apply();}}
    public void OnPointerExit(PointerEventData e){if(isActiveAndEnabled){Animate(1);Apply();}}
    void Animate(float target){if(animacion!=null)StopCoroutine(animacion);animacion=StartCoroutine(ScaleTo(target));}
    IEnumerator ScaleTo(float target)
    {
        Vector3 from=transform.localScale,to=escala*target;float t=0;
        while(t<.12f){t+=Time.unscaledDeltaTime;transform.localScale=Vector3.Lerp(from,to,Mathf.Clamp01(t/.12f));yield return null;}
        transform.localScale=to;animacion=null;
    }
    void OnDisable(){if(animacion!=null)StopCoroutine(animacion);animacion=null;transform.localScale=escala==Vector3.zero?Vector3.one:escala;}
}
