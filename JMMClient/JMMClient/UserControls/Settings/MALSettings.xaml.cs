﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JMMClient.UserControls
{
	/// <summary>
	/// Interaction logic for MALSettings.xaml
	/// </summary>
	public partial class MALSettings : UserControl
	{
		public MALSettings()
		{
			InitializeComponent();

			btnTest.Click += new RoutedEventHandler(btnTest_Click);		
		}

		void btnTest_Click(object sender, RoutedEventArgs e)
		{
			JMMServerVM.Instance.TestMALLogin();
		}
	}
}
