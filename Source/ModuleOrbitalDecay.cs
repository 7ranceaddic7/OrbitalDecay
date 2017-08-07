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

namespace WhitecatIndustries
{
    public class ModuleOrbitalDecay : PartModule
    {

        [KSPField(isPersistant = false , guiActive = true, guiActiveEditor = false, guiName = "Use")]
        public string ODSKengine = "";
        [KSPField(isPersistant = false, guiActive = true, guiName = "Resources")]
        public string StationKeepResources;
        [KSPField(isPersistant = false, guiActive = true, guiName = "Available")]
        public string amounts;
        [KSPField(isPersistant = false, guiActive = true, guiName = "ISP" )]
        public float ISP;
        [KSPField(isPersistant = true, guiActive = false)]
        public int EngineIndex = 0;

        [KSPField(isPersistant = true)]
        public StationKeepData stationKeepData;
        
        public ConfigNode EngineData = new ConfigNode();
        public string[] EngineList = { "" };
        
        private float UPTInterval = 1.0f;
        private float lastUpdate = 0.0f;
       
        [KSPEvent(active = true, guiActive = true, guiName = "Enable Station Keeping")]
        public void ToggleSK()
        {
            VesselData.UpdateStationKeeping(this.vessel, VesselData.FetchStationKeeping(vessel));
            stationKeepData.IsStationKeeping = !stationKeepData.IsStationKeeping;
            updatedisplayedData();
           
        }
        [KSPEvent(active = true, guiActive = true, guiName = "Next engine")]
        public void NextEngine()
        {
            if (EngineList.Count() > 0)
            {
                EngineIndex++;
                if (EngineIndex >= EngineList.Count())
                {
                    EngineIndex = 0;
                }
                updatedisplayedData();
            }
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Previous engine")]
        public void PreviousEngine()
        {
            if (EngineList.Count() > 0)
            {
                EngineIndex--;
                if (EngineIndex <= 0)
                {
                    EngineIndex = EngineList.Count() - 1;
                }
                updatedisplayedData();
            }
        }



        public ModuleOrbitalDecay()
        {

            if (stationKeepData == null)
            {
                stationKeepData = new StationKeepData();

            }
            

        }

        public override void OnStart(StartState state)
        {
            /*
                        BaseField field = this.Fields["ODSKengine"];
                        field.guiActive = stationKeepData.IsStationKeeping;
                        field = this.Fields["StationKeepResources"];
                        field.guiActive = stationKeepData.IsStationKeeping;
                        field = this.Fields["amounts"];
                        field.guiActive = stationKeepData.IsStationKeeping;
                        field = this.Fields["ISP"];
                        field.guiActive = stationKeepData.IsStationKeeping;
                        */
           

            BaseEvent even = this.Events["ToggleSK"];
            if (stationKeepData.IsStationKeeping)
            {
                even.guiName = "Disable Station Keeping";

            }
            else
            {
                even.guiName = "Enable Station Keeping";
            }

          
        }

        
        public void updatedisplayedData()
        {

            foreach (ModuleOrbitalDecay module in vessel.FindPartModulesImplementing<ModuleOrbitalDecay>())
            {
                module.stationKeepData.IsStationKeeping = stationKeepData.IsStationKeeping;
                module.EngineIndex = EngineIndex;
            }

            BaseEvent even = this.Events["ToggleSK"];
            if (stationKeepData.IsStationKeeping)
            {
                even.guiName = "Disable Station Keeping";
            }
            else
            {
                even.guiName = "Enable Station Keeping";
            }
            if (vessel.situation == Vessel.Situations.ORBITING || vessel.situation == Vessel.Situations.SUB_ORBITAL)
            {
                even.guiActive = true;
            }
            else
            {
                even.guiActive = false;
            }


            List<string> proplist = new List<string>();
            List<double> amountlist = new List<double>();
            List<float> ratiolist = new List<float>();

            bool found = false;
            ConfigNode engineNode = new ConfigNode();
            foreach (ConfigNode engine in EngineData.GetNodes("ENGINE"))
            {

                if (engine.GetValue("name") == ODSKengine)
                {
                    engineNode = engine;
                    found = true;
                    break;
                }

            }
            if (found)
            {
                foreach (ConfigNode propellant in engineNode.GetNodes("PROPELLANT"))
                {
                    proplist.Add(propellant.GetValue("name"));
                    amountlist.Add(double.Parse(propellant.GetValue("Available")));
                    ratiolist.Add(float.Parse(propellant.GetValue("ratio")));
                }
                stationKeepData.ISP = float.Parse(engineNode.GetValue("ISP"));

            }
            else
            {
                proplist.Add("No Resoures Available");
                amountlist.Add(0);
                ratiolist.Add(0);
                stationKeepData.ISP = 0;
            }

            stationKeepData.resources = new string[proplist.Capacity];
            stationKeepData.amounts = new double[amountlist.Capacity];
            stationKeepData.ratios = new float[ratiolist.Capacity];
            stationKeepData.resources = proplist.ToArray();
            stationKeepData.amounts = amountlist.ToArray();
            stationKeepData.ratios = ratiolist.ToArray();


            lastUpdate = Time.time-UPTInterval;

            if (EngineIndex < EngineList.Count())
            {
                ODSKengine = EngineList[EngineIndex];
            }
            else ODSKengine = EngineList[EngineList.Count()-1];

        }

        public void fetchEngineData()
        {

            double amount = 0;
            bool engineIsListed = false;
            EngineData.RemoveNodes("ENGINE");
            foreach (ModuleEngines module in vessel.FindPartModulesImplementing<ModuleEngines>())
            {
                engineIsListed = false;
                foreach (ConfigNode engineNode in EngineData.GetNodes())
                {
                    if (engineNode.GetValue("name") == module.part.protoPartSnapshot.partInfo.title)//ugly names used - can't find way to get editor part names 
                    {
                        engineIsListed = true;
                        break;
                    }

                }

                if (module.EngineIgnited && !engineIsListed)
                {
                    ConfigNode engineNode = new ConfigNode("ENGINE");
                    engineNode.AddValue("name", module.part.protoPartSnapshot.partInfo.title);//ugly names used - can't find way to get editor part names
                    engineNode.AddValue("ISP", module.atmosphereCurve.Evaluate(0).ToString());

                    foreach (Propellant propellant in module.propellants)
                    {
                        if (propellant.name != "ElectricCharge")
                        {
                            ConfigNode propellantNode = new ConfigNode("PROPELLANT");
                            //amount = module.part.Resources.Get(propellant.id).amount;
                            amount = fetchPartResource(module.part, propellant.id, ResourceFlowMode.STAGE_PRIORITY_FLOW);
                            propellantNode.AddValue("name", propellant.name);
                            propellantNode.AddValue("id", propellant.id.ToString());
                            propellantNode.AddValue("ratio", propellant.ratio.ToString());
                            propellantNode.AddValue("Available", amount.ToString());
                            engineNode.AddNode(propellantNode);
                        }
                    }
                    EngineData.AddNode(engineNode);
                }
            }

            foreach (ModuleRCS module in vessel.FindPartModulesImplementing<ModuleRCS>())
            {
                engineIsListed = false;
                foreach (ConfigNode engineNode in EngineData.GetNodes())
                {
                    if (engineNode.GetValue("name") == module.part.protoPartSnapshot.partInfo.title)
                    {
                        engineIsListed = true;
                        break;
                    }

                }
                if (module.rcsEnabled && !engineIsListed)
                {
                    ConfigNode engineNode = new ConfigNode("ENGINE");
                    engineNode.AddValue("name", module.part.protoPartSnapshot.partInfo.title);
                    engineNode.AddValue("ISP", module.atmosphereCurve.Evaluate(0).ToString());
                    foreach (Propellant propellant in module.propellants)
                    {
                        if (propellant.name != "ElectricCharge")
                        {
                            ConfigNode propellantNode = new ConfigNode("PROPELLANT");
                            //amount = module.part.Resources.Get(propellant.id).amount
                            //amount =
                            amount = fetchPartResource(module.part, propellant.id, ResourceFlowMode.STAGE_PRIORITY_FLOW);
                            propellantNode.AddValue("name", propellant.name.ToString());
                            propellantNode.AddValue("id", propellant.id.ToString());
                            propellantNode.AddValue("ratio", propellant.ratio.ToString());
                            propellantNode.AddValue("Available", amount.ToString());
                            engineNode.AddNode(propellantNode);
                        }
                    }

                    EngineData.AddNode(engineNode);
                }
            }
        }


        
        public override void OnUpdate()
        {

            
            if ((Time.time - lastUpdate) > UPTInterval)
            {
                lastUpdate = Time.time;
                fetchEngineData();
                


                List<string> namelist = new List<string>();
                if (EngineData.HasNode("ENGINE"))
                    {
                    foreach (ConfigNode engine in EngineData.GetNodes("ENGINE"))
                    {
                        namelist.Add(engine.GetValue("name"));
                    }
                    EngineList = new string[namelist.Count];
                    EngineList = namelist.ToArray();
                }
                else
                {
                    EngineList = new string[] { "None Available" };
                }
                updatedisplayedData();
            


                for (int i = 0; i< stationKeepData.resources.Count() ; i++) 
                {
                    float ratio1 = 10 * stationKeepData.ratios[i];
                    for (int j = 0; j < stationKeepData.resources.Count(); j++) 
                    {
                        float ratio2 = 10 * stationKeepData.ratios[j];
                        if ((stationKeepData.amounts[i] /ratio1) < (stationKeepData.amounts[j]/ ratio2))
                            stationKeepData.amounts[j] = (stationKeepData.amounts[i] / ratio1) * ratio2;
                        /*equalizing fuel amount to comply with consumption ratios
                         * without mutliplying ratios by 10 result is acurate only to 4th position after digital point 
                         * or 7th position in total for huge amounts of fuel
                         * 179.999991330234 instead of 180 - tremendous error, i know ;)
                         * tried casting double type to each variable in equasion
                         * bu multiplying ratios seems to be only working solution
                         * its a math issue/limitation encountered in multiple compilers
                         * *************************************************************/
                    }
                }

                StationKeepResources = "";
                amounts = "";
                ISP = stationKeepData.ISP;
                StationKeepResources += stationKeepData.resources[0];
                amounts += stationKeepData.amounts[0].ToString("F3");
                for (int i = 1; i < stationKeepData.resources.Count(); i++)
                {
                    StationKeepResources += ' ' +stationKeepData.resources[i];
                    amounts += ' '+ stationKeepData.amounts[i].ToString("F3");
                }
            }
        }
     

        private double fetchPartResource(Part part,int Id,ResourceFlowMode flowMode)
        {

            double amount = 0;
            double MaxAmount = 0;
            List<PartResource> Resources = new List<PartResource>();
            
            part.GetConnectedResourceTotals(Id, out amount, out MaxAmount);

            if (Resources.Count > 0)
            {
                foreach (PartResource Res in Resources)
                {
                    amount += Res.amount;
                }
            }
            return amount;
        }
        
       
            
      

    }
    [Serializable]
    public class StationKeepData : IConfigNode
    {
        [SerializeField]
        public bool IsStationKeeping = false;
        [SerializeField]
        public string engine =  "" ;
        [SerializeField]
        public string[] resources = { "" };
        [SerializeField]
        public double[] amounts = { 0 };
        [SerializeField]
        public double fuelLost = 0;
        [SerializeField]
        public float[] ratios = { 0 };
        [SerializeField]
        public float ISP = 0;
        [SerializeField]
        public double Area = 0;

        public void Load(ConfigNode node)
        {
            if (node.HasValue("IsStationKeeping"))
            {
                if (bool.TryParse(node.GetValue("IsStationKeeping"), out IsStationKeeping))
                {

                }
            }
            resources = new string[node.GetValue("resources").Split(' ').Count()];
            if (node.HasValue("resources"))
            {
                resources = node.GetValue("resources").Split(' ');
            }
            double d;
            if (node.HasValue("amounts"))
            {
                int i = 0;
                
                amounts = new double[node.GetValue("amounts").Split(' ').Count()];
                foreach (string str in node.GetValue("amounts").Split(' '))
                {
                    if (double.TryParse(str, out d))
                    {
                        amounts[i++] = d;
                    }
                }

            }
            float f;
            if (node.HasValue("ratios"))
            {
                int i = 0;

                ratios = new float[node.GetValue("ratios").Split(' ').Count()];
                foreach (string str in node.GetValue("ratios").Split(' '))
                {
                    if (float.TryParse(str, out f))
                    {
                        ratios[i++] = f;
                    }
                }

            }
            if (node.HasValue("fuelLost"))
            {
                if (double.TryParse(node.GetValue("fuelLost"), out d))
                {
                    fuelLost = d;
                }
            }
            if (node.HasValue("ISP"))
            {
                if (float.TryParse(node.GetValue("ISP"), out f))
                {
                    ISP = f;
                }
            }
            if (node.HasValue("engine"))
            {
                engine = node.GetValue("engine");
            }
        }

        public void Save(ConfigNode node)
        {
            string temporary;
            temporary = resources[0];
            for (int i = 1; i < resources.Count(); i++)
            {
                temporary += ' ' + resources[i];
            }
            node.AddValue("resources", temporary);
            temporary = amounts[0].ToString();
            for (int i = 1; i < amounts.Count(); i++)
            {
                temporary += ' ' + amounts[i].ToString();
            }
            node.AddValue("amounts", temporary);
            temporary = "";
            for (int i = 0; i < ratios.Count(); i++)
            {
                temporary += ratios[i].ToString() + ' ';
            }
            node.AddValue("ratios", temporary);

            node.AddValue("fuelLost", fuelLost);

            node.AddValue("ISP", ISP);

            node.AddValue("engine", engine);

            node.AddValue("IsStationKeeping", IsStationKeeping);
        }


    }


}



