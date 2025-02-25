// <copyright file="OpcodeWizardConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.Text.Json.Serialization;

public class OpcodeWizardConfiguration : BasicModuleConfig
{
    [JsonPropertyName("Opcodes")]
    public Dictionary<Opcode, ushort> Opcodes { get; } = new();

    [JsonPropertyName("GameVersion")]
    public string GameVersion { get; set; } = string.Empty;

    public bool RemoteUpdate { get; set; }
}