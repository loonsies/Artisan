﻿using Artisan.CraftingLists;
using Artisan.RawInformation;
using Artisan.UI;
using ClickLib.Clicks;
using Dalamud.Logging;
using ECommons;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ECommons.GenericHelpers;

namespace Artisan.Autocraft
{
    internal unsafe class RepairManager
    {
        internal static void Repair()
        {
            if (TryGetAddonByName<AddonRepair>("Repair", out var addon) && addon->AtkUnitBase.IsVisible && addon->RepairAllButton->IsEnabled && Throttler.Throttle(500))
            {
                new ClickRepair((IntPtr)addon).RepairAll();
            }
        }

        internal static void ConfirmYesNo()
        {
            if(TryGetAddonByName<AddonRepair>("Repair", out var r) && 
                r->AtkUnitBase.IsVisible && TryGetAddonByName<AddonSelectYesno>("SelectYesno", out var addon) && 
                addon->AtkUnitBase.IsVisible && 
                addon->YesButton->IsEnabled && 
                addon->AtkUnitBase.UldManager.NodeList[15]->IsVisible && 
                Throttler.Throttle(500))
            {
                new ClickSelectYesNo((IntPtr)addon).Yes();
            }
        }

        internal static int GetMinEquippedPercent()
        {
            ushort ret = ushort.MaxValue;
            var equipment = InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems);
            for(var i  = 0; i < equipment->Size; i++)
            {
                var item = equipment->GetInventorySlot(i);
                if (item != null && item->ItemID > 0)
                {
                    if (item->Condition < ret) ret = item->Condition;
                }
            }
            return (int)Math.Ceiling((double)ret / 300);
        }

        internal static bool ProcessRepair(bool use = true, CraftingList? CraftingList = null)
        {
            int repairPercent = CraftingList != null ? CraftingList.RepairPercent : Service.Configuration.RepairPercent;
            if (GetMinEquippedPercent() >= repairPercent)
            {
                if (DebugTab.Debug) PluginLog.Verbose("Condition good");
                if (TryGetAddonByName<AddonRepair>("Repair", out var r) && r->AtkUnitBase.IsVisible)
                {
                    if (DebugTab.Debug) PluginLog.Verbose("Repair visible");
                    if (Throttler.Throttle(500))
                    {
                        if (DebugTab.Debug) PluginLog.Verbose("Closing repair window");
                        Hotbars.actionManager->UseAction(ActionType.General, 6);
                    }
                    return false;
                }
                if (DebugTab.Debug) PluginLog.Verbose("return true");
                return true;
            }
            else
            {
                if (DebugTab.Debug) PluginLog.Verbose($"Condition bad, condition is {GetMinEquippedPercent()}, config is {Service.Configuration.RepairPercent}");
                if (use)
                {
                    if (DebugTab.Debug) PluginLog.Verbose($"Doing repair");
                    if (TryGetAddonByName<AddonRepair>("Repair", out var r) && r->AtkUnitBase.IsVisible)
                    {
                        //PluginLog.Verbose($"Repair visible");
                        ConfirmYesNo();
                        Repair();
                    }
                    else
                    {
                        if (DebugTab.Debug) PluginLog.Verbose($"Repair not visible");
                        if (Throttler.Throttle(500))
                        {
                            if (DebugTab.Debug) PluginLog.Verbose($"Opening repair");
                            Hotbars.actionManager->UseAction(ActionType.General, 6);
                        }
                    }
                }
                if (DebugTab.Debug) PluginLog.Verbose($"Returning false");
                return false;
            }
        }
    }
}
