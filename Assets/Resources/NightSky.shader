Shader "Tishe/NightSky" {
 SubShader { Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"} Cull Off ZWrite Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;};struct v2f {float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.dir=v.vertex.xyz;return o;}
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
 float4 frag(v2f i):SV_Target {float3 d=normalize(i.dir);float h=saturate(d.y);float3 col=lerp(float3(.062,.031,.044),float3(.006,.012,.025),sqrt(h));
 float2 p=d.xz/(max(.12,d.y)+.2)*3;float n=noise(p)*.55+noise(p*2.1)*.27+noise(p*4.3)*.13+noise(p*8.7)*.05;
 float cloud=smoothstep(.38,.72,n)*smoothstep(0,.15,h);col=lerp(col,lerp(float3(.127,.042,.046),float3(.043,.052,.080),smoothstep(.09,.53,h)),cloud*.65);
 float moon=dot(d,normalize(float3(-.4,.52,-.75)));float disk=smoothstep(.9981,.9986,moon);float halo=pow(saturate(moon),180)*.045;col+=float3(.72,.45,.35)*disk*(1-cloud*.7)+float3(.89,.32,.19)*halo;
 float star=step(.9988,hash(floor(d.xz/max(.1,h)*850)))*smoothstep(.2,.6,h)*(1-cloud);col+=star*.2;return float4(col,1);}
 ENDCG }
 } Fallback Off
}
