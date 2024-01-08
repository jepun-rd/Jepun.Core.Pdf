using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

namespace Jepun.Core.Pdf.Model
{


    public class ChunkTextExtractionStrategy : LocationTextExtractionStrategy
    {
        private List<JepunTextChunk> m_locationResult = new List<JepunTextChunk>();
        private List<JepunTextInfo> m_TextLocationInfo = new List<JepunTextInfo>();
        public List<JepunTextChunk> LocationResult
        {
            get { return m_locationResult; }
        }
        public List<JepunTextInfo> TextLocationInfo
        {
            get { return m_TextLocationInfo; }
        }

        /// <summary>
        /// Creates a new LocationTextExtractionStrategyEx
        /// </summary>
        public ChunkTextExtractionStrategy()
        {
        }

        /// <summary>
        /// Returns the result so far
        /// </summary>
        /// <returns>a String with the resulting text</returns>
        public override String GetResultantText()
        {
            m_locationResult.Sort();

            StringBuilder sb = new StringBuilder();
            JepunTextChunk lastChunk = null;
            JepunTextInfo lastTextInfo = null;
            foreach (JepunTextChunk chunk in m_locationResult)
            {
                if (lastChunk == null)
                {
                    sb.Append(chunk.Text);
                    lastTextInfo = new JepunTextInfo(chunk);
                    m_TextLocationInfo.Add(lastTextInfo);
                }
                else
                {
                    if (chunk.sameLine(lastChunk))
                    {
                        float dist = chunk.distanceFromEndOf(lastChunk);

                        if (dist < -chunk.CharSpaceWidth)
                        {
                            sb.Append(' ');
                            lastTextInfo.addSpace();
                        }
                        //append a space if the trailing char of the prev string wasn't a space && the 1st char of the current string isn't a space
                        else if (dist > chunk.CharSpaceWidth / 2.0f && chunk.Text[0] != ' ' && lastChunk.Text[lastChunk.Text.Length - 1] != ' ')
                        {
                            sb.Append(' ');
                            lastTextInfo.addSpace();
                        }
                        sb.Append(chunk.Text);
                        lastTextInfo.appendText(chunk);
                    }
                    else
                    {
                        sb.Append('\n');
                        sb.Append(chunk.Text);
                        lastTextInfo = new JepunTextInfo(chunk);
                        m_TextLocationInfo.Add(lastTextInfo);
                    }
                }
                lastChunk = chunk;
            }
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderInfo"></param>
        public override void EventOccurred(IEventData renderInfo, EventType type)
        {
            if (renderInfo is TextRenderInfo textRenderInfo)
            {
                LineSegment segment = textRenderInfo.GetBaseline();
                JepunTextChunk location = new JepunTextChunk(textRenderInfo.GetText(), segment.GetStartPoint(), segment.GetEndPoint(), textRenderInfo.GetSingleSpaceWidth(), textRenderInfo.GetAscentLine(), textRenderInfo.GetDescentLine());
                m_locationResult.Add(location);
            }
        }
    }

}
