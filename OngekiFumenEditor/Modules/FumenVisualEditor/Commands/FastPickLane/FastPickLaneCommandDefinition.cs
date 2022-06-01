﻿using Gemini.Framework.Commands;
using OngekiFumenEditor.Base.OngekiObjects.Lane;
using OngekiFumenEditor.Base.OngekiObjects.Wall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OngekiFumenEditor.Modules.FumenVisualEditor.Commands.FastPickLane
{
    public abstract class FastPickLaneCommandDefinition<T> : CommandDefinition
    {
        public override string Name => $"Editor.FastPickLane_{typeof(T).Name}";

        public override string Text => $"编辑器快速选择轨道({typeof(T).Name})";

        public override string ToolTip => Text;
    }

    [CommandDefinition]
    public class FastPickWallLeftLaneCommandDefinition : FastPickLaneCommandDefinition<WallLeftStart>
    {
        [Export]
        public static CommandKeyboardShortcut KeyGesture = new CommandKeyboardShortcut<FastPickWallLeftLaneCommandDefinition>(new(Key.OemTilde, ModifierKeys.Alt));
    }

    [CommandDefinition]
    public class FastPickRightLaneCommandDefinition : FastPickLaneCommandDefinition<LaneRightStart>
    {
        [Export]
        public static CommandKeyboardShortcut KeyGesture = new CommandKeyboardShortcut<FastPickRightLaneCommandDefinition>(new(Key.D1, ModifierKeys.Alt));
    }

    [CommandDefinition]
    public class FastPickCenterLaneCommandDefinition : FastPickLaneCommandDefinition<LaneCenterStart>
    {
        [Export]
        public static CommandKeyboardShortcut KeyGesture = new CommandKeyboardShortcut<FastPickCenterLaneCommandDefinition>(new(Key.D2, ModifierKeys.Alt));
    }

    [CommandDefinition]
    public class FastPickLeftLaneCommandDefinition : FastPickLaneCommandDefinition<LaneLeftStart>
    {
        [Export]
        public static CommandKeyboardShortcut KeyGesture = new CommandKeyboardShortcut<FastPickLeftLaneCommandDefinition>(new(Key.D3, ModifierKeys.Alt));
    }

    [CommandDefinition]
    public class FastPickWallRightLaneCommandDefinition : FastPickLaneCommandDefinition<WallRightStart>
    {
        [Export]
        public static CommandKeyboardShortcut KeyGesture = new CommandKeyboardShortcut<FastPickWallRightLaneCommandDefinition>(new(Key.D4, ModifierKeys.Alt));
    }
}
