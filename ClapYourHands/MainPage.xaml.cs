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
using BitmapAnalyzers.Colors;
using Windows.Storage.Pickers;
using Windows.Storage;

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
        Object3D figure;
        int methodCount = 0;
        object lockObj = new object();
        int zIndex = 500;
        Point3D leftEyePos = new Point3D() { X = -40, Y = 0, Z = 0 };
        Point3D rightEyePos = new Point3D() { X = 40, Y = 0, Z = 0 };


        public MainPage()
        {
            this.InitializeComponent();

            canvLeft.Visibility = Visibility.Collapsed;
            canvRight.Visibility = Visibility.Collapsed;

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

            figure = Figures.GetCube(new Point3D() { X = 0, Y = 0, Z = 100 }, 70);

            await Task.Delay(TimeSpan.FromMilliseconds(100));
            canvLeft.Draw += CanvasControlLeft_Draw;
            canvRight.Draw += CanvasControlRight_Draw;

            timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 100)
            };
            timer.Tick += Timer_Tick;
            timer.Start();
            canvLeft.Visibility = Visibility.Visible;
            canvRight.Visibility = Visibility.Visible;
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

            figure.RotateX(0.1);
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
                    ColorsDetector detector = new ColorsDetector();
                    detector.Detect(background);
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
            Rect destRect;

            lock (lockObj)
            {
                CanvasBitmap c;

                if (background == null)
                {
                    return;
                }
                c = CanvasBitmap.CreateFromSoftwareBitmap(args.DrawingSession, background);

                var rects = GetSourceDest(background, args);

                args.DrawingSession.DrawImage(c, rects.Item2, rects.Item1);
                destRect = rects.Item2;
            }


            if (figure == null)
                return;
            
            DrawFigure(args, destRect, zIndex, leftEyePos);
        }

        private void DrawFigure(Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args, Rect destRect, int zIndex, Point3D eyePos)
        {
            foreach (var plane in figure.Planes)
            {
                var p1 = Projections.GetPerspectiveProjection(plane.Points[plane.Points.Count - 1], zIndex, eyePos);
                var p2 = Projections.GetPerspectiveProjection(plane.Points[0], zIndex, eyePos);
                args.DrawingSession.DrawLine(
                        new Vector2() { X = (float)(destRect.Width - (p1.X + destRect.Width / 2)), Y = (float)(p1.Y + destRect.Height / 2) },
                        new Vector2() { X = (float)(destRect.Width - (p2.X + destRect.Width / 2)), Y = (float)(p2.Y + destRect.Height / 2) },
                        Colors.Black
                );
                for (int i = 0; i < plane.Points.Count - 1; i++)
                {
                    p1 = Projections.GetPerspectiveProjection(plane.Points[i], zIndex, eyePos);
                    p2 = Projections.GetPerspectiveProjection(plane.Points[i + 1], zIndex, eyePos);
                    args.DrawingSession.DrawLine(
                        new Vector2() { X = (float)(destRect.Width - (p1.X + destRect.Width / 2)), Y = (float)(p1.Y + destRect.Height / 2) },
                        new Vector2() { X = (float)(destRect.Width - (p2.X + destRect.Width / 2)), Y = (float)(p2.Y + destRect.Height / 2) },
                        Colors.Black
                    );
                }
            }
        }

        private Tuple<Rect, Rect> GetSourceDest(SoftwareBitmap background, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            double sourceWidth;
            double sourceHeight;
            double destWidth;
            double destHeight;

            if (background == null)
            {
                return null;
            }
            var c = CanvasBitmap.CreateFromSoftwareBitmap(args.DrawingSession, background);
            sourceWidth = background.PixelWidth;
            sourceHeight = background.PixelHeight;

            var sourceAspectRatio = sourceWidth / sourceHeight;
            Rect sourceRect;

            destWidth = canvLeft.ActualWidth;
            destHeight = canvLeft.ActualHeight;
            var destAspectRatio = destWidth / destHeight;
            var destRect = new Rect() { X = 0, Y = 0, Width = destWidth, Height = destHeight };

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

            return new Tuple<Rect, Rect>(sourceRect, destRect);
        }

        private void CanvasControlRight_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            Rect destRect;

            lock (lockObj)
            {
                CanvasBitmap c;

                if (background == null)
                {
                    return;
                }
                c = CanvasBitmap.CreateFromSoftwareBitmap(args.DrawingSession, background);

                var rects = GetSourceDest(background, args);

                args.DrawingSession.DrawImage(c, rects.Item2, rects.Item1);
                destRect = rects.Item2;
            }

            if (figure == null)
                return;

            DrawFigure(args, destRect, zIndex, rightEyePos);
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
            canvLeft.Visibility = Visibility.Collapsed;
            canvRight.Visibility = Visibility.Collapsed;
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
