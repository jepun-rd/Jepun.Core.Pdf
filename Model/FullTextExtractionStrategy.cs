using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

using Rectangle = iText.Kernel.Geom.Rectangle;

namespace Jepun.Core.Pdf.Model
{
    class FullTextExtractionStrategy : LocationTextExtractionStrategy
    {
        private readonly string searchKeyword;
        private string tmpKeyword = "";
        private readonly List<MatchedTextInfo> matchedTexts;

        public FullTextExtractionStrategy(string keyword)
        {
            searchKeyword = keyword;
            matchedTexts = new List<MatchedTextInfo>();
        }

        public override void EventOccurred(IEventData data, EventType type)
        {
            if (type != EventType.RENDER_TEXT)
            {
                base.EventOccurred(data, type);
                return;
            }
            TextRenderInfo renderInfo = (TextRenderInfo)data;
            string text = renderInfo.GetText();
            Rectangle rectangle = renderInfo.GetDescentLine().GetBoundingRectangle();
            Console.WriteLine(text);
            //if(text == "覆")
            //{
            //    Console.WriteLine("find");
            //}
            // 搜尋關鍵字
            if (text.Contains(searchKeyword))
            {  // 找到關鍵字資訊，儲存起來                   
                if (!searchKeyword.Equals(text))
                {//返回   EX:  有權人簽樣 :                           覆核：                        經辦：
                    float count2Byte = 0;
                    foreach(char s in text.Substring(0, text.IndexOf(searchKeyword)))
                    {
                        if(s == ' ') //空白
                        {
                            count2Byte += 0.5f;
                        }
                        else
                        {
                            count2Byte += 1f;
                        }
                    }
                    rectangle.SetX(rectangle.GetLeft() + (searchKeyword.Length + count2Byte) * renderInfo.GetFontSize());                    
                }
                else
                {//完全相同
                    rectangle.SetX(rectangle.GetLeft() + rectangle.GetWidth());
                }                  
                MatchedTextInfo matchedText = new MatchedTextInfo(text, rectangle);
                matchedTexts.Add(matchedText);
                tmpKeyword = "";
                base.EventOccurred(data, type);
                return;
            }

            if (searchKeyword.Contains(text))
            {//一個字返回,要組回來
                tmpKeyword += text;
                //確認順序一致
                if (searchKeyword.IndexOf(tmpKeyword) > 0)
                {//順序不一致
                    tmpKeyword = "";
                    base.EventOccurred(data, type);
                    return;
                }               
                if (searchKeyword == tmpKeyword)
                {                             
                    rectangle.SetX(rectangle.GetLeft() + rectangle.GetWidth());
                    MatchedTextInfo matchedText = new MatchedTextInfo(searchKeyword, rectangle);
                    matchedTexts.Add(matchedText);
                    tmpKeyword = "";
                }
                base.EventOccurred(data, type);
                return;
            }             
            tmpKeyword = "";           
            base.EventOccurred(data, type);
        }

        public List<MatchedTextInfo> GetMatchedTexts()
        {
            return matchedTexts;
        }
    }

    class MatchedTextInfo
    {
        public string Text { get; }
        public Rectangle Rectangle { get; }

        public MatchedTextInfo(string text, Rectangle rectangle)
        {
            Text = text;
            Rectangle = rectangle;
        }
    }
}
