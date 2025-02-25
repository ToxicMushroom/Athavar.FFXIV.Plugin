// <copyright file="Recipe.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

using Athavar.FFXIV.Plugin.Common.Exceptions;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

public class Recipe
{
    public Recipe(Lumina.Excel.GeneratedSheets.Recipe recipe, ExcelSheet<Item> sheets)
    {
        this.GameRecipe = recipe;
        this.RecipeId = recipe.RowId;
        var lvlTable = recipe.RecipeLevelTable.Value ?? throw new AthavarPluginException();
        this.RecipeLevel = lvlTable.RowId;
        this.Level = lvlTable.ClassJobLevel;
        this.Class = (CraftingClass)recipe.CraftType.Row;

        this.MaxQuality = (lvlTable.Quality * recipe.QualityFactor) / 100;
        this.Progress = ((uint)lvlTable.Difficulty * recipe.DifficultyFactor) / 100;
        this.Durability = (lvlTable.Durability * recipe.DurabilityFactor) / 100;

        this.Expert = recipe.IsExpert;

        this.CraftsmanshipReq = recipe.RequiredCraftsmanship == 0 ? null : recipe.RequiredCraftsmanship;
        this.ControlReq = recipe.RequiredControl == 0 ? null : recipe.RequiredControl;
        this.QualityReq = recipe.RequiredQuality == 0 ? null : recipe.RequiredQuality;

        this.PossibleConditions =
            Enum.GetValues<StepState>()
               .Where(f => (f & (StepState)lvlTable.ConditionsFlag) == f)
               .ToArray();

        this.ProgressDivider = lvlTable.ProgressDivider;
        this.QualityDivider = lvlTable.QualityDivider;
        this.ProgressModifier = lvlTable.ProgressModifier;
        this.QualityModifier = lvlTable.QualityModifier;

        Ingredient[] ingredients = recipe.UnkData5.Select(i =>
        {
            var item = sheets.GetRow((uint)i.ItemIngredient);
            if (item is null || item.RowId == 0)
            {
                return null;
            }

            return new Ingredient(item.RowId, item, item.LevelItem.Row, i.AmountIngredient) { CanBeHq = item.CanBeHq };
        }).Where(i => i is not null).ToArray()!;
        var totalItemLevel = ingredients.Sum(i => i.CanBeHq ? i.Amount * i.ILevel : 0);
        var totalContribution = (this.MaxQuality * recipe.MaterialQualityFactor) / 100;

        for (var index = 0; index < ingredients.Length; index++)
        {
            var ingredient = ingredients[index];
            if (ingredient.CanBeHq)
            {
                ingredient.Quality = (uint)(((float)ingredient.ILevel / totalItemLevel) * totalContribution);
            }
        }

        this.Ingredients = ingredients;
    }

    /// <summary>
    ///     Gets the recipe id.
    /// </summary>
    public uint RecipeId { get; }

    /// <summary>
    ///     Gets the lumina game recipe.
    /// </summary>
    public Lumina.Excel.GeneratedSheets.Recipe GameRecipe { get; }

    /// <summary>
    ///     Gets the lvl.
    /// </summary>
    public int Level { get; }

    /// <summary>
    ///     Gets the rlvl.
    /// </summary>
    public uint RecipeLevel { get; }

    /// <summary>
    ///     Gets the max quality of the recipe.
    /// </summary>
    public uint MaxQuality { get; }

    /// <summary>
    ///     Gets or sets the progress of the recipe.
    /// </summary>
    public uint Progress { get; }

    /// <summary>
    ///     Gets or sets the durability of the recipe.
    /// </summary>
    public int Durability { get; }

    public bool Expert { get; }

    public int? CraftsmanshipReq { get; }

    public int? ControlReq { get; }

    public uint? QualityReq { get; }

    public StepState[] PossibleConditions { get; }

    public byte ProgressDivider { get; }

    public byte QualityDivider { get; }

    public byte ProgressModifier { get; }

    public byte QualityModifier { get; }

    public Ingredient[] Ingredients { get; }

    public CraftingClass Class { get; set; }
}