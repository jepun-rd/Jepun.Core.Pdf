using Jepun.Core.Pdf;
using Jepun.Core.Pdf.Model;

namespace testpdf
{
	internal class Program
	{
		static void Main(string[] args)
		{
			byte[] templateBytes = System.IO.File.ReadAllBytes("test.pdf");
			byte[] Img = File.ReadAllBytes("test.png");
			List<PdfImg> pdfImgs = new List<PdfImg> {
			  new PdfImg(Img, 50, 50, 0, 0)
			 };
			List<string> keyWords = new List<string> {
			 "Reviewed by:",
			 "Prepared by:",
			 "Authorized Signature(s):"
			 };
			List<List<PdfImg>> list = new List<List<PdfImg>>();
			list.Add(pdfImgs);
			list.Add(pdfImgs);
			list.Add(pdfImgs);
			Tuple<byte[], int, string> result = PdfHelper.SearchMultiTextAddImgToPdf(templateBytes, keyWords, list);
			File.WriteAllBytes("result.pdf", result.Item1);
		}
	}
}
