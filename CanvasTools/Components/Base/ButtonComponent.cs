﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CanvasTools.Components.Base
{
    public class ButtonComponent : GH_Component
    {
                    //Should this be an abstract class so that it can't be called directly?  That would avoid the exposure issue.

            //This class should be a base implementation with an added parameter that allows it to be triggered from a button
            //To trigger, it needs an execute property or method that runs the solution even when "run" is set to false.


            /// <summary>
            /// Initializes a new instance of the LO_ButtonComponent class.
            /// </summary>
            public ButtonComponent()
              : base("ButtonComponent", "Nickname",
                  "Description",
                  "Category", "Subcategory")
        {
        }
        public ButtonComponent(string name, string nickname, string description, string category, string subcategory)
          : base(name, nickname,
              description,
              category, subcategory)
        {
            Execute = false;
            ButtonName = "Execute";
        }

        public bool Execute { get; set; }
        public string ButtonName { get; set; }
        //public override GH_Exposure Exposure => GH_Exposure.hidden;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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


        public override void CreateAttributes()
        {
            base.m_attributes = new ButtonComponentAttributes(this);
        }


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2D2ABE72-76FE-4947-AE12-0F8EE782F42D"); }
        }
    }
}