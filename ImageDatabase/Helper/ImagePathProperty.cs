using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageDatabase.Helper
{
    public class ImagePath : DependencyObject
    {
       public static readonly DependencyProperty ImagePathProperty =
                                                            DependencyProperty.RegisterAttached(
                                                                    "ImagePath", typeof(string), typeof(ImagePath),
                                                            new PropertyMetadata("")); 
        public static string GetSecurityId(
            DependencyObject d)
        {
            return (string)d.GetValue(ImagePathProperty);
        }
        public static void SetSecurityId(
            DependencyObject d, string value)
        {
            d.SetValue(ImagePathProperty, value);
        }
    }
}
