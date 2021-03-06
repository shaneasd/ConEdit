﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Utilities;
using Utilities.UI;

namespace ConversationEditor
{
    internal partial class PluginSelector : Form
    {
        private CheckList<PluginAssembly> m_list;
        private PluginsConfig m_config;

        public PluginSelector()
        {
            InitializeComponent();

            m_list = new CheckList<PluginAssembly>(item => item.FileName);

            this.greyScrollBar1.Scrolled += () => { drawWindow1.Invalidate(true); };
            Resize += (a, b) => m_list.UpdateScrollBar(greyScrollBar1, drawWindow1.Height);
            FontChanged += (a, b) => m_list.Font = Font;
            m_list.Font = Font;
        }

        public static IEnumerable<PluginAssembly> AllPlugins
        {
            get
            {
                DirectoryInfo pluginsFolder = new DirectoryInfo(@".\Plugins");
                foreach (var file in pluginsFolder.EnumerateFiles("*.dll"))
                {
                    var assembly = new PluginAssembly(file.Name);
                    yield return assembly;
                }
            }
        }

        private void PopulatePluginList()
        {
            foreach (var assembly in AllPlugins)
                m_list.AddItem(assembly, !m_config.FilteredAssemblies.Contains(assembly));
            m_list.UpdateScrollBar(greyScrollBar1, drawWindow1.Height);
        }

        public void Initalize(IColorScheme scheme, PluginsConfig config)
        {
            m_config = config;
            Scheme = scheme;
            PopulatePluginList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m_config.SetFilteredAssemblies(m_list.Items.Where(p => !p.Checked).Select(p => p.Element).ToList());
            Close();
        }

        private void drawWindow1_Paint(object sender, PaintEventArgs e)
        {
            m_list.DrawItems(Scheme, e.Graphics, (int)greyScrollBar1.Value);
            e.Graphics.DrawRectangle(Scheme.ControlBorder, 0, 0, drawWindow1.Width - 1, drawWindow1.Height - 1);
        }

        private void drawWindow1_MouseClick(object sender, MouseEventArgs e)
        {
            m_list.MouseClick(e.Location, (int)greyScrollBar1.Value);
            drawWindow1.Invalidate(true);
        }

        private void drawWindow1_MouseDown(object sender, MouseEventArgs e)
        {
            m_list.MouseDown(e.Location, (int)greyScrollBar1.Value);
            drawWindow1.Invalidate(true);
        }

        private void drawWindow1_MouseUp(object sender, MouseEventArgs e)
        {
            m_list.MouseUp();
            drawWindow1.Invalidate(true);
        }

        private void drawWindow1_MouseCaptureChanged(object sender, EventArgs e)
        {
            m_list.MouseCaptureChanged();
            drawWindow1.Invalidate(true);
        }

        private void drawWindow1_MouseMove(object sender, MouseEventArgs e)
        {
            m_list.MouseMove(e.Location, (int)greyScrollBar1.Value);
            drawWindow1.Invalidate(true);
        }

        private IColorScheme m_scheme;
        public IColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                BackColor = value.FormBackground;
                button1.BackColor = value.Background;
                button2.BackColor = value.Background;
                button1.ForeColor = value.Foreground;
                button2.ForeColor = value.Foreground;
                //greyScrollBar1.ColorScheme = value;
                drawWindow1.ColorScheme = value;
            }
        }
    }

    public class CheckList<T>
    {
        public CheckList(Func<T, string> stringSelector)
        {
            m_stringSelector = stringSelector;
        }
        public Font Font { get; set; }
        public class ListElement
        {
            public ListElement(T assembly, bool check)
            {
                Element = assembly;
                Checked = check;
            }
            public T Element { get; private set; }
            public bool Checked { get; set; }
        }

        const int SPACING = 2;
        const int BOX_SIZE = 15;

        public int PerItemHeight => SPACING + BOX_SIZE;

        public IEnumerable<ListElement> Items => m_items;
        private List<ListElement> m_items = new List<ListElement>();
        private Func<T, string> m_stringSelector;

        public void AddItem(T item, bool check)
        {
            m_items.Add(new ListElement(item, check));
        }

        public Rectangle ItemBox(int index, int scroll)
        {
            index -= scroll;
            return new Rectangle(SPACING, checked(1 + index * (BOX_SIZE + SPACING)), BOX_SIZE, BOX_SIZE);
        }

        public void DrawItems(IColorScheme scheme, Graphics g, int scroll)
        {
            for (int index = 0; index < m_items.Count; index++)
            {
                DrawItem(scheme, g, index, scroll);
            }
        }

        private void DrawItem(IColorScheme scheme, Graphics g, int index, int scroll)
        {
            var item = m_items[index];
            bool check = item.Checked;
            var itemBox = ItemBox(index, scroll);
            DefaultBooleanEditor.DrawCheckBox(g, itemBox, check, m_held == item, m_hovered == item);
            var textSize = g.MeasureString(m_stringSelector(item.Element), Font);
            g.DrawString(m_stringSelector(item.Element), Font, scheme.ForegroundBrush, SPACING + BOX_SIZE + SPACING, itemBox.Y + (BOX_SIZE - textSize.Height) / 2);
        }

        public void UpdateScrollBar(GreyScrollBar scrollBar, int height)
        {
            if (m_items.Any())
            {
                float totalHeight = ItemBox(m_items.Count - 1, 0).Bottom;
                scrollBar.PercentageCovered = (height / totalHeight).Clamp(0, 1);
                scrollBar.Maximum = (int)Math.Ceiling((totalHeight - height) / PerItemHeight).Clamp(0, int.MaxValue);
                scrollBar.LargeChange = 1 / scrollBar.Maximum;
                scrollBar.SmallChange = 1 / scrollBar.Maximum;
            }
            else
            {
                scrollBar.Maximum = 0;
                scrollBar.PercentageCovered = 1;
            }
        }

        public void MouseClick(Point location, int scroll)
        {
            for (int index = 0; index < m_items.Count; index++)
            {
                if (ItemBox(index, scroll).Contains(location))
                {
                    m_items[index].Checked = !m_items[index].Checked;
                }
            }
        }

        ListElement m_held = null;
        ListElement m_hovered = null;

        public void MouseCaptureChanged()
        {
            m_held = null;
            m_hovered = null;
        }

        public void MouseUp()
        {
            m_held = null;
        }

        public void MouseDown(Point location, int scroll)
        {
            for (int index = 0; index < m_items.Count; index++)
            {
                if (ItemBox(index, scroll).Contains(location))
                {
                    m_held = m_items[index];
                }
            }
        }

        public void MouseMove(Point location, int scroll)
        {
            m_hovered = null;
            for (int index = 0; index < m_items.Count; index++)
            {
                if (ItemBox(index, scroll).Contains(location))
                {
                    m_hovered = m_items[index];
                }
            }
        }
    }
}
