// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Instancinator;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInstancinatorModule(this IServiceCollection services)
    {
        services.AddSingleton<InstancinatorModule>();
        services.AddSingleton<InstancinatorWindow>();
        services.AddSingleton<IInstancinatorTab, InstancinatorTab>();

        return services;
    }
}