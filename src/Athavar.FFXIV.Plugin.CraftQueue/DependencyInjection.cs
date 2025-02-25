// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCraftQueueModule(this IServiceCollection services)
    {
        services.AddSingleton<CraftQueueModule>();
        services.AddSingleton<ICraftQueueTab, CraftQueueTab>();
        services.AddSingleton<CraftQueueData>();
        services.AddSingleton<CraftQueue>();

        return services;
    }
}