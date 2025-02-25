// <copyright file="BaseCombatant.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data;

internal abstract class BaseCombatant<T> : BaseCombatant
    where T : BaseCombatant
{
    protected readonly BaseEncounter<T> Encounter;

    protected BaseCombatant(BaseEncounter<T> encounter, uint objectId, uint dataId)
    {
        this.Encounter = encounter;
        this.ObjectId = objectId;
        this.DataId = dataId;
    }

    public static bool operator==(BaseCombatant<T>? left, BaseCombatant<T>? right) => Equals(left, right);

    public static bool operator!=(BaseCombatant<T>? left, BaseCombatant<T>? right) => !Equals(left, right);

    public override void CalcStats()
    {
        var duration = this.Encounter.Duration.TotalSeconds >= 1 ? this.Encounter.Duration.TotalSeconds : 1;
        this.Dps = Math.Round(this.DamageTotal / duration, 2);
        this.Hps = Math.Round(this.HealingTotal / duration, 2);
        this.EffectiveHealing = this.HealingTotal - this.OverHealTotal;
        this.OverHealPct = this.HealingTotal == 0 ? 0 : Math.Round(((double)this.OverHealTotal / this.HealingTotal) * 100, 2);
    }

    public override void PostCalcStats() => this.DamagePct = this.DamageTotal == 0 ? 0 : Math.Round(((double)this.DamageTotal / this.Encounter.DamageTotal) * 100, 2);

    public bool Equals(Combatant other)
    {
        if (this.DataId != 0)
        {
            return this.DataId == other.DataId;
        }

        if (this.ObjectId != 0)
        {
            return this.ObjectId == other.ObjectId;
        }

        return false;
    }

    public override int GetHashCode() => HashCode.Combine(this.ObjectId, this.DataId);
}