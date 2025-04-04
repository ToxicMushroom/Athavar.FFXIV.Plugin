// <copyright file="CommandInterface.Ui.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Common.Exceptions;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

internal sealed partial class CommandInterface
{
    /// <inheritdoc/>
    public unsafe bool IsAddonVisible(string addonName)
    {
        var ptr = this.dalamudServices.GameGui.GetAddonByName(addonName);
        if (ptr == nint.Zero)
        {
            return false;
        }

        var addon = (AtkUnitBase*)ptr;
        return addon->IsVisible;
    }

    /// <inheritdoc/>
    public unsafe bool IsAddonReady(string addonName)
    {
        var ptr = this.dalamudServices.GameGui.GetAddonByName(addonName);
        if (ptr == nint.Zero)
        {
            return false;
        }

        var addon = (AtkUnitBase*)ptr;
        return addon->UldManager.LoadedState == AtkLoadState.Loaded;
    }

    /// <inheritdoc/>
    public unsafe void CloseAddon(string addonName)
    {
        var ptr = this.dalamudServices.GameGui.GetAddonByName(addonName);
        if (ptr == nint.Zero)
        {
            return;
        }

        var addon = (AtkUnitBase*)ptr;
        if (addon->IsVisible)
        {
            addon->Close(true);
        }
    }

    /// <inheritdoc/>
    public unsafe string GetNodeText(string addonName, params int[] nodeNumbers)
    {
        if (nodeNumbers.Length == 0)
        {
            throw new AthavarPluginException("At least one node number is required");
        }

        var ptr = this.dalamudServices.GameGui.GetAddonByName(addonName);
        if (ptr == nint.Zero)
        {
            throw new AthavarPluginException($"Could not find {addonName} addon");
        }

        var addon = (AtkUnitBase*)ptr;
        var uld = addon->UldManager;

        AtkResNode* node = null;
        var debugString = string.Empty;
        for (var i = 0; i < nodeNumbers.Length; i++)
        {
            var nodeNumber = nodeNumbers[i];
            var count = uld.NodeListCount;
            if (nodeNumber < 0 || nodeNumber >= count)
            {
                throw new AthavarPluginException($"Addon node number must be between 0 and {count} for the {addonName} addon");
            }

            node = uld.NodeList[nodeNumber];
            debugString += $"[{nodeNumber}]";

            if (node == null)
            {
                throw new AthavarPluginException($"{addonName} addon node{debugString} is null");
            }

            // More nodes to traverse
            if (i < nodeNumbers.Length - 1)
            {
                if ((int)node->Type < 1000)
                {
                    throw new AthavarPluginException($"{addonName} addon node{debugString} is not a component");
                }

                uld = ((AtkComponentNode*)node)->Component->UldManager;
            }
        }

        if (node->Type != NodeType.Text)
        {
            throw new AthavarPluginException($"{addonName} addon node[{debugString}] is not a text node");
        }

        var textNode = (AtkTextNode*)node;
        return textNode->NodeText.ToString();
    }

    /// <inheritdoc/>
    public unsafe string GetSelectStringText(int index)
    {
        var ptr = this.dalamudServices.GameGui.GetAddonByName("SelectString");
        if (ptr == nint.Zero)
        {
            throw new AthavarPluginException("Could not find SelectString addon");
        }

        var addon = (AddonSelectString*)ptr;
        var popup = &addon->PopupMenu.PopupMenu;

        var count = popup->EntryCount;
        this.logger.Debug($"index={index} // Count={count} // {index < 0 || index > count}");
        if (index < 0 || index > count)
        {
            throw new AthavarPluginException("Index out of range");
        }

        return popup->EntryNames[index];
    }

    /// <inheritdoc/>
    public unsafe int GetSelectStringEntryCount()
    {
        var ptr = this.dalamudServices.GameGui.GetAddonByName("SelectString");
        if (ptr == nint.Zero)
        {
            throw new AthavarPluginException("Could not find SelectString addon");
        }

        var addon = (AddonSelectString*)ptr;
        var popup = &addon->PopupMenu.PopupMenu;

        return popup->EntryCount;
    }

    /// <inheritdoc/>
    public unsafe string GetSelectIconStringText(int index)
    {
        var ptr = this.dalamudServices.GameGui.GetAddonByName("SelectIconString");
        if (ptr == nint.Zero)
        {
            throw new AthavarPluginException("Could not find SelectIconString addon");
        }

        var addon = (AddonSelectIconString*)ptr;
        var popup = &addon->PopupMenu.PopupMenu;

        var count = popup->EntryCount;
        if (index < 0 || index > count)
        {
            throw new AthavarPluginException("Index out of range");
        }

        return popup->EntryNames[index];
    }
}