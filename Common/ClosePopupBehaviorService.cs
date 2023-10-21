using CanvasPractice.Behavior;
using Microsoft.Xaml.Behaviors;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CanvasPractice.Common
{
    public class ClosePopupBehaviorService
    {
        #region CloseOnClick
        public static readonly DependencyProperty CloseOnClickProperty = DependencyProperty.RegisterAttached(
            "CloseOnClick",
            typeof(bool),
            typeof(ClosePopupBehaviorService),
            new PropertyMetadata(default(bool), OnCloseOnClickChanged));

        private static void OnCloseOnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button cb)
            {
                var behaviors = Interaction.GetBehaviors(cb);
                var existingBehavior = behaviors.FirstOrDefault(b => b.GetType() == typeof(ClosePopupBehavior)) as ClosePopupBehavior;

                if ((bool)e.NewValue)
                {
                    behaviors.Add(new ClosePopupBehavior());
                }
                else if ((bool)e.NewValue == false && existingBehavior != null)
                {
                    behaviors.Remove(existingBehavior);
                }
            }
        }

        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static bool GetCloseOnClick(DependencyObject obj) => (bool)obj.GetValue(CloseOnClickProperty);
        public static void SetCloseOnClick(DependencyObject obj, bool value) => obj.SetValue(CloseOnClickProperty, value);
        #endregion
    }
}
