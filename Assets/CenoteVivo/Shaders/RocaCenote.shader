Shader "CenoteVivo/RocaNatural"
{
 Properties { _BaseMap("Roca",2D)="white"{} _Scale("Escala textura",Float)=.32 _Tint("Tono",Color)=(1,1,1,1) }
 SubShader { Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Pass {
 Tags{"LightMode"="UniversalForward"} Cull Back
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #pragma multi_compile _ _ADDITIONAL_LIGHTS
 half _CenoteNoche;
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseMap_ST;half4 _Tint;float _Scale;
 CBUFFER_END
 struct A{float4 p:POSITION;float3 n:NORMAL;half4 c:COLOR;};
 struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;half3 n:TEXCOORD1;half4 c:COLOR;half fog:TEXCOORD2;};
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.c=i.c;o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target{
 half3 n=normalize(i.n);half3 weights=pow(abs(n),4);weights/=max(dot(weights,half3(1,1,1)),.001);
 half3 tex=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.w.zy*_Scale).rgb*weights.x+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.w.xz*_Scale).rgb*weights.y+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.w.xy*_Scale).rgb*weights.z;
 Light light=GetMainLight();half diffuse=saturate(dot(n,light.direction));
 half3 illumination=lerp(half3(.28,.31,.28),half3(.018,.028,.052),_CenoteNoche)+min(light.color,half3(1.2,1.2,1.2))*diffuse*.65;
 #if defined(_ADDITIONAL_LIGHTS)
 uint count=GetAdditionalLightsCount();
 for(uint li=0;li<count;li++){Light l=GetAdditionalLight(li,i.w);illumination+=l.color*l.distanceAttenuation*l.shadowAttenuation*(.12+.88*saturate(dot(n,l.direction)));}
 #endif
 half layer=1-.045*sin(i.w.y*5+sin(i.w.x*.8)+sin(i.w.z*.7));
 half3 col=tex*i.c.rgb*_Tint.rgb*illumination*layer;
 return half4(MixFog(col,i.fog*saturate(dot(i.c.rgb,half3(.333,.333,.333))*1.8)),1);}
 ENDHLSL
 } } }
