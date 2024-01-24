using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;

namespace dataflow_cs
{
    public class GsLcCommands
    {

        [CommandMethod("GsLcSynFromToLocationData")]
        public void GsLcSynFromToLocationData()
        {
            GsLcDataFlow.GenerateManager.GsLcSynFromToLocationData();
        }

        //[CommandMethod("CsTest")]
        //public void CsTest()
        //{
        //    GsLcDataFlow.GenerateManager.CsTest();
        //}

        //[CommandMethod("CsFuncitonTest")]
        //public void GsPgBatchSynPipeData()
        //{
        //    GsLcDataFlow.GenerateManager.GsLcSynFromToLocationData();
        //}

    }
}
