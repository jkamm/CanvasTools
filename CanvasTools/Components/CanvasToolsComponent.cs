using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Grasshopper.Kernel.Undo;
using Grasshopper.Kernel.Undo.Actions;
using CanvasTools.Util;
using System.ComponentModel;

namespace CanvasTools.Components
{
    public class CanvasToolsComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public CanvasToolsComponent()
          : base("Spacer", "Spacer",
            "Arranges grasshopper elements into rows and columns according to connections",
            "Extra", "Grasshopper")
        {
        }

        Dictionary<GH_DocumentObject, int> selectedObjects = new Dictionary<GH_DocumentObject, int>();
        bool direction = true;
        bool cyclic = false;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Get Selected", "Get", "Get Canvas Selection", GH_ParamAccess.item);
            pManager.AddNumberParameter("xSpacing", "X", "Spacing between pivots in X direction", GH_ParamAccess.item, 200);
            pManager.AddNumberParameter("ySpacing", "Y", "Spacing between pivots in Y direction", GH_ParamAccess.item, 100);
            pManager.AddBooleanParameter("Flow", "F", "Left to Right = true, Right to Left = false", GH_ParamAccess.item, false);

            pManager[0].Optional = true;
            pManager[3].Optional = true;
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
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double xSpacing = 200;
            double ySpacing = 100;
            bool flowUp = false; 
            bool getSelected = false;

            DA.GetData(0, ref getSelected);
            DA.GetData(1, ref xSpacing);
            DA.GetData(2, ref ySpacing);
            DA.GetData(3, ref flowUp);

            xSpacing = Math.Abs(xSpacing) < 1 ? 1 : xSpacing;
            ySpacing = Math.Abs(ySpacing) < 1 ? 1 : ySpacing;

            if (getSelected)
            {
                if (flowUp) selectedObjects = TierClimber.ComputeSelectionTiersUp(Getters.JustGetSelectedObjects());
                else selectedObjects = TierClimber.ComputeSelectionTiers(Getters.JustGetSelectedObjects());
            }
            else
            {
                //recompute tiers if Justify direction changes
                if (direction != flowUp)
                {
                    direction = flowUp;
                    if (flowUp) selectedObjects = TierClimber.ComputeSelectionTiersUp(selectedObjects);
                    else selectedObjects = TierClimber.ComputeSelectionTiers(selectedObjects);
                }

                if (selectedObjects.Keys.Count() > 0)
                {
                    //if (cyclic == true) GH_ActiveObject.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cyclical DataStream Detected, please change selection");

                    var pivotDictionary = Getters.GetPivotDictionary(selectedObjects, xSpacing, ySpacing, flowUp);
                    //PrintDictionary(pivotDictionary);
                    Getters.MoveByPivotDictionary(pivotDictionary);
                    //Getters.PrintDictionary(selectedObjects);
                }
            }

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ad3763d6-4140-4138-afb6-6a2620ecc251");
    }
}