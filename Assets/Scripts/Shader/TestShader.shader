Shader "Unlit/TestShader"
{
	Properties
	{
		_MainTex("Hair Texture", 2D) = "white" {}
		_Color("Base Color", Color) = (1,1,1,1)
		_Render("My Player", float) = 1
	}
	SubShader
	{
        Tags {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			// GPU Instancing
			#pragma multi_compile_instancing
			// make fog work
			#pragma multi_compile_fog

			// Receive Shadow
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float fogCoord : TEXCOORD1;
				float3 normal : NORMAL;
				float3 worldPos : TEXCOORD2;
				float4 shadowCoord : TEXCOORD3;
				float3 viewDir : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				//VR 지원
				UNITY_VERTEX_OUTPUT_STEREO
			};

			//최적화를 위함 - 경량 패턴
			CBUFFER_START(UnityPerMaterial)

			TEXTURE2D(_MainTex);
			TEXTURE2D(_ThicknessMap);
			SAMPLER(sampler_MainTex);
			half4 _MainTex_ST;
			float _Render = 1;
			CBUFFER_END

			v2f vert(appdata v)
			{
				v2f o;
				//GPU 인스턴싱 = 경량패턴
				//각 오브젝트의 아이디
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //VR 설정

				//오브젝트 공간에서 월드 공간으로 노말 벡터 변환
				o.normal = TransformObjectToWorldNormal(v.normal);

				//눈이 쌓이는 효과를 위해 먼저 월드 공간으로 정점 변환
				o.vertex = float4(TransformObjectToWorld(v.vertex.xyz), 1.0);

				//텍스처 스케일 및 오프셋이 올바로 적용되는지 확인 및
				//텍스처를 샘플링해서 색상 프로퍼티에 곱한다.
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				//유니티 기본 안개맵
				o.fogCoord = ComputeFogFactor(o.vertex.z);
				//라이팅을 위해 float3의 worldpos를 저장
				o.worldPos = TransformObjectToWorld(v.vertex.xyz);

				//float3 WorldSpacePos = mul(unity_ObjectToWorld, v.vertex);
				o.viewDir = normalize(_WorldSpaceCameraPos.xyz - o.worldPos.xyz);
				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);

				//월드 공간에서 클립 공간으로
				o.vertex = TransformWorldToHClip(o.vertex.xyz);

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				//섀도우를 정점에서 하지 않고 여기서 하는 이유?
				//정점, 프래그먼트 쉐이더 각각 실행되는 환경이 다르다
				i.shadowCoord = TransformWorldToShadowCoord(i.worldPos);
				Light mainLight = GetMainLight(i.shadowCoord);

				float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

				//메인 빛을 단위 벡터로 받아온다.
				float3 lightDir = normalize(_MainLightPosition.xyz);

				//saturate = 음수 제외 max(0, ~)
				//노말과 라이트 벡터의 내적 후 값 보정
				float NdotL = saturate(dot(lightDir, i.normal)) * 0.5 + 0.5;

				//엠비언트 항(정반사)
				//SampleSH는 구면 조화 함수, IBL이라는 이미지 기반 라이팅
				//그냥 적용하면 비용이 비싸므로 SH로 단순하게
				//텍셀을 SH함수에 적용시켜 빛을 받는 방향과 세기를 받는다.
				//= 노멀 방향의 엠비언트 컬러 정보를 얻는다.
				half3 ambient = SampleSH(i.normal);

				//라이팅의 적용
				col.rgb = col.rgb * NdotL;
				col.rgb *= _MainLightColor.rgb * mainLight.shadowAttenuation * mainLight.distanceAttenuation + ambient;
				col.rgb = MixFog(col.rgb, i.fogCoord);
				if (_Render > 1)
				{
					//col.a = 0;
					clip(-1);
				}
				return col;
			}
			ENDHLSL
		}

		Pass
			{
				Name "ShadowCaster"

				Tags{"LightMode" = "ShadowCaster"}

				Cull Back

				HLSLPROGRAM

				#pragma prefer_hlslcc gles
				#pragma exclude_renderers d3d11_9x
				#pragma target 2.0

				#pragma vertex ShadowPassVertex
				#pragma fragment ShadowPassFragment

				#pragma shader_feature _ALPHATEST_ON

				// GPU Instancing
				#pragma multi_compile_instancing

				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"


				CBUFFER_START(UnityPerMaterial)
				half4 _TintColor;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				float   _Alpha;
				CBUFFER_END

				struct VertexInput
				{
				float4 vertex : POSITION;
				float4 normal : NORMAL;

				#if _ALPHATEST_ON
				float2 uv     : TEXCOORD0;
				#endif

				UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct VertexOutput
				{
				float4 vertex : SV_POSITION;
				#if _ALPHATEST_ON
				float2 uv     : TEXCOORD0;
				#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO

				};

				VertexOutput ShadowPassVertex(VertexInput v)
				{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
				float3 normalWS = TransformObjectToWorldNormal(v.normal.xyz);

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz));

				o.vertex = positionCS;
				#if _ALPHATEST_ON
				o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw; ;
				#endif

				return o;
				}

				half4 ShadowPassFragment(VertexOutput i) : SV_TARGET
				{
					UNITY_SETUP_INSTANCE_ID(i);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

					#if _ALPHATEST_ON
					float4 col = tex2D(_MainTex, i.uv);
					clip(col.a - _Alpha);
					#endif

					return 0;
				}
				ENDHLSL
			}
	}
}
