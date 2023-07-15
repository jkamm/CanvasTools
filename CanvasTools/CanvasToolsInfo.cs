using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace CanvasTools
{
    public class CanvasToolsInfo : GH_AssemblyInfo
    {
        public override string Name => "CanvasTools";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Tools for Organizing and Documentation on the Grasshopper Canvas";

        public override Guid Id => new Guid("edf3fe77-057c-4d5e-9315-2337345fad6d");

        //Return a string identifying you or your company.
        public override string AuthorName => "Jo Kamm";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "jokamm@gmail.com";
    }
}