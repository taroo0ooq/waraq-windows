// Waraq for Windows — GPL-3.0 derivative of bahamut42/waraq.
// Copyright (C) Waraq authors and Waraq Windows contributors.
// Best-effort stills for GIF/image; video gets a simple placeholder PNG.

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Waraq.Windows.Core;

namespace Waraq.Windows.App.Library;

public static class ThumbnailFactory
{
    public static string? TryCreate(LibraryPaths paths, string mediaAbsolutePath, string id)
    {
        try
        {
            paths.EnsureDirectories();
            var dest = Path.Combine(paths.ThumbnailsDir, id + ".jpg");
            var kind = MediaPathClassifier.Classify(mediaAbsolutePath);

            if (kind is MediaKind.Gif or MediaKind.Image)
            {
                using var src = Image.FromFile(mediaAbsolutePath);
                using var thumb = Resize(src, 320, 180);
                thumb.Save(dest, ImageFormat.Jpeg);
                return dest;
            }

            // Video: labeled placeholder (full frame extract needs MF decoder pipeline — later)
            using var bmp = new Bitmap(320, 180);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(0x12, 0x12, 0x16));
                using var brush = new SolidBrush(Color.FromArgb(0xC8, 0x3A, 0x4A));
                g.FillRectangle(brush, 0, 150, 320, 30);
                using var font = new Font("Segoe UI", 12, FontStyle.Bold);
                g.DrawString("VIDEO", font, Brushes.White, 12, 12);
                g.DrawString(Path.GetFileName(mediaAbsolutePath), font, Brushes.Gainsboro, 12, 152);
            }

            bmp.Save(dest, ImageFormat.Jpeg);
            return dest;
        }
        catch
        {
            return null;
        }
    }

    private static Bitmap Resize(Image src, int maxW, int maxH)
    {
        var ratio = Math.Min(maxW / (double)src.Width, maxH / (double)src.Height);
        ratio = Math.Min(ratio, 1.0);
        var w = Math.Max(1, (int)(src.Width * ratio));
        var h = Math.Max(1, (int)(src.Height * ratio));
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(src, 0, 0, w, h);
        return bmp;
    }
}
