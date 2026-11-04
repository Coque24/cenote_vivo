Shader "CenoteVivo/HD/MaterialNatural"
{
 Properties{_BaseMap("Albedo",2D)="white"{} _NormalMap("Relieve",2D)="bump"{} _MossMap("Musgo",2D)="white"{} _Tint("Tono",Color)=(1,1,1,1) _Scale("Escala",Float)=.38 _NormalStrength("Relieve",Float)=.65 _Vegetation("Follaje",Float)=0 _Wet("Humedad",Range(0,1))=.7 _Cull("Culling",Float)=2}
 SubShader{Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Cull [_Cull]
 Pass{Tags{"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
 #pragma multi_compile _ _ADDITIONAL_LIGHTS
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);TEXTURE2D(_NormalMap);SAMPLER(sampler_NormalMap);TEXTURE2D(_MossMap);SAMPLER(sampler_MossMap);
 CBUFFER_START(UnityPerMaterial)
 half4 _Tint;float _Scale,_NormalStrength,_Vegetation,_Wet,_Cull;
 CBUFFER_END
 struct A{float4 p:POSITION;float3 n:NORMAL;half4 c:COLOR;float2 uv:TEXCOORD0;};
 struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;half3 n:TEXCOORD1;half4 c:COLOR;float2 uv:TEXCOORD2;half fog:TEXCOORD3;};
 V vert(A i){V o;float3 w=TransformObjectToWorld(i.p.xyz);w.xz+=float2(sin(_Time.y*.65+w.x*.7+w.z),cos(_Time.y*.5+w.z*.8))*i.c.a*_Vegetation*.025;o.w=w;o.p=TransformWorldToHClip(w);o.n=TransformObjectToWorldNormal(i.n);o.c=i.c;o.uv=i.uv;o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i,bool front:SV_IsFrontFace):SV_Target{
 half3 n=normalize(i.n);if(!front)n=-n;float3 w=i.w*_Scale;half3 weights=pow(abs(n),4);weights/=max(dot(weights,half3(1,1,1)),.001);
 half3 tex=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,w.zy).rgb*weights.x+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,w.xz).rgb*weights.y+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,w.xy).rgb*weights.z;
 half3 nx=UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,w.zy),_NormalStrength);half3 ny=UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,w.xz),_NormalStrength);half3 nz=UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap,sampler_NormalMap,w.xy),_NormalStrength);
 half3 detail=half3(nx.z*sign(n.x),nx.y,nx.x)*weights.x+half3(ny.x,ny.z*sign(n.y),ny.y)*weights.y+half3(nz.x,nz.y,nz.z*sign(n.z))*weights.z;
 n=normalize(lerp(normalize(detail),n,_Vegetation));
 half moss=saturate((i.w.y-2.4)*.14)*saturate(i.c.g-i.c.r+.17)*2;
 half3 mossTex=SAMPLE_TEXTURE2D(_MossMap,sampler_MossMap,w.xz*.8).rgb;
 half3 rock=lerp(tex,mossTex,moss*.65)*i.c.rgb*_Tint.rgb;
 half rib=exp(-abs(i.uv.x-.5)*75)*.06;
 half3 albedo=lerp(rock,i.c.rgb*_Tint.rgb+rib,_Vegetation);
 float wet=(1-smoothstep(1.4,3.5,i.w.y))*_Wet*(1-_Vegetation);
 albedo*=1-wet*.18;
 InputData data=(InputData)0;data.positionWS=i.w;data.positionCS=i.p;data.normalWS=n;data.viewDirectionWS=GetWorldSpaceNormalizeViewDir(i.w);data.shadowCoord=TransformWorldToShadowCoord(i.w);data.bakedGI=SampleSH(n)+half3(.065,.085,.06);data.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(i.p);data.shadowMask=half4(1,1,1,1);
 SurfaceData s=(SurfaceData)0;s.albedo=albedo;s.alpha=1;s.metallic=0;s.specular=half3(.04,.04,.04);s.smoothness=lerp(.26,.63,wet);s.occlusion=1;s.normalTS=half3(0,0,1);
 float caustic=pow(saturate(sin(i.w.x*3+sin(i.w.z*2+_Time.y*.2))*sin(i.w.z*3+sin(i.w.x*1.8))),10);
 s.emission=half3(.025,.065,.05)*caustic*wet*saturate(n.y)+albedo*_Vegetation*.18;
 half4 col=UniversalFragmentPBR(data,s);col.rgb=MixFog(col.rgb,i.fog);return col;
 }
 ENDHLSL
 }
 Pass{Name "ShadowCaster" Tags{"LightMode"="ShadowCaster"} ZWrite On ColorMask 0
 HLSLPROGRAM
 #pragma vertex shadowVert
 #pragma fragment shadowFrag
 #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
 float3 _LightDirection;float3 _LightPosition;
 struct A{float4 p:POSITION;float3 n:NORMAL;};
 float4 shadowVert(A i):SV_POSITION{float3 w=TransformObjectToWorld(i.p.xyz);float3 n=TransformObjectToWorldNormal(i.n);float3 l=_LightDirection;
 #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
 l=normalize(_LightPosition-w);
 #endif
 float4 p=TransformWorldToHClip(ApplyShadowBias(w,n,l));
 #if UNITY_REVERSED_Z
 p.z=min(p.z,p.w*UNITY_NEAR_CLIP_VALUE);
 #else
 p.z=max(p.z,p.w*UNITY_NEAR_CLIP_VALUE);
 #endif
 return p;}
 half4 shadowFrag():SV_Target{return 0;}
 ENDHLSL
 }
 Pass{Name "DepthOnly" Tags{"LightMode"="DepthOnly"} ZWrite On ColorMask 0
 HLSLPROGRAM
 #pragma vertex depthVert
 #pragma fragment depthFrag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 float4 depthVert(float4 p:POSITION):SV_POSITION{return TransformObjectToHClip(p.xyz);}
 half4 depthFrag():SV_Target{return 0;}
 ENDHLSL
 }
 }
}
