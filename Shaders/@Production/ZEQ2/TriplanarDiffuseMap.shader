Shader "Zios/ZEQ2/Triplanar Diffuse Map"{
	Properties{
		diffuseMap("Diffuse Map",2D) = "white"{}
		[MaterialToggle] xBlending("X Blending",Float) = 0
		[MaterialToggle] yBlending("Y Blending",Float) = 0
		[MaterialToggle] zBlending("Z Blending",Float) = 0
	}
	SubShader{
		Tags{"LightMode"="ForwardBase"}
		Pass{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#pragma vertex vertexPass
			#pragma fragment pixelPass
			#pragma multi_compile_fwdbase
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D diffuseMap;
			fixed4 diffuseMap_ST;
			fixed xBlending;
			fixed yBlending;
			fixed zBlending;
			struct vertexInput{
				float4 vertex        : POSITION;
				float4 texcoord      : TEXCOORD0;
				float4 texcoord1     : TEXCOORD1;
				float3 normal        : NORMAL;
				fixed4 color         : COLOR;
			};
			struct vertexOutput{
				float4 pos           : POSITION;
				float4 UV            : COLOR0;
				float4 normal        : TEXCOORD1;
				float3 worldNormal   : TEXCOORD4;
				float3 worldPosition : TEXCOORD5;
				LIGHTING_COORDS(6,7)
			};
			struct pixelOutput{
				float4 color         : COLOR0;
			};
			pixelOutput setupPixel(vertexOutput input){
				pixelOutput output;
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output.color = float4(0,0,0,0);
				return output;
			}
			float4 setupTriplanarMap(sampler2D triplanar,float4 offset,vertexOutput input){
				float4 color1 = tex2D(triplanar,input.worldPosition.xy * offset.xy + offset.zw);
				float4 color2 = tex2D(triplanar,input.worldPosition.zx * offset.xy + offset.zw);
				float4 color3 = tex2D(triplanar,input.worldPosition.zy * offset.xy + offset.zw);
				input.worldNormal = normalize(input.worldNormal);
				float3 projectedNormal = saturate(pow(input.worldNormal*1.5,4));
				if(xBlending != 0){projectedNormal.x = ceil(projectedNormal.x-0.5f);}
				if(yBlending != 0){projectedNormal.y = ceil(projectedNormal.y-0.5f);}
				if(zBlending != 0){projectedNormal.z = ceil(projectedNormal.z-0.5f);}
				float3 color = lerp(color2,color1,projectedNormal.z);
				color = lerp(color,color3,projectedNormal.x);
				return float4(color,1.0);
			}
			pixelOutput applyTriplanarDiffuseMap(vertexOutput input,pixelOutput output){
				output.color = setupTriplanarMap(diffuseMap,diffuseMap_ST,input);
				return output;
			}
			vertexOutput vertexPass(vertexInput input){
				vertexOutput output;
				UNITY_INITIALIZE_OUTPUT(vertexOutput,output)
				output.pos = UnityObjectToClipPos(input.vertex);
				output.UV = float4(input.texcoord.x,input.texcoord.y,0,0);
				output.worldNormal = mul(unity_ObjectToWorld,float4(input.normal,0.0f)).xyz;
				output.worldPosition = mul(unity_ObjectToWorld,input.vertex);
				TRANSFER_VERTEX_TO_FRAGMENT(output);
				return output;
			}
			pixelOutput pixelPass(vertexOutput input){
				pixelOutput output = setupPixel(input);
				UNITY_INITIALIZE_OUTPUT(pixelOutput,output)
				output = applyTriplanarDiffuseMap(input,output);
				return output;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}