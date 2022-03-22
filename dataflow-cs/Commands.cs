using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;

namespace dataflow_cs
{
    public class Commands
    {
        [CommandMethod("Hello")]
        public void Hello()
        {
            StudyProgram.StudyProgramMain.Hello();
        }

        [CommandMethod("ENTCOLOR")]
        public void ChangeEntityColor()
        {
            StudyProgram.StudyProgramMain.ChangeEntityColor();
        }

        [CommandMethod("CreateOneLine")]
        public void CreateOneLine()
        {
            StudyProgram.StudyProgramMain.CreateOneLine();
        }
    }
}
