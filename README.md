Use iText to process PDF file imprinting.

<code>
using iText.Licensing.Base;
using Jepun.Core.Pdf;
using Jepun.Core.Pdf.Model;
namespace Jepun.Core.Pdf.Test
{
  static void Main(string[] args)
  {
    byte[] templateBytes = System.IO.File.ReadAllBytes("test.pdf");
    byte[] Img = File.ReadAllBytes("test.png");
    List<PdfImg> pdfImgs = new List<PdfImg> {
      new PdfImg(Img, 50, 50, 0, 0)
     }; 
     keyWords = new List<string> {
     "Reviewed by:",
     "Prepared by:",
     "Authorized Signature(s):"
     };
     List<List<PdfImg>> list = new List<List<PdfImg>>();
     list.Add(pdfImgs);
     list.Add(pdfImgs);
     list.Add(pdfImgs);
     Tuple<byte[], int,string> result = PdfHelper.SearchMultiTextAddImgToPdf(templateBytes, keyWords, list);
     File.WriteAllBytes("result.pdf", result.Item1);
  }
}
</code>
