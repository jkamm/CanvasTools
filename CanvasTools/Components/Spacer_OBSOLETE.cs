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
    public class Spacer_OBSOLETE : Base.ButtonComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Spacer_OBSOLETE()
          : base("Spacer", "Spacer",
            "Arranges grasshopper elements into rows and columns according to connections",
            "Extra", "CanvasTools")
        {
            ButtonName = "Select";
            Message = "Up";
        }

        public override bool Obsolete => true;
        public override GH_Exposure Exposure => GH_Exposure.hidden;


        Dictionary<GH_DocumentObject, int> selectedObjects = new Dictionary<GH_DocumentObject, int>();
        bool direction = true;
        bool cyclic = false;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Get Selected", "Get", "Get Canvas Selection", GH_ParamAccess.item);
            pManager.AddNumberParameter("xSpacing", "X", "Spacing between pivots in X direction \nDefault (0) leaves X spacing unchanged", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("ySpacing", "Y", "Spacing between pivots in Y direction \nDefault (0) leaves Y spacing unchanged", GH_ParamAccess.item, 0);
            //pManager.AddBooleanParameter("Flow", "F", "Left to Right = false, Right to Left = true", GH_ParamAccess.item, false);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            //pManager[3].Optional = true;
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
            double xSpacing = 0;
            double ySpacing = 0;
           // bool flowUp = false;
            bool getSelected = false;

            DA.GetData(0, ref getSelected);
            DA.GetData(1, ref xSpacing);
            DA.GetData(2, ref ySpacing);
            //DA.GetData(3, ref flowUp);

            //xSpacing = Math.Abs(xSpacing) < 1 ? 1 : xSpacing;
            //ySpacing = Math.Abs(ySpacing) < 1 ? 1 : ySpacing;

            if (getSelected || Execute)
            {
                if (Direction) selectedObjects = TierClimber.ComputeSelectionTiersUp(Getters.JustGetSelectedObjects());
                else selectedObjects = TierClimber.ComputeSelectionTiers(Getters.JustGetSelectedObjects());
            }
            else
            {
                //recompute tiers if Justify direction changes
                if (direction != Direction)
                {
                    direction = Direction;
                    if (Direction) selectedObjects = TierClimber.ComputeSelectionTiersUp(selectedObjects);
                    else selectedObjects = TierClimber.ComputeSelectionTiers(selectedObjects);
                }

                if (selectedObjects.Keys.Count() > 0)
                {
                    if (cyclic == true) AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cyclical DataStream Detected, please change selection");

                    var pivotDictionary = Getters.GetPivotDictionary(selectedObjects, xSpacing, ySpacing, Direction);
                    //PrintDictionary(pivotDictionary);
                    Getters.MoveByPivotDictionary(pivotDictionary);
                    //Getters.PrintDictionary(selectedObjects);
                }
                else AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Set selection by selecting items on canvas and pressing 'Select'");
            }

        }

        private bool rightToLeft = false;
        
        public bool Direction
        {
            get { return rightToLeft; }

            set
            {
                rightToLeft = value;
                if (rightToLeft)
                {
                    Message = "Down";
                }
                else
                {
                    Message = "Up";
                }
            }
        }
        

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            //Menu_AppendSeparator(menu);
            ToolStripMenuItem item = Menu_AppendItem(menu, "Toggle Direction", Menu_ToggleDirection, true);
            item.ToolTipText = "Change 'Justification' of arrangement from Upstream (L to R) to Downstream (R to L)";
        }

        private void Menu_ToggleDirection(object sender, EventArgs e)
        {
            RecordUndoEvent("Direction");
            //if Direction is true, turn to false and if false, turn to true.
            Direction = !Direction;
            ExpireSolution(true);            
        }
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("Direction", Direction);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            Direction = reader.GetBoolean("Direction");
            return base.Read(reader);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Spacer;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ad3763d6-4140-4138-afb6-6a2620ecc251");
    }
}