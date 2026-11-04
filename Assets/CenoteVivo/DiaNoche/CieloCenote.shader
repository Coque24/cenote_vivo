Shader "CenoteVivo/CieloDiaNoche"
{
 Properties { _Noche("Noche",Range(0,1))=0 }
 SubShader {
 Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline"}
 Cull Off ZWrite Off
 Pass { HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 float _Noche;
 struct A {float4 p:POSITION;}; struct V {float4 p:SV_POSITION;float3 d:TEXCOORD0;};
 V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.d=i.p.xyz;return o;}
 float hash(float3 p){p=frac(p*.1031);p+=dot(p,p.yzx+33.33);return frac((p.x+p.y)*p.z);}
 float noise(float3 p){float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(a),hash(a+float3(1,0,0)),f.x),lerp(hash(a+float3(0,1,0)),hash(a+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(a+float3(0,0,1)),hash(a+float3(1,0,1)),f.x),lerp(hash(a+float3(0,1,1)),hash(a+1),f.x),f.y),f.z);}
 half4 frag(V i):SV_Target {
 float3 d=normalize(i.d);float h=saturate(d.y);float n=saturate(_Noche);
 float3 day=lerp(float3(.66,.82,.83),float3(.06,.31,.57),pow(h,.45));
 float3 night=lerp(float3(.035,.065,.12),float3(.003,.009,.032),pow(h,.45));
 float3 col=lerp(day,night,n);
 float cloud=noise(d*7+float3(_Time.y*.002,0,0))*.65+noise(d*17)*.35;
 cloud=smoothstep(.52,.74,cloud)*smoothstep(-.02,.20,d.y);
 col=lerp(col,lerp(float3(.92,.91,.82),float3(.06,.09,.15),n),cloud*lerp(.65,.30,n));
 float3 sd=normalize(float3(-.35,.5,.55));float sun=dot(d,sd);
 col+=float3(1,.78,.4)*(pow(saturate(sun),360)*2+pow(saturate(sun),18)*.1)*(1-n);
 float3 md=normalize(float3(.28,.64,.48));float moon=dot(d,md);
 col+=float3(.66,.78,1)*(smoothstep(.99915,.99955,moon)*1.4+pow(saturate(moon),70)*.08)*n;
 float3 grid=d*240;float stars=step(.995,hash(floor(grid)))*pow(saturate(1-length(frac(grid)-.5)*2),8);
 col+=stars*3*n*smoothstep(.02,.3,d.y)*(1-cloud);
 return half4(col,1);
 }
 ENDHLSL }
 }
}
