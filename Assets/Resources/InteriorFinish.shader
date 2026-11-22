Shader "Tishe/Interior finishes" {
Properties {
    _Atlas("Domestic finishes",2D)="white"{}
    _Boards("Floorboards",2D)="white"{}
    _FloorNormal("Board grain",2D)="bump"{}
    _PlasterNormal("Surface grain",2D)="bump"{}
    _Slab("Floor slab",Float)=0
}
SubShader {
    Tags {"RenderType"="Opaque"} LOD 200
    CGPROGRAM
    #pragma surface surf Standard fullforwardshadows vertex:vert
    #pragma target 3.0
    #include "UnityStandardUtils.cginc"
    sampler2D _Atlas,_Boards,_FloorNormal,_PlasterNormal;
    half _Slab;
    struct Input {float3 worldPos;float3 planeNormal;};
    void vert(inout appdata_full v,out Input o){
        UNITY_INITIALIZE_OUTPUT(Input,o);o.planeNormal=UnityObjectToWorldNormal(v.normal);
    }
    float hashCell(float2 p){return frac(sin(dot(p,float2(41.33,73.91)))*12713.53);}
    float patch(float2 uv){
        float2 c=floor(uv),f=frac(uv);f=f*f*(3-2*f);
        return lerp(lerp(hashCell(c),hashCell(c+float2(1,0)),f.x),
                    lerp(hashCell(c+float2(0,1)),hashCell(c+1),f.x),f.y);
    }
    half3 sampleFinish(float2 p,float2 cell){
        return tex2Dgrad(_Atlas,(cell+.004+frac(p)*.992)*.5,ddx(p)*.496,ddy(p)*.496).rgb;
    }
    void surf(Input IN,inout SurfaceOutputStandard o){
        float3 p=IN.worldPos,n=normalize(IN.planeNormal);
        float3 room=p+n*.13;
        float level=room.y>4.13?4.2:0;
        float h=p.y-level;
        float2 wall=float2(abs(n.x)>abs(n.z)?p.z:p.x,p.y);
        float weather=patch(wall*float2(.49,.39))*.68+patch(wall*float2(1.9,1.4))*.32;
        half3 col;
        half gloss=.12;
        half3 bump=half3(0,0,1);
        if(_Slab>.5 && n.y>.55){
            float2 uv=p.xz*float2(.48,.36);
            if(room.x>5.9)uv=uv.yx;
            col=tex2D(_Boards,uv).rgb*half3(.60,.53,.46);
            float track=1-smoothstep(.25,1.48,abs(p.x+.3*sin(p.z*.7)));
            col*=.88+.14*patch(p.xz*.72)+track*.12;
            bump=UnpackScaleNormal(tex2D(_FloorNormal,uv),.46);gloss=.19+track*.07;
            if(room.y>4.13 && room.x>5.95 && room.z< -5.94){
                half3 tile=sampleFinish(p.xz*1.37,float2(0,0));
                col=lerp(tile,half3(dot(tile,half3(.3,.58,.12)),dot(tile,half3(.3,.58,.12)),dot(tile,half3(.3,.58,.12))),.66)*half3(.37,.40,.38);
                bump=half3(0,0,1);gloss=.25;
            }
        }else if(n.y<-.55 || abs(n.y)>.7){
            float2 uv=p.xz*.31;
            // Mirror the atlas edges so the limewash runs across the whole soffit.
            float2 grainUV=uv+float2(.08*sin(uv.y*2.7),.06*sin(uv.x*2.2));
            float2 crossUV=float2(grainUV.y,-grainUV.x)*.73+float2(.37,.12);
            float2 grain=min(abs(frac(grainUV*.5)*2-1),.9999);
            float2 crossGrain=min(abs(frac(crossUV*.5)*2-1),.9999);
            col=lerp(sampleFinish(grain,float2(1,0)),sampleFinish(crossGrain,float2(1,0)),.35)*.78;
            col*=.87+.13*patch(p.xz*.53);
            bump=UnpackScaleNormal(tex2D(_PlasterNormal,p.xz*.64),.19);
        }else{
            bool west=room.x< -5.92;
            bool upper=room.y>4.13;
            bool paper=west&&(room.z>5.91 || room.z< -5.91 || (upper&&abs(room.z)<3.69));
            bool bath=upper&&room.x>5.95&&room.z< -5.94;
            bool cellar=p.x>20 && p.y<0;
            float2 uv=wall*.63;
            half3 paint=sampleFinish(uv,float2(1,1));
            col=paint*half3(.77,.80,.78);
            if(paper){
                float2 repeat=wall*float2(.73,.71);
                half3 floral=sampleFinish(repeat,float2(0,1));
                half3 tint=room.z< -5.9?half3(.77,.69,.62):upper&&room.z<4?half3(.81,.77,.72):half3(.82,.76,.66);
                float lower=1-smoothstep(.16,.92,h);
                float seam=1-smoothstep(.008,.055,abs(frac(wall.x/.53)-.5));
                float cuts=weather*.54+.23*patch(wall*float2(4.1,2.1))+
                    .14*patch(wall*float2(12.3,8.3))+.09*patch(wall*35)+seam*.075+lower*.09;
                float peeled=smoothstep(.724,.755,cuts);
                col=lerp(floral*tint,lerp(floral*tint,paint*half3(.82,.74,.65),.67),peeled);
            }else if(bath && h<1.92){
                col=sampleFinish(wall/.96,float2(0,0))*.81;gloss=.31;
            }else if(cellar){
                col*=half3(.58,.62,.58);
            }else if(!west && !bath && h>0 && h<1.22){
                col*=half3(.72,.78,.74);
            }
            if(n.z<-.7 && abs(p.z-13.2)<.08 && !upper){
                float2 edge=abs(float2(p.x-2.32,h-2.57))-float2(.35,.43);
                float printMask=1-smoothstep(-.005,.012,max(edge.x,edge.y));
                col*=1+.13*printMask;
                col*=.99-.44*(1-smoothstep(.003,.012,length(float2(p.x-2.32,h-2.98))));
            }
            float damp=1-smoothstep(.02,.18+.47*weather,h);
            col*=1-.24*damp;
            bump=UnpackScaleNormal(tex2D(_PlasterNormal,wall*.74),bath?.13:.24);
        }
        o.Albedo=col;o.Normal=bump;o.Smoothness=gloss;o.Metallic=0;
    }
    ENDCG
}
Fallback "Standard"
}
