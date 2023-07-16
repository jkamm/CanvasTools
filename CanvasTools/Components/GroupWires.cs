using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System.Linq;
using CanvasTools.MetaHopper;

namespace CanvasTools.Components
{
    public class GroupWires : MH_SelectButtonComponent
    {
        public GroupWires()
          : base("Manage Group Wires", "GroupWires",
              "Select one or more groups to set the wire state for all wires coming into a group (but not between group members)\nDoes not affect relays",
              "Extra", "CanvasTools")
        {
            Message = "default";
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Group", "G", "Groups to Change", GH_ParamAccess.list);
            Params.Input[0].Optional = true;
            WireStatus = wireState;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Groups Changed", "Gc", "Groups changed by Wire Display Action", GH_ParamAccess.list);
        }



        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_DocumentObject> grpsIn = new List<GH_DocumentObject>();
            DA.GetDataList(0, grpsIn);

            List<Grasshopper.Kernel.Special.GH_Group> inputs = grpsIn.OfType<Grasshopper.Kernel.Special.GH_Group>().ToList();

            InactiveObjects = InactiveObjects.Where(obj => obj is GH_Group).ToList();

            if (InactiveObjects.Count > 0 || inputs.Count > 0)
            {
                //var GrasshopperDocument = this.OnPingDocument();
                List<Grasshopper.Kernel.Special.GH_Group> grps = inputs.Count > 0 ? inputs :
                    InactiveObjects.OfType<Grasshopper.Kernel.Special.GH_Group>().ToList();

                foreach (var grp in grps)
                {
                    var objs = grp.ObjectsRecursive();
                    var prms = new List<IGH_Param>();

                    foreach (var obj in objs)
                    {
                        var comp = obj as GH_Component;
                        if (comp != null)
                        {
                            prms.AddRange(comp.Params);
                            continue;
                        }
                        var prm = obj as IGH_Param;
                        if (prm != null) prms.Add(prm);
                    }

                    var outgoing = prms.Where(p => p.Sources.Any(s => !prms.Contains(s)));
                    switch (WireStatus)
                    {
                        case 2:
                            foreach (var p in outgoing) p.WireDisplay = GH_ParamWireDisplay.hidden;
                            break;
                        case 1:
                            foreach (var p in outgoing) p.WireDisplay = GH_ParamWireDisplay.faint;
                            break;
                        default:
                            foreach (var p in outgoing) p.WireDisplay = GH_ParamWireDisplay.@default;
                            break;
                    }

                }
                DA.SetDataList("Groups Changed", grps);
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


        #region Record and Display WireState
        //implemented based on example: https://developer.rhino3d.com/guides/grasshopper/custom-component-options/
        private int wireState = 1;

        public int WireStatus
        {
            get { return wireState; }
            set
            {
                wireState = value;

                if (wireState == 0) Message = "default";
                else if (wireState == 1) Message = "faint";
                else Message = "hidden";
            }
        }
        protected override void AppendAdditionalComponentMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Default", Menu_DefaultClick, true, wireState == 0);
            Menu_AppendItem(menu, "Faint", Menu_FaintClick, true, wireState == 1);
            Menu_AppendItem(menu, "Hidden", Menu_HiddenClick, true, wireState == 2);
        }
        private void Menu_DefaultClick(object sender, EventArgs e)
        {
            // 0 is Default
            RecordUndoEvent("WireStatus");
            WireStatus = 0;
            ExpireSolution(true);
        }
        private void Menu_FaintClick(object sender, EventArgs e)
        {
            // 1 is Faint
            RecordUndoEvent("WireStatus");
            WireStatus = 1;
            ExpireSolution(true);
        }
        private void Menu_HiddenClick(object sender, EventArgs e)
        {
            // 2 is Hidden
            RecordUndoEvent("WireStatus");
            WireStatus = 2;
            ExpireSolution(true);
        }
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // First add our own field.
            writer.SetInt32("WireStatus", WireStatus);
            // Then call the base class implementation.
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            // First read our own field.
            WireStatus = reader.GetInt32("WireStatus");
            // Then call the base class implementation.
            return base.Read(reader);
        }
        #endregion

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8784C6FB-BC5B-4257-B21F-49C50320E954"); }
        }
    }
}