﻿// <copyright file="AddonGrandCompanySupplyRewardFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;

/// <summary>
///     AddonGrandCompanySupplyReward feature.
/// </summary>
internal class AddonGrandCompanySupplyRewardFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonGrandCompanySupplyRewardFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonGrandCompanySupplyRewardFeature(YesModule module)
        : base("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 30 BA ?? ?? ?? ?? 4D 8B E8 4C 8B F9", module)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "GrandCompanySupplyReward";

    /// <inheritdoc />
    protected override void OnSetupImpl(nint addon, uint a2, nint data)
    {
        if (!this.Configuration.GrandCompanySupplyReward)
        {
            return;
        }

        ClickGrandCompanySupplyReward.Using(addon).Deliver();
    }
}