﻿// <copyright file="YesConfigTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes;

using System;
using System.Linq;
using System.Numerics;
using System.Text;
using Athavar.FFXIV.Plugin.Lib.ClickLib;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Exceptions;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Utils;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface;
using ImGuiNET;

/// <summary>
///     Yes Module configuration Tab.
/// </summary>
internal class YesConfigTab
{
    private readonly IDalamudServices dalamudServices;
    private readonly IChatManager chatManager;
    private readonly IClick click;
    private readonly ZoneListWindow zoneListWindow;
    private readonly Vector4 shadedColor = new(0.68f, 0.68f, 0.68f, 1.0f);

    private readonly string[] hotkeyChoices =
    {
        "None",
        "Control",
        "Alt",
        "Shift",
    };

    private readonly VirtualKey[] hotkeyValues =
    {
        VirtualKey.NO_KEY,
        VirtualKey.CONTROL,
        VirtualKey.MENU,
        VirtualKey.SHIFT,
    };

    private YesModule module = null!;
    private INode? draggedNode;
    private string debugClickName = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="YesConfigTab" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="chatManager"><see cref="IChatManager" /> added by DI.</param>
    /// <param name="click"><see cref="IClick" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="zoneListWindow"><see cref="ZoneListWindow" /> added by DI.</param>
    public YesConfigTab(IDalamudServices dalamudServices, IChatManager chatManager, IClick click, Configuration configuration, ZoneListWindow zoneListWindow)
    {
        this.dalamudServices = dalamudServices;
        this.chatManager = chatManager;
        this.click = click;
        this.Configuration = configuration.Yes!;
        this.zoneListWindow = zoneListWindow;
    }

    private YesConfiguration Configuration { get; }

    private TextFolderNode RootFolder => this.Configuration.RootFolder;

    private TextFolderNode ListRootFolder => this.Configuration.ListRootFolder;

    private TextFolderNode TalkRootFolder => this.Configuration.TalkRootFolder;

    public void DrawTab()
    {
        using var raii = new ImGuiRaii();
        if (!raii.Begin(() => ImGui.BeginTabItem("Yes"), ImGui.EndTabItem))
        {
            return;
        }

#if DEBUG
        this.UiBuilder_TestButton();
#endif

        var enabled = this.Configuration.FunctionEnabled;
        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            this.Configuration.FunctionEnabled = enabled;
            this.Configuration.Save();
        }

        if (ImGui.BeginTabBar("Settings"))
        {
            this.DisplayTextOptions();
            this.DisplayListOptions();
            this.DisplayTalkOptions();
            this.DisplayBotherOptions();

            ImGui.EndTabBar();
        }
    }

    /// <summary>
    ///     Setup the <see cref="YesConfigTab" />.
    /// </summary>
    /// <param name="m">The <see cref="YesModule" />.</param>
    internal void Setup(YesModule m)
    {
        this.module = m;
        this.zoneListWindow.Setup(m);
    }

    private void UiBuilder_TestButton()
    {
        ImGui.InputText("ClickName", ref this.debugClickName, 100);
        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.Check, "Submit"))
        {
            try
            {
                this.debugClickName ??= string.Empty;
                this.click.SendClick(this.debugClickName.Trim());
                this.chatManager.PrintChat($"Clicked {this.debugClickName} successfully.");
            }
            catch (ClickNotFoundError ex)
            {
                this.chatManager.PrintErrorMessage(ex.Message);
            }
            catch (InvalidClickException ex)
            {
                this.chatManager.PrintErrorMessage(ex.Message);
            }
            catch (Exception ex)
            {
                this.chatManager.PrintErrorMessage(ex.Message);
            }
        }
    }

    /* ==================================================================================================== */

    private void DisplayTextOptions()
    {
        if (!ImGui.BeginTabItem("YesNo"))
        {
            return;
        }

        ImGui.PushID("TextOptions");

        this.DisplayTextButtons();

        if (ImGui.BeginChild("##yes-yesNo-tree", ImGui.GetContentRegionAvail(), false))
        {
            this.DisplayTextNodes();
            ImGui.EndChild();
        }

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    private void DisplayListOptions()
    {
        if (!ImGui.BeginTabItem("Lists"))
        {
            return;
        }

        ImGui.PushID("ListOptions");

        this.DisplayListButtons();
        if (ImGui.BeginChild("##yes-lists-tree", ImGui.GetContentRegionAvail(), false))
        {
            this.DisplayListNodes();
            ImGui.EndChild();
        }

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    private void DisplayTalkOptions()
    {
        if (!ImGui.BeginTabItem("Talk"))
        {
            return;
        }

        ImGui.PushID("TalkOptions");

        this.DisplayTalkButtons();
        if (ImGui.BeginChild("##yes-talk-tree", ImGui.GetContentRegionAvail(), false))
        {
            this.DisplayTalkNodes();
            ImGui.EndChild();
        }

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    private void DisplayBotherOptions()
    {
        if (!ImGui.BeginTabItem("Bothers"))
        {
            return;
        }

        static void IndentedTextColored(Vector4 color, string text)
        {
            var indent = 27f * ImGuiHelpers.GlobalScale;
            ImGui.Indent(indent);
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextWrapped(text);
            ImGui.PopStyleColor();
            ImGui.Unindent(indent);
        }

        ImGui.PushID("BotherOptions");

        if (ImGui.BeginChild("##yes-talk-tree", ImGui.GetContentRegionAvail(), false))
        {
            // Disable hotkey
            if (!this.hotkeyValues.Contains(this.Configuration.DisableKey))
            {
                this.Configuration.DisableKey = VirtualKey.NO_KEY;
                this.Configuration.Save();
            }

            var disableHotkeyIndex = Array.IndexOf(this.hotkeyValues, this.Configuration.DisableKey);

            ImGui.SetNextItemWidth(85);
            if (ImGui.Combo("Disable Hotkey", ref disableHotkeyIndex, this.hotkeyChoices, this.hotkeyChoices.Length))
            {
                this.Configuration.DisableKey = this.hotkeyValues[disableHotkeyIndex];
                this.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "While this key is held, the plugin is disabled.");

            // Forced Yes hotkey
            if (!this.hotkeyValues.Contains(this.Configuration.ForcedYesKey))
            {
                this.Configuration.ForcedYesKey = VirtualKey.NO_KEY;
                this.Configuration.Save();
            }

            var forcedYesHotkeyIndex = Array.IndexOf(this.hotkeyValues, this.Configuration.ForcedYesKey);

            ImGui.SetNextItemWidth(85);
            if (ImGui.Combo("Forced Yes Hotkey", ref forcedYesHotkeyIndex, this.hotkeyChoices, this.hotkeyChoices.Length))
            {
                this.Configuration.ForcedYesKey = this.hotkeyValues[forcedYesHotkeyIndex];
                this.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "While this key is held, any Yes/No prompt will always default to yes. Be careful.");

            // SalvageDialog
            {
                var desynthDialog = this.Configuration.DesynthDialogEnabled;
                if (ImGui.Checkbox("SalvageDialog", ref desynthDialog))
                {
                    this.Configuration.DesynthDialogEnabled = desynthDialog;
                    this.Configuration.Save();
                }

                IndentedTextColored(this.shadedColor, "Remove the Desynthesis menu confirmation.");
            }

            // SalvageDialog (Bulk)
            {
                var desynthBulkDialog = this.Configuration.DesynthBulkDialogEnabled;
                if (ImGui.Checkbox("SalvageDialog (Bulk)", ref desynthBulkDialog))
                {
                    this.Configuration.DesynthBulkDialogEnabled = desynthBulkDialog;
                    this.Configuration.Save();
                }

                IndentedTextColored(this.shadedColor, "Check the bulk desynthesis button when using the SalvageDialog feature.");
            }

            // MaterializeDialog
            {
                var materialize = this.Configuration.MaterializeDialogEnabled;
                if (ImGui.Checkbox("MaterializeDialog", ref materialize))
                {
                    this.Configuration.MaterializeDialogEnabled = materialize;
                    this.Configuration.Save();
                }

                IndentedTextColored(this.shadedColor, "Remove the create new (extract) materia confirmation.");
            }

            // MateriaRetrieveDialog
            {
                var materiaRetrieve = this.Configuration.MateriaRetrieveDialogEnabled;
                if (ImGui.Checkbox("MateriaRetrieveDialog", ref materiaRetrieve))
                {
                    this.Configuration.MateriaRetrieveDialogEnabled = materiaRetrieve;
                    this.Configuration.Save();
                }

                IndentedTextColored(this.shadedColor, "Remove the retrieve materia confirmation.");
            }

            // ItemInspectionResult
            {
                var itemInspection = this.Configuration.ItemInspectionResultEnabled;
                if (ImGui.Checkbox("ItemInspectionResult", ref itemInspection))
                {
                    this.Configuration.ItemInspectionResultEnabled = itemInspection;
                    this.Configuration.Save();
                }

                IndentedTextColored(this.shadedColor, "Eureka/Bozja lockboxes, forgotten fragments, and more.\nWarning: this does not check if you are maxed on items.");

                IndentedTextColored(this.shadedColor, "Rate limiter (pause after N items)");
                ImGui.SameLine();

                ImGui.PushItemWidth(100f * ImGuiHelpers.GlobalScale);
                var itemInspectionResultLimiter = this.Configuration.ItemInspectionResultRateLimiter;
                if (ImGui.InputInt("###itemInspectionResultRateLimiter", ref itemInspectionResultLimiter))
                {
                    if (itemInspectionResultLimiter < 0)
                    {
                        itemInspectionResultLimiter = 0;
                    }
                    else
                    {
                        this.Configuration.ItemInspectionResultRateLimiter = itemInspectionResultLimiter;
                        this.Configuration.Save();
                    }
                }
            }

            // RetainerTaskAsk
            {
                var retainerTaskAsk = this.Configuration.RetainerTaskAskEnabled;
                if (ImGui.Checkbox("RetainerTaskAsk", ref retainerTaskAsk))
                {
                    this.Configuration.RetainerTaskAskEnabled = retainerTaskAsk;
                    this.Configuration.Save();
                }

                IndentedTextColored(this.shadedColor, "Skip the confirmation in the final dialog before sending out a retainer.");
            }

            // RetainerTaskResult
            {
                var retainerTaskResult = this.Configuration.RetainerTaskResultEnabled;
                if (ImGui.Checkbox("RetainerTaskResult", ref retainerTaskResult))
                {
                    this.Configuration.RetainerTaskResultEnabled = retainerTaskResult;
                    this.Configuration.Save();
                }

                IndentedTextColored(this.shadedColor, "Automatically send a retainer on the same venture as before when receiving an item.");
            }

            // GrandCompanySupplyReward
            {
                var grandCompanySupplyReward = this.Configuration.GrandCompanySupplyReward;
                if (ImGui.Checkbox("GrandCompanySupplyReward", ref grandCompanySupplyReward))
                {
                    this.Configuration.GrandCompanySupplyReward = grandCompanySupplyReward;
                    this.Configuration.Save();
                }

                IndentedTextColored(this.shadedColor, "Skip the confirmation when submitting Grand Company expert delivery items.");
            }

            // ShopCardDialog
            {
                var shopCard = this.Configuration.ShopCardDialog;
                if (ImGui.Checkbox("ShopCardDialog", ref shopCard))
                {
                    this.Configuration.ShopCardDialog = shopCard;
                    this.Configuration.Save();
                }

                IndentedTextColored(this.shadedColor, "Automatically confirm selling Triple Triad cards in the saucer.");
            }

            // JournalResultComplete
            {
                var journalResultComplete = this.Configuration.JournalResultCompleteEnabled;
                if (ImGui.Checkbox("JournalResultComplete", ref journalResultComplete))
                {
                    this.Configuration.JournalResultCompleteEnabled = journalResultComplete;
                    this.Configuration.Save();
                }

                IndentedTextColored(this.shadedColor, "Automatically confirm quest reward acceptance when there is nothing to choose.");
            }

            // ContentFinderConfirm
            {
                var contentsFinderConfirm = this.Configuration.ContentsFinderConfirmEnabled;
                if (ImGui.Checkbox("ContentsFinderConfirm", ref contentsFinderConfirm))
                {
                    this.Configuration.ContentsFinderConfirmEnabled = contentsFinderConfirm;

                    if (!contentsFinderConfirm)
                    {
                        this.Configuration.ContentsFinderOneTimeConfirmEnabled = false;
                    }

                    this.Configuration.Save();
                }

                IndentedTextColored(this.shadedColor, "Automatically commence duties when ready.");
            }

            // ContentFinderOneTimeConfirm
            {
                var contentsFinderOneTimeConfirm = this.Configuration.ContentsFinderOneTimeConfirmEnabled;
                if (ImGui.Checkbox("ContentsFinderOneTimeConfirm", ref contentsFinderOneTimeConfirm))
                {
                    this.Configuration.ContentsFinderOneTimeConfirmEnabled = contentsFinderOneTimeConfirm;

                    if (contentsFinderOneTimeConfirm)
                    {
                        this.Configuration.ContentsFinderConfirmEnabled = true;
                    }

                    this.Configuration.Save();
                }

                IndentedTextColored(this.shadedColor, "Automatically commence duties when ready, but only once.\nRequires Contents Finder Confirm, and disables both after activation.");
            }

            // InclusionShop
            {
                var inclusionShopRemember = this.Configuration.InclusionShopRememberEnabled;
                if (ImGui.Checkbox("InclusionShopRemember", ref inclusionShopRemember))
                {
                    this.Configuration.InclusionShopRememberEnabled = inclusionShopRemember;
                    this.Configuration.Save();
                }

                IndentedTextColored(this.shadedColor, "Remember the last panel visited on the scrip exchange window.");
            }

            ImGui.EndChild();
        }

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    // ====================================================================================================
    private void DisplayTextButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new TextEntryNode { Enabled = false, Text = "Your text goes here" };
            this.RootFolder.Children.Add(newNode);
            this.Configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
        {
            var io = ImGui.GetIO();
            var zoneRestricted = io.KeyCtrl;
            var createFolder = io.KeyShift;
            var selectNo = io.KeyAlt;

            this.module.CreateTextNode(this.RootFolder, zoneRestricted, createFolder, selectNo);
            this.Configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
        {
            var newNode = new TextFolderNode { Name = "Untitled folder" };
            this.RootFolder.Children.Add(newNode);
            this.Configuration.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("Enter into the input all or part of the text inside a dialog.");
        sb.AppendLine("For example: \"Teleport to \" for the teleport dialog.");
        sb.AppendLine();
        sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
        sb.AppendLine("As such: \"/Teleport to .*? for \\d+(,\\d+)? gil\\?/\"");
        sb.AppendLine("Or simpler: \"/Teleport to .*?/\" (and hope it doesn't match something unexpected)");
        sb.AppendLine();
        sb.AppendLine("If it matches, the yes button (and checkbox if present) will be clicked.");
        sb.AppendLine();
        sb.AppendLine("Right click a line to view options.");
        sb.AppendLine("Double click an entry for quick enable/disable.");
        sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
        sb.AppendLine();
        sb.AppendLine("\"Add last seen as new entry\" button modifiers:");
        sb.AppendLine("   Shift-Click to add to a new or first existing folder with the current zone name, restricted to that zone.");
        sb.AppendLine("   Ctrl-Click to create a entry restricted to the current zone, without a named folder.");
        sb.AppendLine("   Alt-Click to create a \"Select No\" entry instead of \"Select Yes\"");
        sb.AppendLine("   Alt-Click can be combined with Shift/Ctrl-Click.");
        sb.AppendLine();
        sb.AppendLine("Currently supported text addons:");
        sb.AppendLine("  - SelectYesNo");

        ImGui.SameLine();
        ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

        ImGui.PopStyleVar(); // ItemSpacing
    }

    private void DisplayTextNodes()
    {
        var root = this.RootFolder;
        this.TextNodeDragDrop(root);

        if (root.Children.Count == 0)
        {
            root.Children.Add(new TextEntryNode { Enabled = false, Text = "Add some text here!" });
            this.Configuration.Save();
        }

        foreach (var node in root.Children.ToArray())
        {
            this.DisplayTextNode(node, root);
        }
    }

    // ====================================================================================================
    private void DisplayListButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new ListEntryNode { Enabled = false, Text = "Your text goes here" };
            this.ListRootFolder.Children.Add(newNode);
            this.Configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last selected as new entry"))
        {
            var newNode = new ListEntryNode { Enabled = true, Text = this.module.LastSeenListSelection, TargetRestricted = true, TargetText = this.module.LastSeenListTarget };
            this.ListRootFolder.Children.Add(newNode);
            this.Configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
        {
            var newNode = new TextFolderNode { Name = "Untitled folder" };
            this.ListRootFolder.Children.Add(newNode);
            this.Configuration.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("Enter into the input all or part of the text inside a line in a list dialog.");
        sb.AppendLine("For example: \"Purchase a Mini Cactpot ticket\" in the Gold Saucer.");
        sb.AppendLine();
        sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
        sb.AppendLine("As such: \"/Purchase a .*? ticket/\"");
        sb.AppendLine();
        sb.AppendLine("If any line in the list matches, then that line will be chosen.");
        sb.AppendLine();
        sb.AppendLine("Right click a line to view options.");
        sb.AppendLine("Double click an entry for quick enable/disable.");
        sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
        sb.AppendLine();
        sb.AppendLine("Currently supported list addons:");
        sb.AppendLine("  - SelectString");
        sb.AppendLine("  - SelectIconString");

        ImGui.SameLine();
        ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

        ImGui.PopStyleVar(); // ItemSpacing
    }

    private void DisplayListNodes()
    {
        var root = this.ListRootFolder;
        this.TextNodeDragDrop(root);

        if (root.Children.Count == 0)
        {
            root.Children.Add(new ListEntryNode { Enabled = false, Text = "Add some text here!" });
            this.Configuration.Save();
        }

        foreach (var node in root.Children.ToArray())
        {
            this.DisplayTextNode(node, root);
        }
    }

    // ====================================================================================================
    private void DisplayTalkButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new TalkEntryNode { Enabled = false, TargetText = "Your text goes here" };
            this.TalkRootFolder.Children.Add(newNode);
            this.Configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add current target as a new entry"))
        {
            var target = this.dalamudServices.TargetManager.Target;
            var targetName = this.module is null
                ? string.Empty
                : this.module.LastSeenTalkTarget = target != null
                    ? this.module.GetSeStringText(target.Name)
                    : string.Empty;

            var newNode = new TalkEntryNode { Enabled = true, TargetText = targetName };
            this.TalkRootFolder.Children.Add(newNode);
            this.Configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
        {
            var newNode = new TextFolderNode { Name = "Untitled folder" };
            this.TalkRootFolder.Children.Add(newNode);
            this.Configuration.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("Enter into the input all or part of the selected target name while in a talk dialog.");
        sb.AppendLine("For example: \"Moyce\" in the Crystarium.");
        sb.AppendLine();
        sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
        sb.AppendLine("As such: \"/(Moyce|Eirikur)/\"");
        sb.AppendLine();
        sb.AppendLine("To skip your retainers, add the summoning bell.");
        sb.AppendLine();
        sb.AppendLine("Right click a line to view options.");
        sb.AppendLine("Double click an entry for quick enable/disable.");
        sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
        sb.AppendLine();
        sb.AppendLine("Currently supported list addons:");
        sb.AppendLine("  - Talk");

        ImGui.SameLine();
        ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

        ImGui.PopStyleVar(); // ItemSpacing
    }

    private void DisplayTalkNodes()
    {
        var root = this.TalkRootFolder;
        this.TextNodeDragDrop(root);

        if (root.Children.Count == 0)
        {
            root.Children.Add(new TalkEntryNode { Enabled = false, TargetText = "Add some text here!" });
            this.Configuration.Save();
        }

        foreach (var node in root.Children.ToArray())
        {
            this.DisplayTextNode(node, root);
        }
    }

    // ====================================================================================================
    private void DisplayTextNode(INode node, TextFolderNode rootNode)
    {
        if (node is TextFolderNode folderNode)
        {
            this.DisplayFolderNode(folderNode, rootNode);
        }
        else if (node is TextEntryNode textNode)
        {
            this.DisplayTextEntryNode(textNode);
        }
        else if (node is ListEntryNode listNode)
        {
            this.DisplayListEntryNode(listNode);
        }
        else if (node is TalkEntryNode talkNode)
        {
            this.DisplayTalkEntryNode(talkNode);
        }
    }

    private void DisplayTextEntryNode(TextEntryNode node)
    {
        var validRegex = (node.IsTextRegex && node.TextRegex != null) || !node.IsTextRegex;
        var validZone = !node.ZoneRestricted || (node.ZoneIsRegex && node.ZoneRegex != null) || !node.ZoneIsRegex;

        if (!node.Enabled && (!validRegex || !validZone))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        }
        else if (!node.Enabled)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        }
        else if (!validRegex || !validZone)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
        }

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        if (!node.Enabled || !validRegex || !validZone)
        {
            ImGui.PopStyleColor();
        }

        if (!validRegex && !validZone)
        {
            ImGuiEx.TextTooltip("Invalid Text and Zone Regex");
        }
        else if (!validRegex)
        {
            ImGuiEx.TextTooltip("Invalid Text Regex");
        }
        else if (!validZone)
        {
            ImGuiEx.TextTooltip("Invalid Zone Regex");
        }

        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                node.Enabled = !node.Enabled;
                this.Configuration.Save();
                return;
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                var io = ImGui.GetIO();
                if (io.KeyCtrl && io.KeyShift)
                {
                    if (this.Configuration.TryFindParent(node, out var parent))
                    {
                        parent!.Children.Remove(node);
                        this.Configuration.Save();
                    }

                    return;
                }

                ImGui.OpenPopup($"{node.GetHashCode()}-popup");
            }
        }

        this.TextNodePopup(node);
        this.TextNodeDragDrop(node);
    }

    private void DisplayListEntryNode(ListEntryNode node)
    {
        var validRegex = (node.IsTextRegex && node.TextRegex != null) || !node.IsTextRegex;
        var validTarget = !node.TargetRestricted || (node.TargetIsRegex && node.TargetRegex != null) || !node.TargetIsRegex;

        if (!node.Enabled && (!validRegex || !validTarget))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        }
        else if (!node.Enabled)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        }
        else if (!validRegex || !validTarget)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
        }

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        if (!node.Enabled || !validRegex || !validTarget)
        {
            ImGui.PopStyleColor();
        }

        if (!validRegex && !validTarget)
        {
            ImGuiEx.TextTooltip("Invalid Text and Target Regex");
        }
        else if (!validRegex)
        {
            ImGuiEx.TextTooltip("Invalid Text Regex");
        }
        else if (!validTarget)
        {
            ImGuiEx.TextTooltip("Invalid Target Regex");
        }

        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                node.Enabled = !node.Enabled;
                this.Configuration.Save();
                return;
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                var io = ImGui.GetIO();
                if (io.KeyCtrl && io.KeyShift)
                {
                    if (this.Configuration.TryFindParent(node, out var parent))
                    {
                        parent!.Children.Remove(node);
                        this.Configuration.Save();
                    }

                    return;
                }

                ImGui.OpenPopup($"{node.GetHashCode()}-popup");
            }
        }

        this.TextNodePopup(node);
        this.TextNodeDragDrop(node);
    }

    private void DisplayTalkEntryNode(TalkEntryNode node)
    {
        var validTarget = (node.TargetIsRegex && node.TargetRegex != null) || !node.TargetIsRegex;

        if (!node.Enabled && !validTarget)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        }
        else if (!node.Enabled)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        }
        else if (!validTarget)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
        }

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        if (!node.Enabled || !validTarget)
        {
            ImGui.PopStyleColor();
        }

        if (!validTarget)
        {
            ImGuiEx.TextTooltip("Invalid Target Regex");
        }

        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                node.Enabled = !node.Enabled;
                this.Configuration.Save();
                return;
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                var io = ImGui.GetIO();
                if (io.KeyCtrl && io.KeyShift)
                {
                    if (this.Configuration.TryFindParent(node, out var parent))
                    {
                        parent!.Children.Remove(node);
                        this.Configuration.Save();
                    }

                    return;
                }

                ImGui.OpenPopup($"{node.GetHashCode()}-popup");
            }
        }

        this.TextNodePopup(node);
        this.TextNodeDragDrop(node);
    }

    private void DisplayFolderNode(TextFolderNode node, TextFolderNode root)
    {
        var expanded = ImGui.TreeNodeEx($"{node.Name}##{node.GetHashCode()}-tree");

        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                var io = ImGui.GetIO();
                if (io.KeyCtrl && io.KeyShift)
                {
                    if (this.Configuration.TryFindParent(node, out var parent))
                    {
                        parent!.Children.Remove(node);
                        this.Configuration.Save();
                    }

                    return;
                }

                ImGui.OpenPopup($"{node.GetHashCode()}-popup");
            }
        }

        this.TextNodePopup(node, root);
        this.TextNodeDragDrop(node);

        if (expanded)
        {
            foreach (var childNode in node.Children.ToArray())
            {
                this.DisplayTextNode(childNode, root);
            }

            ImGui.TreePop();
        }
    }

    private void TextNodePopup(INode node, TextFolderNode? root = null)
    {
        var style = ImGui.GetStyle();
        var newItemSpacing = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);

        if (ImGui.BeginPopup($"{node.GetHashCode()}-popup"))
        {
            if (node is TextEntryNode entryNode)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                var enabled = entryNode.Enabled;
                if (ImGui.Checkbox("Enabled", ref enabled))
                {
                    entryNode.Enabled = enabled;
                    this.Configuration.Save();
                }

                ImGui.SameLine(100f);
                var isYes = entryNode.IsYes;
                var title = isYes ? "Click Yes" : "Click No";
                if (ImGui.Button(title))
                {
                    entryNode.IsYes = !isYes;
                    this.Configuration.Save();
                }

                var trashAltWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (this.Configuration.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        this.Configuration.Save();
                    }
                }

                var matchText = entryNode.Text;
                if (ImGui.InputText($"##{node.Name}-matchText", ref matchText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    entryNode.Text = matchText;
                    this.Configuration.Save();
                }

                var zoneRestricted = entryNode.ZoneRestricted;
                if (ImGui.Checkbox("Zone Restricted", ref zoneRestricted))
                {
                    entryNode.ZoneRestricted = zoneRestricted;
                    this.Configuration.Save();
                }

                var searchWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.Search);
                var searchPlusWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth);
                if (ImGuiEx.IconButton(FontAwesomeIcon.Search, "Zone List"))
                {
                    this.zoneListWindow.IsOpen = true;
                }

                ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth - searchPlusWidth - newItemSpacing.X);
                if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current zone"))
                {
                    var currentId = this.dalamudServices.ClientState.TerritoryType;
                    if (this.module.TerritoryNames.TryGetValue(currentId, out var zoneName))
                    {
                        entryNode.ZoneText = zoneName;
                        this.Configuration.Save();
                    }
                    else
                    {
                        entryNode.ZoneText = "Could not find name";
                        this.Configuration.Save();
                    }
                }

                ImGui.PopStyleVar(); // ItemSpacing

                var zoneText = entryNode.ZoneText;
                if (ImGui.InputText($"##{node.Name}-zoneText", ref zoneText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    entryNode.ZoneText = zoneText;
                    this.Configuration.Save();
                }
            }

            if (node is ListEntryNode listNode)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                var enabled = listNode.Enabled;
                if (ImGui.Checkbox("Enabled", ref enabled))
                {
                    listNode.Enabled = enabled;
                    this.Configuration.Save();
                }

                var trashAltWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (this.Configuration.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        this.Configuration.Save();
                    }
                }

                var matchText = listNode.Text;
                if (ImGui.InputText($"##{node.Name}-matchText", ref matchText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    listNode.Text = matchText;
                    this.Configuration.Save();
                }

                var targetRestricted = listNode.TargetRestricted;
                if (ImGui.Checkbox("Target Restricted", ref targetRestricted))
                {
                    listNode.TargetRestricted = targetRestricted;
                    this.Configuration.Save();
                }

                var searchPlusWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - searchPlusWidth);
                if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current target"))
                {
                    var target = this.dalamudServices.TargetManager.Target;
                    var name = target?.Name?.TextValue ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        listNode.TargetText = name;
                        this.Configuration.Save();
                    }
                    else
                    {
                        listNode.TargetText = "Could not find target";
                        this.Configuration.Save();
                    }
                }

                ImGui.PopStyleVar(); // ItemSpacing

                var targetText = listNode.TargetText;
                if (ImGui.InputText($"##{node.Name}-targetText", ref targetText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    listNode.TargetText = targetText;
                    this.Configuration.Save();
                }
            }

            if (node is TalkEntryNode talkNode)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                var enabled = talkNode.Enabled;
                if (ImGui.Checkbox("Enabled", ref enabled))
                {
                    talkNode.Enabled = enabled;
                    this.Configuration.Save();
                }

                var trashAltWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (this.Configuration.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        this.Configuration.Save();
                    }
                }

                var searchPlusWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - searchPlusWidth - trashAltWidth - newItemSpacing.X);
                if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current target"))
                {
                    var target = this.dalamudServices.TargetManager.Target;
                    var name = target?.Name?.TextValue ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        talkNode.TargetText = name;
                        this.Configuration.Save();
                    }
                    else
                    {
                        talkNode.TargetText = "Could not find target";
                        this.Configuration.Save();
                    }
                }

                ImGui.PopStyleVar(); // ItemSpacing

                var targetText = talkNode.TargetText;
                if (ImGui.InputText($"##{node.Name}-targetText", ref targetText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    talkNode.TargetText = targetText;
                    this.Configuration.Save();
                }
            }

            if (node is TextFolderNode folderNode)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add entry"))
                {
                    if (root == this.RootFolder)
                    {
                        var newNode = new TextEntryNode { Enabled = false, Text = "Your text goes here" };
                        folderNode.Children.Add(newNode);
                    }
                    else if (root == this.ListRootFolder)
                    {
                        var newNode = new ListEntryNode { Enabled = false, Text = "Your text goes here" };
                        folderNode.Children.Add(newNode);
                    }

                    this.Configuration.Save();
                }

                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
                {
                    if (root == this.RootFolder)
                    {
                        var io = ImGui.GetIO();
                        var zoneRestricted = io.KeyCtrl;
                        var createFolder = io.KeyShift;
                        var selectNo = io.KeyAlt;

                        this.module.CreateTextNode(folderNode, zoneRestricted, createFolder, selectNo);
                        var newNode = new TextEntryNode { Enabled = true, Text = this.module.LastSeenDialogText };
                        folderNode.Children.Add(newNode);
                        this.Configuration.Save();
                    }
                    else if (root == this.ListRootFolder)
                    {
                        var newNode = new ListEntryNode { Enabled = true, Text = this.module.LastSeenListSelection, TargetRestricted = true, TargetText = this.module.LastSeenListTarget };
                        folderNode.Children.Add(newNode);
                        this.Configuration.Save();
                    }
                    else if (root == this.TalkRootFolder)
                    {
                        var newNode = new TalkEntryNode { Enabled = true, TargetText = this.module.LastSeenTalkTarget };
                        folderNode.Children.Add(newNode);
                        this.Configuration.Save();
                    }
                }

                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
                {
                    var newNode = new TextFolderNode { Name = "Untitled folder" };
                    folderNode.Children.Add(newNode);
                    this.Configuration.Save();
                }

                var trashWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);
                ImGui.SameLine(ImGui.GetContentRegionMax().X - trashWidth);
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (this.Configuration.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        this.Configuration.Save();
                    }
                }

                ImGui.PopStyleVar(); // ItemSpacing

                var folderName = folderNode.Name;
                if (ImGui.InputText($"##{node.Name}-rename", ref folderName, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    folderNode.Name = folderName;
                    this.Configuration.Save();
                }
            }

            ImGui.EndPopup();
        }
    }

    private void TextNodeDragDrop(INode node)
    {
        if (node != this.RootFolder && node != this.ListRootFolder && node != this.TalkRootFolder && ImGui.BeginDragDropSource())
        {
            this.draggedNode = node;

            ImGui.Text(node.Name);
            ImGui.SetDragDropPayload("TextNodePayload", nint.Zero, 0);
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload("TextNodePayload");

            bool nullPtr;
            unsafe
            {
                nullPtr = payload.NativePtr == null;
            }

            var targetNode = node;
            if (!nullPtr && payload.IsDelivery() && this.draggedNode != null)
            {
                if (this.Configuration.TryFindParent(this.draggedNode, out var draggedNodeParent))
                {
                    if (targetNode is TextFolderNode targetFolderNode && !ImGui.IsKeyDown(ImGuiKey.ModShift))
                    {
                        draggedNodeParent!.Children.Remove(this.draggedNode);
                        targetFolderNode.Children.Add(this.draggedNode);
                        this.Configuration.Save();
                    }
                    else
                    {
                        if (this.Configuration.TryFindParent(targetNode, out var targetNodeParent))
                        {
                            var targetNodeIndex = targetNodeParent!.Children.IndexOf(targetNode);
                            if (targetNodeParent == draggedNodeParent)
                            {
                                var draggedNodeIndex = targetNodeParent.Children.IndexOf(this.draggedNode);
                                if (draggedNodeIndex < targetNodeIndex)
                                {
                                    targetNodeIndex -= 1;
                                }
                            }

                            draggedNodeParent!.Children.Remove(this.draggedNode);
                            targetNodeParent.Children.Insert(targetNodeIndex, this.draggedNode);
                            this.Configuration.Save();
                        }
                        else
                        {
                            throw new Exception($"Could not find parent of node \"{targetNode.Name}\"");
                        }
                    }
                }
                else
                {
                    throw new Exception($"Could not find parent of node \"{this.draggedNode.Name}\"");
                }

                this.draggedNode = null;
            }

            ImGui.EndDragDropTarget();
        }
    }
}