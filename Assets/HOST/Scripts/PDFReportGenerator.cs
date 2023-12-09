using HOST.Debriefing;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace HOST.Debriefing
{
    public class PDFReportGenerator : MonoBehaviour
    {
        public Texture2D DefaultImage;
        public Texture2D HeaderImage;

        private byte[] _defaultImageData;

        private byte[] _headerImageData;

        private DebriefingManager.Comment[] _comments;
        private string _filename;
        private Thread fileThread;

        public bool Success { get; private set; } = false;


        private XGraphics gfx;
        private XTextFormatter tf;
        private XFont font;
        private XFont titleFont;
        private PdfPage page;

        public void Start()
        {
            _defaultImageData = DefaultImage.EncodeToJPG();
            _headerImageData = HeaderImage.EncodeToJPG();
        }

        private void CreateDocument()
        {
            Success = false;
            titleFont = new XFont("Arial", 20, XFontStyle.Bold);
            font = new XFont("Arial", 12, XFontStyle.Regular);

            var document = new PdfDocument();

            // Draw each comment
            XSize commentDimensions = new XSize(250, 150);

            if (_comments != null)
            {
                for (int i = 0; i < _comments.Length; i++)
                {
                    var comment = _comments[i];

                    if (i % 4 == 0)
                    {
                        NewPage(document);
                    }

                    DrawImage(comment.image, new XPoint(20, 100 + 180 * (i % 4)), 200);
                    gfx.DrawString(comment.time.ToString(@"hh\:mm\:ss"), font, XBrushes.Black, new XRect(300, 100 + 180 * (i % 4), commentDimensions.Width, commentDimensions.Height), XStringFormats.TopLeft);
                    DrawMultilineComment(new XRect(300, 120 + 180 * (i % 4), commentDimensions.Width, commentDimensions.Height), comment.text);
                }
            }
            else
            {
                NewPage(document);
            }

            AddPageNumbers(document);

            // Add extension if missing
            if (!_filename.EndsWith(".pdf"))
            {
                _filename += ".pdf";
            }

            Debug.Log("Saving file to " + _filename);

            try
            {
                document.Save(_filename);
            }
            catch
            {
                // Error when saving the file -> maybe the file is open by another process
                Success = false;
                return;
            }


            Success = true;
        }

        public void CreateDocumentAsTask(List<DebriefingManager.Comment> comments, string filename)
        {
            /*if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new Host.FontResolver();
            }*/

            _comments = comments.ToArray();
            _filename = filename;
            fileThread = new Thread(new ThreadStart(CreateDocument));
            fileThread.Start();
        }

        public bool IsSavingInProgress()
        {
            if (fileThread == null)
                return false;

            return fileThread.IsAlive;
        }

        private void NewPage(PdfDocument document)
        {
            if (gfx != null)
            {
                gfx.Dispose();
            }
            page = document.AddPage();
            gfx = XGraphics.FromPdfPage(page);
            tf = new XTextFormatter(gfx);
            Debug.Log($"Page size: {page.Width}, {page.Height}");

            // Draw the header
            DrawImage(_headerImageData, new XPoint(20, 20), 300);
            gfx.DrawString("Résultats de la simulation", titleFont, XBrushes.Black, new XRect(300, 20, 300, 80), XStringFormats.CenterLeft);
        }

        private void AddPageNumbers(PdfDocument document)
        {
            for (int i = 0; i < document.PageCount; i++)
            {
                var page = document.Pages[i];

                gfx.Dispose();
                gfx = XGraphics.FromPdfPage(page);

                gfx.DrawString($"Page {i + 1}/{document.PageCount}", font, XBrushes.Black, new XRect(page.Width - 310, page.Height - 60, 300, 50), XStringFormats.BottomRight);
                gfx.DrawString(DateTime.Today.ToString("d MMMM yyyy"), font, XBrushes.Black, new XRect(10, page.Height - 60, 300, 50), XStringFormats.BottomLeft);
            }
        }

        private void DrawMultilineComment(XRect rect, string text)
        {
            XPen xpen = new XPen(XColors.Navy, 0.4);
            XStringFormat format = new XStringFormat();
            format.LineAlignment = XLineAlignment.Near;
            format.Alignment = XStringAlignment.Near;

            XBrush brush = XBrushes.Black;
            tf.DrawString(text, font, brush, rect, format);
        }

        private void DrawImage(byte[] texture, XPoint position, int width)
        {
            if (texture == null)
            {
                XImage image = XImage.FromStream(() => new MemoryStream(_defaultImageData));
                gfx.DrawImage(image, new XRect(position.X, position.Y, width, image.PixelHeight * width / image.PixelWidth));
            }
            else
            {
                XImage image = XImage.FromStream(() => new MemoryStream(texture));
                gfx.DrawImage(image, new XRect(position.X, position.Y, width, image.PixelHeight * width / image.PixelWidth));
            }
        }
    }
}