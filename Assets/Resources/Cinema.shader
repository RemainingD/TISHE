Shader "Tishe/Cinema" {
 Properties{_MainTex("Scene",2D)="white"{}}
 SubShader {Cull Off ZWrite Off ZTest Always Pass {CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;
 fixed4 frag(v2f_img i):SV_Target {
    float3 c=tex2D(_MainTex,i.uv).rgb;
    float l=dot(c,float3(.299,.587,.114));
    float shadow=1-smoothstep(.025,.44,l);
    float red=smoothstep(.025,.24,c.r-max(c.g,c.b));
    c=lerp(l.xxx,c,lerp(.81,.98,red));
    c*=lerp(float3(1,1,1),float3(.87,.88,.97),shadow*(1-red));
    float2 p=(i.uv-.5)*float2(1.03,.88);
    float edge=smoothstep(.21,.69,length(p));
    return float4(c*(1-edge*.32),1);
 }
 ENDCG}}
}
