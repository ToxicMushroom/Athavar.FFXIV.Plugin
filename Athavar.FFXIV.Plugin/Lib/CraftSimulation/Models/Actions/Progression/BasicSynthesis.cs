// <copyright file="BasicSynthesis.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Progression;

internal class BasicSynthesis : ProgressAction
{
    private static readonly int[] IdsValue = { 100001, 100015, 100030, 100075, 100045, 100060, 100090, 100105 };

    /// <inheritdoc />
    public override int Level => 1;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 0;

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation) => null;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation)
    {
        if (simulation.CurrentStats?.Level >= 31)
        {
            // Basic Synthesis Mastery
            return 120;
        }

        return 100;
    }

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}