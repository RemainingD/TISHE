Shader "Tishe/Television snow" {
Properties { _Power("Power",Range(0,1))=1 }
SubShader { Tags {"RenderType"="Opaque"} Pass {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
float _Power;
struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord.xy;return o;}
float hash(float2 p){float3 q=frac(p.xyx*float3(.1031,.11369,.13787));q+=dot(q,q.yzx+19.19);return frac((q.x+q.y)*q.z);}
float4 frag(v2f i):SV_Target{
    float2 uv=i.uv,p=uv*2-1;float frame=floor(_Time.y*23);
    float2 cell=floor(uv*float2(150,102));
    float grain=hash(cell+float2(frame*7,frame*11));
    float band=1-.17*(1-smoothstep(0,.035,abs(frac(uv.y-_Time.y*.19)-.52)));
    float level=(.17+.65*grain)*band*(.85+.15*cos(cell.y*3.14159))*(1-.28*dot(p,p));
    float edge=saturate(1-pow(abs(p.x),7)-pow(abs(p.y),7));
    float3 off=float3(.034,.046,.049)+.018*(1-uv.y);
    return float4(lerp(off,level*float3(.94,.98,1),_Power*smoothstep(0,.09,edge)),1);
}
ENDCG
} } }
