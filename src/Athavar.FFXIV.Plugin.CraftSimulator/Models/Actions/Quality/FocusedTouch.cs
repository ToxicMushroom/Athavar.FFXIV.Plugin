// <copyright file="FocusedTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Quality;

using Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Other;

internal class FocusedTouch : QualityAction
{
    private static readonly uint[] IdsValue = { 100243, 100244, 100245, 100246, 100247, 100248, 100249, 100250 };

    /// <inheritdoc />
    public override int Level => 68;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 18;

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => simulation.HasComboAvaiable<Observe>() ? 100 : 50;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 150;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}