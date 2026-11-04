using UnityEngine;
using UnityEngine.Rendering;

/// Perfil gráfico exclusivo de esta escena. No modifica los assets móviles.
public sealed class PerfilGraficoHD : MonoBehaviour
{
    public RenderPipelineAsset perfil;
    RenderPipelineAsset previo;
    bool aplicado;
    void OnEnable(){if(!perfil)return;previo=QualitySettings.renderPipeline;QualitySettings.renderPipeline=perfil;aplicado=true;}
    void OnDisable(){if(aplicado){QualitySettings.renderPipeline=previo;aplicado=false;}}
}
