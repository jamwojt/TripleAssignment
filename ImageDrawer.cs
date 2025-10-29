using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace TripleAssignment;

public static class ImageDrawer
{
    public static byte[]? AddTextToImage(byte[] imageBytes, string text)
    {
        using var image = Image.Load<Rgba32>(imageBytes);

        FontFamily family;
        if (!SystemFonts.TryGet("Arial", out family))
        {
            System.Console.WriteLine("Font not found, returning null");
            return null;
        }
        var font = family.CreateFont(36, FontStyle.Bold);

        var options = new RichTextOptions(font)
        {
            Origin = new PointF(0, 0),
        };

        Console.WriteLine("Drawing on image");
        image.Mutate(ctx =>
        {
            ctx.DrawText(options, text, Color.White);
        });

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }
}
