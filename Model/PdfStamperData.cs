using System;
using System.Collections.Generic;
using System.Text;

namespace Jepun.Core.Pdf.Model
{
    public class PdfStamperData
    {
		private Dictionary<int, List<PdfText>> pdfTexts = new Dictionary<int, List<PdfText>>();
        private Dictionary<int, List<PdfImg>> pdfImgs = new Dictionary<int, List<PdfImg>>();

		public Dictionary<int, List<PdfText>> Texts
        {
            get
            {
                return pdfTexts;
            }
        }
        public Dictionary<int, List<PdfImg>> Imgs
        {
            get
            {
                return pdfImgs;
            }
        }
        public void AddImg(int pageNum, byte[] imggb, float newWidth, float newHeight, float absoluteX, float absoluteY, float rotationDegrees = 0)
        {
            if (pdfImgs.TryGetValue(pageNum, out var imgs))
            {
                imgs.Add(new PdfImg(imggb, newWidth, newHeight, absoluteX, absoluteY, rotationDegrees));
            }
            else
            {
				var list = new List<PdfImg>
				{
					new PdfImg(imggb, newWidth, newHeight, absoluteX, absoluteY, rotationDegrees)
				};
				pdfImgs.Add(pageNum, list);
            }
        }

        public void AddText(int pageNum, string text, float size, float x, float y, string font = "DFKai-SB", int alignment = 0, float rotation = 0, float fillOpacity = 0.5f, float strokeOpacity = 0.5f)
        {
            if (pdfTexts.TryGetValue(pageNum, out var texts))
            {
                texts.Add(new PdfText(text, size, x, y, font, alignment, rotation, "", fillOpacity, strokeOpacity));
            }
            else
            {
				var list = new List<PdfText>
				{
					new PdfText(text, size, x, y, font, alignment, rotation, "", fillOpacity, strokeOpacity)
				};
				pdfTexts.Add(pageNum, list);
            }
        }
        /// <summary>
        /// 加入連結
        /// </summary>
        /// <param name="pageNum">頁碼</param>
        /// <param name="uri">連結</param>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="width">寬</param>
        /// <param name="height">高</param>
        public void AddUri(int pageNum, string uri, float x, float y, float width,float height )
        {
            if (pdfTexts.TryGetValue(pageNum, out var texts))
            {
                texts.Add(new PdfText("", 0, x, y, "", 0,0, uri, width, height));
            }
            else
            {
                var list = new List<PdfText>
                {
                    new PdfText("", 0, x, y, "", 0, 0, uri, width, height)
                };
                pdfTexts.Add(pageNum, list);
            }
        }



    }

}
