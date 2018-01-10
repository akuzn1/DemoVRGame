using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI;
using System.Runtime.InteropServices;

namespace BitmapAnalyzers.Colors
{
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    public class ColorsDetector : IDetection
    {
        public Color Color { get; set; }
        public double Delta { get; set; }

        public ColorsDetector()
        {
            Color = Windows.UI.Colors.White;
            Delta = 100;
        }

        public unsafe object Detect(SoftwareBitmap image)
        {
            using (BitmapBuffer buffer = image.LockBuffer(BitmapBufferAccessMode.Write))
            {
                using (var reference = buffer.CreateReference())
                {
                    byte* dataInBytes;
                    uint capacity;
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacity);

                    // Fill-in the BGRA plane
                    BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);
                    for (int i = 0; i < bufferLayout.Height; i++)
                    {
                        for (int j = 0; j < bufferLayout.Width; j++)
                        {
                            int b = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0] - Color.B;
                            int g = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1] - Color.G;
                            int r = dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2] - Color.R;

                            if (b * b + g * g + r * r < Delta * Delta)
                            {
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0] = 0;
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1] = 0;
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2] = 255;
                                dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 3] = (byte)255;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
