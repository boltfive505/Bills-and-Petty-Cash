using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Petty_Cash.Classes
{
    public static class VisualHelper
    {
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject == null)
                yield break;

            int totalChildrenAmount = VisualTreeHelper.GetChildrenCount(dependencyObject);
            for (int childIndex = 0; childIndex < totalChildrenAmount; childIndex++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, childIndex);
                if (child is T)
                {
                    yield return (T)child;
                }

                foreach (T deeperChild in FindVisualChildren<T>(child))
                {
                    yield return deeperChild;
                }
            }
        }
    }
}
