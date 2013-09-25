using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace JMS.DVB.Administration.SourceScanner
{
    /// <summary>
    /// Verwaltet eine Liste von Quellgruppen für die Aktualisierung von Quellen.
    /// </summary>
    internal class LocationItem : ListViewItem
    {
        /// <summary>
        /// Die verwaltete Liste.
        /// </summary>
        public ScanLocation Location { get; private set; }

        /// <summary>
        /// Erzeugt eine neue Verwaltungsinstanz.
        /// </summary>
        /// <param name="location">Die zu verwaltende Liste.</param>
        public LocationItem( ScanLocation location )
        {
            // Remember
            Location = location;

            // Check names
            if (Equals( location.DisplayName, location.UniqueName ))
                Text = location.DisplayName;
            else
                Text = string.Format( "{0} ({1})", location.DisplayName, location.UniqueName );

            // See if this is optimized
            if (!location.AutoConvert)
                BackColor = Color.LightGreen;
        }
    }
}
