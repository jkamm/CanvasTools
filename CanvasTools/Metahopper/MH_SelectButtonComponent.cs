using System;
using System.Collections.Generic;
using System.Linq;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;

namespace CanvasTools.MetaHopper
{
    //This code comes directly from Andrew Heumann's Metahopper plugin: https://www.food4rhino.com/en/app/metahopper
    //used with permission
    public abstract class MH_SelectButtonComponent : GH_Component
    {
        internal List<IGH_ActiveObject> ActiveObjects;

        internal List<IGH_DocumentObject> InactiveObjects;

        internal List<Guid> guidList;

        public MH_SelectButtonComponent(string name, string nickname, string description, string category, string subCategory)
            : base(name, nickname, description, category, subCategory)
        {
            ActiveObjects = new List<IGH_ActiveObject>();
            InactiveObjects = new List<IGH_DocumentObject>();
            guidList = new List<Guid>();
        }

        protected abstract override void RegisterInputParams(GH_InputParamManager pManager);

        protected abstract override void RegisterOutputParams(GH_OutputParamManager pManager);

        protected abstract override void SolveInstance(IGH_DataAccess DA);

        protected override void BeforeSolveInstance()
        {
            if (guidList.Count > 0)
            {
                processGuids();
            }
            base.BeforeSolveInstance();
        }

        internal void processGuids()
        {
            try
            {
                GH_Document gH_Document = Instances.ActiveCanvas.Document;
                if (gH_Document == null)
                {
                    gH_Document = OnPingDocument();
                }
                foreach (Guid guid in guidList)
                {
                    try
                    {
                        IGH_DocumentObject iGH_DocumentObject = gH_Document.FindObject(guid, topLevelOnly: false);
                        if (iGH_DocumentObject != null)
                        {
                            if (iGH_DocumentObject is IGH_ActiveObject)
                            {
                                ActiveObjects.Add(iGH_DocumentObject as IGH_ActiveObject);
                            }
                            else
                            {
                                InactiveObjects.Add(iGH_DocumentObject);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }
            guidList.Clear();
        }

        public virtual void UpdateSelection()
        {
            ActiveObjects.Clear();
            InactiveObjects.Clear();
            IEnumerable<IGH_DocumentObject> enumerable = OnPingDocument().SelectedObjects().Where(isValidObject);
            ActiveObjects.AddRange(enumerable.Where((IGH_DocumentObject s) => s is IGH_ActiveObject).Cast<IGH_ActiveObject>());
            InactiveObjects.AddRange(enumerable.Except(ActiveObjects));
        }

        public virtual bool isValidObject(object o)
        {
            return true;
        }

        public override void CreateAttributes()
        {
            m_attributes = new MH_ButtonObject_Attributes(this);
        }

        public override bool Write(GH_IWriter writer)
        {
            List<IGH_DocumentObject> list = InactiveObjects.Union(ActiveObjects.Cast<IGH_DocumentObject>().ToList()).ToList();
            writer.SetInt32("ObjectCount", list.Count);
            GH_IWriter gH_IWriter = writer.CreateChunk("AllObjects");
            int num = 0;
            foreach (IGH_DocumentObject item in list)
            {
                gH_IWriter.SetGuid("Object", num, item.InstanceGuid);
                num++;
            }
            return base.Write(writer);
        }

        internal void SelectSelected()
        {
            GH_Document gH_Document = OnPingDocument();
            foreach (IGH_DocumentObject item in ActiveObjects.Union(InactiveObjects))
            {
                item.Attributes.Selected = true;
            }
        }

        public override bool Read(GH_IReader reader)
        {
            int @int = reader.GetInt32("ObjectCount");
            GH_IReader gH_IReader = reader.FindChunk("AllObjects");
            List<Guid> list = new List<Guid>();
            for (int i = 0; i < @int; i++)
            {
                list.Add(gH_IReader.GetGuid("Object", i));
            }
            guidList.AddRange(list);
            return base.Read(reader);
        }
    }
}