using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BB_Extensions.MaterialEditor {
  public class MaterialEditor_Compiler {
    public static void Compile(MaterialData materialData) {
      string path = AssetDatabase.GetAssetPath(materialData);
      path = path.Substring(0, path.LastIndexOf('/')) + "/";

      string fileName = materialData.name;

      if (File.Exists(path + fileName + ".shader")) {
        StreamWriter sw = new StreamWriter(path + fileName + ".shader", false);
        sw.Write(CreateShaderSource(materialData));
        sw.Close();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
      } else {
        StreamWriter sw = File.CreateText(path + fileName + ".shader");
        sw.Write(CreateShaderSource(materialData));
        sw.Close();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
      }
      
      Material material = AssetDatabase.LoadAssetAtPath<Material>(path + fileName + ".mat");

      if (material == null) {
        AssetDatabase.CreateAsset(new Material(AssetDatabase.LoadAssetAtPath<Shader>(path + fileName + ".shader")), path + fileName + ".mat");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
      } else {
        if (material.shader == null) {
          material.shader = AssetDatabase.LoadAssetAtPath<Shader>(path + fileName + ".shader");
        }

        // TODO(Brajan): here and somewhere else in code do checks that node is connected
        for (int i = 0; i < materialData.nodes.Count; ++i) {
          if (materialData.nodes[i].type == Node.Type.Texture) {
            material.SetTexture("_Texture" + i, materialData.nodes[i].outputSlots[0].texture2dValue);
          }
        }

        EditorUtility.SetDirty(material);
      }
    }

    const string subshaderStart = 
@"SubShader {
Tags { ""RenderType""=""Opaque"" }
LOD 200
		
CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0";


    const string subshaderEnd = 
@"ENDCG
}
FallBack ""Diffuse""";
    static string CreateShaderSource(MaterialData materialData) {
      string shaderName = "Shader \"MaterialEditor/" + materialData.name + "\"";
      string properties = "";
      string uniforms = "";		  
      string inputStructContent = "";

      for (int i = 0; i < materialData.nodes.Count; ++i) {
        if (materialData.nodes[i].type == Node.Type.Texture) {
          properties += "_Texture" + i + " (\"Texture" + i + "\", 2D) = \"white\" {}";
          uniforms += string.Format("sampler2D {0};\n", "_Texture" + i);
          inputStructContent += "float2 uv_Texture" + i + ";";
        }
      }

      string combined = shaderName + "{";
      combined += "Properties {\n" + properties + "\n}\n";
      combined += subshaderStart + "\n";
      if (inputStructContent.Length > 0) {
        combined += "struct Input {\n" + inputStructContent + "\n};\n";
      } else {
        combined += "struct Input {\nfloat fakeValue; \n};\n";
      }
      combined += uniforms + "\n";
      combined += GetShaderFunction(materialData) + "\n";
      combined += subshaderEnd;
      combined += "\n}";
      return combined;
    }

    static string GetShaderFunction(MaterialData materialData) {
      string albedoOutput = "o.Albedo = fixed4(1, 1, 1, 1);";
      string normalOutput = "";
      string metallicOutput = "o.Metallic = 0;";
      string smoothnessOutput = "o.Smoothness = 0;";
      string opacityOutput = "o.Alpha = 0;";
      string emissionOutput = "";
      string heightMapOutput = "";
      string occlussionOutput = "";

      int materialNodeIndex = -1;
      for (int i = 0; i < materialData.nodes.Count; ++i) {
        if (materialData.nodes[i].type == Node.Type.Output) {
          materialNodeIndex = i;
          break;
        }
      }

      if (materialNodeIndex < 0) {
        Debug.LogError("Couldnt find output node");
      }

      // albedo
      {
        string code = ParseMaterialInputSlot(materialData, materialNodeIndex, 0);
        if (code != null) {
          albedoOutput = "o.Albedo = " + code + ";";
        }
      }

      // normal
      {
        string code = ParseMaterialInputSlot(materialData, materialNodeIndex, 1);
        if (code != null) {
          normalOutput = "o.Normal = UnpackNormal(" + code + ");";
        }
      }

      // metallic
      {
        string code = ParseMaterialInputSlot(materialData, materialNodeIndex, 2);
        if (code != null) {
          metallicOutput = "o.Metallic = " + code + ";";
        }
      }

      // smoothness
      {
        string code = ParseMaterialInputSlot(materialData, materialNodeIndex, 3);
        if (code != null) {
          smoothnessOutput = "o.Smoothness = " + code + ";";
        }
      }

      // opacity
      {
        string code = ParseMaterialInputSlot(materialData, materialNodeIndex, 4);
        if (code != null) {

        }
      }

      // emission
      {
        string code = ParseMaterialInputSlot(materialData, materialNodeIndex, 5);
        if (code != null) {
          emissionOutput = "o.Emission = " + code + ";";
        }
      }

      // heightmap
      {
        string code = ParseMaterialInputSlot(materialData, materialNodeIndex, 6);
        if (code != null) {

        }
      }

      // occlusion
      {
        string code = ParseMaterialInputSlot(materialData, materialNodeIndex, 7);
        if (code != null) {
          occlussionOutput = "o.Albedo *= " + code + ";";
        }
      }

      string combined = "void surf (Input IN, inout SurfaceOutputStandard o) {\n";
      combined += albedoOutput + "\n";
      combined += normalOutput + "\n";
      combined += metallicOutput + "\n";
      combined += smoothnessOutput + "\n";
      combined += opacityOutput + "\n";
      combined += emissionOutput + "\n";
      combined += heightMapOutput + "\n";
      combined += occlussionOutput + "\n";
      combined += "\n}";
      return combined;
    }

    static string ParseNodeOutput(MaterialData materialData, int nodeIndex, int outputSlot) {
      string result = null;

      switch (materialData.nodes[nodeIndex].type) {
        case Node.Type.ConstFloat: {
          if (outputSlot == 0) {
            result = materialData.nodes[nodeIndex].outputSlots[outputSlot].floatValue.ToString();
          }
        } break;

        case Node.Type.ConstVector2: {
          Vector2 vec = materialData.nodes[nodeIndex].outputSlots[0].vector2Value;

          switch (outputSlot) {
            case 0: {
              result = string.Format("float2({0}, {1})", vec.x, vec.y);
            } break;

            case 1: {
              result = vec.x.ToString();
            } break;

            case 2: {
              result = vec.y.ToString();
            } break;
          }
        } break;

        case Node.Type.ConstVector3: {
          Vector3 vec = materialData.nodes[nodeIndex].outputSlots[0].vector3Value;

          switch (outputSlot) {
            case 0: {
              result = string.Format("float3({0}, {1}, {2})", vec.x, vec.y, vec.z);
            } break;

            case 1: {
              result = vec.x.ToString();
            } break;

            case 2: {
              result = vec.y.ToString();
            } break;

            case 3: {
              result = vec.z.ToString();
            } break;
          }
        } break;

        case Node.Type.ConstColor: {
          Color color = materialData.nodes[nodeIndex].outputSlots[0].colorValue;

          switch (outputSlot) {
            case 0: {
              result = string.Format("float4({0}, {1}, {2}, {3})", color.r, color.g, color.b, color.a);
            } break;

            case 1: {
              result = color.r.ToString();
            } break;

            case 2: {
              result = color.g.ToString();
            } break;

            case 3: {
              result = color.b.ToString();
            } break;

            case 4: {
              result = color.a.ToString();
            } break;
          }
        } break;

        case Node.Type.Texture: {
          switch (outputSlot) {
            case 1: {
              result = "tex2D(_Texture" + nodeIndex + ", IN.uv_Texture" + nodeIndex + ")";
            } break;

            case 2: {
              result = "tex2D(_Texture" + nodeIndex + ", IN.uv_Texture" + nodeIndex + ").r";
            } break;

            case 3: {
              result = "tex2D(_Texture" + nodeIndex + ", IN.uv_Texture" + nodeIndex + ").g";
            } break;

            case 4: {
              result = "tex2D(_Texture" + nodeIndex + ", IN.uv_Texture" + nodeIndex + ").b";
            } break;

            case 5: {
              result = "tex2D(_Texture" + nodeIndex + ", IN.uv_Texture" + nodeIndex + ").a";
            } break;
          }
        } break;

        case Node.Type.Time: {
          result = "_Time.y";
        } break;

        case Node.Type.SinTime: {
          result = "_SinTime.w";
        } break;

        case Node.Type.CosTime: {
          result = "_CosTime.w";
        } break;

        case Node.Type.Add: {
          string a = ParseInputSlot(materialData, nodeIndex, 0);
          string b = ParseInputSlot(materialData, nodeIndex, 1);

          if (a == null) {
            return null;
          }

          if (b == null) {
            return null;
          }

          result = string.Format("({0} + {1})", a, b);
        } break;

        case Node.Type.Subtraction: {
          string a = ParseInputSlot(materialData, nodeIndex, 0);
          string b = ParseInputSlot(materialData, nodeIndex, 1);

          if (a == null) {
            return null;
          }

          if (b == null) {
            return null;
          }

          result = string.Format("({0} - {1})", a, b);
        } break;

        case Node.Type.Multiply: {
          string a = ParseInputSlot(materialData, nodeIndex, 0);
          string b = ParseInputSlot(materialData, nodeIndex, 1);

          if (a == null) {
            return null;
          }

          if (b == null) {
            return null;
          }

          result = string.Format("({0} * {1})", a, b);
        } break;

        case Node.Type.Divide: {
          string a = ParseInputSlot(materialData, nodeIndex, 0);
          string b = ParseInputSlot(materialData, nodeIndex, 1);

          if (a == null) {
            return null;
          }

          if (b == null) {
            return null;
          }

          result = string.Format("({0} / {1})", a, b);
        } break;

        case Node.Type.PackVector2: {
          string x = ParseInputSlot(materialData, nodeIndex, 0);
          string y = ParseInputSlot(materialData, nodeIndex, 1);

          if (x == null) {
            x = "0";
          }

          if (y == null) {
            y = "0";
          }
          result = string.Format("float2({0}, {1})", x, y);
        } break;

        case Node.Type.PackVector3: {
          string x = ParseInputSlot(materialData, nodeIndex, 0);
          string y = ParseInputSlot(materialData, nodeIndex, 1);
          string z = ParseInputSlot(materialData, nodeIndex, 2);

          if (x == null) {
            x = "0";
          }

          if (y == null) {
            y = "0";
          }

          if (z == null) {
            z = "0";
          }

          result = string.Format("float3({0}, {1}, {2})", x, y, z);
        } break;

        case Node.Type.PackColor: {
          string r = ParseInputSlot(materialData, nodeIndex, 0);
          string g = ParseInputSlot(materialData, nodeIndex, 1);
          string b = ParseInputSlot(materialData, nodeIndex, 2);
          string a = ParseInputSlot(materialData, nodeIndex, 2);

          if (r == null) {
            r = "0";
          }

          if (g == null) {
            g = "0";
          }

          if (b == null) {
            b = "0";
          }

          if (a == null) {
            a = "0";
          }

          result = string.Format("float4({0}, {1}, {2}, {3})", r, g, b, a);
        } break;

        case Node.Type.Lerp: {
          string a = ParseInputSlot(materialData, nodeIndex, 0);
          string b = ParseInputSlot(materialData, nodeIndex, 1);
          string t = ParseInputSlot(materialData, nodeIndex, 2);

          if (a == null) {
            return null;
          }

          if (b == null) {
            return null;
          }

          if (t == null) {
            t = "0";
          }

          result = string.Format("lerp({0}, {1}, {2})", a, b, t);
        } break;
      }

      return result;
    }

    static Connection FindConnectionByInputSlot(MaterialData materialData, int nodeIndex, int inputSlot) {
      foreach (Connection connection in materialData.connections) {
        if (connection.nodeB == nodeIndex && connection.nodeBSlot == inputSlot) {
          return connection;
        }
      }

      return null;
    }

    static string ParseInputSlot(MaterialData materialData, int nodeIndex, int slot) {
      Connection connection = FindConnectionByInputSlot(materialData, nodeIndex, slot);
      if (connection == null) {
        return null;
      }

      return ParseNodeOutput(materialData, connection.nodeA, connection.nodeASlot);
    }

    static string ParseMaterialInputSlot(MaterialData materialData, int materialNodeIndex, int slot) {
      string result = null;
      foreach (Connection connection in materialData.connections) {
        if (connection.nodeB == materialNodeIndex && connection.nodeBSlot == slot) {
          result = ParseNodeOutput(materialData, connection.nodeA, connection.nodeASlot);
          break;   
        }
      }

      if (result == null) {
        return null;
      }

      return result;
    }
  }
}