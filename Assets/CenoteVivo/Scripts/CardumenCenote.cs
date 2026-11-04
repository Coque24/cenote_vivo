using UnityEngine;

/// Curvas contenidas en un elipsoide: sin fisica ni asignaciones por fotograma.
public sealed class CardumenCenote : MonoBehaviour
{
    public enum EstadoCenote { Normal, Danado, Contaminado }
    public GameObject prefabPez;
    [Tooltip("Paleta compartida: se asigna un material por pez a todos sus LOD, sin duplicar mallas ni materiales.")]
    public Material[] materialesVariantes;
    [Range(1, 60)] public int cantidadMaxima = 24;
    [Tooltip("Semiejes del elipsoide LOCAL de agua libre, dejando margen para el cuerpo del pez.")]
    public Vector3 extension = new Vector3(8.2f, 2.8f, 8.2f);
    public Vector2 velocidad = new Vector2(.3f, .65f);
    public Vector2 longitud = new Vector2(.28f, .42f);
    [SerializeField] EstadoCenote estado;
    struct Pez { public Transform t; public Vector3 a,b,c,d; public float progreso, rapidez; }
    Pez[] peces;
    float actividad = 1;
    public int PecesVisibles { get; private set; }
    public EstadoCenote Estado => estado;
    public event System.Action<EstadoCenote> EstadoCambiado;
    Vector3 Punto() => Vector3.Scale(Random.insideUnitSphere * .92f, extension);
    Vector3 Contener(Vector3 p)
    {
        var q = new Vector3(p.x / extension.x, p.y / extension.y, p.z / extension.z);
        return Vector3.Scale(Vector3.ClampMagnitude(q,.96f),extension);
    }
    void Start()
    {
        if (!prefabPez) { Debug.LogError("Cardumen: falta prefab de pez", this); enabled=false;return; }
        peces=new Pez[cantidadMaxima];
        for(int i=0;i<peces.Length;i++)
        {
            var t=Instantiate(prefabPez,transform).transform;t.name="Pez_"+(i+1).ToString("00");
            t.localScale=Vector3.one*Random.Range(longitud.x,longitud.y);
            if(materialesVariantes!=null && materialesVariantes.Length>0)
            {
                var material=materialesVariantes[i % materialesVariantes.Length];
                if(material) foreach(var r in t.GetComponentsInChildren<MeshRenderer>(true)) r.sharedMaterial=material;
            }
            var a=Punto(); var b=Contener(a+Random.onUnitSphere*1.5f);
            peces[i]=new Pez {t=t,a=a,b=b,c=Punto(),d=Punto(),rapidez=Random.Range(velocidad.x,velocidad.y)};
            Posicionar(ref peces[i],0);
        }
        AplicarEstado();
    }
    void Update()
    {
        if(peces==null)return;
        for(int i=0;i<PecesVisibles;i++) Posicionar(ref peces[i],Time.deltaTime*actividad);
    }
    void Posicionar(ref Pez p,float dt)
    {
        float u=p.progreso, v=1-u;
        Vector3 tangente=3*v*v*(p.b-p.a)+6*v*u*(p.c-p.b)+3*u*u*(p.d-p.c);
        p.progreso=Mathf.Min(1,u+dt*p.rapidez/Mathf.Max(.1f,tangente.magnitude));
        u=p.progreso;v=1-u;
        p.t.localPosition=v*v*v*p.a+3*v*v*u*p.b+3*v*u*u*p.c+u*u*u*p.d;
        tangente=3*v*v*(p.b-p.a)+6*v*u*(p.c-p.b)+3*u*u*(p.d-p.c);
        if(tangente.sqrMagnitude>.0001f) p.t.localRotation=Quaternion.LookRotation(tangente);
        if(u>=1)
        {
            var siguiente=Contener(p.d+(p.d-p.c)*.6f);
            p.a=p.d;p.b=siguiente;p.d=Punto();p.c=Contener(Vector3.Lerp(p.b,p.d,.65f)+Random.insideUnitSphere*1.5f);p.progreso=0;
        }
    }
    public void PonerNormal()=>CambiarEstado(EstadoCenote.Normal);
    public void PonerDanado()=>CambiarEstado(EstadoCenote.Danado);
    public void PonerContaminado()=>CambiarEstado(EstadoCenote.Contaminado);
    public void CambiarEstado(EstadoCenote valor){estado=valor;AplicarEstado();}
    void AplicarEstado()
    {
        EstadoCambiado?.Invoke(estado);
        if(peces==null)return;
        float proporcion=estado==EstadoCenote.Normal?1:estado==EstadoCenote.Danado?.5f:.15f;
        actividad=estado==EstadoCenote.Normal?1:estado==EstadoCenote.Danado?.75f:.4f;
        PecesVisibles=Mathf.RoundToInt(peces.Length*proporcion);
        for(int i=0;i<peces.Length;i++) peces[i].t.gameObject.SetActive(i<PecesVisibles);
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.matrix=transform.localToWorldMatrix*Matrix4x4.Scale(extension);Gizmos.color=Color.cyan;Gizmos.DrawWireSphere(Vector3.zero,1);
    }
    void OnValidate(){extension=Vector3.Max(extension,Vector3.one*.1f);if(Application.isPlaying)AplicarEstado();}
}
