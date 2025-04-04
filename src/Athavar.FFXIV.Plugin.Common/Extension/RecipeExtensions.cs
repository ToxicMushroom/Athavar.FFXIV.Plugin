// <copyright file="RecipeExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Extension;

using Athavar.FFXIV.Plugin.Models;
using Lumina.Excel.Sheets;

public static class RecipeExtensions
{
    public static Job GetJobType(this Recipe recipe)
    {
        Job jobType;
        switch (recipe.CraftType.RowId)
        {
            case 0:
                jobType = Job.Carpenter;
                break;
            case 1:
                jobType = Job.Blacksmith;
                break;
            case 2:
                jobType = Job.Armorer;
                break;
            case 3:
                jobType = Job.Goldsmith;
                break;
            case 4:
                jobType = Job.Leatherworker;
                break;
            case 5:
                jobType = Job.Weaver;
                break;
            case 6:
                jobType = Job.Alchemist;
                break;
            case 7:
                jobType = Job.Culinarian;
                break;
            default:
                jobType = 0;
                break;
        }

        return jobType;
    }
}