using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Grasshopper.Kernel.Undo.Actions;
using Grasshopper.Kernel.Undo;

namespace CanvasTools.Util
{
    internal class Getters
    {
        static public Dictionary<GH_DocumentObject, int> JustGetSelectedObjects()
        {
            Dictionary<GH_DocumentObject, int> dictionary = new Dictionary<GH_DocumentObject, int>();

            // Get the active document in Grasshopper
            GH_Document doc = Grasshopper.Instances.ActiveCanvas.Document;

            // Get the selected objects in the document
            List<IGH_DocumentObject> selObjList = doc.SelectedObjects();

            //initialize dictionary with no tier values
            dictionary = selObjList.OfType<GH_DocumentObject>()
              .Where(ghObj => !(ghObj is Grasshopper.Kernel.Special.GH_Group))
              .ToDictionary(ghObj => ghObj, _ => 0);

            return dictionary;
        }

        static public IGH_DocumentObject GetParentObject(IGH_DocumentObject childObject)
        {
            if (childObject is IGH_Param)
            {
                // Check if the parameter has a parent component
                IGH_Param param = (IGH_Param)childObject;
                if (param.Kind != GH_ParamKind.floating)
                {
                    IGH_DocumentObject parentComponent = param.Attributes.Parent.DocObject;
                    return parentComponent;
                }
            }
            return childObject;
        }

        static public void CheckForCycle(int tier, Dictionary<GH_DocumentObject, int> thisDict)
        {
            if (tier > thisDict.Keys.Count())
            {
                //cyclic = true;
                //Component.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cyclical DataStream Detected");
                throw new Exception("Cyclical datastream detected");
            }
            //else cyclic = false;
        }

        static public void PrintDictionary(Dictionary<GH_DocumentObject, int> dictionary)
        {
            foreach (KeyValuePair<GH_DocumentObject, int> kvp in dictionary)
            {
                GH_DocumentObject key = kvp.Key;
                int tier = kvp.Value;
                string output = string.Format("Key: {0}, Tier: {1}, Current Pivot: {2}", key.ToString(), tier.ToString(), key.Attributes.Pivot.ToString());
                //Print(output);
            }
        }
        static public void PrintDictionary(Dictionary<GH_DocumentObject,PointF> dictionary)
        {
            foreach (KeyValuePair<GH_DocumentObject, PointF> kvp in dictionary)
            {
                GH_DocumentObject key = kvp.Key;
                PointF pivot = kvp.Value;
                string output = string.Format("Key: {0}, NewPivot: {1}, CurrentPivot: {2}", key.ToString(), pivot.ToString(), key.Attributes.Pivot.ToString());
                //Print(output);
            }
        }
        static public Dictionary<GH_DocumentObject, PointF> GetPivotDictionary(Dictionary<GH_DocumentObject, int> dictionary, double xSpacing, double ySpacing, bool reversed)
        {
            Dictionary<GH_DocumentObject, PointF> pivotDictionary = new Dictionary<GH_DocumentObject, PointF>();

            bool positiveY = ySpacing > 0;
            bool positiveX = xSpacing > 0;

            positiveX = reversed ? !positiveX : positiveX;

            float pivotYRef = positiveY ?
              dictionary.Keys.Min(key => key.Attributes.Pivot.Y) :
              dictionary.Keys.Max(key => key.Attributes.Pivot.Y);

            float pivotXRef = positiveX ?
              dictionary.Keys.Min(key => key.Attributes.Pivot.X) :
              dictionary.Keys.Max(key => key.Attributes.Pivot.X);

            positiveX = reversed ? !positiveX : positiveX;

            int maxTier = dictionary.Values.Max();

            //Get unique values in dictionary
            List<int> columns = dictionary.Values.Distinct().ToList();

            //Iterate over unique values in the Dictionary
            foreach (int val in columns)
            {
                List<GH_DocumentObject> columnObjects = dictionary.Where(kvp => kvp.Value.Equals(val))
                  .Select(kvp => kvp.Key)
                  .ToList();
                columnObjects.Sort((obj1, obj2) => obj1.Attributes.Pivot.Y.CompareTo(obj2.Attributes.Pivot.Y));
                if (!positiveY)
                    columnObjects.Reverse();

                int index = 0;
                foreach (GH_DocumentObject obj in columnObjects)
                {
                    float positionY = pivotYRef + (float)index * (float)ySpacing;

                    int tier = dictionary[obj];

                    float positionX = positiveX ?
                      (float)(tier) * (float)xSpacing + pivotXRef :
                      (float)(maxTier - tier) * (float)xSpacing + pivotXRef;

                    if (reversed)
                    {
                        positionX = positiveX ?
                          pivotXRef - (float)(tier) * (float)xSpacing :
                          pivotXRef - (float)(maxTier - tier) * (float)xSpacing;
                    }

                    pivotDictionary.Add(obj, new PointF(positionX, positionY));
                    index++;
                }
            }
            return pivotDictionary;
        }

        static public void MoveByPivotDictionary(Dictionary<GH_DocumentObject, PointF> dictionary)
        {
            var first = dictionary.First();
            GH_Document doc = first.Key.OnPingDocument();

            GH_UndoRecord undoRecord = new GH_UndoRecord("pivotChange");
            foreach (KeyValuePair<GH_DocumentObject, PointF> kvp in dictionary)
            {
                GH_DocumentObject key = kvp.Key;
                PointF pivot = kvp.Value;
                undoRecord.AddAction(new GH_PivotAction(key));
                key.Attributes.Pivot = pivot;
                key.Attributes.ExpireLayout();
            }
            doc.UndoServer.PushUndoRecord(undoRecord);
            doc.ExpireSolution();
        }
    }
}
