using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace FluentSlider.Controls
{
    public sealed class FluentSlider : Slider
    {
        public FluentSlider()
        {
            this.DefaultStyleKey = typeof(FluentSlider);
        }

        Rectangle HorizontalTrackRect1;
        Rectangle HorizontalTrackRect2;
        Rectangle HorizontalDecreaseRect;
        Grid HorizontalTemplate;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            HorizontalTrackRect1 = GetTemplateChild(nameof(HorizontalTrackRect1)) as Rectangle;
            HorizontalTrackRect2 = GetTemplateChild(nameof(HorizontalTrackRect2)) as Rectangle;
            HorizontalDecreaseRect = GetTemplateChild(nameof(HorizontalDecreaseRect)) as Rectangle;
            HorizontalTemplate = GetTemplateChild(nameof(HorizontalTemplate)) as Grid;

            if (HorizontalDecreaseRect != null)
            {
                HorizontalDecreaseRect.SizeChanged += HorizontalDecreaseRect_SizeChanged;
            }

            if (HorizontalTemplate != null)
            {
                var visual = ElementCompositionPreview.GetElementVisual(HorizontalTemplate);
                visual.Clip = visual.Compositor.CreateInsetClip(0, -20, 0, 0);
            }
        }

        private void HorizontalDecreaseRect_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (HorizontalTrackRect1 != null && HorizontalTrackRect2 != null)
            {
                var width = HorizontalTemplate.ActualWidth;
                var width1 = HorizontalDecreaseRect.ActualWidth - 37;
                var left2 = HorizontalDecreaseRect.ActualWidth + 53;
                var width2 = width - left2;

                Canvas.SetLeft(HorizontalTrackRect2, left2);
                HorizontalTrackRect1.Width = Math.Max(0, width1);
                HorizontalTrackRect2.Width = Math.Max(0, width2);
            }
        }
    }
}
