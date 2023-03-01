// <copyright file="CraftQueueModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue;

using Athavar.FFXIV.Plugin.Common;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;

[Module(ModuleName, ModuleConfigurationType = typeof(CraftQueueConfiguration))]
internal class CraftQueueModule : Module<CraftQueueTab, CraftQueueConfiguration>
{
    private const string ModuleName = "CraftQueue";

    private readonly IServiceProvider provider;

    private CraftQueue? craftQueue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CraftQueueModule" /> class.
    /// </summary>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider" /> added by DI.</param>
    public CraftQueueModule(Configuration configuration, IServiceProvider provider)
        : base(configuration, configuration.CraftQueue!)
    {
        this.provider = provider;

        PluginLog.LogDebug("Module 'CraftQueue' init");
    }

    /// <inheritdoc />
    public override string Name => ModuleName;

    /// <inheritdoc />
    public override bool Hidden => false;

    /// <inheritdoc />
    public override void Dispose()
    {
        base.Dispose();
        this.craftQueue?.Dispose();
    }

    protected override CraftQueueTab InitTab()
    {
        this.craftQueue = ActivatorUtilities.CreateInstance<CraftQueue>(this.provider);
        return new CraftQueueTab(this.provider, this.craftQueue, this.Configuration);
    }
}