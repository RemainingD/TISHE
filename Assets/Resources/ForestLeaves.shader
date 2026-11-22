Shader "Tishe/Forest needles" {
Properties {
    _MainTex ("Colour", 2D)="white"{}
    _Mask ("Silhouette", 2D)="white"{}
    _Color ("Tint",Color)=(1,1,1,1)
    _Cutoff ("Alpha cutoff",Range(0,1))=.33
}
SubShader {
    Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"}
    Cull Off
    CGPROGRAM
    #pragma surface surf Lambert alphatest:_Cutoff addshadow
    #pragma multi_compile_instancing
    sampler2D _MainTex,_Mask;
    fixed4 _Color;
    struct Input {float2 uv_MainTex;};
    void surf(Input IN,inout SurfaceOutput o){
        o.Albedo=tex2D(_MainTex,IN.uv_MainTex).rgb*_Color.rgb;
        o.Alpha=tex2D(_Mask,IN.uv_MainTex).r;
    }
    ENDCG
}
Fallback "Transparent/Cutout/Diffuse"
}
