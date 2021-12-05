using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// A single entry in a <see cref="Tables.PMT"/>.
    /// </summary>
    public class ProgramEntry : EntryBase
    {
        /// <summary>
        /// Maps ISO language names to their native representation.
        /// </summary>
        private static Dictionary<string, CultureInfo> m_CultureMap = new Dictionary<string, CultureInfo>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// The constructor is private to make this class static.
        /// </summary>
        static ProgramEntry()
        {
            // Load all
            foreach (CultureInfo info in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
                // Remember
                m_CultureMap[info.ThreeLetterISOLanguageName] = info;
            }

            // Add special entries (bibliographic - see ISO639-2)
            AddBibliographicShortcut("bod", "tib");
            AddBibliographicShortcut("ces", "cze");
            AddBibliographicShortcut("cym", "wel");
            AddBibliographicShortcut("deu", "ger");
            AddBibliographicShortcut("ell", "gre");
            AddBibliographicShortcut("eus", "baq");
            AddBibliographicShortcut("fas", "per");
            AddBibliographicShortcut("fra", "fre");
            AddBibliographicShortcut("hrv", "scr");
            AddBibliographicShortcut("hye", "arm");
            AddBibliographicShortcut("isl", "ice");
            AddBibliographicShortcut("kat", "geo");
            AddBibliographicShortcut("mkd", "mac");
            AddBibliographicShortcut("mri", "mao");
            AddBibliographicShortcut("msa", "may");
            AddBibliographicShortcut("mya", "bur");
            AddBibliographicShortcut("nld", "dut");
            AddBibliographicShortcut("ron", "rum");
            AddBibliographicShortcut("slk", "slo");
            AddBibliographicShortcut("sqi", "alb");
            AddBibliographicShortcut("srp", "scc");
            AddBibliographicShortcut("zho", "chi");
        }

        /// <summary>
        /// Create an alternate language map entry.
        /// </summary>
        /// <param name="terminologyCode">Official three letter code.</param>
        /// <param name="bibliographicCode">Alternat (bibliographic) three letter code.</param>
        private static void AddBibliographicShortcut(string terminologyCode, string bibliographicCode)
        {
            // See if code already exists
            if (m_CultureMap.ContainsKey(bibliographicCode))
                return;

            // Load entry
            CultureInfo terminologic;
            if (!m_CultureMap.TryGetValue(terminologyCode, out terminologic))
                return;

            // Connect
            m_CultureMap[bibliographicCode] = terminologic;
        }

        /// <summary>
        /// The <see cref="Descriptor"/> instances related to this program.
        /// </summary>
        /// <remarks>
        /// Please refer to the original documentation to find out which descriptor
        /// type is allowed in a <see cref="Tables.PMT"/> table.
        /// </remarks>
        public readonly Descriptor[] Descriptors;

        /// <summary>
        /// Set if the program entry is consistent.
        /// </summary>
        public readonly bool IsValid = false;

        /// <summary>
        /// The total length of the entry in bytes.
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// The PID of this program entry.
        /// </summary>
        public ushort ElementaryPID;

        /// <summary>
        /// The type of this stream
        /// </summary>
        public StreamTypes StreamType;

        /// <summary>
        /// Create a new program instance.
        /// </summary>
        /// <param name="table">The related <see cref="Tables.PMT"/> table.</param>
        /// <param name="offset">The first byte of this program in the <see cref="EPG.Table.Section"/>
        /// for the related <see cref="Table"/>.</param>
        /// <param name="length">The maximum number of bytes available. If this number
        /// is greater than the <see cref="Length"/> of this program another event will
        /// follow in the same table.</param>
        internal ProgramEntry(Table table, int offset, int length)
            : base(table)
        {
            // Access section
            Section section = Section;

            // Load
            ElementaryPID = (ushort)(0x1fff & Tools.MergeBytesToWord(section[offset + 2], section[offset + 1]));
            StreamType = (StreamTypes)section[offset + 0];

            // Read the length
            int descrLength = 0xfff & Tools.MergeBytesToWord(section[offset + 4], section[offset + 3]);

            // Calculate the total length
            Length = 5 + descrLength;

            // Verify
            if (Length > length)
                return;

            // Try to load descriptors
            Descriptors = Descriptor.Load(this, offset + 5, descrLength);

            // Can use it
            IsValid = true;
        }

        /// <summary>
        /// Create a new program instance.
        /// </summary>
        /// <param name="table">The related <see cref="Tables.PMT"/> table.</param>
        /// <param name="offset">The first byte of this service in the <see cref="EPG.Table.Section"/>
        /// for the related <see cref="Table"/>.</param>
        /// <param name="length">The maximum number of bytes available. If this number
        /// is greater than the <see cref="Length"/> of this program entry another entry will
        /// follow in the same table.</param>
        /// <returns>A new service instance or <i>null</i> if there are less than
        /// 5 bytes available.</returns>
        static internal ProgramEntry Create(Table table, int offset, int length)
        {
            // Validate
            if (length < 5)
                return null;

            // Create
            return new ProgramEntry(table, offset, length);
        }

        /// <summary>
        /// Ermittelt zu einem Datenstrom den ISO Namen der Sprache.
        /// </summary>
        public string ProgrammeName
        {
            get
            {
                // Check all descriptors
                foreach (Descriptor descriptor in Descriptors)
                {
                    // Check type
                    Descriptors.ISOLanguage language = descriptor as Descriptors.ISOLanguage;

                    // None
                    if ((null == language) || (language.Languages.Count < 1))
                        continue;

                    // Remember the first one set
                    string name = language.Languages[0].ISOLanguage;
                    if (string.IsNullOrEmpty(name))
                        continue;

                    // Convert
                    return GetLanguageFromISOLanguage(name);
                }

                // Use default
                return string.Format("#{0}", ElementaryPID);
            }
        }

        /// <summary>
        /// Ermittelt zu einer ISO Kurzbezeichnung einer Sprache die zugehörige 
        /// Sprache.
        /// </summary>
        /// <param name="language">Eine ISO Kurzbezeichnung.</param>
        /// <returns>Der Name der Sprache, ausgerückt in der Sprache selbst.</returns>
        public static string GetLanguageFromISOLanguage(string language)
        {
            // Find
            CultureInfo cult;
            if (m_CultureMap.TryGetValue(language, out cult))
                return cult.NativeName;

            // Report as is
            return language;
        }
    }
}
