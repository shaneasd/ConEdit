﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;
using Utilities;
using Utilities.UI;

namespace ConversationEditor
{
    public class DefaultDecimalEditorFactory : IParameterEditorFactory
    {
        public static Guid StaticId { get; } = Guid.Parse("846c1908-4e19-4b11-bbb3-ea40b58ef72a");
        public bool WillEdit(ParameterType type, WillEdit queries)
        {
            return queries.IsDecimal(type);
        }

        public string Name => "Default Decimal Editor";

        public Guid Guid => StaticId;

        public IParameterEditor Make(IColorScheme scheme)
        {
            return new DefaultDecimalEditor(scheme);
        }
    }

    internal partial class DefaultDecimalEditor : UserControl, IParameterEditor
    {
        MyNumericUpDown<decimal> m_numericUpDown;

        public DefaultDecimalEditor(IColorScheme scheme) : this()
        {
            Scheme = scheme;
        }

        public DefaultDecimalEditor()
        {
            InitializeComponent();

            m_numericUpDown = new MyNumericUpDown<decimal>(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), true, Fonts.Default);
            m_numericUpDown.RequestedAreaChanged += () =>
            {
                MinimumSize = new Size(0, (int)m_numericUpDown.RequestedArea.Height);
                Size = m_numericUpDown.RequestedArea.ToSize();
            };
            m_numericUpDown.SetupCallbacks(drawWindow1);
        }

        IDecimalParameter m_parameter;
        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IDecimalParameter;
            m_numericUpDown.Minimum = () => m_parameter.Min;
            m_numericUpDown.Maximum = () => m_parameter.Max;
            m_numericUpDown.Value = m_parameter.Value;
        }

        public Control AsControl => this;

        public UpdateParameterData UpdateParameterAction()
        {
            return m_parameter.SetValueAction(m_numericUpDown.Value);
        }

        public string IsValid()
        {
            if (m_numericUpDown.Value > m_parameter.Max)
                return "Entered value is greater than maximum allowed value";
            else if (m_numericUpDown.Value < m_parameter.Min)
                return "Entered value is less than minimum allowed value";
            else
                return null;
        }

        public event Action Ok { add { } remove { } }

        IColorScheme m_scheme;
        public IColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                m_numericUpDown.Colors.BorderPen = value.ControlBorder;
                //drawWindow1.ColorScheme = value;
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_numericUpDown.Dispose();
            }
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
