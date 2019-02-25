﻿/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) 2013-2019 Herbert Aitenbichler

  CNCLib is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  CNCLib is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.
  http://www.gnu.org/licenses/
*/

using System.Windows;

using Framework.Dependency;
using Framework.Wpf.Views;

namespace CNCLib.WpfClient.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MachineView : Window
    {
        public MachineView()
        {
            var vm = Dependency.Resolve<ViewModels.MachineViewModel>();
            DataContext = vm;

            InitializeComponent();

            this.DefaultInitForBaseViewModel();
        }
    }
}