// <copyright file="IFrameworkManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

using Dalamud.Game;

public interface IFrameworkManager
{
    void Subscribe(Framework.OnUpdateDelegate updateDelegate);

    void Unsubscribe(Framework.OnUpdateDelegate updateDelegate);
}