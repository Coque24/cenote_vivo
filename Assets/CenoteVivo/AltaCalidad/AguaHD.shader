Shader "CenoteVivo/HD/AguaCristalina"
{
 Properties{_RippleMap("Ondulaciones",2D)="gray"{} _Shallow("Agua somera",Color)=(.06,.53,.36,1) _Deep("Agua profunda",Color)=(.012,.19,.22,1) _Refraction("Refraccion",Range(0,.04))=.012 _WaveStrength("Ondulacion",Float)=.10}
 SubShader{Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"} Pass{Tags{"LightMode"="SRPDefaultUnlit"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
 #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_RippleMap);SAMPLER(sampler_RippleMap);
 CBUFFER_START(UnityPerMaterial)
 half4 _Shallow,_Deep;float _Refraction,_WaveStrength;
 CBUFFER_END
 struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;};
 V vert(A i){V o;float3 w=TransformObjectToWorld(i.p.xyz);w.y+=sin(w.x*1.8+_Time.y*.4)*.014+sin(w.z*2.1-_Time.y*.35)*.012;o.w=w;o.p=TransformWorldToHClip(w);return o;}
 half4 frag(V i):SV_Target{
 float2 uv=GetNormalizedScreenSpaceUV(i.p);float2 q=i.w.xz*.16;
 float2 a=SAMPLE_TEXTURE2D(_RippleMap,sampler_RippleMap,q+_Time.y*float2(.011,.007)).rg*2-1;float2 b=SAMPLE_TEXTURE2D(_RippleMap,sampler_RippleMap,q.yx*2.13-_Time.y*float2(.006,.009)).rg*2-1;
 float2 wave=float2(cos(i.w.x*1.8+_Time.y*.4)*.42,cos(i.w.z*2.1-_Time.y*.35)*.38)+(a+b)*.15;float3 n=normalize(float3(wave.x*_WaveStrength,1,wave.y*_WaveStrength));float3 view=normalize(GetCameraPositionWS()-i.w);float above=step(i.w.y,GetCameraPositionWS().y);
 float depth=LinearEyeDepth(SampleSceneDepth(uv),_ZBufferParams);float surface=-TransformWorldToView(i.w).z;float thick=max(depth-surface,0);float absorption=1-exp(-thick*.17);
 half3 tint=lerp(_Shallow.rgb,_Deep.rgb,absorption);half3 refracted=SampleSceneColor(saturate(uv+n.xz*_Refraction*saturate(thick)));
 half3 reflection=GlossyEnvironmentReflection(reflect(-view,n),i.w,.24,1,uv);
 float fresnel=.04+.96*pow(1-saturate(abs(dot(n,view))),5);
 half3 col=lerp(refracted*lerp(half3(1,1,1),tint,.48+absorption*.25),reflection*half3(.72,.90,.78),saturate(fresnel*.62));
 col+=tint*(.12+absorption*.38);
 Light l=GetMainLight();float3 h=normalize(view+l.direction);col+=pow(saturate(dot(n,h)),180)*l.color*.18*above;
 col=lerp(tint*.8,col,above);return half4(col,lerp(.20,.94,above));
 }
 ENDHLSL
 }} }
