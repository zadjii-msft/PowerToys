// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.CodeDom;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Settings.Ui.VNext.ViewModels;

namespace Settings.Ui.VNext.Converters
{
    public class ModuleItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TextTemplate { get; set; }

        public DataTemplate ButtonTemplate { get; set; }

        public DataTemplate ShortcutTemplate { get; set; }

        public DataTemplate KBMTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            throw new NotImplementedException();
            /*switch (item)
            {
                case DashboardModuleButtonItem: return ButtonTemplate;
                case DashboardModuleShortcutItem: return ShortcutTemplate;
                case DashboardModuleTextItem: return TextTemplate;
                case DashboardModuleKBMItem: return KBMTemplate;
                default: return TextTemplate;
            }*/
        }
    }
}
