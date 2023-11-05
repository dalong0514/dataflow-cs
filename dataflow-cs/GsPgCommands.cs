using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;

namespace dataflow_cs
{
    public class GsPgCommands
    {

        [CommandMethod("CsTest")]
        public void CsTest()
        {
            GsPgDataFlow.GenerateManager.CsTest();
        }

        [CommandMethod("CsFuncitonTest")]
        public void GsPgBatchSynPipeData()
        {
            GsPgDataFlow.GenerateManager.GsPgBatchSynPipeData();
        }
    }
}
