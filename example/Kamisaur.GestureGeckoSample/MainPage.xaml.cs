using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Kamisaur.GestureGeckoSample
{
    public partial class MainPage : ContentPage
    {
        void Button_Clicked(object sender, EventArgs e)
        {
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            var pinch = new PinchGestureRecognizer();
            pinch.PinchUpdated += OnPinchUpdated;
            zoomableContainer.GestureRecognizers.Add(pinch);

            var pan = new PanGestureRecognizer();
            pan.PanUpdated += OnPanUpdated;
            zoomableContainer.GestureRecognizers.Add(pan);


            var doubleTap = new TapGestureRecognizer() { NumberOfTapsRequired = 2 };
            doubleTap.Tapped += OnDoubleTapped;
            zoomableContainer.GestureRecognizers.Add(doubleTap);

        }

        private const double MinScale = 1;
        private const double MaxScale = 2;
        private const double OVERSHOOT = 0.15;
        private double StartScale;


        private void OnDoubleTapped(object sender, EventArgs e)
        {
            if (zoomableContainer.Content.Scale > MinScale)
            {
                zoomableContainer.Content.ScaleTo(MinScale, 250, Easing.CubicInOut);
                zoomableContainer.Content.TranslateTo(0, 0, 250, Easing.CubicInOut);
            }
            else
            {
                zoomableContainer.Content.AnchorX = AnchorY = 0.5;
                zoomableContainer.Content.ScaleTo(MaxScale, 250, Easing.CubicInOut);
            }
        }




        private bool PreventPinchWhenMaxMinReached = false;
        private bool DisablePinchFinishedAnimation = false;

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Started:
                    StartScale = zoomableContainer.Content.Scale;
                    zoomableContainer.Content.AnchorX = e.ScaleOrigin.X;
                    zoomableContainer.Content.AnchorY = e.ScaleOrigin.Y;
                    break;

                case GestureStatus.Running:
                    double current = zoomableContainer.Content.Scale + (e.Scale - 1) * StartScale;

                    if(PreventPinchWhenMaxMinReached)
                        zoomableContainer.Content.Scale = current.Clamp(MinScale * (1 - OVERSHOOT), MaxScale * (1 + OVERSHOOT));
                    else
                        zoomableContainer.Content.Scale = current;
                    break;

                case GestureStatus.Completed:
                    if(!DisablePinchFinishedAnimation)
                    {

                        if (zoomableContainer.Content.Scale > MaxScale)
                        {
                            zoomableContainer.Content.ScaleTo(MaxScale, 250, Easing.SpringOut);
                        }
                        else if (zoomableContainer.Content.Scale < MinScale)
                        {
                            zoomableContainer.Content.ScaleTo(MinScale, 250, Easing.SpringOut);
                        }
                        else
                        {
                            //var finalWithEase = zoomableContainer.Content.Scale - (zoomableContainer.Content.Scale * 0.10);
                            //zoomableContainer.Content.ScaleTo(finalWithEase, 250, Easing.SpringOut);
                        }
                    }
                    break;
            }
        }

        private double StartX, StartY;
        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    StartX = (1 - zoomableContainer.Content.AnchorX) * zoomableContainer.Width;
                    StartY = (1 - zoomableContainer.Content.AnchorY) * zoomableContainer.Height;

                    StartXprop = StartX; //
                    StartYprop = StartY; //
                    break;

                case GestureStatus.Running:
                    var x = 1 - (StartX + e.TotalX) / zoomableContainer.Width;
                    var y = 1 - (StartY + e.TotalY) / zoomableContainer.Height;

                    NewXprop = x; //
                    NewYprop = y; //

                    zoomableContainer.Content.AnchorX = x.Clamp(0, 1);
                    zoomableContainer.Content.AnchorY = y.Clamp(0, 1);
                    break;

                case GestureStatus.Completed:

                    break;
            }
        }



        private double _startXprop;
        public double StartXprop
        {
            get => _startXprop;
            set { _startXprop = value; OnPropertyChanged(); }
        }


        private double _startYprop;
        public double StartYprop
        {
            get => _startYprop;
            set { _startYprop = value; OnPropertyChanged(); }
        }


        private double _newXprop;
        public double NewXprop
        {
            get => _newXprop;
            set { _newXprop = value; OnPropertyChanged(); }
        }


        private double _newYprop;
        public double NewYprop
        {
            get => _newYprop;
            set { _newYprop = value; OnPropertyChanged(); }
        }

    }
}
