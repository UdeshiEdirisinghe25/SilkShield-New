using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace SilkShield_New.Data
{
    public class ImageBackgroundEventHandler : PdfPageEventHelper
    {
        private Image _backgroundImage;

        public ImageBackgroundEventHandler(string imagePath)
        {
            try
            {
                _backgroundImage = Image.GetInstance(imagePath);
                _backgroundImage.SetAbsolutePosition(0, 0); // Position at bottom-left
                _backgroundImage.ScaleAbsolute(PageSize.A4.Width, PageSize.A4.Height); // Scale to fit A4 page
            }
            catch (Exception ex)
            {
                // Handle image loading error, maybe log it or throw
                Console.WriteLine($"Error loading background image: {ex.Message}");
                _backgroundImage = null; // Set to null if image fails to load
            }
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            if (_backgroundImage != null)
            {
                document.Add(_backgroundImage); // 'writer.AddImage()' වෙනුවට 'document.Add()' යොදන්න
            }
        }
    }
}