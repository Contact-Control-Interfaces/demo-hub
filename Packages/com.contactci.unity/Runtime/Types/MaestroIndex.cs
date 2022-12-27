namespace Maestro
{
    public enum WhichFinger
    {
        Thumb, Index, Middle, Ring, Little, Palm
    };

    public enum PointOnFinger
    {
        Tip, Middle, Base, ProximalDigit, Distal, DistalDigit
    };

    public struct MaestroIndex
    {
        public WhichFinger finger { get; set; }
        public PointOnFinger point { get; set; }

        public MaestroIndex(WhichFinger finger, PointOnFinger point)
        {
            this.finger = finger;
            this.point = point;
        }
        
        public override string ToString()
        {
            return string.Format("({0}, {1})", ToString(finger), ToString(point));
        }

        public static string ToString(WhichFinger whichFinger)
        {
            string result = "";
            switch (whichFinger) {
                case WhichFinger.Thumb: result = "Thumb"; break;
                case WhichFinger.Index: result = "Index"; break;
                case WhichFinger.Middle: result = "Middle"; break;
                case WhichFinger.Ring: result = "Ring"; break;
                case WhichFinger.Little: result = "Little"; break;
                case WhichFinger.Palm: result = "Palm"; break;
            }
            return result;
        }

        public static string ToString(PointOnFinger pof)
        {
            string result = "";
            switch (pof) {
                case PointOnFinger.Base: result = "Base"; break;
                case PointOnFinger.Middle: result = "Middle"; break;
                case PointOnFinger.Distal: result = "Distal"; break;
                case PointOnFinger.Tip: result = "Tip"; break;
                case PointOnFinger.DistalDigit: result = "DistalDigit"; break;
                case PointOnFinger.ProximalDigit: result = "ProximalDigit"; break;
            }
            return result;
        }
    }
}
