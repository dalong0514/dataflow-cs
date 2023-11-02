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

        [CommandMethod("CsTest")]
        public void DLGetAllPolyline()
        {
            GsLcDataFlow.GenerateManager.DLGetAllPolyline();
        }
    }
}
