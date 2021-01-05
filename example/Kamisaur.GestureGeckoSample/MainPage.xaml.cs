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
        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
        }

        public MainPage()
        {
            InitializeComponent();

            var pinch = new PinchGestureRecognizer();
            pinch.PinchUpdated += PinchUpdated;
            zoomableContent.GestureRecognizers.Add(pinch);
        }

        public double MaxScale = 3;

        double currentScale = 1;
        double startScale = 1;
        double xOffset = 0;
        double yOffset = 0;

        double scaleTrashold = 1;



        public double CalculateScale(double scale, double startScale, double currentScale)
        {
            currentScale += (scale - 1) * startScale;
            currentScale = Math.Max(1, currentScale);

            return currentScale;
        }

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


        private async void PinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            var Content = zoomableContent;


            if (e.Status == GestureStatus.Started)
            {
                startScale = Content.Scale;
                Content.AnchorX = 0;
                Content.AnchorY = 0;
            }

            if (e.Status == GestureStatus.Running)
            {
                // Calculate the scale factor to be applied.
                currentScale = CalculateScale(e.Scale, startScale, currentScale);

                // Apply scale factor.
                Content.Scale = currentScale;


                // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
                // get the X pixel coordinate.
                Content.TranslationX = CalculateOffset(
                    e.ScaleOrigin.X
                    , Content.X
                    , Content.Width
                    , Content.Content.Width
                    , startScale
                    , currentScale
                    , xOffset);

                // get the Y pixel coordinate.
                Content.TranslationY = CalculateOffset(
                    e.ScaleOrigin.Y
                    , Content.Y
                    , Content.Height
                    , Content.Content.Height
                    , startScale
                    , currentScale
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
