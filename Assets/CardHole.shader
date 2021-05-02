// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/CardHole"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        _HoleRadius ("HoleRadius", Range(0,100)) = 1
        [Header(remap mousePos to  uvPos(0to1) )]
        [Header(lastMousePos(x,y),currentMousePos(z,w) )]
        _MousePos ("MousePos In UV", vector) = (0,0,0,0)

    }
    CGINCLUDE
    #include "unitycg.cginc"

    struct v2f
    {
        float4 pos:SV_POSITION ;
        float2 uv:TEXCOORD1;
    };

    sampler2D _MainTex;
    //{TextureName}_TexelSize - a float4 property contains texture size information:
    // - x contains 1.0/width
    // - y contains 1.0/height
    // - z contains width
    // - w contains height
    float4 _MainTex_TexelSize; //texture size
    float _HoleRadius;
    float4 _MousePos;

    //left bottom (0,0)
    inline float2 UVToTexturePos(float2 uv)
    {
        int x = (int)(uv.x * _MainTex_TexelSize.z);
        int y = (int)(uv.x * _MainTex_TexelSize.w);
        return float2(x,y);
    }

    v2f vert(appdata_base v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = v.texcoord;
        return o;
    }

    float4 frag(v2f i):COLOR
    {
        float texAspect = _MainTex_TexelSize.w *_MainTex_TexelSize.x;
        // #if UNITY_UV_STARTS_AT_TOP
        // _MousePos.y = _ScreenParams.y - _MousePos.y;
        // _MousePos.w = _ScreenParams.y - _MousePos.w;
        // #endif
        float4 mainColor = tex2D(_MainTex, i.uv);
        i.uv.y *= texAspect;
        _MousePos.y *= texAspect;
        _MousePos.w *= texAspect;
        //本次挖洞点相对于上次的方向
        float2 dir = float2(_MousePos.zw - _MousePos.xy);

        float2 lastVec = float2(i.uv.xy - _MousePos.xy);
        float2 thisVec = float2(i.uv.xy - _MousePos.zw);

        bool isInMiddleAera = dot(lastVec, dir) * dot(thisVec, dir) < 0;

        if (isInMiddleAera)
        {
            float2 lastToVertical = normalize(dir) * dot(lastVec, normalize(dir));
            float2 distVec = lastVec - lastToVertical;
            //距离挖洞点的距离
            float dist = length(distVec);
            if (dist < _HoleRadius)
            {
                mainColor.a = 0;
            }

        }
        else
        {
            float lastDist = length(lastVec);
            float thisDist = length(thisVec);
            if (lastDist < _HoleRadius || thisDist < _HoleRadius )
            {
                mainColor.a = 0;
            }
        
        }
        return mainColor;
    }

    fixed4 frag_black(v2f i): SV_TARGET
    {
        return fixed4(0, 0, 0, 1);
    }

    fixed4 frag_white(v2f i): SV_TARGET
    {
        return fixed4(1, 1, 1, 1);
    }
    ENDCG
    SubShader
    {
        // Blend SrcAlpha OneMinusSrcAlpha
        // Tags
        // {
        //     "RenderType" = "Transparent" "Queue" = "Transparent"
        // }
        pass
        {
            //必须加Name，否则Graphics.Blit会出错
            Name "Update" // 0
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }

        pass//清零pass,用来重置RT
        {
            Name "Clear" // 1
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_black
            ENDCG
        }
        pass//清零pass,用来重置RT
        {
            Name "Clear2" // 2
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_white
            ENDCG
        }
    }
    FallBack "Diffuse"
}