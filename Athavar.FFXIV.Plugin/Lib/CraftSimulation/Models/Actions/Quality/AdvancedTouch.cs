// <copyright file="AdvancedTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Quality;

internal class AdvancedTouch : QualityAction
{
    private static readonly int[] IdsValue = { 100411, 100412, 100413, 100414, 100415, 100416, 100417, 100418 };

    /// <inheritdoc />
    public override int Level => 84;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => this.HasCombo(simulation) ? 18 : 46;

    public override bool HasCombo(Simulation simulation)
    {
        for (var index = simulation.Steps.Count - 1; index >= 0; index--)
        {
            var step = simulation.Steps[index];
            if (step.Success == true && step.Action is StandardTouch && step.Combo)
            {
                return true;
            }

            // If there's an action that isn't skipped (fail or not), combo is broken
            if (!step.Skipped)
            {
                return false;
            }
        }

        return false;
    }

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation) => null;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 150;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}