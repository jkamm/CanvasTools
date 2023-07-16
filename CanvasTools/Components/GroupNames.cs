using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

using CanvasTools.MetaHopper;

namespace CanvasTools.Components
{
    public class GroupNames : MH_SelectButtonComponent
    {
        /// <summary>
        /// Initializes a new instance of the GroupNames class.
        /// </summary>
        public GroupNames()
          : base("Modify Group Icons", "GroupIcons",
              "Changes the Icon/Name Display mode on incoming and outgoing parameters on selected Groups",
              "Extra", "CanvasTools")
        {
            Message = "name";

        }
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Group", "G", "Groups to Change", GH_ParamAccess.list);
            Params.Input[0].Optional = true;
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

            List<GH_Group> inputs = grpsIn.OfType<GH_Group>().ToList();

            InactiveObjects = InactiveObjects.Where(obj => obj is GH_Group).ToList();

            if (InactiveObjects.Count > 0 || inputs.Count > 0)
            {
                List<GH_Group> grps = inputs.Count > 0 ? inputs :
                    InactiveObjects.OfType<GH_Group>().ToList();

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
                    var incoming = prms.Where(p => p.Recipients.Any(s => !prms.Contains(s)));
                    var outgoing = prms.Where(p => p.Sources.Any(s => !prms.Contains(s)));

                    GH_IconDisplayMode mode = new GH_IconDisplayMode();
                    switch (IconMode)
                    {
                        case 2:
                            mode = GH_IconDisplayMode.name;
                            break;
                        case 1:
                            mode = GH_IconDisplayMode.icon;
                            break;
                        default:
                            mode = GH_IconDisplayMode.application;
                            break;
                    }
                    foreach (IGH_Param p in incoming.Union(outgoing))
                    {
                        p.IconDisplayMode = mode;
                        p.Attributes.ExpireLayout();
                    }
                }
                DA.SetDataList("Groups Changed", grps);
            }
        }


        #region Record and Display WireState
        //implemented based on example: https://developer.rhino3d.com/guides/grasshopper/custom-component-options/
        private int iconState = 2;

        public int IconMode
        {
            get { return iconState; }
            set
            {
                iconState = value;

                if (iconState == 0) Message = "default";
                else if (iconState == 1) Message = "icon";
                else Message = "name";
            }
        }
        protected override void AppendAdditionalComponentMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Application", Menu_DefaultClick, true, iconState == 0);
            Menu_AppendItem(menu, "Icon", Menu_IconClick, true, iconState == 1);
            Menu_AppendItem(menu, "Name", Menu_NameClick, true, iconState == 2);
        }
        private void Menu_DefaultClick(object sender, EventArgs e)
        {
            // 0 is Default
            RecordUndoEvent("IconMode");
            IconMode = 0;
            ExpireSolution(true);
        }
        private void Menu_IconClick(object sender, EventArgs e)
        {
            // 1 is Faint
            RecordUndoEvent("IconMode");
            IconMode = 1;
            ExpireSolution(true);
        }
        private void Menu_NameClick(object sender, EventArgs e)
        {
            // 2 is Hidden
            RecordUndoEvent("IconMode");
            IconMode = 2;
            ExpireSolution(true);
        }
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // First add our own field.
            writer.SetInt32("IconMode", IconMode);
            // Then call the base class implementation.
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            // First read our own field.
            IconMode = reader.GetInt32("IconMode");
            // Then call the base class implementation.
            return base.Read(reader);
        }
        #endregion

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
            get { return new Guid("18D4AE8C-E9B3-4DD3-9323-401667307CD5"); }
        }
    }
}