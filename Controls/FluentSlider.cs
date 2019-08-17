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
        Grid HorizontalTemplate;
        Thumb HorizontalThumb;
        Canvas CurveHost;


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            HorizontalTrackRect1 = GetTemplateChild(nameof(HorizontalTrackRect1)) as Rectangle;
            HorizontalTrackRect2 = GetTemplateChild(nameof(HorizontalTrackRect2)) as Rectangle;
            HorizontalTemplate = GetTemplateChild(nameof(HorizontalTemplate)) as Grid;
            HorizontalThumb = GetTemplateChild(nameof(HorizontalThumb)) as Thumb;
            CurveHost = GetTemplateChild(nameof(CurveHost)) as Canvas;

            if (HorizontalTemplate != null)
            {
                var visual = ElementCompositionPreview.GetElementVisual(HorizontalTemplate);
                visual.Clip = visual.Compositor.CreateInsetClip(0, -20, 0, 0);
            }

            CreateThumbAnimation();
            CreateTrackRectAnimation();

            orientationEventToken = RegisterPropertyChangedCallback(OrientationProperty, OrientationChanged);
            this.Unloaded += FluentSlider_Unloaded;

            this.AddHandler(PointerPressedEvent, new PointerEventHandler(FluentSlider_PointerPressed), true);
            this.AddHandler(PointerReleasedEvent, new PointerEventHandler(FluentSlider_PointerReleased), true);
            this.AddHandler(PointerCanceledEvent, new PointerEventHandler(FluentSlider_PointerCanceled), true);
        }

        #region Composition

        private void CreateTrackRectAnimation()
        {
            if (HorizontalThumb != null && HorizontalTrackRect1 != null && HorizontalTrackRect2 != null)
            {
                var visual1 = ElementCompositionPreview.GetElementVisual(HorizontalTrackRect1);
                var visual2 = ElementCompositionPreview.GetElementVisual(HorizontalTrackRect2);

                var clip1 = Window.Current.Compositor.CreateInsetClip();
                var clip2 = Window.Current.Compositor.CreateInsetClip();

                visual1.Clip = clip1;
                visual2.Clip = clip2;

                var thumbOffsetXExp = "thumb.Offset.X";
                var thisSizeXExp = "host.Size.X";
                var right1 = Window.Current.Compositor.CreateExpressionAnimation($"{thisSizeXExp} - {thumbOffsetXExp} + 37");
                var left2 = Window.Current.Compositor.CreateExpressionAnimation($"{thumbOffsetXExp} + 53");

                right1.SetReferenceParameter("thumb", ElementCompositionPreview.GetElementVisual(HorizontalThumb));
                right1.SetReferenceParameter("host", visual1);
                left2.SetReferenceParameter("thumb", ElementCompositionPreview.GetElementVisual(HorizontalThumb));
                left2.SetReferenceParameter("host", visual2);

                clip1.StartAnimation("RightInset", right1);
                clip2.StartAnimation("LeftInset", left2);
            }
        }

        private void CreateThumbAnimation()
        {
            if (HorizontalThumb != null)
            {
                // // 平移动画 由于动画是上推Thumb操作带动Track弯曲，正确的行为是Thumb始终跟手，所以移除了平移动画
                //var imp = Window.Current.Compositor.CreateImplicitAnimationCollection();
                //var offsetAnimation = Window.Current.Compositor.CreateVector3KeyFrameAnimation();
                //offsetAnimation.InsertExpressionKeyFrame(0f, "(abs(this.FinalValue.X - this.StartingValue.X) > 80) ? this.StartingValue : this.FinalValue");
                //offsetAnimation.InsertExpressionKeyFrame(1f, "this.FinalValue");
                //offsetAnimation.Duration = TimeSpan.FromSeconds(0.1d);
                //offsetAnimation.Target = "Offset";
                //imp["Offset"] = offsetAnimation;
                //ElementCompositionPreview.GetElementVisual(HorizontalThumb).ImplicitAnimations = imp;

                //if (CurveHost != null)
                //{
                //    ElementCompositionPreview.GetElementVisual(CurveHost).ImplicitAnimations = imp;
                //}

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
        }

        #endregion Composition


        #region Event Callbacks

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

        private void OrientationChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (Orientation == Orientation.Vertical)
                throw new NotSupportedException();
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

        #endregion Event Callbacks

    }
}
