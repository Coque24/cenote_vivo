Shader "CenoteVivo/PezVertex"
{
 Properties { _Tint("Tinte", Color) = (1,1,1,1) _BodyColor("Color del cuerpo", Color) = (0.65,0.75,0.77,1) _ColorVariation("Mezcla de color", Range(0,1)) = 0 _TailAmplitude("Amplitud cola", Range(0,0.15)) = 0.055 _TailSpeed("Ritmo cola", Float) = 7 }
 SubShader
 {
  Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
  Pass
  {
   Name "Peces" Tags { "LightMode"="SRPDefaultUnlit" }
   Cull Back ZWrite On
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #pragma multi_compile _ _ADDITIONAL_LIGHTS
   half _CenoteNoche;
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _Tint; half4 _BodyColor; float _ColorVariation; float _TailAmplitude; float _TailSpeed;
   CBUFFER_END
   struct Attributes { float4 positionOS:POSITION; half4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
   struct Varyings { float4 positionCS:SV_POSITION; half4 color:COLOR; float3 w:TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };
   Varyings vert(Attributes input)
   {
    UNITY_SETUP_INSTANCE_ID(input);
    Varyings o; UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    float3 p = input.positionOS.xyz;
    float tail = saturate((0.15 - p.z) / 0.65);
    float phase = dot(float2(UNITY_MATRIX_M._m03, UNITY_MATRIX_M._m23), float2(2.3, 3.7));
    p.x += sin(_Time.y * _TailSpeed + p.z * 5 + phase) * tail * tail * _TailAmplitude;
    o.positionCS = TransformObjectToHClip(p);
    o.w=TransformObjectToWorld(p);
    half3 baseColor = input.color.rgb;
    #ifndef UNITY_COLORSPACE_GAMMA
    baseColor = SRGBToLinear(baseColor);
    #endif
    half luminance = dot(baseColor, half3(0.2126, 0.7152, 0.0722));
    // Mantiene ojos oscuros y usa la luminosidad original como patron de sombreado.
    half mask = smoothstep(0.05, 0.22, luminance);
    half3 variation = _BodyColor.rgb * saturate(luminance * 1.8);
    baseColor = lerp(baseColor, variation, _ColorVariation * mask);
    o.color = half4(baseColor, input.color.a) * _Tint;
    return o;
   }
   half4 frag(Varyings i):SV_Target { half3 illumination=lerp(half3(1,1,1),half3(.10,.15,.25),_CenoteNoche);
    #if defined(_ADDITIONAL_LIGHTS)
    uint count=GetAdditionalLightsCount();for(uint li=0;li<count;li++){Light l=GetAdditionalLight(li,i.w);illumination+=l.color*l.distanceAttenuation*.45;}
    #endif
    return half4(i.color.rgb*illumination,1); }
   ENDHLSL
  }
 }
}
