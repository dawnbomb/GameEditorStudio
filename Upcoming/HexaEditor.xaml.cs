﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameEditorStudio
{
    /// <summary>
    /// Interaction logic for HexaEditor.xaml
    /// </summary>
    public partial class HexaEditor : UserControl
    {
        public HexaEditor()
        {
            InitializeComponent();
        
            string filePath = @"D:\Crystal Editor\Release\Other\HexB\skill.bin";
            if (System.IO.File.Exists(filePath))
            {
                HexControl.FileName = filePath;
            }
        }
    }
}
