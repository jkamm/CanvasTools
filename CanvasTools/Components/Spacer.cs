﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using GH_IO.Serialization;
using CanvasTools.Util;
using CanvasTools.MetaHopper;

namespace CanvasTools.Components
{
    public class Spacer : MH_SelectButtonComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Spacer()
          : base("Spacer", "Spacer",
            "Arranges grasshopper elements into rows and columns according to connections",
            "Extra", "CanvasTools")
        {
            Message = "Up";
        }


        Dictionary<GH_DocumentObject, int> selectedObjects = new Dictionary<GH_DocumentObject, int>();
        
        bool direction = true;
        bool cyclic = false;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("xSpacing", "X", "Spacing between pivots in X direction \nDefault (0) leaves X spacing unchanged", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("ySpacing", "Y", "Spacing between pivots in Y direction \nDefault (0) leaves Y spacing unchanged", GH_ParamAccess.item, 0);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
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
                       
            DA.GetData(0, ref xSpacing);
            DA.GetData(1, ref ySpacing);
                       
            if (Direction) selectedObjects = TierClimber.ComputeSelectionTiersUp(Getters.JustGetSelectedObjects(ActiveObjects, InactiveObjects));
            else selectedObjects = TierClimber.ComputeSelectionTiers(Getters.JustGetSelectedObjects(ActiveObjects, InactiveObjects));
       
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
                    
                    Getters.MoveByPivotDictionary(pivotDictionary);
                    
                }
                else AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Set selection by selecting items on canvas and pressing 'Select'");
          

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

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("BBFB90C9-32A2-4683-96BF-A9C12E73003E"); }
        }
    }
}