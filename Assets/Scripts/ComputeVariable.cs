using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PVTG.Graph
{
    public abstract class ComputeVariable
    {
        private GraphVariable variable;
        private string name;
        private int kernel;

        public int KernelID
        {
            get { return kernel; }
        }

        public ComputeVariable(string variableName,int kernelID)
        {
            name = variableName;
            kernel = kernelID;
        }




    }
}
