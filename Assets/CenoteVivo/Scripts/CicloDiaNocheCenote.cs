using System.Collections;
using UnityEngine;

public sealed class CicloDiaNocheCenote : MonoBehaviour
{
    public Light sol;
    public Light[] faroles;
    public Renderer[] bombillas;
    public Material cieloPlantilla, emisionPlantilla;
    public AmbienteCenote ambiente;
    public UnityEngine.UI.Button boton;
    public TMPro.TextMeshProUGUI etiqueta;
    [Min(.1f)] public float duracionTransicion = 6;
    [Min(0)] public float intensidadFaroles = 9;
    public float Noche { get; private set; }
    public bool EsNoche { get; private set; }
    public bool EnTransicion => transicion != null;
    public event System.Action<bool> ModoCambiado;
    Material cielo, emision, cieloAnterior;
    Coroutine transicion;
    Color solColorAnterior, ambienteAnterior;
    float solIntensidadAnterior;
    Quaternion solRotacionAnterior;
    UnityEngine.Rendering.AmbientMode modoAnterior;
    bool inicializado;

    void Awake()
    {
        cielo = new Material(cieloPlantilla);
        emision = new Material(emisionPlantilla);
        foreach (var r in bombillas) if (r) r.sharedMaterial = emision;
        inicializado = true;
        if (boton) boton.onClick.AddListener(Alternar);
    }
    void OnEnable()
    {
        if (!inicializado) return;
        cieloAnterior = RenderSettings.skybox;
        ambienteAnterior = RenderSettings.ambientLight;
        modoAnterior = RenderSettings.ambientMode;
        solColorAnterior = sol.color; solIntensidadAnterior = sol.intensity;
        solRotacionAnterior = sol.transform.rotation;
        if (etiqueta) etiqueta.text = EsNoche ? "Cambiar a día" : "Cambiar a noche";
        RenderSettings.skybox = cielo;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        Aplicar(Noche);
    }
    public void Alternar() { PonerNoche(!EsNoche); }
    public void PonerNoche(bool noche)
    {
        EsNoche = noche;
        if (etiqueta) etiqueta.text = EsNoche ? "Cambiar a día" : "Cambiar a noche";
        ModoCambiado?.Invoke(noche);
        if (transicion != null) StopCoroutine(transicion);
        transicion = StartCoroutine(Transicion(noche ? 1 : 0));
    }
    IEnumerator Transicion(float destino)
    {
        float origen = Noche;
        float duracion = Mathf.Max(.1f, duracionTransicion * Mathf.Abs(destino - origen));
        float tiempo = 0;
        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            Aplicar(Mathf.Lerp(origen, destino, Mathf.SmoothStep(0, 1, tiempo / duracion)));
            yield return null;
        }
        Aplicar(destino); transicion = null;
    }
    void Aplicar(float valor)
    {
        Noche = Mathf.Clamp01(valor);
        Shader.SetGlobalFloat("_CenoteNoche", Noche);
        cielo.SetFloat("_Noche", Noche);
        sol.intensity = Mathf.Lerp(solIntensidadAnterior, .16f, Noche);
        Color atardecer = new Color(1, .52f, .24f);
        sol.color = Noche < .5f ? Color.Lerp(solColorAnterior, atardecer, Noche * 2) : Color.Lerp(atardecer, new Color(.40f,.56f,1), (Noche-.5f)*2);
        sol.transform.rotation = Quaternion.Slerp(solRotacionAnterior, Quaternion.Euler(36, -35, 0), Noche);
        RenderSettings.ambientLight = Color.Lerp(new Color(.24f,.34f,.38f), new Color(.028f,.042f,.085f), Noche);
        float encendido = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.18f, .85f, Noche));
        foreach (var l in faroles) if(l) { l.intensity = intensidadFaroles * encendido; l.enabled = encendido > .001f; }
        emision.SetColor("_LuzCalida", new Color(1,.48f,.16f,1) * Mathf.Lerp(.04f, 5f, encendido));
        ActualizarNiebla();
    }
    public void ActualizarNiebla()
    {
        if (!ambiente || !ambiente.cardumen) return;
        int estado = (int)ambiente.cardumen.Estado;
        Color dia = estado == 0 ? ambiente.colorNormal : estado == 1 ? ambiente.colorDanado : ambiente.colorContaminado;
        RenderSettings.fogColor = Color.Lerp(dia, dia * .14f + new Color(.006f,.012f,.026f,0), Noche);
        RenderSettings.fogDensity = (estado == 0 ? ambiente.densidadNormal : estado == 1 ? ambiente.densidadDanado : ambiente.densidadContaminado) * Mathf.Lerp(1,.65f,Noche);
    }
    void OnDisable()
    {
        if (!inicializado) return;
        if (transicion != null) StopCoroutine(transicion);
        transicion = null;
        RenderSettings.skybox = cieloAnterior;
        RenderSettings.ambientLight = ambienteAnterior;RenderSettings.ambientMode = modoAnterior;
        if(sol) { sol.intensity = solIntensidadAnterior; sol.color = solColorAnterior; sol.transform.rotation = solRotacionAnterior; }
        Shader.SetGlobalFloat("_CenoteNoche", 0);
        foreach (var l in faroles) if(l) l.enabled = false;
    }
    void OnDestroy()
    {
        if (boton) boton.onClick.RemoveListener(Alternar);
        if (cielo) Destroy(cielo);
        if (emision) Destroy(emision);
    }
}
