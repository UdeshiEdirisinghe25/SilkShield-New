using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace SilkShield_New.Data
{
    public class ImageBackgroundEventHandler : PdfPageEventHelper
    {
        private readonly string _backgroundImagePath;
        private readonly string _logoPath;

        public ImageBackgroundEventHandler(string backgroundImagePath, string logoPath)
        {
            _backgroundImagePath = backgroundImagePath;
            _logoPath = logoPath;
        }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            try
            {
                if (!string.IsNullOrEmpty(_logoPath))
                {
                    Image logoImage = Image.GetInstance(_logoPath);

                    float logoWidth = 150f;
                    float logoHeight = (logoImage.Height / logoImage.Width) * logoWidth;

                    logoImage.ScaleAbsolute(logoWidth, logoHeight);

                    // Position the logo higher on the page
                    float x = (document.PageSize.Width - logoWidth) / 2;
                    float y = document.PageSize.Height - logoHeight - 20f; // Adjusted to move the logo higher

                    logoImage.SetAbsolutePosition(x, y);

                    writer.DirectContent.AddImage(logoImage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading logo image: {ex.Message}");
            }
        }

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