using System;
using System.Drawing;
using Xunit;

namespace PdfSharpCore.Test
{
    public class LayerTests
    {
        [Fact]
        public void AddsLayer()
        {
            PdfSharpCore.Pdf.PdfDocument xDoc = new PdfSharpCore.Pdf.PdfDocument();
            xDoc.Pages.Add(new PdfSharpCore.Pdf.PdfPage() { Width = 800, Height = 800 });

            PdfSharpCore.Pdf.PdfPage xPage = xDoc.Pages[0];
            PdfSharpCore.Drawing.XGraphics gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(xPage);

            PdfSharpCore.Drawing.XPen pen = PdfSharpCore.Drawing.XPens.Red;
            //System.Drawing.PointF p0 = new PointF();
            //System.Drawing.PointF p1 = new PointF(800, 800);
            Drawing.XPoint p2 = new Drawing.XPoint();
            Drawing.XPoint p3 = new Drawing.XPoint(800, 800);

            gfx.BeginMarkedContentPropList("oc1");
            gfx.DrawLine(pen, p2, p3);

            gfx.EndMarkedContent();
            PdfSharpCore.Pdf.Advanced.PdfResources rsx = (xPage.Elements["/Resources"] as PdfSharpCore.Pdf.Advanced.PdfResources);
            rsx.AddOCG("oc1", "Layer 1");
            xDoc.Save("PdfDoc.pdf");

            xDoc.Close();

            Assert.True(true);
        }
    }
}
