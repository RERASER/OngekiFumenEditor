﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OngekiFumenEditor.Modules.FumenPreviewer.Graphics.Drawing
{
    public interface IPerfomenceMonitor
    {
        public interface IRenderPerformenceStatisticsData
        {
            public double AveSpendTicks { get; }
            public double MostSpendTicks { get; }
            public int AveDrawCall { get; }
        }

        public interface IDrawingPerformenceStatisticsData
        {
            public record PerformenceItem(string Name, double AveSpendTicks, int AveDrawCall);
            public IEnumerable<PerformenceItem> PerformenceRanks { get; }
            public double AveSpendTicks { get; }
            public double MostSpendTicks { get; }
        }

        void OnBeforeRender();
        void OnBeginDrawing(IDrawing drawing);
        void OnBeginTargetDrawing(IDrawingTarget drawing);

        void CountDrawCall(IDrawing drawing);

        void OnAfterTargetDrawing(IDrawingTarget drawing);
        void OnAfterDrawing(IDrawing drawing);
        void OnAfterRender();

        IDrawingPerformenceStatisticsData GetDrawingPerformenceData();
        IDrawingPerformenceStatisticsData GetDrawingTargetPerformenceData();
        IRenderPerformenceStatisticsData GetRenderPerformenceData();

        void Clear();
    }
}