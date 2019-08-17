using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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

        long? orientationEventToken = null;
        Ellipse horizontalThumbContent;
        ScalarKeyFrameAnimation pressedThumbAnimation;
        SpringScalarNaturalMotionAnimation releasedThumbAnimation;

        Rectangle HorizontalTrackRect1;
        Rectangle HorizontalTrackRect2;
        Rectangle HorizontalDecreaseRect;
        Grid HorizontalTemplate;
        Thumb HorizontalThumb;


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            HorizontalTrackRect1 = GetTemplateChild(nameof(HorizontalTrackRect1)) as Rectangle;
            HorizontalTrackRect2 = GetTemplateChild(nameof(HorizontalTrackRect2)) as Rectangle;
            HorizontalDecreaseRect = GetTemplateChild(nameof(HorizontalDecreaseRect)) as Rectangle;
            HorizontalTemplate = GetTemplateChild(nameof(HorizontalTemplate)) as Grid;
            HorizontalThumb = GetTemplateChild(nameof(HorizontalThumb)) as Thumb;

            if (HorizontalDecreaseRect != null)
            {
                HorizontalDecreaseRect.SizeChanged += HorizontalDecreaseRect_SizeChanged;
            }

            if (HorizontalTemplate != null)
            {
                var visual = ElementCompositionPreview.GetElementVisual(HorizontalTemplate);
                visual.Clip = visual.Compositor.CreateInsetClip(0, -20, 0, 0);
            }

            if (HorizontalThumb != null)
            {
                horizontalThumbContent = VisualTreeHelper.GetChild(HorizontalThumb, 0) as Ellipse;

                if (horizontalThumbContent == null)
                {
                    HorizontalThumb.Loaded += HorizontalThumb_Loaded;
                }
                else
                {
                    CreateAnimation();
                }

                void HorizontalThumb_Loaded(object sender, RoutedEventArgs e)
                {
                    HorizontalThumb.Loaded -= HorizontalThumb_Loaded;
                    horizontalThumbContent = VisualTreeHelper.GetChild(HorizontalThumb, 0) as Ellipse;

                    if (horizontalThumbContent != null)
                    {
                        CreateAnimation();
                    }
                }

                void CreateAnimation()
                {
                    ElementCompositionPreview.SetIsTranslationEnabled(horizontalThumbContent, true);

                    pressedThumbAnimation = Window.Current.Compositor.CreateScalarKeyFrameAnimation();
                    pressedThumbAnimation.InsertKeyFrame(1f, -8f);
                    pressedThumbAnimation.Duration = TimeSpan.FromSeconds(0.05d);

                    releasedThumbAnimation = Window.Current.Compositor.CreateSpringScalarAnimation();
                    releasedThumbAnimation.DampingRatio = 0.25f;
                    releasedThumbAnimation.Period = TimeSpan.FromSeconds(0.01d);
                    releasedThumbAnimation.FinalValue = 0f;
                }
            }

            orientationEventToken = RegisterPropertyChangedCallback(OrientationProperty, OrientationChanged);
            this.Unloaded += FluentSlider_Unloaded;

            this.AddHandler(PointerPressedEvent, new PointerEventHandler(FluentSlider_PointerPressed), true);
            this.AddHandler(PointerReleasedEvent, new PointerEventHandler(FluentSlider_PointerReleased), true);
            this.AddHandler(PointerCanceledEvent, new PointerEventHandler(FluentSlider_PointerCanceled), true);
        }


        private void OrientationChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (Orientation == Orientation.Vertical)
                throw new NotSupportedException();
        }

        private void FluentSlider_Unloaded(object sender, RoutedEventArgs e)
        {
            if (orientationEventToken.HasValue)
            {
                UnregisterPropertyChangedCallback(OrientationProperty, orientationEventToken.Value);
            }
            this.RemoveHandler(PointerPressedEvent, new PointerEventHandler(FluentSlider_PointerPressed));
            this.RemoveHandler(PointerReleasedEvent, new PointerEventHandler(FluentSlider_PointerReleased));
            this.RemoveHandler(PointerCanceledEvent, new PointerEventHandler(FluentSlider_PointerCanceled));
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

        private void FluentSlider_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (horizontalThumbContent != null)
            {
                ElementCompositionPreview.GetElementVisual(horizontalThumbContent).StartAnimation("Translation.Y", pressedThumbAnimation);
            }
        }

        private void FluentSlider_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (horizontalThumbContent != null)
            {
                ElementCompositionPreview.GetElementVisual(horizontalThumbContent).StartAnimation("Translation.Y", releasedThumbAnimation);
            }
        }

        private void FluentSlider_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            if (horizontalThumbContent != null)
            {
                ElementCompositionPreview.GetElementVisual(horizontalThumbContent).StartAnimation("Translation.Y", releasedThumbAnimation);
            }
        }
    }
}
