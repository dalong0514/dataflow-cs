using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DLCommonUtils.CADUtils;
using Autodesk.AutoCAD.ApplicationServices;

namespace DLCommonUtils.CADUtils
{

    public static class UtilsCommnon
    {
        public static void UtilsChangeColor(ObjectId objectId, int colorIndex)
        {
            Polyline polyline = objectId.GetObject(OpenMode.ForWrite) as Polyline;
            polyline.ColorIndex = colorIndex;
        }

        public static double UtilsStringToDouble(string stringContent)
        {
            double result;
            bool success = double.TryParse(stringContent, out result);

            if (success)
            {
                return result;
            }
            else
            {
                return double.NaN;
            }
        }

        public static bool UtilsIsTwoNumEqual(double num1, double num2, double tolerance)
        {
            return Math.Abs(num1 - num2) < tolerance;
        }
    }
}
