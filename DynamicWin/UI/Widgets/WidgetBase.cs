﻿using DynamicWin.Main;
using DynamicWin.Resources;
using DynamicWin.UI.UIElements;
using DynamicWin.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicWin.UI.Widgets
{
    public class WidgetBase : UIObject
    {
        public bool isEditMode = true;

        protected bool isSmallWidget = false;
        public bool IsSmallWidget { get { return isSmallWidget; } }

        //DWText widgetName;

        public WidgetBase(UIObject? parent, Vec2 position, UIAlignment alignment = UIAlignment.TopCenter) : base(parent, position, Vec2.zero, alignment)
        {
            Size = GetWidgetSize();

            var objs = InitializeWidget();
            objs.ForEach(obj => AddLocalObject(obj));

            roundRadius = 15f;

            /*widgetName = new DWText(this, GetWidgetName(), Vec2.zero, UIAlignment.Center)
            {
                Font = Res.InterBold,
                textSize = 20
            };*/
        }

        public Vec2 GetWidgetSize() { return new Vec2(GetWidgetWidth(), GetWidgetHeight()); }

        protected virtual float GetWidgetHeight() { return 100; }
        protected virtual float GetWidgetWidth() { return 200; }

        public List<UIObject> InitializeWidget()
        {
            return new List<UIObject>();
        }

        public override void Draw(SKCanvas canvas)
        {
            Size = GetWidgetSize();

            /*if (!isEditMode || isSmallWidget)
            {
                drawLocalObjects = true; */ 
                DrawWidget(canvas);
/*            }
            else
            {
                widgetName.blurAmount = GetBlur();
                widgetName.DrawCall(canvas);
                drawLocalObjects = false;
            }*/

            var paint = GetPaint();

            if (!IsSmallWidget)
            {
                var bPaint = GetPaint();
                bPaint.ImageFilter = SKImageFilter.CreateBlur(100, 100);
                bPaint.BlendMode = SKBlendMode.SrcOver;
                bPaint.Color = Col.White.Override(a: 0.4f).Value();

                int canvasSave = canvas.Save();
                canvas.ClipRoundRect(GetRect(), antialias: true);
                canvas.DrawCircle(RendererMain.CursorPosition.X + 12.5f, RendererMain.CursorPosition.Y + 20, 35, bPaint);

                canvas.RestoreToCount(canvasSave);
            }

            /*if (isEditMode && !isSmallWidget)
            {
                paint.IsStroke = true;
                paint.StrokeCap = SKStrokeCap.Round;
                paint.StrokeJoin = SKStrokeJoin.Round;
                paint.StrokeWidth = 2f;

                float expand = 10;
                var brect = SKRect.Create(Position.X - expand / 2, Position.Y - expand/2, Size.X + expand, Size.Y + expand);
                var broundRect = new SKRoundRect(brect, roundRadius);

                int noClip = canvas.Save();

                //if(!RendererMain.Instance.MainIsland.IsHovering)
                //    canvas.ClipRect(clipRect, SKClipOperation.Difference);

                paint.Color = SKColors.DimGray;
                canvas.DrawRoundRect(broundRect, paint);

                canvas.RestoreToCount(noClip);
            }*/
        }

        public virtual void DrawWidget(SKCanvas canvas) { }
    }
}
