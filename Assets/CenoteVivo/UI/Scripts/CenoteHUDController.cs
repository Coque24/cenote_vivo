using UnityEngine;

public sealed class CenoteHUDController : MonoBehaviour
{
    public CicloDiaNocheCenote ciclo;
    public UnityEngine.UI.Image iconoDiaNoche;
    public Sprite sol, luna;
    void OnEnable(){if(ciclo){ciclo.ModoCambiado+=Actualizar;Actualizar(ciclo.EsNoche);}}
    void OnDisable(){if(ciclo)ciclo.ModoCambiado-=Actualizar;}
    void Actualizar(bool noche){if(iconoDiaNoche){iconoDiaNoche.sprite=noche?sol:luna;var style=ciclo.boton.GetComponent<CenoteUIButton>();if(style && style.theme)iconoDiaNoche.color=style.theme.damage;}}
}
