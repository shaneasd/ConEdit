using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;

    public class EditableUIFactory : NodeUI.IFactory
    {
        public static EditableUIFactory Instance { get; } = new EditableUIFactory();

        public bool WillRender(Id<NodeTypeTemp> nodeType)
        {
            return true;
        }

        public string DisplayName
        {
            get { return "Default Conversation Node Renderer"; }
        }

        public INodeGui GetRenderer(ConversationNode n, PointF p, Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            return new EditableUI(n, p, localizer);
        }

        public Guid Guid { get; } = Guid.Parse("2cbbf5fa-4e42-4670-9c10-c3578a2400eb");
    }

    public class EditableUI : NodeUI
    {
        public static Font Font { get; } = SystemFonts.DefaultFont;
        public static Font BoldFont { get; } = new Font(SystemFonts.DefaultFont, FontStyle.Bold);
        public static Pen Thin { get; } = new Pen(Brushes.Black, 1);
        public static Pen Thick { get; } = new Pen(Brushes.White, 3);

        protected virtual bool ShouldRender(IParameter p)
        {
            return true;
        }

        public EditableUI(ConversationNode node, PointF p, Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localizer)
            : base(node, p)
        {
            m_titleSection = new TitleSection(node);
            m_descriptionSection = null; //TODO: Do we want the description section to 
            //m_descriptionSection = new DescriptionSection(node);
            m_outputsSection = new OutputsSection(node);
            m_parametersSection = new ParametersSection(node, localizer, ShouldRender);
            m_rounded = RoundedConfig.TryGet(Node.Data.Config) ?? false;
        }

        private Section m_titleSection;
        private Section m_descriptionSection;
        private Section m_parametersSection;
        private Section m_outputsSection;
        private bool m_rounded;

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
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        protected abstract class Section
        {
            private SizeF m_size;
            protected ConversationNode Node { get; private set; }

            protected Section(ConversationNode node)
            {
                Node = node;
            }

            private Color baseColor
            {
                get
                {
                    if (Node != null)
                    {
                        Color result;
                        result = BackgroundColor.TryGet(Node.Data.Config) ?? Color.Gray;
                        result = Color.FromArgb(result.R, result.G, result.B);
                        return result;
                    }
                    return Color.Gray;
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

        protected class TitleSection : Section
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

        protected class DescriptionSection : Section
        {
            public DescriptionSection(ConversationNode node) : base(node) { }

            public override SizeF Measure(Graphics g)
            {
                return SizeF.Add(g.MeasureString(Node.Data.Description, BoldFont), new SizeF(4, 4));
            }

            public override void Draw(Graphics g, PointF location)
            {
                float brightnessFactor = 0.75f;
                float darknessFactor = 0;
                DrawChunk(g, brightnessFactor, darknessFactor, location);
                g.DrawString(Node.Data.Description, BoldFont, Brushes.Black, new PointF(location.X + 2, location.Y + 2));
            }
        }

        protected class OutputsSection : Section
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
                        float per = Width / (float)BottomNodes.Count();
                        string s = GetName(transitions[i]);
                        SizeF size = g.MeasureString(s, Font);
                        g.DrawString(s, Font, Brushes.Black, new PointF(location.X + per * (i + 0.5f) - size.Width / 2, location.Y + 2));
                    }
                }
            }

            public static string GetName(Output connector)
            {
                return connector.GetName();
                //var name = connector.Parameters.Where(p => p.Id == ConnectorDefinitionData.OUTPUT_NAME).Select(p => p as IStringParameter).Select(p => p.Value).SingleOrDefault() ?? "";
                //return name;
            }
        }

        protected class ParametersSection : Section
        {
            Func<Id<LocalizedStringType>, Id<LocalizedText>, string> m_localizer;
            Func<IParameter, bool> ShouldRender;
            private float m_maxWidth;

            public ParametersSection(ConversationNode node, Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localizer, Func<IParameter, bool> shouldRender)
                : base(node)
            {
                m_localizer = localizer;
                ShouldRender = shouldRender;
                m_maxWidth = MaxWidthConfig.TryGet(Node.Data.Config) ?? EditableUI.MaxWidth;
            }

            public float MaxWidth
            {
                get
                {
                    float result = MaxWidthConfig.TryGet(Node.Data.Config) ?? EditableUI.MaxWidth;
                    //TODO: Profiling of a test in which I scrolled around a large conversation a bunch
                    //      idicated that 8% of time was spent querying MaxWidth. As far as I can see,
                    //      since this is a property of the nodes config it should not change unless the
                    //      domain changes and thus the conversation is reloaded (or at the very least
                    //      the open document is changed). As such I'm reasonably confident we can just
                    //      cache it in the constructor. Once this check has been around for a while to
                    //      test this assertion this property can just return m_maxWidth. 19/04/2017
                    if (Math.Abs(m_maxWidth - result) > 1)
                    {
                        throw new Exception("Optimization failure in ParameterSection.MaxWidth");
                    }
                    return result;
                }
            }

            private IEnumerable<IParameter> ParametersToRender
            {
                get
                {
                    return Node.Data.Parameters.Where(ShouldRender);
                }
            }

            public override SizeF Measure(Graphics g)
            {
                if (ParametersToRender.Any())
                {
                    IEnumerable<SizeF> titleSizes = ParametersToRender.Select(p => g.MeasureString(p.Name + " ", BoldFont, MaxTitleWidth));
                    float headingWidth = titleSizes.Max(s => s.Width + 2);

                    IEnumerable<SizeF> dataSizes = ParametersToRender.Select(p => g.MeasureString(p.DisplayValue(m_localizer), Font, (int)(MaxWidth - headingWidth)));
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
                if (ParametersToRender.Any())
                {
                    float brightnessFactor = 0.5f;
                    float darknessFactor = 0.5f;

                    DrawChunk(g, brightnessFactor, darknessFactor, location);
                    PointF renderAt = new PointF(location.X + 2, location.Y + 2);

                    IEnumerable<SizeF> titleSizes = ParametersToRender.Select(p => g.MeasureString(p.Name + " ", BoldFont, MaxTitleWidth));
                    float headingWidth = titleSizes.Max(s => s.Width + 2);

                    foreach (var parameter in ParametersToRender.OrderBy(p => p.Name))
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

                    float descriptionHeight = m_descriptionSection?.Height ?? 0;

                    m_titleSection.Draw(g, Area.Location);
                    m_descriptionSection?.Draw(g, new PointF(Area.X, Area.Y + m_titleSection.Height));
                    m_parametersSection.Draw(g, new PointF(Area.X, Area.Y + m_titleSection.Height + descriptionHeight));
                    m_outputsSection.Draw(g, new PointF(Area.X, Area.Y + m_titleSection.Height + descriptionHeight + m_parametersSection.Height));

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
                    return gp;
                }
                catch
                {
                    gp.Dispose();
                    throw;
                }
            }
        }

        private bool Rounded
        {
            get
            {
                bool result = RoundedConfig.TryGet(Node.Data.Config) ?? false;
                //TODO: Profiling of a test in which I scrolled around a large conversation a bunch
                //      idicated that 9% of time was spent querying Rounded. As far as I can see,
                //      since this is a property of the nodes config it should not change unless the
                //      domain changes and thus the conversation is reloaded (or at the very least
                //      the open document is changed). As such I'm reasonably confident we can just
                //      cache it in the constructor. Once this check has been around for a while to
                //      test this assertion this property can just return m_maxWidth. 19/04/2017
                if (m_rounded != result)
                {
                    throw new Exception("Optimization failure in ParameterSection.Rounded");
                }
                return result;
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
            Section[] sections = m_descriptionSection != null
                                 ? new Section[] { m_titleSection, m_descriptionSection, m_outputsSection, m_parametersSection }
                                 : new Section[] { m_titleSection, m_outputsSection, m_parametersSection };

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
