using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Carbon.Business.Domain.DataTree
{
    public sealed class DataNodePath : IStructuralEquatable, IStructuralComparable, IComparable
    {
        public string Segment1 { get; set; }
        public string Segment2 { get; set; }
        public string Segment3 { get; set; }

        public DataNodePath() : this(string.Empty, string.Empty, string.Empty)
        {
        }

        public DataNodePath(string segment1) : this(segment1, string.Empty, string.Empty)
        {
        }

        public DataNodePath(string segment1, string segment2) : this(segment1, segment2, string.Empty)
        {
        }

        public DataNodePath(string segment1, string segment2, string segment3)
        {
            Segment1 = segment1;
            Segment2 = segment2;
            Segment3 = segment3;
        }

        public DataNodePath Clone()
        {
            return new DataNodePath(Segment1, Segment2, Segment3);
        }

        public string this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return Segment1;
                    case 1:
                        return Segment2;
                    case 2:
                        return Segment3;
                    default:
                        throw new Exception("Bad segment index " + i);

                }
            }
            set
            {
                switch (i)
                {
                    case 0:
                        Segment1 = value;
                        break;
                    case 1:
                        Segment2 = value;
                        break;
                    case 2:
                        Segment3 = value;
                        break;
                    default:
                        throw new Exception("Bad segment index " + i);

                }
            }
        }

        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            var tuple = other as DataNodePath;
            if (tuple == null || !comparer.Equals(Segment1, tuple.Segment1) || !comparer.Equals(Segment2, tuple.Segment2))
                return false;
            return comparer.Equals(Segment3, tuple.Segment3);
        }
        
        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
                return 1;
            var tuple = other as DataNodePath;
            if (tuple == null)
                throw new ArgumentException("other");
            int num1 = comparer.Compare(Segment1, tuple.Segment1);
            if (num1 != 0)
                return num1;
            int num2 = comparer.Compare(Segment2, tuple.Segment2);
            if (num2 != 0)
                return num2;
            return comparer.Compare(Segment3, tuple.Segment3);
        }
        
        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }
        
        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return CombineHashCodes(comparer.GetHashCode(Segment1), comparer.GetHashCode(Segment2), comparer.GetHashCode(Segment3));
        }        
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            sb.Append((object)Segment1);
            sb.Append(", ");
            sb.Append((object)Segment2);
            sb.Append(", ");
            sb.Append((object)Segment3);
            sb.Append(")");
            return sb.ToString();
        }

        internal static int CombineHashCodes(int h1, int h2, int h3)
        {
            return CombineHashCodes(CombineHashCodes(h1, h2), h3);
        }

        internal static int CombineHashCodes(int h1, int h2)
        {
            return (h1 << 5) + h1 ^ h2;
        }
    }
}