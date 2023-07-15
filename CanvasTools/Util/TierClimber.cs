using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CanvasTools.Util;

namespace CanvasTools.Util
{
    internal class TierClimber
    {
        static public Dictionary<GH_DocumentObject, int> ComputeSelectionTiers(Dictionary<GH_DocumentObject, int> dictionary)
        {
            foreach (GH_DocumentObject key in dictionary.Keys.ToList())
            {
                dictionary[key] = ComputeTierConnected(key, 0, dictionary);
            }
            return dictionary;
        }

        static public Dictionary<GH_DocumentObject, int> ComputeSelectionTiersUp(Dictionary<GH_DocumentObject, int> dictionary)
        {
            foreach (GH_DocumentObject key in dictionary.Keys.ToList())
            {
                dictionary[key] = ComputeTierConnectedUp(key, 0, dictionary);
            }
            return dictionary;
        }

        static public int ComputeTierConnected(GH_DocumentObject key, int tier, Dictionary<GH_DocumentObject, int> thisDict)
        {
            Getters.CheckForCycle(tier, thisDict);

            IGH_DocumentObject child = Getters.GetParentObject(key);

            if (!thisDict.ContainsKey((GH_DocumentObject)child))
            {
                //Print("{0} is not in the dictionary", key.ToString());
                return Math.Max(tier - 1, 0);
            }

            if (child is IGH_Param)
            {
                // Cast the child object to IGH_Param
                IGH_Param childParam = (IGH_Param)child;

                // Check if the parameter has sources
                if (childParam.Sources.Count > 0)
                {
                    // Recursively compute the tier for each source parameter
                    tier = GetMaxTier(childParam, tier, 0, thisDict);
                }
            }
            else if (child is IGH_Component)
            {
                // Cast the child object to IGH_Component
                IGH_Component childComponent = (IGH_Component)child;

                // Check if the component has input parameters
                if (childComponent.Params.Input.Count > 0)
                {
                    // Recursively compute the tier for each input parameter
                    int maxChildTier = 0;
                    foreach (var inputParam in childComponent.Params.Input)
                    {
                        //if (inputParam.Sources.Count > 0)
                        maxChildTier = GetMaxTier(inputParam, tier, maxChildTier, thisDict);
                    }
                    //tier = Math.Max(maxChildTier,tier);
                    tier = Math.Max(maxChildTier, tier);
                }

            }

            return tier;
        }

        static public int ComputeTierConnectedUp(GH_DocumentObject key, int tier, Dictionary<GH_DocumentObject, int> thisDict)
        {
            Getters.CheckForCycle(tier, thisDict);

            IGH_DocumentObject child = Getters.GetParentObject(key);

            if (!thisDict.ContainsKey((GH_DocumentObject)child))
            {
                //Print("{0} is not in the dictionary", key.ToString());
                return Math.Max(tier - 1, 0);
            }

            if (child is IGH_Param)
            {
                // Cast the child object to IGH_Param
                IGH_Param childParam = (IGH_Param)child;

                // Check if the parameter has sources
                if (childParam.Recipients.Count > 0)
                {
                    // Recursively compute the tier for each source parameter
                    tier = GetMaxTierUp(childParam, tier, 0, thisDict);
                }
            }
            else if (child is IGH_Component)
            {
                // Cast the child object to IGH_Component
                IGH_Component childComponent = (IGH_Component)child;

                // Check if the component has input parameters
                if (childComponent.Params.Output.Count > 0)
                {
                    // Recursively compute the tier for each input parameter
                    int maxParentTier = 0;
                    foreach (var outputParam in childComponent.Params.Output)
                    {
                        maxParentTier = GetMaxTierUp(outputParam, tier, maxParentTier, thisDict);
                    }
                    tier = Math.Max(maxParentTier, tier);
                }
            }

            return tier;
        }

        static public int GetMaxTier(IGH_Param childParam, int tier, int maxChildTier, Dictionary<GH_DocumentObject, int> dictionary)
        {
            Getters.CheckForCycle(tier, dictionary);

            foreach (var source in childParam.Sources)
            {
                // Cast the source object to GH_DocumentObject
                GH_DocumentObject sourceObject = (GH_DocumentObject)source;

                int sourceTier = ComputeTierConnected(sourceObject, tier + 1, dictionary);
                maxChildTier = Math.Max(maxChildTier, sourceTier);
            }
            return maxChildTier;
        }

        static public int GetMaxTierUp(IGH_Param childParam, int tier, int maxParentTier, Dictionary<GH_DocumentObject, int> dictionary)
        {
            Getters.CheckForCycle(tier, dictionary);

            foreach (var recipient in childParam.Recipients)
            {
                // Cast the source object to GH_DocumentObject
                GH_DocumentObject recipientObject = (GH_DocumentObject)recipient;

                int recipientTier = ComputeTierConnectedUp(recipientObject, tier + 1, dictionary);
                maxParentTier = Math.Max(maxParentTier, recipientTier);
            }
            return maxParentTier;
        }
    }
}
