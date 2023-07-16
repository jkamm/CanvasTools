using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Undo.Actions;

namespace CanvasTools.MetaHopper
{
    //This code comes directly from Andrew Heumann's Metahopper plugin: https://www.food4rhino.com/en/app/metahopper
    //used with permission
    internal class MH_ButtonObject_Attributes : GH_ComponentAttributes
    {
        private bool mouseOver;

        private bool mouseDown;

        private RectangleF buttonArea;

        private RectangleF textArea;

        public MH_ButtonObject_Attributes(MH_SelectButtonComponent owner)
            : base(owner)
        {
            mouseOver = false;
            mouseDown = false;
        }

        protected override void Layout()
        {
            Bounds = RectangleF.Empty;
            base.Layout();
            buttonArea = new RectangleF(Bounds.Left, Bounds.Bottom, Bounds.Width, 20f);
            textArea = buttonArea;
            Bounds = RectangleF.Union(Bounds, buttonArea);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);
            if (channel != GH_CanvasChannel.Objects)
            {
                return;
            }
            GH_PaletteStyle impliedStyle = GH_CapsuleRenderEngine.GetImpliedStyle(GH_Palette.Black, Selected, base.Owner.Locked, hidden: true);
            GH_Capsule gH_Capsule = GH_Capsule.CreateTextCapsule(buttonArea, textArea, GH_Palette.Black, "Select", GH_FontServer.Small, 1, 9);
            gH_Capsule.RenderEngine.RenderBackground(graphics, canvas.Viewport.Zoom, impliedStyle);
            if (!mouseDown)
            {
                gH_Capsule.RenderEngine.RenderHighlight(graphics);
            }
            gH_Capsule.RenderEngine.RenderOutlines(graphics, canvas.Viewport.Zoom, impliedStyle);
            if (mouseOver)
            {
                gH_Capsule.RenderEngine.RenderBackground_Alternative(graphics, Color.FromArgb(50, Color.Blue), drawAlphaGrid: false);
                MH_SelectButtonComponent mH_SelectButtonComponent = base.Owner as MH_SelectButtonComponent;
                Pen pen = new Pen(Color.Blue, 4f);
                foreach (IGH_DocumentObject item in mH_SelectButtonComponent.ActiveObjects.Union(mH_SelectButtonComponent.InactiveObjects))
                {
                    RectangleF bounds = item.Attributes.Bounds;
                    bounds.Inflate(4f, 4f);
                    graphics.DrawRectangle(pen, GH_Convert.ToRectangle(bounds));
                }
            }
            gH_Capsule.RenderEngine.RenderText(graphics, Color.White);
            gH_Capsule.Dispose();
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Right && sender.Viewport.Zoom >= 0.5f && buttonArea.Contains(e.CanvasLocation))
            {
                mouseDown = true;
                MH_SelectButtonComponent mH_SelectButtonComponent = base.Owner as MH_SelectButtonComponent;
                mH_SelectButtonComponent.SelectSelected();
                return GH_ObjectResponse.Capture;
            }
            if (e.Button == MouseButtons.Left && sender.Viewport.Zoom >= 0.5f && buttonArea.Contains(e.CanvasLocation))
            {
                mouseDown = true;
                base.Owner.RecordUndoEvent("Update Selection", new GH_GenericObjectAction(base.Owner));
                MH_SelectButtonComponent mH_SelectButtonComponent2 = base.Owner as MH_SelectButtonComponent;
                mH_SelectButtonComponent2.UpdateSelection();
                base.Owner.ExpireSolution(recompute: true);
                return GH_ObjectResponse.Capture;
            }
            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (!buttonArea.Contains(e.CanvasLocation))
            {
                mouseOver = false;
            }
            if (mouseDown)
            {
                mouseDown = false;
                sender.Invalidate();
                return GH_ObjectResponse.Release;
            }
            return base.RespondToMouseUp(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            Point point = GH_Convert.ToPoint(e.CanvasLocation);
            if (e.Button != 0)
            {
                return base.RespondToMouseMove(sender, e);
            }
            if (buttonArea.Contains(point))
            {
                if (!mouseOver)
                {
                    mouseOver = true;
                    sender.Invalidate();
                }
                return GH_ObjectResponse.Capture;
            }
            if (mouseOver)
            {
                mouseOver = false;
                sender.Invalidate();
            }
            return GH_ObjectResponse.Release;
        }
    }
}