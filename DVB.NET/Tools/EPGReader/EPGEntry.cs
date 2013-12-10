using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace EPGReader
{
    public class EPGEntry : ListViewItem
    {
        public class Comparer : System.Collections.IComparer
        {
            public Comparer()
            {
            }

            #region IComparer Members

            public int Compare( object x, object y )
            {
                // Change type
                EPGEntry leftObject = x as EPGEntry;
                EPGEntry rightObject = y as EPGEntry;

                // Not sortable
                if (null == rightObject)
                    if (null == leftObject)
                        return 0;
                    else
                        return -1;
                else if (null == leftObject)
                    return +1;

                // Attach to parent
                ReaderMain main = (ReaderMain) leftObject.ListView.Parent;

                // Get the sort index
                int sortIndex = main.SortIndex;
                bool ascending = (sortIndex > 0);

                // Correct
                if (!ascending)
                    sortIndex = -sortIndex;

                // Load
                IComparable left = (IComparable) leftObject.CompareData[--sortIndex];
                object right = rightObject.CompareData[sortIndex];

                // Not sortable
                if (null == right)
                    if (null == left)
                        return 0;
                    else
                        return -1;
                else if (null == left)
                    return +1;

                // Process
                int result = left.CompareTo( right );

                // Correct
                if (ascending)
                    return result;
                else
                    return -result;
            }

            #endregion
        }

        public readonly object[] CompareData;

        public EPGEntry( ushort service, string name, string description, DateTime start, TimeSpan duration, ushort? identifier )
        {
            // Main
            Text = string.Format( "{0} (0x{0:x})", service );

            // Load all
            SubItems.Add( start.ToString() );
            SubItems.Add( start.Add( duration ).TimeOfDay.ToString() );
            SubItems.Add( name );
            SubItems.Add( description );
            SubItems.Add( identifier.ToString() );

            // Create for compare
            CompareData = new object[] { service, start, start.Add( duration ), name };
        }
    }
}