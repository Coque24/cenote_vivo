Shader "CenoteVivo/VegetacionNatural"
{
 Properties{_Tint("Tono vegetacion",Color)=(1,1,1,1)}
 SubShader{Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Pass{Tags{"LightMode"="UniversalForward"} Cull Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #pragma multi_compile _ _ADDITIONAL_LIGHTS
 half _CenoteNoche;
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _Tint;
 CBUFFER_END
 struct A{float4 p:POSITION;float3 n:NORMAL;half4 c:COLOR;};struct V{float4 p:SV_POSITION;half3 n:TEXCOORD0;half4 c:COLOR;half fog:TEXCOORD1;float3 w:TEXCOORD2;};
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformObjectToHClip(i.p.xyz);o.n=TransformObjectToWorldNormal(i.n);o.c=i.c;o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target{Light l=GetMainLight();half3 n=normalize(i.n);half3 d=lerp(half3(.42,.42,.42),half3(.018,.028,.05),_CenoteNoche)+.58*abs(dot(n,l.direction))*min(l.color,half3(1,1,1));
 #if defined(_ADDITIONAL_LIGHTS)
 uint count=GetAdditionalLightsCount();for(uint li=0;li<count;li++){Light a=GetAdditionalLight(li,i.w);d+=a.color*a.distanceAttenuation*(.15+.85*abs(dot(n,a.direction)));}
 #endif
 return half4(MixFog(i.c.rgb*_Tint.rgb*d,i.fog),1);}
 ENDHLSL
 }} }
