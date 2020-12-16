using System;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.GestureGecko.Platforms.Shared
{
    public class ZoomableContainer : ContentView
    {
        public static BindableProperty InitialScaleProperty = BindableProperty.Create(
            propertyName: nameof(InitialScale)
            , returnType: typeof(double)
            , declaringType: typeof(ZoomableContainer)
            , defaultValue: 1.0
            , defaultBindingMode: BindingMode.OneWay
            , propertyChanged: InitialScalePropertyChanged);

        private static void InitialScalePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ZoomableContainer)bindable;

            if (control.Content is not null)
                control.Content.Scale = (double)newValue;
        }

        public double InitialScale
        {
            get => (double)GetValue(InitialScaleProperty);
            set => SetValue(InitialScaleProperty, value);
        }


        public static BindableProperty MaxScaleProperty = BindableProperty.Create(
            propertyName: nameof(MaxScale)
            , returnType: typeof(double)
            , declaringType: typeof(ZoomableContainer)
            , defaultValue: 3.0
            , defaultBindingMode: BindingMode.OneWay
            , propertyChanged: MaxScalePropertyChanged);

        private static void MaxScalePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (ZoomableContainer)bindable;

        }

        public double MaxScale
        {
            get => (double)GetValue(MaxScaleProperty);
            set => SetValue(MaxScaleProperty, value);
        }


        public ZoomableContainer()
        {
            var pinch = new PinchGestureRecognizer();
            pinch.PinchUpdated += PinchUpdated;
            GestureRecognizers.Add(pinch);
        }

        protected override void OnChildAdded(Element child)
        {
            base.OnChildAdded(child);
            Content.Scale = InitialScale;
        }

        double currentScale = 1;
        double startScale = 1;
        double xOffset = 0;
        double yOffset = 0;

        private async void PinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            var abc = Content.Scale;

            if (e.Status == GestureStatus.Started)
            {
                startScale = Content.Scale;
                Content.AnchorX = 0;
                Content.AnchorY = 0;
            }

            if (e.Status == GestureStatus.Running)
            {
                // Calculate the scale factor to be applied.
                currentScale += (e.Scale - 1) * startScale;
                currentScale = Math.Max(1, currentScale);

                // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                // so get the X pixel coordinate.
                double renderedX = Content.X + xOffset;
                double deltaX = renderedX / Width;
                double deltaWidth = Width / (Content.Width * startScale);
                double originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

                // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                // so get the Y pixel coordinate.
                double renderedY = Content.Y + yOffset;
                double deltaY = renderedY / Height;
                double deltaHeight = Height / (Content.Height * startScale);
                double originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

                // Calculate the transformed element pixel coordinates.
                double targetX = xOffset - (originX * Content.Width) * (currentScale - startScale);
                double targetY = yOffset - (originY * Content.Height) * (currentScale - startScale);

                // Apply translation based on the change in origin.
                Content.TranslationX = targetX.Clamp(-Content.Width * (currentScale - 1), 0);
                Content.TranslationY = targetY.Clamp(-Content.Height * (currentScale - 1), 0);

                // Apply scale factor.
                Content.Scale = currentScale;

            }

            if (e.Status == GestureStatus.Completed)
            {
                //if (Content.Scale > MaxScale)
                //{
                //    var transX = Content.X + xOffset;
                //    var transY = Content.Y + yOffset;

                //    await Content.TranslateTo(0, 0, 1000, Easing.SinIn);
                //    await Content.ScaleTo(MaxScale, 1000, Easing.SinIn);
                //    currentScale = MaxScale;
                //}


                xOffset = Content.TranslationX;
                yOffset = Content.TranslationY;
            }
        }

    }

}