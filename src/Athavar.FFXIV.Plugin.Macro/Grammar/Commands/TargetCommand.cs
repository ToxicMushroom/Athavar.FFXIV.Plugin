﻿// <copyright file="TargetCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;
using Dalamud.Logging;

/// <summary>
///     The /target command.
/// </summary>
[MacroCommand("target", null, "Target anyone and anything that can be selected.", new[] { "wait" }, new[] { "/target Eirikur", "/target Moyce" }, RequireLogin = true)]
internal class TargetCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/target\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string targetName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TargetCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="targetName">Target name.</param>
    /// <param name="waitMod">Wait value.</param>
    private TargetCommand(string text, string targetName, WaitModifier waitMod)
        : base(text, waitMod)
        => this.targetName = targetName.ToLowerInvariant();

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static TargetCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var nameValue = ExtractAndUnquote(match, "name");

        return new TargetCommand(text, nameValue, waitModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        var target = DalamudServices.ObjectTable.FirstOrDefault(obj => obj.Name.TextValue.ToLowerInvariant() == this.targetName);

        if (target == default)
        {
            throw new MacroCommandError("Could not find target");
        }

        DalamudServices.TargetManager.SetTarget(target);

        await this.PerformWait(token);
    }
}