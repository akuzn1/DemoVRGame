using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Media;
using Basic3DLib;
using Windows.UI;
using System.Numerics;
using Microsoft.Graphics.Canvas;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ClapYourHands
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture mediaCapture;
        bool isPreviewing;
        DisplayRequest displayRequest = new DisplayRequest();
        DispatcherTimer timer;
        CaptureElement previewControl;
        SoftwareBitmap background;
        Object3D figureLeft;
        Object3D figureRight;
        int methodCount = 0;
        object lockObj = new object();


        public MainPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += Application_Suspending;

        }

        private async Task StartPreviewAsync()
        {
            try
            {

                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

                displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            catch (UnauthorizedAccessException)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                //ShowMessageToUser("The app was denied access to the camera");
                return;
            }

            try
            {
                previewControl = new CaptureElement()
                {
                    Source = mediaCapture
                };
                await mediaCapture.StartPreviewAsync();
                isPreviewing = true;
            }
            catch (System.IO.FileLoadException)
            {
                //mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;
                return;
            }

            figureLeft = Figures.GetCube(new Point3D() { X = 40, Y = 0, Z = 100 }, 70);
            figureRight = Figures.GetCube(new Point3D() { X = -40, Y = 0, Z = 100 }, 70);

            await Task.Delay(TimeSpan.FromMilliseconds(100));
            canvLeft.Draw += CanvasControlLeft_Draw;
            canvRight.Draw += CanvasControlRight_Draw;

            timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async void Timer_Tick(object sender, object e)
        {
            lock (lockObj)
            {
                if (mediaCapture == null || mediaCapture.CameraStreamState != Windows.Media.Devices.CameraStreamState.Streaming)
                    return;
                if (methodCount >= 4)
                    return;
                methodCount++;
            }

            figureLeft.RotateX(0.1);
            figureRight.RotateX(0.1);
            SoftwareBitmap previewBitmap = null;
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                var previewProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

                VideoFrame videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);

                VideoFrame previewFrame = await mediaCapture.GetPreviewFrameAsync(videoFrame);

                previewBitmap = previewFrame.SoftwareBitmap;

                lock (lockObj)
                {
                    if (background == null)
                        background = new SoftwareBitmap(BitmapPixelFormat.Bgra8, previewBitmap.PixelWidth, previewBitmap.PixelHeight, previewBitmap.BitmapAlphaMode);

                    previewBitmap.CopyTo(background);
                }


            });
            lock (lockObj)
            {
                methodCount--;
            }

            canvLeft.Invalidate();
            canvRight.Invalidate();
        }

        private void CanvasControlLeft_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            double sourceWidth;
            double sourceHeight;
            double destWidth;
            double destHeight;

            lock (lockObj)
            {
                CanvasBitmap c;
                if (background == null)
                {
                    return;
                }
                c = CanvasBitmap.CreateFromSoftwareBitmap(args.DrawingSession, background);
                sourceWidth = background.PixelWidth;
                sourceHeight = background.PixelHeight;

                var sourceAspectRatio = sourceWidth / sourceHeight;
                Rect sourceRect;

                destWidth = canvLeft.ActualWidth;
                destHeight = canvLeft.ActualHeight;
                var destAspectRatio = destWidth / destHeight;
                Rect destRect = new Rect() { X = 0, Y = 0, Width = destWidth, Height = destHeight };

                if (sourceAspectRatio > destAspectRatio)
                {
                    var sourceNewWidth = sourceHeight * destAspectRatio;
                    sourceRect = new Rect() { X = (sourceWidth - sourceNewWidth) / 2, Y = 0, Width = sourceNewWidth, Height = sourceHeight };
                }
                else
                {
                    var sourceNewHeight = sourceWidth * destAspectRatio;
                    sourceRect = new Rect() { X = 0, Y = (sourceHeight - sourceNewHeight) / 2, Width = sourceWidth, Height = sourceNewHeight };
                }
                args.DrawingSession.DrawImage(c, destRect, sourceRect);
            }


            if (figureLeft == null)
                return;

            var zIndex = 500;

            foreach (var plane in figureLeft.Planes)
            {
                var p1 = Projections.GetPerspectiveProjection(plane.Points[plane.Points.Count - 1], zIndex);
                var p2 = Projections.GetPerspectiveProjection(plane.Points[0], zIndex);
                args.DrawingSession.DrawLine(
                        new Vector2() { X = (float)(p1.X + destWidth / 2), Y = (float)(p1.Y + destHeight / 2) },
                        new Vector2() { X = (float)(p2.X + destWidth / 2), Y = (float)(p2.Y + destHeight / 2) },
                        Colors.Black
                );
                for (int i = 0; i < plane.Points.Count - 1; i++)
                {
                    p1 = Projections.GetPerspectiveProjection(plane.Points[i], zIndex);
                    p2 = Projections.GetPerspectiveProjection(plane.Points[i + 1], zIndex);
                    args.DrawingSession.DrawLine(
                        new Vector2() { X = (float)(p1.X + destWidth / 2), Y = (float)(p1.Y + destHeight / 2) },
                        new Vector2() { X = (float)(p2.X + destWidth / 2), Y = (float)(p2.Y + destHeight / 2) },
                        Colors.Black
                    );
                }
            }
        }
        private void CanvasControlRight_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            double sourceWidth;
            double sourceHeight;
            double destWidth;
            double destHeight;

            lock (lockObj)
            {
                CanvasBitmap c = null;
                if (background == null)
                {
                    return;
                }
                c = CanvasBitmap.CreateFromSoftwareBitmap(args.DrawingSession, background);

                sourceWidth = background.PixelWidth;
                sourceHeight = background.PixelHeight;

                var sourceAspectRatio = sourceWidth / sourceHeight;
                Rect sourceRect;

                destWidth = canvRight.ActualWidth;
                destHeight = canvRight.ActualHeight;
                var destAspectRatio = destWidth / destHeight;
                Rect destRect = new Rect() { X = 0, Y = 0, Width = destWidth, Height = destHeight };

                if (sourceAspectRatio > destAspectRatio)
                {
                    var sourceNewWidth = sourceHeight * destAspectRatio;
                    sourceRect = new Rect() { X = (sourceWidth - sourceNewWidth) / 2, Y = 0, Width = sourceNewWidth, Height = sourceHeight };
                }
                else
                {
                    var sourceNewHeight = sourceWidth * destAspectRatio;
                    sourceRect = new Rect() { X = 0, Y = (sourceHeight - sourceNewHeight) / 2, Width = sourceWidth, Height = sourceNewHeight };
                }


                args.DrawingSession.DrawImage(c, destRect, sourceRect);
            }


            if (figureRight == null)
                return;

            var zIndex = 500;

            foreach (var plane in figureRight.Planes)
            {
                var p1 = Projections.GetPerspectiveProjection(plane.Points[plane.Points.Count - 1], zIndex);
                var p2 = Projections.GetPerspectiveProjection(plane.Points[0], zIndex);
                args.DrawingSession.DrawLine(
                        new Vector2() { X = (float)(p1.X + destWidth / 2), Y = (float)(p1.Y + destHeight / 2) },
                        new Vector2() { X = (float)(p2.X + destWidth / 2), Y = (float)(p2.Y + destHeight / 2) },
                        Colors.Black
                );
                for (int i = 0; i < plane.Points.Count - 1; i++)
                {
                    p1 = Projections.GetPerspectiveProjection(plane.Points[i], zIndex);
                    p2 = Projections.GetPerspectiveProjection(plane.Points[i + 1], zIndex);
                    args.DrawingSession.DrawLine(
                        new Vector2() { X = (float)(p1.X + destWidth / 2), Y = (float)(p1.Y + destHeight / 2) },
                        new Vector2() { X = (float)(p2.X + destWidth / 2), Y = (float)(p2.Y + destHeight / 2) },
                        Colors.Black
                    );
                }
            }
        }


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await StartPreviewAsync();
            base.OnNavigatedTo(e);
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            await CleanupCameraAsync();
            base.OnNavigatedFrom(e);
        }

        private async Task CleanupCameraAsync()
        {
            timer.Stop();

            do
            {
                lock (lockObj)
                {
                    if (methodCount <= 0)
                        break;
                }
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            } while (true);

            if (mediaCapture != null)
            {
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (displayRequest != null)
                    {
                        displayRequest.RequestRelease();
                    }

                    mediaCapture.Dispose();
                    mediaCapture = null;
                });
            }
        }
        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            // Handle global application events only if this page is active
            if (Frame.CurrentSourcePageType == typeof(MainPage))
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await CleanupCameraAsync();
                deferral.Complete();
            }
        }
    }
}
