// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.OpcodeWizard;

using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.OpcodeWizard.PacketDetection;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddOpcodeWizard(this IServiceCollection services)
    {
        services.AddTransient<OpcodeWizardModule>();
        services.AddSingleton<DetectionProgram>();
        services.AddSingleton<ScannerRegistry>();
        OpcodeWizardConfiguration.AddToDependencyInjection(services);

        services.AddSingleton<IOpcodeManager, OpcodeManager>();
        return services;
    }
}