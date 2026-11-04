using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public sealed class CenoteUIScreenFade : MonoBehaviour
{
    [Range(.1f,.5f)] public float duration=.22f;
    CanvasGroup group;
    Coroutine fade;
    void OnEnable(){group=GetComponent<CanvasGroup>();group.alpha=0;fade=StartCoroutine(Appear());}
    IEnumerator Appear(){float t=0;while(t<duration){t+=Time.unscaledDeltaTime;group.alpha=Mathf.Clamp01(t/duration);yield return null;}group.alpha=1;fade=null;}
    void OnDisable(){if(fade!=null)StopCoroutine(fade);fade=null;if(group)group.alpha=1;}
}
