﻿// <copyright file="EventType.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Lib.ClickLib;

/// <summary>
///     Various event types.
/// </summary>
public enum EventType : ushort
{
    Normal = 1,
    NormalMax = 2,
    MouseDown = 3,
    MouseUp = 4,
    MouseMove = 5,
    MouseRollOver = 6,
    MouseRollOut = 7,
    MouseWheel = 8,
    MouseClick = 9,
    MouseDoubleClick = 10,
    MouseMax = 11,
    Input = 12,
    InputKey = 13,
    InputMax = 14,
    Pad = 15,
    PadMax = 16,
    FocusIn = 17,
    FocusOut = 18,
    FocusMax = 19,
    Resize = 20,
    ResizeMax = 21,
    ButtonPress = 22,
    ButtonClick = 23,
    ButtonMax = 24,
    Change = 25,
    ChangeMax = 26,
    SliderChange = 27,
    SliderChangeEnd = 28,
    ListItemPress = 29,
    ListItemUp = 30,
    ListItemRollOver = 31,
    ListItemRollOut = 32,
    ListItemClick = 33,
    ListItemDoubleClick = 34,
    ListIndexChange = 35,
    ListFocusChange = 36,
    ListItemCancel = 37,
    ListItemPickupStart = 38,
    ListItemPickupEnd = 39,
    ListItemExchange = 40,
    ListTreeExpand = 41,
    ListMax = 42,
    DdlListOpen = 43,
    DdlListClose = 44,
    DdDragStart = 45,
    DdDragEnd = 46,
    DdDrop = 47,
    DdDropExchange = 48,
    DdDropNotice = 49,
    DdRollOver = 50,
    DdRollOut = 51,
    DdDropStage = 52,
    DdExecute = 53,
    IconTextRollOver = 54,
    IconTextRollOut = 55,
    IconTextClick = 56,
    DialogueClose = 57,
    DialogueSubmit = 58,
    Timer = 59,
    TimerComplete = 60,
    SimpletweenUpdate = 61,
    SimpletweenComplete = 62,
    SetupAddon = 63,
    UnitBaseOver = 64,
    UnitBaseOut = 65,
    UnitScaleChaneged = 66,
    UnitResolutionScaleChaneged = 67,
    TimelineStatechange = 68,
    WordlinkClick = 69,
    WordlinkRollOver = 70,
    WordlinkRollOut = 71,
    ChangeText = 72,
    ComponentIn = 73,
    ComponentOut = 74,
    ComponentScroll = 75,
    ComponentFocused = 76, // Maybe
}