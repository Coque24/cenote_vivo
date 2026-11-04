using UnityEngine;

public sealed class ControlesCenote : MonoBehaviour
{
    public CardumenCenote cardumen;
    public TMPro.TextMeshProUGUI estado;
    public RectTransform zonaSegura;
    public UnityEngine.UI.Button normal, danado, contaminado;
    Rect ultimaZona; int ancho, alto;
    void OnEnable(){if(cardumen)cardumen.EstadoCambiado+=EstadoCambiado;Refrescar();}
    void OnDisable(){if(cardumen)cardumen.EstadoCambiado-=EstadoCambiado;}
    void EstadoCambiado(CardumenCenote.EstadoCenote e){Refrescar();}
    void Start(){Refrescar();ActualizarZona();}
    void Update(){if(ultimaZona!=Screen.safeArea || ancho!=Screen.width || alto!=Screen.height)ActualizarZona();}
    void ActualizarZona()
    {
        ultimaZona=Screen.safeArea;ancho=Screen.width;alto=Screen.height;
        if(!zonaSegura || ancho==0 || alto==0)return;
        zonaSegura.anchorMin=new Vector2(ultimaZona.xMin/ancho,ultimaZona.yMin/alto);
        zonaSegura.anchorMax=new Vector2(ultimaZona.xMax/ancho,ultimaZona.yMax/alto);
        zonaSegura.offsetMin=zonaSegura.offsetMax=Vector2.zero;
    }
    public void Normal(){cardumen.PonerNormal();Refrescar();}
    public void Danado(){cardumen.PonerDanado();Refrescar();}
    public void Contaminado(){cardumen.PonerContaminado();Refrescar();}
    void Refrescar()
    {
        if(!cardumen)return;
        var e=cardumen.Estado;
        if(estado)estado.text=e==CardumenCenote.EstadoCenote.Normal?"NORMAL · Agua clara y vida activa":e==CardumenCenote.EstadoCenote.Danado?"DAÑADO · Ecosistema alterado":"CONTAMINADO · Agua y entorno afectados";
        Pintar(normal,e==CardumenCenote.EstadoCenote.Normal);Pintar(danado,e==CardumenCenote.EstadoCenote.Danado);Pintar(contaminado,e==CardumenCenote.EstadoCenote.Contaminado);
    }
    void Pintar(UnityEngine.UI.Button b,bool seleccionado)
    {
        if(!b)return;
        var estilo=b.GetComponent<CenoteUIButton>();
        if(estilo)estilo.SetSelected(seleccionado);
        else b.GetComponent<UnityEngine.UI.Image>().color=seleccionado?new Color(.08f,.42f,.37f,1):new Color(.1f,.16f,.19f,1);
    }
}
