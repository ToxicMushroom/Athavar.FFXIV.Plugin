﻿// <copyright file="PluginWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.Config;
using Dalamud.Interface.Windowing;
using ImGuiNET;

/// <summary>
///     The main <see cref="Window"/> of the plugin.
/// </summary>
internal sealed class PluginWindow : Window, IDisposable, IPluginWindow
{
    private readonly IModuleManager manager;

    private readonly TabBarHandler tabBarHandler = new("athavar-toolbox");

    private readonly SettingsTab settingsTab;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginWindow"/> class.
    /// </summary>
    /// <param name="localizeManager"><see cref="ILocalizeManager"/> added by DI.</param>
    /// <param name="manager"><see cref="IModuleManager"/> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration"/> added by DI.</param>
    public PluginWindow(ILocalizeManager localizeManager, IModuleManager manager, CommonConfiguration configuration, IDalamudServices services, IGearsetManager gearsetManager)
        : base("ConfigRoot###mainWindow")
    {
        this.manager = manager;
        this.LaunchButton = new PluginLaunchButton(services, this.Toggle);
        if (configuration.ShowLaunchButton)
        {
            this.LaunchButton.AddEntry();
        }

        this.settingsTab = new SettingsTab(this, services, this.manager, localizeManager, configuration, gearsetManager);
        this.tabBarHandler.Add(this.settingsTab);

        this.Size = new Vector2(525, 600);
        this.SizeCondition = ImGuiCond.FirstUseEver;
        this.PositionCondition = ImGuiCond.Appearing;
        this.RespectCloseHotkey = false;

        this.manager.StateChange += this.OnModuleStateChange;

#if DEBUG
        this.Toggle();
#endif
    }

    internal PluginLaunchButton LaunchButton { get; }

    /// <inheritdoc/>
    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.ResizeGrip, 0);
        this.WindowName = $"{Plugin.PluginName} - {this.tabBarHandler.GetTabTitle()}###mainWindow";
        ImGui.SetNextWindowSize(this.Size.GetValueOrDefault());
    }

    /// <inheritdoc/>
    public override void PostDraw() => ImGui.PopStyleColor();

    // this.Position = ImGui.GetWindowPos();
    /// <inheritdoc/>
    public override void Draw() => this.tabBarHandler.Draw();

    /// <inheritdoc/>
    public void SelectTab(string tabIdentifier) => this.tabBarHandler.SelectTab(tabIdentifier);

    /// <inheritdoc/>
    public void Dispose() => this.manager.StateChange -= this.OnModuleStateChange;

    private void OnModuleStateChange(Module module, IModuleManager.IModuleData data)
    {
        if (!data.HasTab || module.Tab is null)
        {
            return;
        }

        if (module.Enabled && data.TabEnabled)
        {
            this.tabBarHandler.Add(module.Tab);
        }
        else
        {
            this.tabBarHandler.Remove(module.Tab);
        }
    }
}