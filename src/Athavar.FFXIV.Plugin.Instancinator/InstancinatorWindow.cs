// <copyright file="InstancinatorWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Instancinator;

using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;

internal class InstancinatorWindow : Window
{
    private InstancinatorModule module = null!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstancinatorWindow" /> class.
    /// </summary>
    /// <param name="windowSystem"><see cref="WindowSystem" /> added by DI.</param>
    public InstancinatorWindow(WindowSystem windowSystem)
        : base("Instancinator")
    {
        this.Flags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar;
        windowSystem.AddWindow(this);
    }

    private unsafe int InstanceNumber => UIState.Instance()->AreaInstance.Instance;

    /// <inheritdoc />
    public override void Draw()
    {
        var selectedInst = this.module.SelectedInstance;
        var currentInst = this.InstanceNumber;
        var numInst = this.module.GetNumberOfInstances();

        void DrawInstanceButton(FontAwesomeIcon icon, int index)
        {
            if (index > numInst)
            {
                return;
            }

            var current = currentInst == index;
            if (current)
            {
                ImGui.BeginDisabled();
            }

            if (this.ImGuiColoredButton(icon, selectedInst == index))
            {
                this.module.EnableInstance(index);
            }

            if (current)
            {
                ImGui.EndDisabled();
            }
        }

        ImGui.SetWindowFontScale(1f);
        ImGui.Text($"Sel: {selectedInst}");
        ImGui.SetWindowFontScale(2f);

        DrawInstanceButton(FontAwesomeIcon.DiceOne, 1);
        DrawInstanceButton(FontAwesomeIcon.DiceTwo, 2);
        DrawInstanceButton(FontAwesomeIcon.DiceThree, 3);

        if (this.ImGuiIconButton(FontAwesomeIcon.TimesCircle))
        {
            this.module.DisableAllAndCreateIfNotExists();
        }
    }

    /// <summary>
    ///     Setup the <see cref="InstancinatorWindow" />.
    /// </summary>
    /// <param name="m">The <see cref="InstancinatorModule" />.</param>
    internal void Setup(InstancinatorModule m) => this.module = m;

    private bool ImGuiColoredButton(FontAwesomeIcon icon, bool colored)
    {
        if (colored)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DalamudRed);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ImGuiColors.DalamudOrange);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, ImGuiColors.DPSRed);
        }

        var val = this.ImGuiIconButton(icon);
        if (colored)
        {
            ImGui.PopStyleColor(3);
        }

        return val;
    }

    private bool ImGuiIconButton(FontAwesomeIcon icon)
    {
        ImGui.PushFont(UiBuilder.IconFont);
        var result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}Inst", new Vector2(45f));
        ImGui.PopFont();
        return result;
    }
}