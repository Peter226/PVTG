using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PVTG {
    public class ComputePattern
    {
        static Dictionary<string, System.Type> computeBufferTypes = new Dictionary<string, System.Type>() {
            { "Bool", typeof(bool) },
            { "Float", typeof(float) },
            { "Texture", typeof(Texture) },
            { "Vector", typeof(Vector4) },
            { "Int", typeof(int) }
        };
        public List<ComputeProperty> computeProperties = new List<ComputeProperty>();
        public ComputeShader shader;
        public int kernelHandle;

        public ComputePattern(string path, string pattern)
        {
            shader = Resources.Load<ComputeShader>(path);
            ComputeShader[] computeShaders = Resources.FindObjectsOfTypeAll<ComputeShader>();
            foreach (ComputeShader computeShader in computeShaders)
            {
                if (computeShader.name == path)
                {
                    shader = computeShader;
                    break;
                }
            }
            kernelHandle = shader.FindKernel("CSMain");

            string[] lines = pattern.Split('\n');
            foreach (string line in lines)
            {
                string[] firstPart = line.Split(':');
                if (firstPart.Length == 2)
                {
                    string[] secondPart = firstPart[1].Split('=');
                    string[] firstSplit = firstPart[0].Split('>');
                    if (secondPart.Length == 2 && firstSplit.Length == 2)
                    {
                        if (computeBufferTypes.ContainsKey(firstSplit[1])) {
                            computeProperties.Add(new ComputeProperty(secondPart[0],secondPart[1], firstSplit[0] == "O", computeBufferTypes[firstSplit[1]],shader));
                        }
                    }
                }
            }
        }
        



    }

    public struct ComputeProperty
    {
        public int id;
        public string name;
        public System.Type type;
        public bool isOutput;
        public ComputeProperty(string displayName, string propertyName, bool isOutput, System.Type kernelType, ComputeShader shader)
        {
            id = Shader.PropertyToID(propertyName);
            name = displayName;
            type = kernelType;
            this.isOutput = isOutput;
        }
    }
}