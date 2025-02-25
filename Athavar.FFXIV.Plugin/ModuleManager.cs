﻿// <copyright file="ModuleManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using System;
using System.Collections.Generic;
using System.Linq;
using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Manage instances of <see cref="Module" />.
/// </summary>
internal class ModuleManager : IModuleManager
{
    private readonly Dictionary<string, Module> modules = new();

    private readonly Configuration configuration;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModuleManager" /> class.
    /// </summary>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="serviceProvider"><see cref="IServiceProvider" /> added by DI.</param>
    public ModuleManager(Configuration configuration, IServiceProvider serviceProvider)
    {
        this.configuration = configuration;
        this.serviceProvider = serviceProvider;
    }

    public event IModuleManager.ModuleStateChange? StateChange;

    /// <inheritdoc />
    public bool Register<T>()
        where T : Module
    {
        var module = this.serviceProvider.GetRequiredService<T>();
        this.StateChange?.Invoke(module, true);
        return this.modules.TryAdd(module.Name, module);
    }

    /// <inheritdoc />
    public IEnumerable<string> GetModuleNames() => this.modules.Where(m => !m.Value.Hidden).Select(m => m.Key);

    /// <inheritdoc />
    public bool IsEnables(string moduleName) => this.modules.TryGetValue(moduleName, out var mod) && mod.Enabled;

    /// <inheritdoc />
    public void Enable(string moduleName, bool state = true)
    {
        if (this.modules.TryGetValue(moduleName, out var mod))
        {
            mod.Enable(state);
            this.configuration.Save();

            this.StateChange?.Invoke(mod, false);
        }
    }
}