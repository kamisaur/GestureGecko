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


        protected override void OnChildAdded(Element child)
        {
            base.OnChildAdded(child);
            Content.Scale = InitialScale;


            var pinch = new PinchGestureRecognizer();
            pinch.PinchUpdated += PinchUpdated;
            Content.GestureRecognizers.Add(pinch);


            var pan = new PanGestureRecognizer();
            pan.PanUpdated += OnPanUpdated;
            Content.GestureRecognizers.Add(pan);
        }


        public ZoomableContainer()
        {
            //var pinch = new PinchGestureRecognizer();
            //pinch.PinchUpdated += PinchUpdated;
            //GestureRecognizers.Add(pinch);


            //var pan = new PanGestureRecognizer();
            //pan.PanUpdated += OnPanUpdated;
            //GestureRecognizers.Add(pan);


            //var doubleTap = new TapGestureRecognizer() { NumberOfTapsRequired = 2 };
            //doubleTap.Tapped += DoubleTapped;
            //GestureRecognizers.Add(doubleTap);
        }

        double currentScale = 1;
        double startScale = 1;
        double xOffset = 0;
        double yOffset = 0;

        double scaleTrashold = 1;

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
                if (Content.Scale > MaxScale + scaleTrashold)
                {
                    return;
                }

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


                // for debug

            }

            if (e.Status == GestureStatus.Completed)
            {
                if (Content.Scale > MaxScale)
                {
                    var transX = Content.X + xOffset;
                    var transY = Content.Y + yOffset;

                    //await Content.TranslateTo(0, 0, 1000, Easing.SinIn);
                    await Content.ScaleTo(MaxScale, 1000, Easing.SinInOut);
                    currentScale = MaxScale;
                }


                xOffset = Content.TranslationX;
                yOffset = Content.TranslationY;
            }
        }

        public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (Content.Scale == 1)
            {
                return;
            }

            if (e.StatusType == GestureStatus.Running)
            {
                double newX = (e.TotalX * Scale) + xOffset;
                double newY = (e.TotalY * Scale) + yOffset;

                double width = (Content.Width * Content.Scale);
                double height = (Content.Height * Content.Scale);

                bool canMoveX = width > Application.Current.MainPage.Width;
                bool canMoveY = height > Application.Current.MainPage.Height;

                if (canMoveX)
                {
                    double minX = (width - (Application.Current.MainPage.Width / 2)) * -1;
                    double maxX = Math.Min(Application.Current.MainPage.Width / 2, width / 2);

                    if (newX < minX)
                    {
                        newX = minX;
                    }

                    if (newX > maxX)
                    {
                        newX = maxX;
                    }
                }
                else
                {
                    newX = 0;
                }

                if (canMoveY)
                {
                    double minY = (height - (Application.Current.MainPage.Height / 2)) * -1;
                    double maxY = Math.Min(Application.Current.MainPage.Width / 2, height / 2);

                    if (newY < minY)
                    {
                        newY = minY;
                    }

                    if (newY > maxY)
                    {
                        newY = maxY;
                    }
                }
                else
                {
                    newY = 0;
                }

                Content.TranslationX = newX;
                Content.TranslationY = newY;
            }
            else if (e.StatusType == GestureStatus.Completed)
            {

                xOffset = Content.TranslationX;
                yOffset = Content.TranslationY;
            }
        }

        public async void DoubleTapped(object sender, EventArgs e)
        {
            double multiplicator = Math.Pow(2, 1.0 / 10.0);
            startScale = Content.Scale;
            Content.AnchorX = 0;
            Content.AnchorY = 0;

            for (int i = 0; i < 10; i++)
            {
                currentScale *= multiplicator;
                double renderedX = Content.X + xOffset;
                double deltaX = renderedX / Width;
                double deltaWidth = Width / (Content.Width * startScale);
                double originX = (0.5 - deltaX) * deltaWidth;

                double renderedY = Content.Y + yOffset;
                double deltaY = renderedY / Height;
                double deltaHeight = Height / (Content.Height * startScale);
                double originY = (0.5 - deltaY) * deltaHeight;

                double targetX = xOffset - (originX * Content.Width) * (currentScale - startScale);
                double targetY = yOffset - (originY * Content.Height) * (currentScale - startScale);

                Content.TranslationX = Math.Min(0, Math.Max(targetX, -Content.Width * (currentScale - 1)));
                Content.TranslationY = Math.Min(0, Math.Max(targetY, -Content.Height * (currentScale - 1)));

                Content.Scale = currentScale;
                await Task.Delay(10);
            }

            xOffset = Content.TranslationX;
            yOffset = Content.TranslationY;
        }
    }

}