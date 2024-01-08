using iText.Kernel.Geom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jepun.Core.Pdf.Model
{
    public class JepunTextInfo
    {
        public Vector TopLeft;
        public Vector BottomRight;
        private string m_Text;
        private float m_CharSpaceWidth;

        public string Text
        {
            get { return m_Text; }
        }
        public float Top
        {
            get { return TopLeft.Get(1); }
        }
        public float Left
        {

            get { return TopLeft.Get(0); }
        }
        public float Bottom
        {
            get { return BottomRight.Get(1); }
        }
        public float Right
        {
            get { return BottomRight.Get(0); }

        }
        public float CharSpaceWidth
        {
            get { return m_CharSpaceWidth; }

        }
        /// <summary>
        /// Create a TextInfo.
        /// </summary>
        /// <param name="initialTextChunk"></param>
        public JepunTextInfo(JepunTextChunk initialTextChunk)
        {
            TopLeft = initialTextChunk.AscentLine.GetStartPoint();
            BottomRight = initialTextChunk.DecentLine.GetEndPoint();
            m_Text = initialTextChunk.Text;
            m_CharSpaceWidth = initialTextChunk.CharSpaceWidth;
        }

        /// <summary>
        /// Add more text to this TextInfo.
        /// </summary>
        /// <param name="additionalTextChunk"></param>
        public void appendText(JepunTextChunk additionalTextChunk)
        {
            BottomRight = additionalTextChunk.DecentLine.GetEndPoint();
            m_Text += additionalTextChunk.Text;
        }

        /// <summary>
        /// Add a space to the TextInfo.  This will leave the endpoint out of sync with the text.
        /// The assumtion is that you will add more text after the space which will correct the endpoint.
        /// </summary>
        public void addSpace()
        {
            m_Text += ' ';
        }


    }
}
