
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Pdfoptimizer;
using iText.Pdfoptimizer.Handlers;
using Jepun.Core.Pdf.Compressor;
using Jepun.Core.Pdf.Model;
using System.Globalization;
using System.Text;
using iText.Pdfa;
using iText.Layout.Element;
using iText.Licensing.Base.Strategy;
using iText.IO.Font.Constants;
using iText.Layout;
using static System.Net.Mime.MediaTypeNames;
using iText.Kernel.Pdf.Action;
using iText.Kernel.Pdf.Annot;
using iText.Layout.Borders;

namespace Jepun.Core.Pdf
{
    public static class PdfHelper
    {
        #region IText7 Core
        /// <summary>
        /// pdf加密
        /// </summary>
        /// <param name="reader">來源</param>   
        /// <param name="userPwd">user密碼</param>
        /// <param name="strength">強度(高:安全,但耗時)</param>
        /// <param name="owrPwd">owner密碼</param>
        /// <param name="pmss">權限(ex. EncryptionConstants.ALLOW_SCREENREADERS)</param>
        public static byte[] EncryptPDF(byte[] pdf, string userPwd, bool strength = false, string owrPwd = "jepun", int pmss = EncryptionConstants.ALLOW_SCREENREADERS)
        {

            using (var input = new MemoryStream(pdf))
            using (var output = new MemoryStream())
            {
                var writerProperties = new WriterProperties();
                if (strength)
                {
                    writerProperties.SetStandardEncryption(Encoding.UTF8.GetBytes(userPwd), Encoding.UTF8.GetBytes(owrPwd), pmss, EncryptionConstants.ENCRYPTION_AES_256);
                }
                else
                {
                    writerProperties.SetStandardEncryption(Encoding.UTF8.GetBytes(userPwd), Encoding.UTF8.GetBytes(owrPwd), pmss, EncryptionConstants.ENCRYPTION_AES_128);
                }

                var pdfDoc = new PdfDocument(new PdfReader(input), new PdfWriter(output, writerProperties));
                pdfDoc.Close();

                return output.ToArray();
            }
        }

        /// <summary>
        /// 加簽章檔案加到PDF,每頁都加到一個指定位置
        /// </summary>
        /// <param name="pdfFile">PDF檔案</param>    
        /// <param name="signFile">簽章檔案</param>
        /// <param name="newWidth">寬</param>
        /// <param name="newHeight">高</param>
        /// <param name="absoluteX">X位置</param>
        /// <param name="absoluteY">Y位置</param>
        /// <returns></returns>
        public static byte[] AddStamper(byte[] pdfFile, byte[] signFile, float newWidth, float newHeight, float absoluteX, float absoluteY, float rotationDegrees = 0)
        {

            using (var input = new MemoryStream(pdfFile))
            using (var output = new MemoryStream())
            {
                using (var pdfDoc = new PdfDocument(new PdfReader(input), new PdfWriter(output)))
                {
                    ImageData img = ImageDataFactory.Create(signFile);
                    for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                    {
                        PdfPage page = pdfDoc.GetPage(i);
                        PdfCanvas canvas = new PdfCanvas(page);
                        if (page.GetRotation() > 0)
                        {
                            switch (page.GetRotation())
                            {
                                case 90:
                                    canvas.ConcatMatrix(0.0, 1.0, -1.0, 0.0, page.GetPageSizeWithRotation().GetTop(), 0.0);
                                    break;
                                case 180:
                                    canvas.ConcatMatrix(-1.0, 0.0, 0.0, -1.0, page.GetPageSizeWithRotation().GetRight(), page.GetPageSizeWithRotation().GetTop());
                                    break;
                                case 270:
                                    canvas.ConcatMatrix(0.0, -1.0, 1.0, 0.0, 0.0, page.GetPageSizeWithRotation().GetRight());
                                    break;
                            }
                        }
                        canvas.SaveState().AddImageWithTransformationMatrix(img, newWidth, 0, 0, newHeight, absoluteX, absoluteY, false);
                    }
                    
                }
                return output.ToArray();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pdfFile"></param>
        /// <param name="pdfStamperData"></param>
        /// <returns></returns>
        public static byte[] AddStamper(byte[] pdfFile, PdfStamperData pdfStamperData)
        {
            byte[] retVal = Array.Empty<byte>();
            MemoryStream outStream = null;
            PdfDocument pdfDoc = null;
            try
            {
                outStream = new MemoryStream();
                pdfDoc = new PdfDocument(new PdfReader(new MemoryStream(pdfFile)), new PdfWriter(outStream));
                int pageCount = pdfDoc.GetNumberOfPages();
                var texts = pdfStamperData.Texts;
                var imgs = pdfStamperData.Imgs;
                for (int i = 1; i <= pageCount; i++)
                {
                    PdfPage page = pdfDoc.GetPage(i);
                    PdfCanvas canvas = new PdfCanvas(page);
                    if (page.GetRotation() > 0)
                    {
                        switch (page.GetRotation())
                        {
                            case 90:
                                canvas.ConcatMatrix(0.0, 1.0, -1.0, 0.0, page.GetPageSizeWithRotation().GetTop(), 0.0);
                                break;
                            case 180:
                                canvas.ConcatMatrix(-1.0, 0.0, 0.0, -1.0, page.GetPageSizeWithRotation().GetRight(), page.GetPageSizeWithRotation().GetTop());
                                break;
                            case 270:
                                canvas.ConcatMatrix(0.0, -1.0, 1.0, 0.0, 0.0, page.GetPageSizeWithRotation().GetRight());
                                break;
                        }
                    }


                    if (texts.TryGetValue(i, out List<PdfText>? textList))
                    {
                        foreach (PdfText txt in textList)
                        {
                            if (string.IsNullOrEmpty(txt.Uri))
                            {
                                AddText(canvas, txt.Text, txt.Size, txt.X, txt.Y, txt.FontFamily);
                            }
                            else
                            {
                                //width 用  txt.FillOpacity  height 用 txt.StrokeOpacity
                                page.AddAnnotation(AddUri(txt.Uri,txt.X, txt.Y, txt.FillOpacity, txt.StrokeOpacity));
                            }                            
                        }
                    }
                    if (imgs.TryGetValue(i, out List<PdfImg>? imgList))
                    {
                        foreach (PdfImg img in imgList)
                        {
                            AddImage(canvas, img.Imggb, img.NewWidth, img.NewHeight, img.AbsoluteX, img.AbsoluteY, img.RotationDegrees);
                        }
                    }
                }
                pdfDoc.Close();
                retVal = outStream.ToArray();
                outStream.Close();
            }
            catch 
            {
                throw;
            }
            finally
            {
                pdfDoc?.Close();
                outStream?.Close();
            }
            return retVal;
        }
        private static PdfLinkAnnotation AddUri(string uri, float x, float y, float w, float h) 
        {             
            // Create a link annotation
            Rectangle rect = new Rectangle(x, y, w, h);          
            PdfLinkAnnotation link = new PdfLinkAnnotation(rect);
            // Create an action (for example, opening a website)
            PdfAction action = PdfAction.CreateURI(uri);
            // Set the action for the link annotation
            link.SetAction(action);
            PdfAnnotationBorder border = new PdfAnnotationBorder(0,0,0);
            link.SetBorder(border);
            return link;
        }
        private static void AddText(PdfCanvas canvas, string text, float fontSize, float x, float y, string fonts = "DFKai-SB")
        {

            //Begin text mode
            canvas.BeginText();

            //Set the font and font size
            string fontPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            FontProgramFactory.RegisterFontDirectory(fontPath);
            //canvas.SetFontAndSize(PdfFontFactory.CreateRegisteredFont("Microsoft JhengHei"), fontSize);//正黑體
            if (PdfFontFactory.IsRegistered(fonts))
            {
                canvas.SetFontAndSize(PdfFontFactory.CreateRegisteredFont(fonts), fontSize);
            }
            else {
				canvas.SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), fontSize);
			}
            canvas.SetLeading(fontSize);
            //canvas.SetFontAndSize(PdfFontFactory.CreateRegisteredFont("Verdana"), fontSize);

            //FontProgramFactory.RegisterSystemFontDirectories(); 
            //canvas.SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN), fontSize);

            //canvas.SetFontAndSize(PdfFontFactory.CreateFont("NotoSansCJKsc-Regular", "UniGB-UCS2-H",EmbeddingStrategy.FORCE_EMBEDDED), fontSize);

            //Set the text color
            //canvas.SetFillColorRgb(0, 0, 0);

            //set xy
            canvas.MoveText(x, y);

            //Add the text to the canvas
            //canvas.ShowTextAligned("Hello World", 100, 100, iText.Layout.Properties.TextAlignment.LEFT);

           
			var lines = text.Split('\n');  // 使用換行符號分割文字為多行
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
				//canvas.NewlineShowText(line);
                if (i == 0) { canvas.ShowText(line); }
                else { canvas.NewlineShowText(line); }
            }

			//End text mode
			canvas.EndText();
        }
        private static void AddImage(PdfCanvas canvas, byte[] imgData, float newWidth, float newHeight, float absoluteX, float absoluteY, float rotationDegrees = 0)
        {
            ImageData img = ImageDataFactory.Create(imgData);
            canvas.SaveState().AddImageWithTransformationMatrix(img, newWidth, 0, 0, newHeight, absoluteX, absoluteY, false);
        }


        public static int GetPages(byte[] pdf)
        {
            using (MemoryStream input = new MemoryStream(pdf))
            using (PdfReader reader = new PdfReader(input))
            using (PdfDocument document = new PdfDocument(reader))
            {
                return document.GetNumberOfPages();
            }
        }
        #endregion

        #region IText7 PdfOptimizer
        /// <summary>
        /// PDF 壓縮 
        /// 注意!!!!!!!!!!
        /// 需引用itext.licensing.base
        /// 載入方法LicenseKey.LoadLicenseFile(new FileInfo(System.IO.Path.Combine(IOHelper.BaseDirectory, "49a3851a95e3fb62bca311993ed1c095d8d3dc6af847af4caa9ae18d8a6310ab.json")));
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pdfOption"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static byte[] PdfOptimizer(byte[] data,PdfOption pdfOption, Dictionary<string, Func<byte[], int, byte[]>> func)
        {
            //LicenseKey外面載入
            //LicenseKey.LoadLicenseFile(new FileInfo(System.IO.Path.Combine(IOHelper.BaseDirectory, "49a3851a95e3fb62bca311993ed1c095d8d3dc6af847af4caa9ae18d8a6310ab.json")));

            PdfOptimizer optimizer = new PdfOptimizer();
            //Report Log 先關閉
            //FileReportPublisher publisher = new FileReportPublisher(new FileInfo(System.IO.Path.Combine("PdfOptimize", "report.txt")));
            //FileReportBuilder builder = new FileReportBuilder(SeverityLevel.INFO, publisher);
            //optimizer.SetReportBuilder(builder);
            if (pdfOption.GethasFontDuplication())
            {
                optimizer.AddOptimizationHandler(new FontDuplicationOptimizer());
            }
            if (pdfOption.GethasFontSubsetting())
            {
                optimizer.AddOptimizationHandler(new FontSubsettingOptimizer());
            }

            if (pdfOption.GethasImageQuality())
            {
                ImageQualityOptimizer tiff_optimizer = new ImageQualityOptimizer();
                tiff_optimizer.SetTiffProcessor(new JepunImgCompressor(pdfOption.GetImageQuality(), func["Png"]));
                optimizer.AddOptimizationHandler(tiff_optimizer);

                ImageQualityOptimizer jpeg_optimizer = new ImageQualityOptimizer();
                jpeg_optimizer.SetJpegProcessor(new JepunImgCompressor(pdfOption.GetImageQuality(), func["Jpg"]));
                optimizer.AddOptimizationHandler(jpeg_optimizer);

                ImageQualityOptimizer png_optimizer = new ImageQualityOptimizer();
                png_optimizer.SetPngProcessor(new JepunImgCompressor(pdfOption.GetImageQuality(), func["Png"]));
                optimizer.AddOptimizationHandler(png_optimizer);
            }
            if (pdfOption.GethasCompression())
            {
                optimizer.AddOptimizationHandler(new CompressionOptimizer());
            }
            using (MemoryStream reader = new MemoryStream(data))
            using (MemoryStream write = new MemoryStream())
            {
                var datas = optimizer.Optimize(
                        reader,
                        write);
                byte[] returnData = write.ToArray();
                return returnData;
            }



        }

        #endregion

        #region IText7 SearchText
        public static List<(string Text, float StartX, float StartY, float EndX, float EndY, int Page)> SearchText(byte[] pdf, string Text)
        {
            List<(string, float, float, float, float, int)> result = new List<(string, float, float, float, float, int)>();

            using (MemoryStream input = new MemoryStream(pdf))
            using (PdfReader reader = new PdfReader(input))
            using (PdfDocument document = new PdfDocument(reader))
            {
                PdfDocumentContentParser parser = new PdfDocumentContentParser(document);
                //List<string> searchKeywords = new List<string>() { Text };
                for (int pageNumber = 1; pageNumber <= document.GetNumberOfPages(); pageNumber++)
                {
                    // 創建文字搜尋方式
                    FullTextExtractionStrategy strategy = new FullTextExtractionStrategy(Text);
                    parser.ProcessContent(pageNumber, strategy);
                    // 抓到所有文本所在訊息
                    List<MatchedTextInfo> matchedTextContainers = strategy.GetMatchedTexts();

                    // 處理匹配的文本及其位置資訊
                    foreach (MatchedTextInfo container in matchedTextContainers)
                    {
                        string matchedText = container.Text;
                        Rectangle boundingRectangle = container.Rectangle;
                        (string, float, float, float, float, int) resultdetail;
                        resultdetail.Item1 = container.Text;
                        resultdetail.Item2 = boundingRectangle.GetLeft();
                        resultdetail.Item3 = boundingRectangle.GetTop();
                        resultdetail.Item4 = boundingRectangle.GetX();//boundingRectangle.GetLeft() + boundingRectangle.GetWidth();
                        resultdetail.Item5 = boundingRectangle.GetTop() - boundingRectangle.GetHeight();
                        resultdetail.Item6 = pageNumber;
                        result.Add(resultdetail);
                    }
                }
            }
            return result;
        }

        public static List<(string Text, float StartX, float StartY, float EndX, float EndY, int Page, float CharSpaceWidth, string SearchText)> SearchTextChunk(byte[] pdf, string searchText)
        {
            List<(string, float, float, float, float, int, float, string)> result = new List<(string, float, float, float, float, int, float, string)>();

            using (MemoryStream input = new MemoryStream(pdf))
            using (PdfReader reader = new PdfReader(input))
            using (PdfDocument document = new PdfDocument(reader))
            {
                for (int page = 1; page <= document.GetNumberOfPages(); page++)
                {
                    var strategy = new ChunkTextExtractionStrategy();
                    var parser = new PdfCanvasProcessor(strategy);
                    parser.ProcessPageContent(document.GetPage(page));

                    List<JepunTextInfo> textLocationInfo = strategy.TextLocationInfo;
                    strategy.GetResultantText();

                    var searchResults = textLocationInfo.Where(p => p.Text.Contains(searchText)).OrderBy(p => p.Top).Reverse().ToList();
                    // var searchResults = textLocationInfo.Where(p => p.Text.Contains(searchText)).OrderBy(p => p.GetBaseline().GetBoundingRectange().GetBottom()).Reverse().ToList();
                    foreach (var searchResult in searchResults)
                    {
                        (string, float, float, float, float, int, float, string) resultDetail;
                        resultDetail.Item1 = searchResult.Text;
                        resultDetail.Item2 = searchResult.Left;
                        resultDetail.Item3 = searchResult.Top;
                        resultDetail.Item4 = searchResult.Right;
                        resultDetail.Item5 = searchResult.Bottom;
                        resultDetail.Item6 = page;
                        resultDetail.Item7 = searchResult.CharSpaceWidth;
                        resultDetail.Item8 = searchText;
                        //resultDetail.Item1 = searchResult.Text();
                        //resultDetail.Item2 = searchResult.GetBaseline().GetBoundingRectange().GetLeft();
                        //resultDetail.Item3 = searchResult.GetBaseline().GetBoundingRectange().GetTop();
                        //resultDetail.Item4 = searchResult.GetBaseline().GetBoundingRectange().GetRight();
                        //resultDetail.Item5 = searchResult.GetBaseline().GetBoundingRectange().GetBottom();
                        //resultDetail.Item6 = page;
                        //resultDetail.Item7 = searchResult.GetSingleSpaceWidth();
                        //resultDetail.Item8 = searchText;
                        result.Add(resultDetail);
                    }
                }

            }
            
            return result;
        }

        /// <summary>
        /// 尋找Pdf多個字元位置並插入新的圖片
        /// </summary>
        /// <param name="pdfFile"></param>
        /// <param name="searchTexts"></param>
        /// <param name="pdfImgs"></param>        
        /// <returns></returns>
        public static Tuple<byte[], int, string> SearchMultiTextAddImgToPdf(byte[] pdfFile, List<string> searchTexts, List<List<PdfImg>> pdfImgs)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<List<(string, float, float, float, float, int)>> results = new List<List<(string, float, float, float, float, int)>>();

            foreach (string keyWord in searchTexts)
            {
                // 取得各個關鍵字,所在頁數與位置
                List<(string Text, float StartX, float StartY, float EndX, float EndY, int Page)> tmp = SearchText(pdfFile, keyWord);
                if (tmp.Count == 0)
                {
                    stringBuilder.Append($"找不到該關鍵字{keyWord}");
                    continue;
                }
                results.Add(tmp);
            }
            if (!string.IsNullOrEmpty(stringBuilder.ToString()))
            {
                Console.WriteLine(stringBuilder.ToString());
                //return Tuple.Create<byte[], int, string>(pdfFile, 0, stringBuilder.ToString());
            }
            using (var input = new MemoryStream(pdfFile))
            using (var output = new MemoryStream())
            {
                int pageCount = 0;
                using (var pdfDoc = new PdfDocument(new PdfReader(input), new PdfWriter(output)))
                {
                    pageCount = pdfDoc.GetNumberOfPages();
                    for (int i = 1; i <= pageCount; i++)
                    {
                        PdfPage page = pdfDoc.GetPage(i);
                        PdfCanvas canvas = new PdfCanvas(page);
                        if (page.GetRotation() > 0)
                        {
                            switch (page.GetRotation())
                            {
                                case 90:
                                    canvas.ConcatMatrix(0.0, 1.0, -1.0, 0.0, page.GetPageSizeWithRotation().GetTop(), 0.0);
                                    break;
                                case 180:
                                    canvas.ConcatMatrix(-1.0, 0.0, 0.0, -1.0, page.GetPageSizeWithRotation().GetRight(), page.GetPageSizeWithRotation().GetTop());
                                    break;
                                case 270:
                                    canvas.ConcatMatrix(0.0, -1.0, 1.0, 0.0, 0.0, page.GetPageSizeWithRotation().GetRight());
                                    break;
                            }
                        }
                        for (int j = 0; j < results.Count(); j++)
                        {// 各個關鍵字,所在頁數與位置 集合
                            for (int k = 0; k < results[j].Count(); k++)
                            {// 所在頁數與位置
                                (string, float, float, float, float, int) result = results[j][k];
                                if (result.Item6 == i)// 確認 關鍵字 的頁數
                                {
                                    for (int l = 0; l < pdfImgs[j].Count(); l++)
                                    {
                                        float absoluteX = result.Item4 + pdfImgs[j][l].AbsoluteX;
                                        float absoluteY = result.Item5 + pdfImgs[j][l].AbsoluteY;
                                        AddImage(canvas, pdfImgs[j][l].Imggb, pdfImgs[j][l].NewWidth, pdfImgs[j][l].NewHeight, absoluteX, absoluteY);
                                    }
                                }
                            }
                        }
                    }
                }
                return Tuple.Create<byte[], int, string>(output.ToArray(), pageCount, "");
            }
        }

        /// <summary>
        /// 尋找Pdf多個字元位置並插入新的圖片 ,但X的位置是固定傳入
        /// </summary>
        /// <param name="pdfFile"></param>
        /// <param name="searchTexts"></param>
        /// <param name="startXs">X的位置是固定傳入</param>
        /// <param name="pdfImgs"></param>
        /// <returns></returns>
        public static Tuple<byte[], int> SearchMultiTextChunkAddImgToPdf(byte[] pdfFile, List<string> searchTexts, List<float> startXs, List<List<PdfImg>> pdfImgs)
        {
            List<List<(string, float, float, float, float, int, float, string)>> results = new List<List<(string, float, float, float, float, int, float, string)>>();
            foreach (string keyWord in searchTexts)
            {
                // 取得各個關鍵字,所在頁數與位置
                results.Add(SearchTextChunk(pdfFile, keyWord));
            }
            using (var input = new MemoryStream(pdfFile))
            using (var output = new MemoryStream())
            {
                int pageCount = 0;
                using (var pdfDoc = new PdfDocument(new PdfReader(input), new PdfWriter(output)))
                {
                    pageCount = pdfDoc.GetNumberOfPages();
                    for (int i = 1; i <= pageCount; i++)
                    {
                        PdfPage page = pdfDoc.GetPage(i);
                        PdfCanvas canvas = new PdfCanvas(page);
                        if (page.GetRotation() > 0)
                        {
                            switch (page.GetRotation())
                            {
                                case 90:
                                    canvas.ConcatMatrix(0.0, 1.0, -1.0, 0.0, page.GetPageSizeWithRotation().GetTop(), 0.0);
                                    break;
                                case 180:
                                    canvas.ConcatMatrix(-1.0, 0.0, 0.0, -1.0, page.GetPageSizeWithRotation().GetRight(), page.GetPageSizeWithRotation().GetTop());
                                    break;
                                case 270:
                                    canvas.ConcatMatrix(0.0, -1.0, 1.0, 0.0, 0.0, page.GetPageSizeWithRotation().GetRight());
                                    break;
                            }
                        }
                        for (int j = 0; j < results.Count(); j++)
                        {// 各個關鍵字,所在頁數與位置 集合
                            for (int k = 0; k < results[j].Count(); k++)
                            {// 所在頁數與位置
                                (string, float, float, float, float, int, float, string) result = results[j][k];
                                if (result.Item6 == i)// 確認 關鍵字 的頁數
                                {
                                    for (int l = 0; l < pdfImgs[j].Count(); l++)
                                    {
                                        float absoluteX = startXs[j] + pdfImgs[j][l].AbsoluteX;
                                        float absoluteY = result.Item5 + pdfImgs[j][l].AbsoluteY;
                                        AddImage(canvas, pdfImgs[j][l].Imggb, pdfImgs[j][l].NewWidth, pdfImgs[j][l].NewHeight, absoluteX, absoluteY);
                                    }
                                }
                            }
                        }
                    }
                }
                return Tuple.Create<byte[], int>(output.ToArray(), pageCount);
            }
        }
        #endregion

        #region IText7 GetText
        public static Dictionary<int, string> GetText(byte[] pdf)
        {
            Dictionary<int, string> extractedTextDict = new Dictionary<int, string>();
            using (MemoryStream input = new MemoryStream(pdf))
            using (PdfReader reader = new PdfReader(input))
            using (PdfDocument document = new PdfDocument(reader))
            {
                //斷行
                // Loop through all the pages and extract text
                for (int page = 1; page <= document.GetNumberOfPages(); page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    PdfPage pdfPage = document.GetPage(page);
                    PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
                    parser.ProcessPageContent(pdfPage);
                    string extractedText = strategy.GetResultantText();
                    extractedTextDict.Add(page, extractedText);
                }

                //不斷行 + 空白
                //for (int page = 1; page <= document.GetNumberOfPages(); page++)
                //{
                //    var strategy = new CustomTextEventListener();
                //    PdfPage pdfPage = document.GetPage(page);
                //    PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
                //    parser.ProcessPageContent(pdfPage);
                //    string extractedText = strategy.GetResultantText();
                //    extractedTextDict.Add(page, extractedText.Replace(" ", ""));
                //    extractedTextDict.Add(page, extractedText);
                //}

                //
                //for (int page = 1; page <= document.GetNumberOfPages(); page++)
                //{
                //    var strategy = new ChunkTextExtractionStrategy();
                //    var parser = new PdfCanvasProcessor(strategy);
                //    parser.ProcessPageContent(document.GetPage(page));

                //    List<JepunTextInfo> textLocationInfo = strategy.TextLocationInfo;
                //    //strategy.GetResultantText();
                //    string extractedText = strategy.GetResultantText();
                //    extractedTextDict.Add(page, extractedText);
                //}

            }

            return extractedTextDict;
        }

      
        public static Dictionary<int, string> GetTextLine(byte[] pdf)
        {
            Dictionary<int, string> extractedTextDict = new Dictionary<int, string>();
            using (MemoryStream input = new MemoryStream(pdf))
            using (PdfReader reader = new PdfReader(input))
            using (PdfDocument document = new PdfDocument(reader))
            {
                //不斷行 + 空白
                for (int page = 1; page <= document.GetNumberOfPages(); page++)
                {
                    var strategy = new CustomTextEventListener();
                    PdfPage pdfPage = document.GetPage(page);
                    PdfCanvasProcessor parser = new PdfCanvasProcessor(strategy);
                    parser.ProcessPageContent(pdfPage);
                    string extractedText = strategy.GetResultantText();                     
                    extractedTextDict.Add(page, extractedText);
                }
            }

            return extractedTextDict;
        }

        public static Dictionary<int, string> GetTextChunk(byte[] pdf)
        {
            Dictionary<int, string> extractedTextDict = new Dictionary<int, string>();
            using (MemoryStream input = new MemoryStream(pdf))
            using (PdfReader reader = new PdfReader(input))
            using (PdfDocument document = new PdfDocument(reader))
            {
                for (int page = 1; page <= document.GetNumberOfPages(); page++)
                {
                    var strategy = new ChunkTextExtractionStrategy();
                    var parser = new PdfCanvasProcessor(strategy);
                    parser.ProcessPageContent(document.GetPage(page));

                    List<JepunTextInfo> textLocationInfo = strategy.TextLocationInfo;
                    //strategy.GetResultantText();
                    string extractedText = strategy.GetResultantText();
                    extractedTextDict.Add(page, extractedText);
                }
            }
            return extractedTextDict;
        }


        private class CustomTextEventListener : IEventListener
        {

            private string currentParagraph;
            private List<string> datas = new List<string>();
            public CustomTextEventListener()
            {
                this.currentParagraph = string.Empty;
            }

            public void EventOccurred(IEventData data, EventType type)
            {
                if (type == EventType.RENDER_TEXT)
                {
                    TextRenderInfo renderInfo = (TextRenderInfo)data;
                    string text = renderInfo.GetText();
                    // Append the text to the current paragraph
                    currentParagraph += (text.Replace(" ", "") + " ");
                }
            }

            public ICollection<EventType> GetSupportedEvents()
            {
                return new List<EventType> { EventType.RENDER_TEXT };
            }

            public string GetResultantText() {
                return currentParagraph;
            }
        }



        #endregion
    }


}
