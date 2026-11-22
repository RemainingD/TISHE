Shader "Tishe/Forest ground" {
Properties {
    _MainTex ("Forest litter",2D)="white"{}
    _Trail ("Trail soil",2D)="white"{}
    _Normal ("Forest relief",2D)="bump"{}
}
SubShader {
    Tags {"RenderType"="Opaque"}
    CGPROGRAM
    #pragma surface surf Standard
    #include "UnityStandardUtils.cginc"
    sampler2D _MainTex,_Trail,_Normal;
    struct Input {float2 uv_MainTex;float4 color:COLOR;};
    void surf(Input IN,inout SurfaceOutputStandard o){
        float2 uv=IN.uv_MainTex;
        o.Albedo=lerp(tex2D(_MainTex,uv).rgb,tex2D(_Trail,uv*.68).rgb,IN.color.r)*IN.color.g*float3(.58,.63,.63);
        o.Normal=UnpackScaleNormal(tex2D(_Normal,uv),.57);
        o.Smoothness=.13;
    }
    ENDCG
}
Fallback "Diffuse"
}
