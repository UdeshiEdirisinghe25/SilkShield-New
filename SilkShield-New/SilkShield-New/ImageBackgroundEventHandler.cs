using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace SilkShield_New.Data
{
    public class ImageBackgroundEventHandler : PdfPageEventHelper
    {
        private readonly string _backgroundImagePath;

        public ImageBackgroundEventHandler(string backgroundImagePath)
        {
            _backgroundImagePath = backgroundImagePath;
        }

        // Remove the OnStartPage() method completely

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            try
            {
                if (!string.IsNullOrEmpty(_backgroundImagePath))
                {
                    Image backgroundImage = Image.GetInstance(_backgroundImagePath);
                    backgroundImage.SetAbsolutePosition(0, 0);
                    backgroundImage.ScaleAbsolute(document.PageSize.Width, document.PageSize.Height);

                    writer.DirectContentUnder.AddImage(backgroundImage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading background image: {ex.Message}");
            }
        }
    }
}