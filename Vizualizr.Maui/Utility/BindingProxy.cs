using Microsoft.Maui.Controls;

namespace Vizualizr.Utility
{
    public class BindingProxy : BindableObject
    {
        public static readonly BindableProperty DataProperty =
            BindableProperty.Create(nameof(Data), typeof(object), typeof(BindingProxy), null);

        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
    }
}
