﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FlyMasterSyncGui.FormsUtils
{
    class MultiGroupTemplateSelector : DataTemplateSelector
    {
        public DataTemplate InnerTemplate { get; set; }
        public DataTemplate OuterTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ContentPresenter cp = container as ContentPresenter;
            if (cp != null)
            {
                CollectionViewGroup cvg = cp.Content as CollectionViewGroup;
                if (cvg.Items.Count > 0)
                {
                    if (cvg.Items[0].GetType().Name=="CollectionViewGroupInternal")
                    {
                        return OuterTemplate;
                    }
                    else return InnerTemplate;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
