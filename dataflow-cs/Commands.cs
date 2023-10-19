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

        [CommandMethod("CsTestGetMTextContent")]
        public void CsTestGetMTextContent()
        {
            StudyProgram.StudyProgramMain.CsTestGetMTextContent();
        }

        // 任务1: 生成命令{CsTestGetAllBlockAttributes}
        // 任务2: 调用StudyProgramMain下的CsTestGetAllBlockAttributes方法
        [CommandMethod("CsTestGetAllBlockAttributes")]
        public void CsTestGetAllBlockAttributes()
        {
            StudyProgram.StudyProgramMain.CsTestGetAllBlockAttributes();
        }
    }
}
