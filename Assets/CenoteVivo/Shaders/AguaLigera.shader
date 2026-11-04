Shader "CenoteVivo/AguaLigera"
{
 Properties { _Color("Color agua",Color)=(0.06,0.32,0.34,1) _Opacity("Transparencia base",Range(0,1))=.2 _WaveStrength("Ondulacion",Range(0,.2))=.035 }
 SubShader
 {
 Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
 Pass
 {
  Tags { "LightMode"="SRPDefaultUnlit" }
  Blend SrcAlpha OneMinusSrcAlpha
  Cull Off ZWrite Off
  HLSLPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #pragma target 3.0
  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
  CBUFFER_START(UnityPerMaterial)
  half4 _Color; float _Opacity; float _WaveStrength;
  CBUFFER_END
  struct A {float4 positionOS:POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID};
  struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO};
  V vert(A i){UNITY_SETUP_INSTANCE_ID(i);V o;UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.world=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);return o;}
  half4 frag(V i):SV_Target
  {
   float3 view=normalize(GetCameraPositionWS()-i.world);
   float2 slope=float2(sin(i.world.x*1.7+_Time.y*.65),cos(i.world.z*2.1-_Time.y*.5))*_WaveStrength;
   float3 n=normalize(float3(slope.x,1,slope.y));
   float fresnel=pow(1-saturate(abs(dot(n,view))),4);
   float top=step(i.world.y,GetCameraPositionWS().y);
   half3 col=lerp(_Color.rgb,half3(.33,.54,.55),fresnel*.5);
   float alpha=saturate(_Opacity+fresnel*.28+top*.12);
   return half4(col,alpha);
  }
  ENDHLSL
 }
 }
}
