// <copyright file="CraftQueue.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue;

using Athavar.FFXIV.Plugin.Click;
using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Plugin.Services;

internal sealed class CraftQueue : IDisposable
{
    private readonly List<CraftingJob> queuedJobs = new();
    private readonly List<CraftingJob> completedJobs = new();

    private readonly IFrameworkManager frameworkManager;

    public CraftQueue(IDalamudServices dalamudServices, ICommandInterface commandInterface, IGearsetManager gearsetManager, IChatManager chatManager, IClick click, CraftQueueConfiguration configuration, IFrameworkManager frameworkManager)
    {
        this.DalamudServices = dalamudServices;
        this.CommandInterface = commandInterface;
        this.GearsetManager = gearsetManager;
        this.ChatManager = chatManager;
        this.Click = click;
        this.Data = new CraftQueueData(dalamudServices);
        this.Configuration = configuration ?? throw new AthavarPluginException();
        this.frameworkManager = frameworkManager;

        this.frameworkManager.Subscribe(this.FrameworkOnUpdate);
    }

    public ICommandInterface CommandInterface { get; }

    public IGearsetManager GearsetManager { get; }

    public IChatManager ChatManager { get; }

    public IDalamudServices DalamudServices { get; }

    public CraftQueueConfiguration Configuration { get; }

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

        this.queuedJobs.Add(new CraftingJob(this, recipe, rotationNode, gs.ToCrafterStats(), count, food, potion, hqIngredients));
        return true;
    }

    /// <inheritdoc/>
    public void Dispose() => this.frameworkManager.Unsubscribe(this.FrameworkOnUpdate);

    internal void DequeueJob(int idx) => this.queuedJobs.RemoveAt(idx);

    internal void DeleteHistory(int idx) => this.completedJobs.RemoveAt(idx);

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
            var found = job.Recipe.Ingredients.FirstOrDefault(i => i.Id == itemId);
            if (found is null)
            {
                return 0;
            }

            var hqFound = job.HqIngredients.FirstOrDefault(i => i.ItemId == itemId);
            if (hqFound == default)
            {
                if (hqState)
                {
                    return 0;
                }

                return found.Amount * job.RemainingLoops;
            }

            if (hqState)
            {
                return hqFound.Amount * job.RemainingLoops;
            }

            return (uint)Math.Max(0, (found.Amount - hqFound.Amount) * job.RemainingLoops);
        }
    }

    private void FrameworkOnUpdate(IFramework framework)
    {
        try
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
        catch (AthavarPluginException ex)
        {
            this.DalamudServices.PluginLogger.Error(ex, ex.Message);
        }
    }
}