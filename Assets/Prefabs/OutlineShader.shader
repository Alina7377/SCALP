Shader "Custom/HDRPOutlineShader"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" }
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode"="ForwardOnly" }

            Cull Front
            ZWrite On // Enable depth writing
            Offset -1, -1 // Offset to prevent z-fighting
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            // Подключение HDRP библиотек
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            // Свойства шейдера
            float4 _OutlineColor;
            float _OutlineWidth;

            // Вершинные данные
            struct appdata
            {
                float4 vertex : POSITION; // Позиция в объектном пространстве
                float3 normal : NORMAL;   // Нормаль в объектном пространстве
            };

            struct v2f
            {
                float4 pos : SV_POSITION; // Позиция в clip space
            };

            v2f vert(appdata v)
            {
                v2f o;

                // Преобразование координат в мировое пространство
                float3 worldPosition = TransformObjectToWorld(v.vertex.xyz);
                float3 worldNormal = normalize(TransformObjectToWorldNormal(v.normal));

                // Увеличение размера объекта вдоль нормали
                float3 inflatedPosition = worldPosition + worldNormal * _OutlineWidth;

                // Преобразование в clip space
                o.pos = TransformWorldToHClip(inflatedPosition);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return _OutlineColor; // Цвет окантовки
            }

            ENDHLSL
        }

        Pass
        {
            Name "BASE"
            Tags { "LightMode"="ForwardOnly" }

            HLSLPROGRAM
            #pragma vertex vertBase
            #pragma fragment fragBase
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            // Базовый шейдер
            struct appdata_base
            {
                float4 vertex : POSITION; // Позиция в объектном пространстве
            };

            struct v2f_base
            {
                float4 pos : SV_POSITION; // Позиция в clip space
            };

            v2f_base vertBase(appdata_base v)
            {
                v2f_base o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                return o;
            }

            float4 fragBase(v2f_base i) : SV_Target
            {
                return float4(1, 1, 1, 1); // Основной цвет (белый)
            }

            ENDHLSL
        }
    }
}
