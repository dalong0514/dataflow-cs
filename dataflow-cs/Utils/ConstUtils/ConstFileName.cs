using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using dataflow_cs.Utils.CADUtils;
namespace dataflow_cs.Utils.ConstUtils
{
    internal class ConstFileName
    {
        /// <summary>
        /// 程序集所在目录
        /// </summary>
        public static string AssemblyDirectory = UtilsCommon.UtilsGetAssemblyDirectory();
        /// <summary>
        /// 程序集所在目录的上级目录
        /// </summary>
        public static string AssemblyParentDirectory = UtilsCommon.UtilsGetAssemblyParentDirectory();
        /// <summary>
        /// 程序集所在目录的上级目录的上级目录
        /// </summary>
        public static string AssemblyParentParentDirectory = UtilsCommon.UtilsGetAssemblyParentParentDirectory();

        /// <summary>
        /// 工艺数据流组件块文件路径
        /// </summary>
        public static string GsLcBlocksPath = Path.Combine(AssemblyParentDirectory, "allBlocks", "GsLcBlocks.dwg");
        // public const string GsLcBlocksPath = @"D:\fsd-cad\allBlocks\src\GsLcBlocks.dwg";


        /// <summary>
        /// 工艺管道数据导入文件路径
        /// </summary>
        public static string GsLcPipeDataImportPath = Path.Combine(AssemblyParentParentDirectory, "tempdata", "GsLcPipeImport.json");
        // public const string GsLcPipeDataImportPath = @"D:\fsd-cad\tempdata\GsLcPipeImport.json";

        /// <summary>
        /// 工艺设备数据导入文件路径
        /// </summary>
        public static string GsLcEquipmentDataImportPath = Path.Combine(AssemblyParentParentDirectory, "tempdata", "GsLcEquipmentImport.json");


        /// <summary>
        /// 工艺仪表数据导入文件路径
        /// </summary>
        public static string GsLcInstrumentDataImportPath = Path.Combine(AssemblyParentParentDirectory, "tempdata", "GsLcInstrumentImport.json");


    }
}
