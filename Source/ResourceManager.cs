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
        public static bool RemoveResources(Vessel vessel, string resource, double quantity)
        {
            double PartFuel = 0.0;
            bool AnyFuelLeft = false;
            
            if (vessel == FlightGlobals.ActiveVessel)
            {
               
                int MonoPropId = PartResourceLibrary.Instance.GetDefinition(resource).id;
                                
                if (0 < (vessel.rootPart.RequestResource(MonoPropId, quantity)))
                {
                    AnyFuelLeft = true;
                }
            }
            else
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
                                PartFuel = double.Parse(protopartresourcesnapshot.resourceValues.GetValue("amount"));
                                if (PartFuel > quantity)
                                {
                                    PartFuel -= quantity;
                                    protopartresourcesnapshot.resourceValues.SetValue("amount", PartFuel.ToString(), 0);
                                    AnyFuelLeft = true;
                                    break;
                                }
                                else
                                {
                                    protopartresourcesnapshot.resourceValues.SetValue("amount", "0", 0);
                                    quantity -= PartFuel;
                                }
                            }
                        }
                    }
                }
            }
            return AnyFuelLeft;
        }

        public static bool RemoveResources2(Vessel vessel, double quantity)//151 new wersion consuming multiple resources saved on vessel
        {
            
            bool AnyFuelLeft = false;
            float ratio = 0;
            string resource = GetResourceNames(vessel);
            int index = 0;
            if (vessel == FlightGlobals.ActiveVessel)
            {
                
                foreach (string res in resource.Split(' '))
                {
                    ratio = GetResourceRatio(vessel, index++);
                    int MonoPropId = PartResourceLibrary.Instance.GetDefinition(res).id;
                    if (0 < (vessel.rootPart.RequestResource(MonoPropId, (quantity/2*ratio),ResourceFlowMode.STAGE_PRIORITY_FLOW_BALANCE)))
                    {
                        AnyFuelLeft = true;
                    }
                }
            }
            else
            {
                ProtoVessel proto = vessel.protoVessel;

                foreach (ProtoPartSnapshot protopart in proto.protoPartSnapshots)
                {
                    foreach (ProtoPartModuleSnapshot protopartmodulesnapshot in protopart.modules)
                    {
                        if (protopartmodulesnapshot.moduleName == "ModuleOrbitalDecay")
                        {
                            ConfigNode node = protopartmodulesnapshot.moduleValues.GetNode("stationKeepData");
                          //  quantity += double.Parse(node.GetValue("fuelLost"));
                            node.SetValue("fuelLost", (quantity + double.Parse(node.GetValue("fuelLost"))).ToString());
                            AnyFuelLeft = true;
                            break;
                        }
                    }
                }
            }
            return AnyFuelLeft;
        }

        public static void CatchUp(Vessel vessel, string resource)
        {
            int MonoPropId = PartResourceLibrary.Instance.GetDefinition(resource).id;
            if (VesselData.FetchFuel(vessel) > 0) // 1.5.0 Resource Instant drain fix
            {
               vessel.rootPart.RequestResource(MonoPropId, (Math.Abs(GetResources(vessel, resource) - VesselData.FetchFuel(vessel))));
               

            }
        }

        public static string GetResourceNames(Vessel vessel)//151 
        {
            string ResourceNames = "";
            if (vessel == FlightGlobals.ActiveVessel)
            {
                List<ModuleOrbitalDecay> modlist = vessel.FindPartModulesImplementing<ModuleOrbitalDecay>();
                ResourceNames = modlist.ElementAt(0).StationKeepResources;
            }
            else
            {
                ProtoVessel proto = vessel.protoVessel;

                foreach (ProtoPartSnapshot protopart in proto.protoPartSnapshots)
                {
                    foreach (ProtoPartModuleSnapshot protopartmodulesnapshot in protopart.modules)
                    {
                        if (protopartmodulesnapshot.moduleName == "ModuleOrbitalDecay")
                        {
                            ConfigNode node = protopartmodulesnapshot.moduleValues.GetNode("stationKeepData");
                            ResourceNames = node.GetValue("resources");
                            break;

                        }
                    }
                }
            }
            return ResourceNames;
        }
        public static float GetResourceRatio(Vessel vessel,int index)//151 
        {
            float ResourceRatio = 0;
            if (vessel == FlightGlobals.ActiveVessel)
            {
                List<ModuleOrbitalDecay> modlist = vessel.FindPartModulesImplementing<ModuleOrbitalDecay>();
                ResourceRatio = modlist.ElementAt(0).stationKeepData.ratios[index];
            }
            else
            {
                ProtoVessel proto = vessel.protoVessel;

                foreach (ProtoPartSnapshot protopart in proto.protoPartSnapshots)
                {
                    foreach (ProtoPartModuleSnapshot protopartmodulesnapshot in protopart.modules)
                    {
                        if (protopartmodulesnapshot.moduleName == "ModuleOrbitalDecay")
                        {
                            ConfigNode node = protopartmodulesnapshot.moduleValues.GetNode("stationKeepData");
                            int i = 0;
                            foreach (string str in node.GetValue("ratios").Split(' '))
                            {
                                if (i == index)
                                {
                                    ResourceRatio = float.Parse(str);
                                break;
                                }
                                i++;
                            }
                            break;

                        }
                    }
                }
            }
            return ResourceRatio;
        }

        public static double GetResources2(Vessel vessel)//returns sum of used resources
        {
            double fuel = 0;
            if (vessel == FlightGlobals.ActiveVessel)
            {
                List<ModuleOrbitalDecay> modlist = vessel.FindPartModulesImplementing<ModuleOrbitalDecay>();
                foreach (ModuleOrbitalDecay module in modlist)
                {
                    for(int i = 0; i < module.stationKeepData.amounts.Count(); i++)
                    {
                        fuel += module.stationKeepData.amounts[i];
                    }
                    break; 
                }

            }
            else
            {
                ProtoVessel proto = vessel.protoVessel;

                foreach (ProtoPartSnapshot protopart in proto.protoPartSnapshots)
                {
                    foreach (ProtoPartModuleSnapshot protopartmodulesnapshot in protopart.modules)
                    {
                        if (protopartmodulesnapshot.moduleName == "ModuleOrbitalDecay" && fuel == 0)
                        {
                            ConfigNode node = protopartmodulesnapshot.moduleValues.GetNode("stationKeepData");
                            foreach( string str in node.GetValue("amounts").Split(' '))
                            {
                                fuel += double.Parse(str);
                            }
                            fuel -= double.Parse(node.GetValue("fuelLost"));
                            break;

                        }
                    }
                }
            }
            return fuel;
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
           //151
           /*PartResourceDefinition resourceDef = PartResourceLibrary.Instance.GetDefinition(resource);
            if (Settings.ReadRD())
            {
                Efficiency = resourceDef.density * 0.9f; // Balance here!
            }
            else
            {
                Efficiency = resourceDef.density * 10.0f;
            }*/
            foreach (string res in resource.Split(' '))
            {
                PartResourceDefinition resourceDef = PartResourceLibrary.Instance.GetDefinition(res);
                if (Settings.ReadRD())
                {
                    Efficiency += resourceDef.density * 0.9f; // Balance here!
                }
                else
                {
                    Efficiency += resourceDef.density * 10.0f;
                }
            }
            return (Efficiency / resource.Split(' ').Count());
        }
    }
}
