using Carbon.Framework.Extensions;
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
        public string Segment4 { get; set; }

        public DataNodePath() : this(string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }

        public DataNodePath(string segment1) : this(segment1, string.Empty, string.Empty, string.Empty)
        {
        }

        public DataNodePath(string segment1, string segment2) : this(segment1, segment2, string.Empty, string.Empty)
        {
        }

        public DataNodePath(string segment1, string segment2, string segment3) : this(segment1, segment2, segment3, string.Empty)
        {
        }

        public DataNodePath(string segment1, string segment2, string segment3, string segment4)
        {
            Segment1 = segment1;
            Segment2 = segment2;
            Segment3 = segment3;
            Segment4 = segment4;
        }

        public DataNodePath Clone()
        {
            return new DataNodePath(Segment1, Segment2, Segment3, Segment4);
        }

        public DataNodePath Shortened(int length = 6)
        {
            return new DataNodePath(Segment1.Shorten(length), Segment2.Shorten(length), Segment3.Shorten(length), Segment4.Shorten(length));
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
                    case 3:
                        return Segment4;
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
                    case 3:
                        Segment4 = value;
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
            if (tuple == null)
            {
                return false;
            }
            return comparer.Equals(Segment1, tuple.Segment1)
                && comparer.Equals(Segment2, tuple.Segment2)
                && comparer.Equals(Segment3, tuple.Segment3)
                && comparer.Equals(Segment4, tuple.Segment4);
        }

        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }

            var tuple = other as DataNodePath;
            if (tuple == null)
            {
                throw new ArgumentException(nameof(other));
            }

            int diff = comparer.Compare(Segment1, tuple.Segment1);
            if (diff != 0)
            {
                return diff;
            }

            diff = comparer.Compare(Segment2, tuple.Segment2);
            if (diff != 0)
            {
                return diff;
            }

            diff = comparer.Compare(Segment3, tuple.Segment3);
            if (diff != 0)
            {
                return diff;
            }

            return comparer.Compare(Segment4, tuple.Segment4);
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return CombineHashCodes(comparer.GetHashCode(Segment1), comparer.GetHashCode(Segment2),
                comparer.GetHashCode(Segment3), comparer.GetHashCode(Segment4));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            if (!string.IsNullOrEmpty(Segment1))
            {
                sb.Append(Segment1);
            }
            if (!string.IsNullOrEmpty(Segment2))
            {
                sb.Append(", ");
                sb.Append((object)Segment2);
            }
            if (!string.IsNullOrEmpty(Segment3))
            {
                sb.Append(", ");
                sb.Append((object)Segment3);
            }
            if (!string.IsNullOrEmpty(Segment4))
            {
                sb.Append(", ");
                sb.Append((object)Segment4);
            }
            sb.Append("]");
            return sb.ToString();
        }

        internal static int CombineHashCodes(int h1, int h2, int h3, int h4)
        {
            return CombineHashCodes(CombineHashCodes(CombineHashCodes(h1, h2), h3), h4);
        }

        internal static int CombineHashCodes(int h1, int h2)
        {
            return (h1 << 5) + h1 ^ h2;
        }
    }
}