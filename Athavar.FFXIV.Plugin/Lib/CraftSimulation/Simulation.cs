// <copyright file="Simulation.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation;

using System;
using System.Collections.Generic;
using System.Linq;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Buff;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Other;

internal partial class Simulation
{
    private readonly Random random = new((int)DateTime.UtcNow.Ticks);
    private readonly long startingQuality;

    public Simulation(CrafterStats crafterStats, Recipe recipe, (uint ItemId, byte Amount)[] hqIngredients, int startingQuality = 0)
    {
        this.CrafterStats = crafterStats;
        this.Recipe = recipe;

        foreach (var ingredient in hqIngredients)
        {
            var ingredientDetails = recipe.Ingredients.FirstOrDefault(i => i.Id == ingredient.ItemId);
            if (ingredientDetails?.Quality != null)
            {
                this.Quality += ingredientDetails.Quality.Value * ingredient.Amount;
            }
        }

        if (hqIngredients.Length == 0)
        {
            this.Quality = startingQuality;
        }

        this.startingQuality = this.Quality;
    }

    public void Repair(int amount)
    {
        this.Durability += amount;
        if (this.Durability > this.Recipe.Durability)
        {
            this.Durability = this.Recipe.Durability;
        }
    }

    public SimulationResult Run(IEnumerable<CraftingAction> actions, int maxTurns = int.MaxValue)
    {
        SimulationFailCause? simulationFailCause = null;
        this.Reset();
        var currentStats = this.CurrentStats;
        if (currentStats is null || currentStats.Level < this.Recipe.Level)
        {
            simulationFailCause = SimulationFailCause.MISSING_LEVEL_REQUIREMENT;
        }
        else if ((this.Recipe.CraftsmanshipReq is not null && currentStats.Craftsmanship < this.Recipe.CraftsmanshipReq) ||
                 (this.Recipe.ControlReq is not null && currentStats.Control < this.Recipe.ControlReq))
        {
            simulationFailCause = SimulationFailCause.MISSING_STATS_REQUIREMENT;
        }
        else
        {
            foreach (var craftingAction in actions)
            {
                ActionResult result;
                var failCause = craftingAction.CanBeUsed(this);
                var hasEnoughCP = craftingAction.GetBaseCPCost(this) <= this.AvailableCP;
                if (!hasEnoughCP)
                {
                    failCause = SimulationFailCause.NOT_ENOUGH_CP;
                }

                if (this.Success is null && hasEnoughCP && failCause is null)
                {
                    result = this.RunAction(craftingAction);
                }
                else
                {
                    result = new ActionResult(
                        craftingAction,
                        null,
                        0,
                        0,
                        0,
                        true,
                        0,
                        this.State,
                        failCause,
                        false);
                }

                if (this.Steps.Count < maxTurns)
                {
                    var skipTicksOnFail = this.Success == false && craftingAction.SkipOnFail();
                    if (this.Success is null && !craftingAction.IsSkipsBuffTicks() && !skipTicksOnFail)
                    {
                        this.TickBuffs(craftingAction);
                    }
                }

                if (!this.Linear && actions is not FinalAppraisal or RemoveFinalAppraisal)
                {
                    this.TickState();
                }

                this.Steps.Add(result);
            }
        }

        simulationFailCause ??= this.Steps.FirstOrDefault(step => step.FailCause != null)?.FailCause ?? null;

        var res = new SimulationResult
        {
            Steps = this.Steps,
            Success = this.Progression >= this.Recipe.Progress,
            Simulation = this,
        };

        if (res.Success && this.Recipe.QualityReq is not null)
        {
            var qualityRequirementMet = this.Quality >= this.Recipe.QualityReq;
            res.Success &= qualityRequirementMet;
            if (!res.Success)
            {
                simulationFailCause = SimulationFailCause.QUALITY_TOO_LOW;
            }
        }

        res.FailCause = simulationFailCause;

        return res;
    }

    /**
   * Changes the state of the craft,
   * source: https://github.com/Ermad/ffxiv-craft-opt-web/blob/master/app/js/ffxivcraftmodel.js#L255
   */
    private void TickState()
    {
        // If current state is EXCELLENT, then next one is poor
        if (this.State == StepState.EXCELLENT)
        {
            this.State = StepState.POOR;
            return;
        }

        var currentStats = this.CrafterStats.Jobs[(int)this.Recipe.Job];
        if (currentStats is null)
        {
            return;
        }

        // LV 63 Trait for improved Good chances (Quality Assurance)
        var goodChance = currentStats.Level >= 63 ? 0.25 : 0.2;

        var statesAndRates = this.Recipe.PossibleConditions
           .Where(condition => condition != StepState.NORMAL)
           .Select(condition =>
            {
                // Default rate - most conditions are 12% so here we are.
                var rate = 0.12;
                switch (condition)
                {
                    case StepState.GOOD:
                        rate = this.Recipe.Expert ? 0.12 : goodChance;
                        break;
                    case StepState.EXCELLENT:
                        rate = this.Recipe.Expert ? 0 : 0.04;
                        break;
                    case StepState.POOR:
                        rate = 0;
                        break;
                    case StepState.CENTERED:
                        rate = 0.15;
                        break;
                    case StepState.PLIANT:
                        rate = 0.12;
                        break;
                    case StepState.STURDY:
                        rate = 0.15;
                        break;
                    case StepState.MALLEABLE:
                        rate = 0.12;
                        break;
                    case StepState.PRIMED:
                        rate = 0.12;
                        break;
                }

                return (
                    item: condition,
                    weight: rate);
            }).ToList();

        var nonNormalRate = statesAndRates
           .Select(val => val.weight)
           .Sum();

        statesAndRates.Add((StepState.NORMAL, 1 - nonNormalRate));

        var threshold = this.random.NextDouble();

        var check = 0.0;
        foreach (var (item, weight) in statesAndRates)
        {
            check += weight;
            if (check > threshold)
            {
                this.State = item;
                return;
            }
        }

        this.State = statesAndRates.Last().item;
    }

    private ActionResult RunAction(CraftingAction action)
    {
        // The roll for the current action's success rate, 0 if ideal mode, as 0 will even match a 1% chances.
        var probabilityRoll = this.Linear ? 0 : this.random.Next(0, 100);

        var success = false;
        var qualityBefore = this.Quality;
        var progressionBefore = this.Progression;
        var durabilityBefore = this.Durability;
        var cpBefore = this.AvailableCP;
        SimulationFailCause? failCause = null;
        var combo = action.HasCombo(this);

        if (this.SafeMode &&
            (action.GetSuccessRate(this) < 100 ||
             (action.IsRequiresGood && !this.HasBuff(Buffs.HEART_AND_SOUL))))
        {
            failCause = SimulationFailCause.UNSAFE_ACTION;
            this.Safe = false;
        }
        else
        {
            if (action.GetSuccessRate(this) >= probabilityRoll)
            {
                action.Execute(this);
                success = true;
            }
            else
            {
                action.OnFail(this);
            }
        }


        // Even if the action failed, we have to remove the durability cost
        this.Durability -= action.GetDurabilityCost(this);

        // Even if the action failed, CP has to be consumed too
        this.AvailableCP -= action.GetCPCost(this);

        if (this.Progression >= this.Recipe.Progress)
        {
            this.Success = true;
        }
        else if (this.Durability <= 0)
        {
            // Check durability to see if the craft is failed or not
            this.Success = false;
            failCause = SimulationFailCause.DURABILITY_REACHED_ZERO;
        }

        return new ActionResult(
            action,
            success,
            this.Quality - qualityBefore,
            this.Progression - progressionBefore,
            this.AvailableCP - cpBefore,
            false,
            this.Durability - durabilityBefore,
            this.State,
            failCause,
            combo);
    }

    private void TickBuffs(CraftingAction action)
    {
        foreach (var effectiveBuff in this.effectiveBuffs)
        {
            // We are checking the appliedStep because ticks only happen at the beginning of the second turn after the application,
            // For instance, Great strides launched at turn 1 will start to loose duration at the beginning of turn 3
            if (effectiveBuff.AppliedStep < this.Steps.Count)
            {
                // If the buff has something to do, let it do it
                effectiveBuff.TickAction?.Invoke(this, action);

                effectiveBuff.Duration--;
            }
        }

        foreach (var effectiveBuff in this.effectiveBuffs
           .Where(buff => buff.Duration <= 0))
        {
            effectiveBuff.ExpireAction?.Invoke(this);
        }

        this.effectiveBuffs.RemoveAll(buff => buff.Duration > 0);
    }
}