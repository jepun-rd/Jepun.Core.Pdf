
using iText.Kernel.Geom;

namespace Jepun.Core.Pdf.Model
{
    public class JepunTextChunk : IComparable, ICloneable
    {
        string m_text;
        Vector m_startLocation;
        Vector m_endLocation;
        Vector m_orientationVector;
        int m_orientationMagnitude;
        int m_distPerpendicular;
        float m_distParallelStart;
        float m_distParallelEnd;
        float m_charSpaceWidth;

        public LineSegment AscentLine;
        public LineSegment DecentLine;

        public object Clone()
        {
            JepunTextChunk copy = new JepunTextChunk(m_text, m_startLocation, m_endLocation, m_charSpaceWidth, AscentLine, DecentLine);
            return copy;
        }

        public string Text
        {
            get { return m_text; }
            set { m_text = value; }
        }
        public float CharSpaceWidth
        {
            get { return m_charSpaceWidth; }
            set { m_charSpaceWidth = value; }
        }
        public Vector StartLocation
        {
            get { return m_startLocation; }
            set { m_startLocation = value; }
        }
        public Vector EndLocation
        {
            get { return m_endLocation; }
            set { m_endLocation = value; }
        }

        /// <summary>
        /// Represents a chunk of text, it's orientation, and location relative to the orientation vector
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="startLoc"></param>
        /// <param name="endLoc"></param>
        /// <param name="charSpaceWidth"></param>
        public JepunTextChunk(string txt, Vector startLoc, Vector endLoc, float charSpaceWidth, LineSegment ascentLine, LineSegment decentLine)
        {
            m_text = txt;
            m_startLocation = startLoc;
            m_endLocation = endLoc;
            m_charSpaceWidth = charSpaceWidth;
            AscentLine = ascentLine;
            DecentLine = decentLine;

            m_orientationVector = m_endLocation.Subtract(m_startLocation).Normalize();
            m_orientationMagnitude = (int)(Math.Atan2(m_orientationVector.Get(Vector.I2), m_orientationVector.Get(Vector.I1)) * 1000);

            //see http://mathworld.wolfram.com/Point-LineDistance2-Dimensional.html
            // the two vectors we are crossing are in the same plane, so the result will be purely
            // in the z-axis(out of plane) direction, so we just take the I3 component of the result
            Vector origin = new Vector(0, 0, 1);
            m_distPerpendicular = (int)(m_startLocation.Subtract(origin)).Cross(m_orientationVector).Get(Vector.I3);

            m_distParallelStart = m_orientationVector.Dot(m_startLocation);
            m_distParallelEnd = m_orientationVector.Dot(m_endLocation);
        }

        /// <summary>
        /// true if this location is on the the same line as the other text chunk
        /// </summary>
        /// <param name="textChunkToCompare">the location to compare to</param>
        /// <returns>true if this location is on the the same line as the other</returns>
        public bool sameLine(JepunTextChunk textChunkToCompare)
        {
            if (m_orientationMagnitude != textChunkToCompare.m_orientationMagnitude) return false;
            if (m_distPerpendicular != textChunkToCompare.m_distPerpendicular) return false;
            return true;
        }

        /// <summary>
        /// Computes the distance between the end of 'other' and the beginning of this chunk
        /// in the direction of this chunk's orientation vector.  Note that it's a bad idea
        /// to call this for chunks that aren't on the same line and orientation, but we don't
        /// explicitly check for that condition for performance reasons.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>the number of spaces between the end of 'other' and the beginning of this chunk</returns>
        public float distanceFromEndOf(JepunTextChunk other)
        {
            float distance = m_distParallelStart - other.m_distParallelEnd;
            return distance;
        }

        /// <summary>
        /// Compares based on orientation, perpendicular distance, then parallel distance
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object? obj)
        {
            if (obj == null) throw new ArgumentException("Object is now a TextChunk");

            JepunTextChunk rhs = obj as JepunTextChunk;
            if (rhs != null)
            {
                if (this == rhs) return 0;

                int rslt;
                rslt = m_orientationMagnitude - rhs.m_orientationMagnitude;
                if (rslt != 0) return rslt;

                rslt = m_distPerpendicular - rhs.m_distPerpendicular;
                if (rslt != 0) return rslt;

                //note: it's never safe to check floating point numbers for equality, and if two chunks
                // are truly right on top of each other, which one comes first or second just doesn't matter
                // so we arbitrarily choose this way.
                rslt = m_distParallelStart < rhs.m_distParallelStart ? -1 : 1;

                return rslt;
            }
            else
            {
                throw new ArgumentException("Object is now a TextChunk");
            }
        }
    }
}
