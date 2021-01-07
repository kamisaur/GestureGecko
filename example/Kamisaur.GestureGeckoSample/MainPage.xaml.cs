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

            DeltaScale = 0;
            StartScale = GestureStartedScale;
            CurrentScale = Content.Scale;


            var pinch = new PinchGestureRecognizer();
            pinch.PinchUpdated += PinchUpdated;
            zoomableContainer.GestureRecognizers.Add(pinch);
        }




        /// <summary>
        /// Captures the scale of the element when gesure starts
        /// </summary>
        double GestureStartedScale { get; set; } = 1;


        public double MaxScale = 3;
        double scaleTrashold = 1;

        //double currentScale = 1;
        double xOffset = 0;
        double yOffset = 0;





        public double CalculateOffset(
            double scaleOriginCoordinate
            , double contentCoordinate
            , double containerDimension
            , double contentDimension
            , double startScale
            , double currentScale
            , double offset)
        {
            double renderedCoordinate = contentCoordinate + offset;
            double deltaCoordinate = renderedCoordinate / containerDimension;

            double deltaDimension = containerDimension / (contentDimension * startScale);
            double originCoordinate = (scaleOriginCoordinate - deltaCoordinate) * deltaDimension;

            // Calculate the transformed element pixel coordinates.
            double targetCoordinate = offset - (originCoordinate * contentDimension) * (currentScale - startScale);

            // Apply translation based on the change in origin.
            var translationCoordinate = targetCoordinate.Clamp(-contentDimension * (currentScale - 1), 0);
            return translationCoordinate;
        }



        private double _deltaScale;
        public double DeltaScale
        {
            get => _deltaScale;
            set { _deltaScale = value; OnPropertyChanged(); }
        }
        

        private double _startScale;
        public double StartScale
        {
            get => _startScale;
            set { _startScale = value; OnPropertyChanged(); }
        }
        

        private double _currentScale;
        public double CurrentScale
        {
            get => _currentScale;
            set { _currentScale = value; OnPropertyChanged(); }
        }
        

        private double _scaleToAdd;
        public double ScaleToAdd
        {
            get => _scaleToAdd;
            set { _scaleToAdd = value; OnPropertyChanged(); }
        }



        public double CalculateScale(double deltaScale, double startScale, double currentScale)
        {
            var scaleToAdd = (deltaScale - 1) * startScale;
            ScaleToAdd = scaleToAdd;

            currentScale += scaleToAdd;
            currentScale = Math.Max(1, currentScale);

            return currentScale;
        }

        private void PinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            var Content = zoomableContainer;


            if (e.Status == GestureStatus.Started)
            {
                GestureStartedScale = Content.Scale;
                Content.AnchorX = 0;
                Content.AnchorY = 0;
            }

            if (e.Status == GestureStatus.Running)
            {
                // 1. Calculate and apply the scale factor.
                Content.Scale = CalculateScale(e.Scale, GestureStartedScale, Content.Scale);
                DeltaScale = e.Scale;
                StartScale = GestureStartedScale;
                CurrentScale = Content.Scale;


                // 2. The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                // 2.1 Calculate and apply the X pixel coordinate.
                Content.TranslationX = CalculateOffset(
                    e.ScaleOrigin.X
                    , Content.X
                    , Content.Width
                    , Content.Content.Width
                    , GestureStartedScale
                    , Content.Scale
                    , xOffset);

                // 2.2 Calculate and apply the Y pixel coordinate.
                Content.TranslationY = CalculateOffset(
                    e.ScaleOrigin.Y
                    , Content.Y
                    , Content.Height
                    , Content.Content.Height
                    , GestureStartedScale
                    , Content.Scale
                    , yOffset);
            }

            if (e.Status == GestureStatus.Completed)
            {
                xOffset = Content.TranslationX;
                yOffset = Content.TranslationY;
            }
        }
    }
}
