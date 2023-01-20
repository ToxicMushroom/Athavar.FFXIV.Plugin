// <copyright file="CraftingQueue.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.CraftQueue;

using System;
using System.Collections.Generic;
using System.Linq;
using Athavar.FFXIV.Plugin.Lib.ClickLib;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Dalamud.Game;

internal class CraftQueue : IDisposable
{
    private readonly List<CraftingJob> queuedJobs = new();
    private readonly List<CraftingJob> completedJobs = new();
    private CraftingJob? currentJob;

    public CraftQueue(IDalamudServices dalamudServices, ICommandInterface commandInterface, IGearsetManager gearsetManager, IChatManager chatManager, IClick click, CraftQueueData craftQueueData)
    {
        this.DalamudServices = dalamudServices;
        this.CommandInterface = commandInterface;
        this.GearsetManager = gearsetManager;
        this.ChatManager = chatManager;
        this.Click = click;
        this.Data = craftQueueData;

        this.DalamudServices.Framework.Update += this.FrameworkOnUpdate;
    }

    public ICommandInterface CommandInterface { get; }

    public IGearsetManager GearsetManager { get; }

    public IChatManager ChatManager { get; }

    public IDalamudServices DalamudServices { get; }

    public IClick Click { get; }

    public CraftQueueData Data { get; }

    internal IReadOnlyList<CraftingJob> Jobs => this.queuedJobs;

    internal IReadOnlyList<CraftingJob> JobsCompleted => this.completedJobs;

    internal CraftingJob? CurrentJob { get; private set; }

    internal QueueState Paused { get; set; } = QueueState.Paused;

    public bool CreateJob(Recipe recipe, RotationNode rotationNode, uint count, BuffInfo? food, BuffInfo? potion, (uint ItemId, byte Amount)[] hqIngredients)
    {
        var gs = this.GearsetManager.AllGearsets.FirstOrDefault(g => g.GetCraftingJob() == recipe.Class);
        if (gs is null)
        {
            return false;
        }

        this.queuedJobs.Add(new CraftingJob(this, recipe, rotationNode, new CrafterStats(Constants.MaxLevel, gs.Control, gs.Craftsmanship, gs.CP, gs.HasSoulStone), count, food, potion, hqIngredients));
        return true;
    }

    /// <inheritdoc />
    public void Dispose() => this.DalamudServices.Framework.Update -= this.FrameworkOnUpdate;

    internal void DequeueJob(int idx) => this.queuedJobs.RemoveAt(idx);

    internal void CancelCurrentJob()
    {
        if (this.CurrentJob == null)
        {
            return;
        }

        var job = this.CurrentJob;
        this.CurrentJob = null;
        job.Cancel();
        this.completedJobs.Add(job);
    }

    internal uint CountItemInQueueAndCurrent(uint itemId, bool hqState)
    {
        uint num = 0;
        var nq = itemId % 1000000U;
        var hq = nq + 1000000U;

        if (this.CurrentJob != null)
        {
            num += AmountInJob(this.CurrentJob);
        }

        foreach (var job in this.Jobs)
        {
            num += AmountInJob(job);
        }

        return num;

        uint AmountInJob(CraftingJob job)
        {
            if (hqState)
            {
                var found = job.HqIngredients.FirstOrDefault();
                if (found == default)
                {
                    return 0;
                }

                return found.Amount * job.RemainingLoops;
            }
            else
            {
                var found = job.Recipe.Ingredients.FirstOrDefault(i => i.Id == itemId);
                if (found is null)
                {
                    return 0;
                }

                return found.Amount * job.RemainingLoops;
            }
        }
    }

    private void FrameworkOnUpdate(Framework framework)
    {
        if (this.Paused == QueueState.Paused)
        {
            if (this.CurrentJob is null)
            {
                return;
            }

            this.CurrentJob.Pause();
            return;
        }

        if (this.CurrentJob is null)
        {
            if (this.queuedJobs.Count == 0)
            {
                this.Paused = QueueState.Paused;
                return;
            }

            this.CurrentJob = this.queuedJobs[0];
            this.queuedJobs.RemoveAt(0);
        }

        if (this.Paused == QueueState.PausedSoon && this.CurrentJob.CurrentStep == 0)
        {
            this.Paused = QueueState.Paused;
            return;
        }

        bool finish, error = false;
        try
        {
            finish = this.CurrentJob.Tick();
        }
        catch (CraftingJobException ex)
        {
            this.ChatManager.PrintErrorMessage(ex.Message);
            error = finish = true;
        }

        if (!finish)
        {
            return;
        }

        var job = this.CurrentJob;
        this.CurrentJob = null;
        if (error)
        {
            job.Failure();
        }

        this.completedJobs.Add(job);
    }
}