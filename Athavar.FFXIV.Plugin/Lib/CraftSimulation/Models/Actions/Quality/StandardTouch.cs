// <copyright file="StandardTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Quality;

internal class StandardTouch : QualityAction
{
    private static readonly int[] IdsValue = { 100004, 100018, 100034, 100078, 100048, 100064, 100093, 100109 };

    /// <inheritdoc />
    public override int Level => 18;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => this.HasCombo(simulation) ? 18 : 32;

    /// <inheritdoc />
    public override bool HasCombo(Simulation simulation) => simulation.HasComboAvaiable<BasicTouch>();

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation) => null;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 125;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}