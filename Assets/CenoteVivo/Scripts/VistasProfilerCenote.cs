using UnityEngine;

// Herramienta temporal de comparación: una sola cámara, tres posiciones y sin Update.
public sealed class VistasProfilerCenote : MonoBehaviour
{
    public Camera camara;
    public Transform cenote;
    public RectTransform zonaSegura;
    [Min(1)] public float distanciaAlejada = 30f;
    [Min(1)] public float distanciaLejana = 70f;
    public int VistaActual { get; private set; }
    Vector3 posicionOriginal;
    Quaternion rotacionOriginal;
    bool preparado;
    public UnityEngine.UI.Button[] botones = new UnityEngine.UI.Button[3];

    void Start()
    {
        if (!camara) camara = Camera.main;
        if (!camara || !cenote || !zonaSegura) { enabled = false; return; }
        posicionOriginal = camara.transform.position;
        rotacionOriginal = camara.transform.rotation;
        preparado = true;
        if (botones == null || botones.Length != 3 || !botones[0] || !botones[1] || !botones[2])
        { botones = new UnityEngine.UI.Button[3]; CrearBotones(); }
        else for (int i=0;i<botones.Length;i++){int indice=i;botones[i].onClick.AddListener(()=>CambiarVista(indice));}
        CambiarVista(0);
    }

    public void CambiarVista(int vista)
    {
        if (!preparado || !camara) return;
        VistaActual = Mathf.Clamp(vista, 0, 2);
        if (VistaActual == 0) camara.transform.SetPositionAndRotation(posicionOriginal, rotacionOriginal);
        else
        {
            // Vista exterior elevada para evitar quedar detrás de las paredes subterráneas.
            Vector3 foco = cenote.position + Vector3.up * 1.8f;
            Vector3 direccion = Vector3.ProjectOnPlane(posicionOriginal - foco, Vector3.up).normalized;
            if (direccion.sqrMagnitude < .01f) direccion = Vector3.back;
            direccion = (direccion + Vector3.up * .65f).normalized;
            float distancia = VistaActual == 1 ? distanciaAlejada : distanciaLejana;
            camara.transform.position = foco + direccion * distancia;
            camara.transform.rotation = Quaternion.LookRotation(foco - camara.transform.position);
        }
        for (int i = 0; i < botones.Length; i++)
            if (botones[i])
            {
                var estilo=botones[i].GetComponent<CenoteUIButton>();
                if(estilo)estilo.SetSelected(i==VistaActual);
                else botones[i].GetComponent<UnityEngine.UI.Image>().color = i == VistaActual
                    ? new Color(.08f, .42f, .37f, 1) : new Color(.1f, .16f, .19f, 1);
            }
    }

    void OnDisable()
    {
        if (preparado && camara) camara.transform.SetPositionAndRotation(posicionOriginal, rotacionOriginal);
        VistaActual = 0;
        if (preparado) CambiarVista(0);
    }

    void CrearBotones()
    {
        var fila = new GameObject("VistasProfiler", typeof(RectTransform)).GetComponent<RectTransform>();
        fila.SetParent(zonaSegura, false);
        fila.anchorMin = new Vector2(.50f, 1); fila.anchorMax = Vector2.one;
        fila.pivot = Vector2.one; fila.offsetMin = new Vector2(0, -88); fila.offsetMax = new Vector2(-24, -24);
        var layout = fila.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layout.spacing = 8; layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = true;
        var modelo = GetComponent<ControlesCenote>()?.normal?.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        string[] nombres = { "Vista actual", "Alejada", "Lejana" };
        for (int i = 0; i < 3; i++)
        {
            int indice = i;
            var r = new GameObject(nombres[i], typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(fila, false);
            var image = r.gameObject.AddComponent<UnityEngine.UI.Image>();
            var b = r.gameObject.AddComponent<UnityEngine.UI.Button>(); b.targetGraphic = image;
            b.onClick.AddListener(() => CambiarVista(indice)); botones[i] = b;
            var tr = new GameObject("Etiqueta", typeof(RectTransform)).GetComponent<RectTransform>();
            tr.SetParent(r, false); tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(4, 4); tr.offsetMax = new Vector2(-4, -4);
            var text = tr.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
            if (modelo) text.font = modelo.font;
            text.text = nombres[i]; text.fontSize = 27; text.color = Color.white;
            text.alignment = TMPro.TextAlignmentOptions.Center; text.raycastTarget = false;
        }
    }
}
