Shader "CenoteVivo/AguaNatural"
{
 Properties{_RippleMap("Textura de ondulaciones",2D)="gray"{} _Shallow("Agua somera",Color)=(.06,.46,.4,1) _Deep("Agua profunda",Color)=(.015,.13,.19,1) _WaveStrength("Ondulacion",Range(0,.2))=.055 _Opacity("Opacidad",Range(0,1))=.48 _UnderOpacity("Opacidad submarina",Range(0,1))=.13}
 SubShader{Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"} Pass{Tags{"LightMode"="SRPDefaultUnlit"} Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #pragma multi_compile _ _ADDITIONAL_LIGHTS
 half _CenoteNoche;
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_RippleMap);SAMPLER(sampler_RippleMap);
 CBUFFER_START(UnityPerMaterial)
 float4 _RippleMap_ST;half4 _Shallow,_Deep;float _WaveStrength,_Opacity,_UnderOpacity;
 CBUFFER_END
 struct A{float4 p:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
 V vert(A i){UNITY_SETUP_INSTANCE_ID(i);V o;UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);return o;}
 half4 frag(V i):SV_Target{
 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 float2 uv=i.w.xz*.19;
 float2 a=SAMPLE_TEXTURE2D(_RippleMap,sampler_RippleMap,uv+_Time.y*float2(.012,.008)).rg*2-1;
 float2 b=SAMPLE_TEXTURE2D(_RippleMap,sampler_RippleMap,uv.yx*1.43+_Time.y*float2(-.009,.006)).rg*2-1;
 float3 n=normalize(float3((a.x+b.x)*_WaveStrength,1,(a.y+b.y)*_WaveStrength));
 float3 view=normalize(GetCameraPositionWS()-i.w);float above=step(i.w.y,GetCameraPositionWS().y);
 float depth=LinearEyeDepth(SampleSceneDepth(GetNormalizedScreenSpaceUV(i.p)),_ZBufferParams);
 float surface=-TransformWorldToView(i.w).z;float thickness=max(0,depth-surface);
 float deep=1-exp(-thickness*.18);deep=lerp(.35,deep,above);
 half3 col=lerp(_Shallow.rgb,_Deep.rgb,deep);
 float fresnel=pow(1-saturate(abs(dot(n,view))),5);
 col=lerp(col,lerp(half3(.28,.47,.46),_Shallow.rgb,saturate((_Opacity-.6)*3)),fresnel*.5);
 Light light=GetMainLight();float3 h=normalize(view+light.direction);float sparkle=pow(saturate(dot(n,h)),96)*.10*above;
 col+=sparkle*min(light.color,half3(1,1,1));
 col*=lerp(1,.18,_CenoteNoche);
 #if defined(_ADDITIONAL_LIGHTS)
 uint count=GetAdditionalLightsCount();for(uint li=0;li<count;li++){Light l=GetAdditionalLight(li,i.w);float3 lh=normalize(view+l.direction);col+=l.color*l.distanceAttenuation*(.06+pow(saturate(dot(n,lh)),55)*.5)*above;}
 #endif
 float alpha=lerp(_UnderOpacity,_Opacity,above)+fresnel*.2;
 return half4(col,saturate(alpha));}
 ENDHLSL
 }} }
