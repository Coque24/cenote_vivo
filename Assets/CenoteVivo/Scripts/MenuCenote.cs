using UnityEngine;

[ExecuteAlways]
public sealed class MenuCenote : MonoBehaviour
{
    public GameObject inicio, objetivo, creditos, fondo, botonVolver;
    public GameObject contenidoCenote, controlesCenote;
    public RectTransform zonaSegura;
    [Tooltip("Arrastra aquí el Sprite del logo. Se mostrará en Inicio y Créditos.")]
    public Sprite logoUniversidad;
    public UnityEngine.UI.Image[] espaciosLogo;
    public GameObject[] etiquetasLogo;
    Sprite ultimoLogo; bool logoInicializado;
    Rect ultimaZona; int ancho, alto;
    void Awake(){if(Application.isPlaying)MostrarInicio();ActualizarLogo();}
    void Update()
    {
        if(!logoInicializado || ultimoLogo!=logoUniversidad)ActualizarLogo();
        if(ultimaZona!=Screen.safeArea || ancho!=Screen.width || alto!=Screen.height)ActualizarZona();
    }
    void ActualizarLogo()
    {
        ultimoLogo=logoUniversidad;logoInicializado=true;
        if(espaciosLogo!=null)foreach(var img in espaciosLogo)if(img){img.sprite=logoUniversidad;img.preserveAspect=true;img.enabled=logoUniversidad!=null;}
        if(etiquetasLogo!=null)foreach(var label in etiquetasLogo)if(label)label.SetActive(logoUniversidad==null);
    }
    void ActualizarZona()
    {
        ultimaZona=Screen.safeArea;ancho=Screen.width;alto=Screen.height;
        if(!zonaSegura || ancho==0 || alto==0)return;
        zonaSegura.anchorMin=new Vector2(ultimaZona.xMin/ancho,ultimaZona.yMin/alto);
        zonaSegura.anchorMax=new Vector2(ultimaZona.xMax/ancho,ultimaZona.yMax/alto);
        zonaSegura.offsetMin=zonaSegura.offsetMax=Vector2.zero;
    }
    void Mostrar(GameObject pantalla,bool explorar)
    {
        if(inicio)inicio.SetActive(pantalla==inicio);
        if(objetivo)objetivo.SetActive(pantalla==objetivo);
        if(creditos)creditos.SetActive(pantalla==creditos);
        if(fondo)fondo.SetActive(!explorar);
        if(botonVolver)botonVolver.SetActive(explorar);
        if(contenidoCenote)contenidoCenote.SetActive(explorar);
        if(controlesCenote)controlesCenote.SetActive(explorar);
        if(Application.isPlaying && pantalla && (pantalla==objetivo || pantalla==creditos))
        {
            var scroll=pantalla.GetComponent<UnityEngine.UI.ScrollRect>();
            if(scroll){Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=1;}
        }
    }
    public void MostrarInicio()=>Mostrar(inicio,false);
    public void MostrarObjetivo()=>Mostrar(objetivo,false);
    public void MostrarCreditos()=>Mostrar(creditos,false);
    public void AccederAlCenote()=>Mostrar(null,true);
}
