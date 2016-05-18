﻿/*
 * Whitecat Industries Orbital Decay for Kerbal Space Program. 
 * 
 * Written by Whitecat106 (Marcus Hehir).
 * 
 * Kerbal Space Program is Copyright (C) 2013 Squad. See http://kerbalspaceprogram.com/. This
 * project is in no way associated with nor endorsed by Squad.
 * 
 * This code is licensed under the Attribution-NonCommercial-ShareAlike 3.0 (CC BY-NC-SA 3.0)
 * creative commons license. See <http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode>
 * for full details.
 * 
 * Attribution — You are free to modify this code, so long as you mention that the resulting
 * work is based upon or adapted from this code.
 * 
 * Non-commercial - You may not use this work for commercial purposes.
 * 
 * Share Alike — If you alter, transform, or build upon this work, you may distribute the
 * resulting work only under the same or similar license to the CC BY-NC-SA 3.0 license.
 * 
 * Note that Whitecat Industries is a ficticious entity created for entertainment
 * purposes. It is in no way meant to represent a real entity. Any similarity to a real entity
 * is purely coincidental.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace WhitecatIndustries
{
    public class ResourceManager : MonoBehaviour
    {
        public static void RemoveResources(Vessel vessel, string resource, double quantity)
        {
            if (vessel = FlightGlobals.ActiveVessel)
            {
                int MonoPropId = PartResourceLibrary.Instance.GetDefinition(resource).id;
                vessel.rootPart.RequestResource(MonoPropId, quantity);
            }
        }

        public static void CatchUp(Vessel vessel, string resource)
        {
            int MonoPropId = PartResourceLibrary.Instance.GetDefinition(resource).id;
            if (VesselData.FetchFuel(vessel) > 0) // 1.5.0 Resource Instant drain fix
            {
                vessel.rootPart.RequestResource(MonoPropId, (Math.Abs(GetResources(vessel, resource) - VesselData.FetchFuel(vessel))));
            }
        }

        public static double GetResources(Vessel vessel, string resource)
        {
            double quantity = 0.0;

            if (vessel != FlightGlobals.ActiveVessel)
            {
                ProtoVessel proto = vessel.protoVessel;

                foreach (ProtoPartSnapshot protopart in proto.protoPartSnapshots)
                {
                    foreach (ProtoPartResourceSnapshot protopartresourcesnapshot in protopart.resources)
                    {
                        if (protopartresourcesnapshot.resourceName == resource)
                        {
                            if (bool.Parse(protopartresourcesnapshot.resourceValues.GetValue("flowState")) == true) // Fixed resource management 1.4.0
                            {
                                quantity = quantity + double.Parse(protopartresourcesnapshot.resourceValues.GetValue("amount"));
                            }
                        }
                    }
                }
            }

            else
            {
                int MonoPropId = PartResourceLibrary.Instance.GetDefinition(resource).id;
                List<PartResource> Resources = new List<PartResource>();
                vessel.rootPart.GetConnectedResources(MonoPropId, ResourceFlowMode.STAGE_PRIORITY_FLOW, Resources);

                    if (Resources.Count > 0)
                    {
                        foreach (PartResource Res in Resources)
                        {
                            quantity = quantity + Res.amount;
                        }
                    }
            }

            return quantity;
        }

        public static float GetEfficiency(string resource) // Eventually combine with engine ISP but quite nice like this!
        {
            float Efficiency = 0.0f;
            PartResourceDefinition resourceDef = PartResourceLibrary.Instance.GetDefinition(resource);
            if (Settings.ReadRD())
            {
                Efficiency = resourceDef.density * 0.9f; // Balance here!
            }
            else
            {
                Efficiency = resourceDef.density * 10.0f;
            }
            return Efficiency;
        }
    }
}
