using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SixLabors.ImageSharp;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using static MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes.ImageSource;
using SixLabors.ImageSharp.Formats;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.Primitives;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace PdfSharpCore.ImageSharp
{
    public class ImageSharpImageSource : ImageSource
    {
        protected override IImageSource FromBinaryImpl(string name, Func<byte[]> imageSource, int? quality = 75)
        {
            return new ImageSharpImageSourceImpl(name, () =>
            {
                return Image.Load(imageSource.Invoke());
            }, (int)quality);
        }

        protected override IImageSource FromFileImpl(string path, int? quality = 75)
        {
            return new ImageSharpImageSourceImpl(path, () =>
            {
                return Image.Load(path);
            }, (int)quality);
        }

        protected override IImageSource FromStreamImpl(string name, Func<Stream> imageStream, int? quality = 75)
        {
            return new ImageSharpImageSourceImpl(name, () =>
            {
                using (var stream = imageStream.Invoke())
                {
                    return Image.Load(stream);
                }
            }, (int)quality);
        }

        private class ImageSharpImageSourceImpl : IImageSource
        {

            private Image<Rgba32> _image;
            private Image<Rgba32> Image
            {
                get
                {
                    if (_image == null)
                    {
                        _image = _getImage.Invoke();
                    }
                    return _image;
                }
            }
            private Func<Image<Rgba32>> _getImage;
            private readonly int _quality;

            public int Width => Image.Width;
            public int Height => Image.Height;
            public string Name { get; }

            public ImageSharpImageSourceImpl(string name, Func<Image<Rgba32>> getImage, int quality)
            {
                Name = name;
                _getImage = getImage;
                _quality = quality;
            }

            public void SaveAsJpeg(MemoryStream ms, CancellationToken ct)
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                ct.Register(() => {
                    tcs.TrySetCanceled();
                });
                var imageProcessingContext = new ImageProcessingContext<Rgba32>(Image, false);
                var task = Task.Run(() => {
                    imageProcessingContext.AutoOrient();
                    Image.SaveAsJpeg(ms, new JpegEncoder() { Quality = _quality });
                });
                Task.WaitAny(task, tcs.Task);
                tcs.TrySetCanceled();
                ct.ThrowIfCancellationRequested();
                if (task.IsFaulted) throw task.Exception;
            }

            public void Dispose()
            {
                Image.Dispose();
            }
        }
    }

    public class ImageProcessingContext<TPixel> : IImageProcessingContext<TPixel>
    where TPixel : struct, IPixel<TPixel>
    {
        public Image<TPixel> source;

        public List<AppliedOpperation> applied = new List<AppliedOpperation>();
        public bool mutate;

        public ImageProcessingContext(Image<TPixel> source, bool mutate)
        {
            this.mutate = mutate;
            if (mutate)
            {
                this.source = source;
            }
            else
            {
                this.source = source?.Clone();
            }
        }

        public Image<TPixel> Apply()
        {
            return source;
        }

        public IImageProcessingContext<TPixel> ApplyProcessor(IImageProcessor<TPixel> processor, Rectangle rectangle)
        {
            applied.Add(new AppliedOpperation
            {
                Processor = processor,
                Rectangle = rectangle
            });
            return this;
        }

        public IImageProcessingContext<TPixel> ApplyProcessor(IImageProcessor<TPixel> processor)
        {
            applied.Add(new AppliedOpperation
            {
                Processor = processor
            });
            return this;
        }
        public struct AppliedOpperation
        {
            public Rectangle? Rectangle { get; set; }
            public IImageProcessor<TPixel> Processor { get; set; }
        }
    }
}
