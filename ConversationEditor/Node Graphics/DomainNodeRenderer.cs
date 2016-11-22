using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Utilities;
using Conversation;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;

namespace ConversationEditor
{
    public class DomainNodeRendererFactory : NodeUI.IFactory
    {
        public static DomainNodeRendererFactory Instance { get; } = new DomainNodeRendererFactory();

        public bool WillRender(Id<NodeTypeTemp> nodeType)
        {
            return true;
        }

        public string DisplayName
        {
            get { return "Default Domain Node Renderer"; }
        }

        public INodeGui GetRenderer(ConversationNode n, PointF p, Func<Id<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            return new DomainNodeRenderer(n, p, localizer);
        }

        public Guid Guid { get; } = Guid.Parse("3a724c77-1707-479b-a256-7dc319f229b4");
    }

    internal class DomainNodeRenderer : NodeUI
    {
        private readonly static Font Font = SystemFonts.DefaultFont;
        private readonly static Font BoldFont = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
        private readonly static Pen Thin = new Pen(Brushes.Black, 1);
        private readonly static Pen Thick = new Pen(Brushes.White, 3);

        public DomainNodeRenderer(ConversationNode node, PointF p, Func<Id<LocalizedText>, string> localizer)
            : base(node, p)
        {
            m_titleSection = new TitleSection(node);
            m_outputsSection = new OutputsSection(node);
            m_parametersSection = new ParametersSection(node, localizer);
        }

        private Section m_titleSection;
        private Section m_parametersSection;
        private Section m_outputsSection;

        static GraphicsPath RoundedRectangle(RectangleF notRounded, int radius)
        {
            GraphicsPath result = new GraphicsPath(FillMode.Winding);
            try
            {
                var o = notRounded.Location;
                var w = notRounded.Width;
                var h = notRounded.Height;
                result.AddLine(o.X + w, o.Y + radius, o.X + w, o.Y + h - radius);
                result.AddArc(o.X + w - radius * 2, o.Y + h - radius * 2, radius * 2, radius * 2, 0, 90);
                result.AddLine(o.X + w - radius, o.Y + h, o.X + radius, o.Y + h);
                result.AddArc(o.X, o.Y + h - radius * 2, radius * 2, radius * 2, 90, 90);
                result.AddLine(o.X, o.Y + h - radius, o.X, o.Y + radius);
                result.AddArc(o.X, o.Y, radius * 2, radius * 2, 180, 90);
                result.AddLine(o.X + radius, o.Y, o.X + w - radius, o.Y);
                result.AddArc(o.X + w - radius * 2, o.Y, radius * 2, radius * 2, -90, 90);
                result.CloseFigure();
            }
            catch
            {
                result.Dispose();
                throw;
            }
            return result;
        }

        public abstract class Section
        {
            protected SizeF m_size;
            protected ConversationNode Node;
            Color baseColor = Color.Gray;
            protected Section(ConversationNode node)
            {
                Node = node;

                if (node != null)
                {
                    baseColor = BackgroundColor.TryGet(Node.Data.Config) ?? baseColor;
                    baseColor = Color.FromArgb(baseColor.R, baseColor.G, baseColor.B);
                }
            }

            public void UpdateMeasurement(Graphics g)
            {
                m_size = Measure(g);
            }

            public abstract SizeF Measure(Graphics g);
            public abstract void Draw(Graphics g, PointF location);

            public void Stretch(float width)
            {
                m_size.Width = width;
            }

            protected void DrawChunk(Graphics g, float brightnessFactor, float darknessFactor, PointF location)
            {
                RectangleF area = new RectangleF(location, m_size);
                if (area.Height > 0)
                {
                    using (var theRestGradient = CalculateGradient(baseColor, brightnessFactor, darknessFactor, area))
                    {
                        g.FillRectangle(theRestGradient, area);
                    }
                    g.DrawRectangle(Thin, Rectangle.Round(area));
                }
            }

            public float Height { get { return m_size.Height; } }
            public float Width { get { return m_size.Width; } }
        }

        class TitleSection : Section
        {
            public TitleSection(ConversationNode node) : base(node) { }
            public override SizeF Measure(Graphics g)
            {
                return SizeF.Add(g.MeasureString(Node.Data.Name, BoldFont), new SizeF(4, 4));
            }

            public override void Draw(Graphics g, PointF location)
            {
                float brightnessFactor = 0.75f;
                float darknessFactor = 0;
                DrawChunk(g, brightnessFactor, darknessFactor, location);
                g.DrawString(Node.Data.Name, BoldFont, Brushes.Black, new PointF(location.X + 2, location.Y + 2));
            }
        }

        class OutputsSection : Section
        {
            public OutputsSection(ConversationNode node) : base(node) { }

            IEnumerable<Output> BottomNodes
            {
                get
                {
                    return Node.Data.Connectors.Where(c => c.Definition.Position == ConnectorPosition.Bottom);
                }
            }

            public override SizeF Measure(Graphics g)
            {
                if (BottomNodes.Any(o => GetName(o).Length != 0))
                {
                    float largestWidth = BottomNodes.Max(o => g.MeasureString(GetName(o), Font).Width) + 4;
                    float largestHeight = BottomNodes.Max(o => g.MeasureString(GetName(o), Font).Height) + 4;
                    return new SizeF(largestWidth * BottomNodes.Count(), largestHeight);
                }
                else
                {
                    return SizeF.Empty;
                }
            }

            public override void Draw(Graphics g, PointF location)
            {
                float brightnessFactor = 0.5f;
                float darknessFactor = 0.5f;
                if (BottomNodes.Any())
                {
                    DrawChunk(g, brightnessFactor, darknessFactor, location);

                    var transitions = BottomNodes.ToArray();
                    for (int i = 0; i < transitions.Length; i++)
                    {
                        float per = m_size.Width / (float)BottomNodes.Count();
                        string s = GetName(transitions[i]);
                        SizeF size = g.MeasureString(s, Font);
                        g.DrawString(s, Font, Brushes.Black, new PointF(location.X + per * (i + 0.5f) - size.Width / 2, location.Y + 2));
                    }
                }
            }

            public static string GetName(Output connector)
            {
                if (connector.Definition.Id == DomainIDs.NodeOutputConfigDefinition.Id)
                    return "Config";
                else if (connector.Definition.Id == DomainIDs.NodeOutputParametersDefinition.Id)
                    return "Parameters";
                else if (connector.Definition.Id == DomainIDs.NodeOutputConnectorsDefinition.Id)
                    return "Connectors";
                else
                    return "";
            }
        }

        class ParametersSection : Section
        {
            Func<Id<LocalizedText>, string> m_localizer;

            public ParametersSection(ConversationNode node, Func<Id<LocalizedText>, string> localizer)
                : base(node)
            {
                m_localizer = localizer;
            }

            public float MaxWidth
            {
                get
                {
                    return MaxWidthConfig.TryGet(Node.Data.Config) ?? DomainNodeRenderer.MaxWidth;
                }
            }

            public override SizeF Measure(Graphics g)
            {
                if (Node.Data.Parameters.Any())
                {
                    IEnumerable<SizeF> titleSizes = Node.Data.Parameters.Select(p => g.MeasureString(p.Name + " ", BoldFont, MaxTitleWidth));
                    float headingWidth = titleSizes.Max(s => s.Width + 2);

                    IEnumerable<SizeF> dataSizes = Node.Data.Parameters.Select(p => g.MeasureString(p.DisplayValue(m_localizer), Font, (int)(MaxWidth - headingWidth)));
                    float dataWidth = dataSizes.Max(s => s.Width + 2);

                    float totalHeight = titleSizes.Zip(dataSizes, (a, b) => Math.Max(a.Height, b.Height)).Sum() + 4;
                    float totalWidth = headingWidth + dataWidth + 2;

                    return new SizeF(totalWidth, totalHeight);
                }
                else
                {
                    return SizeF.Empty;
                }
            }

            public override void Draw(Graphics g, PointF location)
            {
                if (Node.Data.Parameters.Any())
                {
                    float brightnessFactor = 0.5f;
                    float darknessFactor = 0.5f;

                    DrawChunk(g, brightnessFactor, darknessFactor, location);
                    PointF renderAt = new PointF(location.X + 2, location.Y + 2);

                    IEnumerable<SizeF> titleSizes = Node.Data.Parameters.Select(p => g.MeasureString(p.Name + " ", BoldFont, MaxTitleWidth));
                    float headingWidth = titleSizes.Max(s => s.Width + 2);

                    foreach (var parameter in Node.Data.Parameters)
                    {
                        var name = parameter.Name;
                        var data = parameter.DisplayValue(m_localizer);

                        SizeF headingSize = g.MeasureString(name + " ", BoldFont, MaxTitleWidth);
                        SizeF dataSize = g.MeasureString(data, Font, (int)MaxWidth - (int)(Math.Ceiling(headingWidth)));

                        g.DrawString(name, BoldFont, Brushes.Black, new RectangleF(renderAt, headingSize));
                        g.DrawString(data, Font, Brushes.Black, new RectangleF(renderAt.Plus(new PointF(headingWidth, 0)), dataSize));
                        renderAt.Y += Math.Max(headingSize.Height, dataSize.Height);
                    }
                }
            }
        }

        protected override void InnerDraw(Graphics g, bool selected)
        {
            using (var nodeShape = NodeShape())
            {
                using (Region clip = new Region(nodeShape))
                {
                    var savedState = g.Save();
                    clip.Intersect(g.Clip);
                    g.Clip = clip;

                    m_titleSection.Draw(g, Area.Location);
                    m_parametersSection.Draw(g, new PointF(Area.X, Area.Y + m_titleSection.Height));
                    m_outputsSection.Draw(g, new PointF(Area.X, Area.Y + m_titleSection.Height + m_parametersSection.Height));

                    g.Restore(savedState);

                    DrawShadow(g);
                    DrawBorder(g, nodeShape, selected);
                }
            }
        }

        private GraphicsPath NodeShape()
        {
            if (Rounded)
            {
                return RoundedRectangle(Area, 5);
            }
            else
            {
                GraphicsPath gp = new GraphicsPath();
                try
                {
                    gp.AddRectangle(Area);
                }
                catch
                {
                    gp.Dispose();
                    throw;
                }
                return gp;
            }
        }

        private bool Rounded
        {
            get
            {
                return RoundedConfig.TryGet(Node.Data.Config) ?? false;
            }
        }

        private static void DrawBorder(Graphics g, GraphicsPath nodeShape, bool selected)
        {
            if (selected)
            {
                g.DrawPath(Thick, nodeShape);
                g.DrawPath(Thin, nodeShape);
            }
            else
            {
                g.DrawPath(Thin, nodeShape);
            }
        }

        private void DrawShadow(Graphics g)
        {
            Color shadowDark = Color.FromArgb(0x40, Color.Black);
            Color shadowLight = Color.FromArgb(0x00, Color.Black);

            const int SHADOW_SIZE = 5; //This has the wrong effect currently

            RectangleF shadowAreaBottom = new RectangleF(Area.X + (Rounded ? 5 : 0), Area.Y + Area.Height, Area.Width - (Rounded ? 5 : 0) - (Rounded ? SHADOW_SIZE : 0), SHADOW_SIZE);
            using (var lgb = new LinearGradientBrush(shadowAreaBottom, shadowDark, shadowLight, LinearGradientMode.Vertical))
            {
                lgb.WrapMode = WrapMode.TileFlipXY;
                g.FillRectangle(lgb, shadowAreaBottom);
            }

            RectangleF shadowAreaSide = new RectangleF(Area.X + Area.Width, Area.Y + (Rounded ? 5 : 0), SHADOW_SIZE, Area.Height - (Rounded ? 5 : 0) - (Rounded ? 5 : 0));
            using (var lgb = new LinearGradientBrush(shadowAreaSide, shadowDark, shadowLight, LinearGradientMode.Horizontal))
            {
                lgb.WrapMode = WrapMode.TileFlipXY;
                g.FillRectangle(lgb, shadowAreaSide);
            }

            RectangleF shadowAreaCorner = new RectangleF(Area.X + Area.Width - 5, Area.Y + Area.Height - 5, SHADOW_SIZE * 2, SHADOW_SIZE * 2);
            using (GraphicsPath path = new GraphicsPath())
            {
                float radius = Rounded ? 2 * SHADOW_SIZE * 2 : SHADOW_SIZE * 2;
                path.AddEllipse(new RectangleF(Area.X + Area.Width - radius / 2 - (Rounded ? SHADOW_SIZE : 0), Area.Y + Area.Height - radius / 2 - (Rounded ? SHADOW_SIZE : 0), radius, radius));
                using (PathGradientBrush pthGrBrush = new PathGradientBrush(path))
                {
                    if (!Rounded)
                    {
                        pthGrBrush.CenterPoint = shadowAreaCorner.Center();
                        pthGrBrush.CenterColor = shadowDark;
                        pthGrBrush.SurroundColors = new Color[] { shadowLight };
                    }
                    else
                    {
                        pthGrBrush.CenterPoint = shadowAreaCorner.Location;
                        pthGrBrush.CenterColor = shadowLight;
                        var cb = new ColorBlend();
                        cb.Colors = new Color[] { shadowLight, shadowDark, Color.Transparent, Color.Transparent };
                        cb.Positions = new float[] { 0, 0.5f, 0.5f, 1 };
                        pthGrBrush.InterpolationColors = cb;
                    }
                    pthGrBrush.WrapMode = WrapMode.TileFlipXY;
                    var testPoints = new[] { new PointF(shadowAreaCorner.Right, shadowAreaCorner.Bottom), shadowAreaCorner.Location };
                    g.Transform.TransformVectors(testPoints);
                    if (Math.Abs(testPoints[0].X - testPoints[1].X) > 1 && Math.Abs(testPoints[0].Y - testPoints[1].Y) > 1)
                        g.FillRectangle(pthGrBrush, new RectangleF(Area.X + Area.Width - (Rounded ? SHADOW_SIZE : 0), Area.Y + Area.Height - (Rounded ? SHADOW_SIZE : 0), Rounded ? SHADOW_SIZE * 2 : SHADOW_SIZE, Rounded ? SHADOW_SIZE * 2 : SHADOW_SIZE));
                }
            }
        }

        private static LinearGradientBrush CalculateGradient(Color baseColor, float brightnessFactor, float darknessFactor, RectangleF area)
        {
            return new LinearGradientBrush(new RectangleF(0, area.Y - (int)(area.Height * (brightnessFactor)), 1, (int)(area.Height * (brightnessFactor + darknessFactor + 1))), Color.White, baseColor, LinearGradientMode.Vertical);
        }

        private const int MaxTitleWidth = 200;
        private const int MaxWidth = 500;

        /// <summary>
        /// Increase the area of the node to include all the text
        /// </summary>
        protected override SizeF CalculateArea(Graphics g)
        {
            var sections = new Section[] { m_titleSection, m_outputsSection, m_parametersSection };

            foreach (var section in sections)
                section.UpdateMeasurement(g);

            float maxWidth = sections.Max(s => s.Width);

            foreach (var section in sections)
                section.Stretch(maxWidth);

            return new SizeF(maxWidth, sections.Sum(s => s.Height));
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
