// <copyright file="BuffAction.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions;

using System.Collections.Generic;

internal abstract class BuffAction : CraftingAction
{
    public delegate void OnTick(Simulation simulation, CraftingAction action);

    public delegate void OnExpire(Simulation simulation);

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Buff;

    public override void Execute(Simulation simulation)
    {
        foreach (var @override in this.GetOverrides())
        {
            simulation.RemoveBuff(@override);
        }

        simulation.AddBuff(this.GetAppliedBuff(simulation));
    }

    public abstract int GetDuration(Simulation simulation);

    /// <inheritdoc />
    public override int GetDurabilityCost(Simulation simulation) => 0;

    public virtual IEnumerable<Buffs> GetOverrides() => new[] { this.GetBuff() };

    /// <inheritdoc />
    public override bool SkipOnFail() => true;

    public abstract Buffs GetBuff();

    public abstract int GetInitialStacks();

    protected virtual bool CanBeClipped() => false;

    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation)
    {
        if (this.CanBeClipped())
        {
            return null;
        }

        return simulation.HasBuff(this.GetBuff()) ? SimulationFailCause.INVALID_ACTION : null;
    }

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    protected abstract OnTick? GetOnTick();

    protected virtual OnExpire? GetOnExpire() => null;

    private EffectiveBuff GetAppliedBuff(Simulation simulation)
        => new(
            simulation.State == StepState.PRIMED
                ? this.GetDuration(simulation) + 2
                : this.GetDuration(simulation),
            this.GetInitialStacks(),
            this.GetBuff(),
            simulation.Steps.Count,
            this.GetOnTick(),
            this.GetOnExpire());
}