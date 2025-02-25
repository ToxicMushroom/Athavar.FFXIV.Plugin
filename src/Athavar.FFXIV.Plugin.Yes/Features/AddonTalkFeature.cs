﻿// <copyright file="AddonTalkFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonTalk feature.
/// </summary>
internal class AddonTalkFeature : UpdateFeature
{
    private readonly YesModule module;
    private ClickTalk? clickTalk;
    private nint lastTalkAddon = nint.Zero;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonTalkFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    /// <param name="services">ServiceContainer of all dalamud services.</param>
    public AddonTalkFeature(YesModule module)
        : base("48 89 74 24 ?? 57 48 83 EC 40 0F 29 74 24 ?? 48 8B F9 0F 29 7C 24 ?? 0F 28 F1", module)
        => this.module = module;

    /// <inheritdoc />
    protected override string AddonName => "Talk";

    /// <inheritdoc />
    protected override unsafe void UpdateImpl(nint addon, nint a2, nint a3)
    {
        var addonPtr = (AddonTalk*)addon;
        if (!addonPtr->AtkUnitBase.IsVisible)
        {
            return;
        }

        var target = this.module.DalamudServices.TargetManager.Target;
        var targetName = this.module.LastSeenTalkTarget = target != null
            ? this.module.GetSeStringText(target.Name)
            : string.Empty;

        var nodes = this.Configuration.GetAllNodes().OfType<TalkEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.TargetText))
            {
                continue;
            }

            var matched = this.EntryMatchesTargetName(node, targetName);
            if (!matched)
            {
                continue;
            }

            if (this.clickTalk == null || this.lastTalkAddon != addon)
            {
                this.clickTalk = ClickTalk.Using(this.lastTalkAddon = addon);
            }

            PluginLog.Debug("AddonTalk: Advancing");
            this.clickTalk.Click();
            return;
        }
    }

    private bool EntryMatchesTargetName(TalkEntryNode node, string targetName)
        => (node.TargetIsRegex && (node.TargetRegex?.IsMatch(targetName) ?? false)) ||
           (!node.TargetIsRegex && targetName.Contains(node.TargetText));
}