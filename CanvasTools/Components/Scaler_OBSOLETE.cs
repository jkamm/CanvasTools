using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Grasshopper;
using Grasshopper.Kernel;
using GH_IO.Serialization;
using CanvasTools.Util;

namespace CanvasTools.Components
{
    public class Scaler_OBSOLETE : Base.ButtonComponent
    {
        /// <summary>
        /// Initializes a new instance of the Scaler class.
        /// </summary>
        public Scaler_OBSOLETE()
          : base("Canvas Scaler", "Scaler",
              "Scales position of objects on the canvas",
              "Extra", "CanvasTools")
        {
            ButtonName = "Select";
        }

        public override bool Obsolete => true;
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        Dictionary<GH_DocumentObject, int> selectedObjects = new Dictionary<GH_DocumentObject, int>();

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Get Selected", "Get", "Get Canvas Selection", GH_ParamAccess.item);
            pManager.AddNumberParameter("xScale", "X", "Scale in X", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("yScale", "Y", "Scale in Y", GH_ParamAccess.item, 1.0);
            
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double xScale = 1.0;
            double yScale = 1.0;
            bool getSelected = false;

            DA.GetData(0, ref getSelected);
            DA.GetData(1, ref xScale);
            DA.GetData(2, ref yScale);

            if (getSelected || Execute)
            {
                selectedObjects = Getters.JustGetSelectedObjects();
            }
            else
            {
                if (selectedObjects.Keys.Count() > 0)
                {
                    var pivotDictionary = Getters.GetScalingPivots(selectedObjects, xScale, yScale);
                    
                    Getters.MoveByPivotDictionary(pivotDictionary);
                }
                else AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Set selection by selecting items on canvas and pressing 'Select'");
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6BC3F851-E967-4853-B9C7-84FE49CD1442"); }
        }
    }
}