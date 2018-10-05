﻿////////////////////////////////////////////////////////
/*
  This file is part of CNCLib - A library for stepper motors.

  Copyright (c) 2013-2018 Herbert Aitenbichler

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

using System.Threading.Tasks;
using System.Windows.Controls;
using CNCLib.Wpf.ViewModels;
using Framework.Tools.Dependency;
using Framework.Wpf.View;

namespace CNCLib.Wpf.Views
{
    /// <summary>
    /// Interaction logic for SetupPage.xaml
    /// </summary>
    public partial class SetupPage : Page
    {
        public SetupPage()
        {
            var vm = Dependency.Resolve<SetupWindowViewModel>();
            DataContext = vm;

            InitializeComponent();

            this.DefaulInitForBaseViewModel();
/*
            RoutedEventHandler loaded =null;
			loaded = new RoutedEventHandler(async (object v, RoutedEventArgs e) =>
			{
				var vmm = DataContext as BaseViewModel;
				await vmm.Loaded();
				((SetupPage)e.Source).Loaded -= loaded;
			});

			Loaded += loaded;
*/
            if (vm.EditMachine == null)
            {
                vm.EditMachine = mId =>
                {
                    var dlg = new MachineView();
                    if (dlg.DataContext is MachineViewModel vmdlg)
                    {
                        Task.Run(() => { vmdlg.LoadMachine(mId).ConfigureAwait(false).GetAwaiter().GetResult(); }).Wait();
                        dlg.ShowDialog();
                    }
                };
            }

            if (vm.ShowEeprom == null)
            {
                vm.ShowEeprom = () =>
                {
                    var dlg   = new EepromView();
                    var vmdlg = dlg.DataContext as EepromViewModel;
                    dlg.ShowDialog();
                };
            }

            if (vm.EditJoystick == null)
            {
                vm.EditJoystick = () =>
                {
                    var dlg   = new JoystickView();
                    var vmdlg = dlg.DataContext as JoystickView;
                    dlg.ShowDialog();
                };
            }
        }
    }
}