Shader "Tishe/Hearth flame" {
Properties { _Seed("Variation",Float)=0 }
SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" "DisableBatching"="True" }
Pass { Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
float _Seed;
struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord.xy;return o;}
float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*4375.5453);}
float noise(float2 p){float2 f=frac(p);f=f*f*(3-2*f);float2 i=floor(p);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
float4 frag(v2f i):SV_Target{
    float2 uv=i.uv;float t=_Time.y;float y=uv.y;
    float drift=(noise(float2(y*4.0-t*.75,_Seed+3))-.5)*(.025+y*.19);
    float x=uv.x+drift;
    float2 q=float2(x*10+_Seed,y*2.7-t*2.25);
    float n=noise(q)*.54+noise(q*2.07+4)*.30+noise(q*4.13+17)*.16;
    float width=saturate(1-abs(x-.5)*1.95);
    float height=(.12+1.16*n*n)*pow(width,.58);
    float heat=(height-y)*3.8+(n-.52)*.34;
    float a=smoothstep(-.015,.095,heat)*smoothstep(0,.07,uv.y)*smoothstep(0,.06,min(uv.x,1-uv.x));
    float3 col=lerp(float3(.68,.095,.012),float3(1.0,.43,.033),saturate(heat*2.6));
    col=lerp(col,float3(1,.90,.46),smoothstep(.40,1.0,heat));
    float2 sparkUV=float2(uv.x*28+sin(uv.y*8+t)*.28,uv.y*8-t*1.4);
    float spark=step(.974,hash(floor(sparkUV)+_Seed))*saturate(1-length((frac(sparkUV)-.5)*float2(7,5)))*smoothstep(.25,.48,y)*(1-smoothstep(.73,.97,y));
    return float4(lerp(col,float3(1,.54,.15),spark),max(a*.95,spark));
}
ENDCG
} } }
