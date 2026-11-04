using UnityEngine;

/// Cambios de ambiente solo al cambiar estado. Sin Update, fisica o materiales por instancia.
public sealed class AmbienteCenote : MonoBehaviour
{
    public CardumenCenote cardumen;
    public CicloDiaNocheCenote cicloDiaNoche;
    public Renderer agua;
    public Material[] materialesAgua;
    public Renderer[] vegetacion;
    public Material[] materialesVegetacion;
    public GameObject escombros;
    public GameObject[] vegetacionReducible;
    [Header("Niebla submarina: ejemplo visual educativo")]
    public Color colorNormal = new Color(.13f,.43f,.44f);
    public Color colorDanado = new Color(.58f,.61f,.58f);
    public Color colorContaminado = new Color(.25f,.18f,.09f);
    public float densidadNormal=.024f, densidadDanado=.045f, densidadContaminado=.085f;
    Color nieblaPrevia; float densidadPrevia; bool nieblaActivaPrevia;
    void Awake(){nieblaPrevia=RenderSettings.fogColor;densidadPrevia=RenderSettings.fogDensity;nieblaActivaPrevia=RenderSettings.fog;}
    void OnEnable()
    {
        if(!cardumen)return;
        cardumen.EstadoCambiado+=Aplicar;
        Aplicar(cardumen.Estado);
    }
    void OnDisable()
    {
        if(cardumen)cardumen.EstadoCambiado-=Aplicar;
        RenderSettings.fogColor=nieblaPrevia;RenderSettings.fogDensity=densidadPrevia;RenderSettings.fog=nieblaActivaPrevia;
    }
    void Aplicar(CardumenCenote.EstadoCenote estado)
    {
        int i=(int)estado;
        if(agua && materialesAgua!=null && i<materialesAgua.Length && materialesAgua[i])agua.sharedMaterial=materialesAgua[i];
        if(vegetacion!=null && materialesVegetacion!=null && i<materialesVegetacion.Length && materialesVegetacion[i])
            foreach(var r in vegetacion)if(r)r.sharedMaterial=materialesVegetacion[i];
        if(escombros)escombros.SetActive(estado==CardumenCenote.EstadoCenote.Danado);
        if(vegetacionReducible!=null)foreach(var g in vegetacionReducible)if(g)g.SetActive(estado!=CardumenCenote.EstadoCenote.Contaminado);
        RenderSettings.fog=true;
        RenderSettings.fogColor=i==0?colorNormal:i==1?colorDanado:colorContaminado;
        RenderSettings.fogDensity=i==0?densidadNormal:i==1?densidadDanado:densidadContaminado;
        if(cicloDiaNoche && cicloDiaNoche.isActiveAndEnabled)cicloDiaNoche.ActualizarNiebla();
    }
}
