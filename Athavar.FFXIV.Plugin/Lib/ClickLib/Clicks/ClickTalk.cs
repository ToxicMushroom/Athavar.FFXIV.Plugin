﻿namespace ClickLib.Clicks
{
    using System;
    using FFXIVClientStructs.FFXIV.Client.UI;

    /// <summary>
    /// Addon Talk.
    /// </summary>
    public sealed class ClickTalk : ClickBase<AddonTalk>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClickTalk"/> class.
        /// </summary>
        /// <param name="addon">Addon pointer.</param>
        public ClickTalk(IntPtr addon = default)
            : base(addon)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "Talk";

        /// <summary>
        /// Click the talk dialog.
        /// </summary>
        [ClickName("talk")]
        public unsafe void Click()
        {
            ClickAddonStage(&this.Addon->AtkUnitBase, this.Addon->AtkStage, 0);
        }
    }
}
