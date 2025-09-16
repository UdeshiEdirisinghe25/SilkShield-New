using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace SilkShield_New.Data
{
    public class ImageBackgroundEventHandler : PdfPageEventHelper
    {
        private string _imagePath;

        public ImageBackgroundEventHandler(string imagePath)
        {
            _imagePath = imagePath;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            try
            {
                // Create a new image instance for EACH page
                Image backgroundImage = Image.GetInstance(_imagePath);
                backgroundImage.SetAbsolutePosition(0, 0); 
                backgroundImage.ScaleAbsolute(document.PageSize.Width, document.PageSize.Height);

                // Add the new image to the page's background
                writer.DirectContentUnder.AddImage(backgroundImage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading background image: {ex.Message}");
            }
        }
    }
}