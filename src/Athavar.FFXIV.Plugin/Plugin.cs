﻿// <copyright file="Plugin.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using Athavar.FFXIV.Plugin.AutoSpear;
using Athavar.FFXIV.Plugin.Cheat;
using Athavar.FFXIV.Plugin.Click;
using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.CraftQueue;
using Athavar.FFXIV.Plugin.Data;
using Athavar.FFXIV.Plugin.Dps;
using Athavar.FFXIV.Plugin.DutyHistory;
using Athavar.FFXIV.Plugin.Instancinator;
using Athavar.FFXIV.Plugin.Macro;
using Athavar.FFXIV.Plugin.OpcodeWizard;
using Athavar.FFXIV.Plugin.SliceIsRight;
using Athavar.FFXIV.Plugin.UI;
using Athavar.FFXIV.Plugin.Yes;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Main plugin implementation.
/// </summary>
public sealed class Plugin : IDalamudPlugin
{
    /// <summary>
    ///     prefix of the command.
    /// </summary>
    internal const string CommandName = "/ath";

    /// <summary>
    ///     The Plugin name.
    /// </summary>
    internal const string PluginName = "Athavar's Toolbox";

    private readonly DalamudPluginInterface pluginInterface;

    private readonly ServiceProvider provider;
    private readonly PluginService servive;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="pluginInterface">Dalamud plugin interface.</param>
    public Plugin(
        [RequiredVersion("1.0")]
        DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;

        this.provider = this.BuildProvider();
        this.servive = this.provider.GetRequiredService<PluginService>();
        this.servive.Start();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.servive.Stop();
        this.provider.DisposeAsync().GetAwaiter().GetResult();
    }

    private ServiceProvider BuildProvider()
    {
        return new ServiceCollection()
           .AddSingleton(o =>
            {
                if (this.pluginInterface.ConfigFile.Exists)
                {
                    // migrate old configuration
                    // TODO: replace static PluginLog
                    PluginLog.LogInformation("Start the migrate of the configuration");
                    Configuration.Migrate(this.pluginInterface);
                    PluginLog.LogInformation("Finish the migrate of the configuration");
                }

                return this.pluginInterface;
            })
           .AddSingleton<IPluginWindow, PluginWindow>()
           .AddSingleton<IModuleManager, ModuleManager>()
           .AddSingleton(_ => new WindowSystem("Athavar's Toolbox"))
           .AddCommon()
           .AddData()
           .AddAutoSpearModule()
           .AddClick()
           .AddMacroModule()
           .AddYesModule()
           .AddInstancinatorModule()
           .AddCheatModule()
           .AddCraftQueueModule()
           .AddDps()
           .AddOpcodeWizard()
           .AddSliceIsRightModule()
           .AddDutyHistory()
#if DEBUG
#endif
           .AddSingleton<AutoTranslateWindow>()
           .AddSingleton<PluginService>()
           .BuildServiceProvider();
    }
}