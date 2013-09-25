using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
	public class TeletextItem
	{
        public readonly string ISOLanguage;

        public readonly TeletextTypes Type;

		public readonly byte MagazineNumber;

		public readonly byte PageNumberBCD;

		public TeletextItem(string language, TeletextTypes type, byte magazine, byte pageBCD)
		{
			// Remember
			MagazineNumber = magazine;
			PageNumberBCD = pageBCD;
			ISOLanguage = language;
			Type = type;
		}

        private TeletextItem(Section section, int offset)
        {
            // Load
            ISOLanguage = section.ReadString(offset + 0, 3);
            Type = (TeletextTypes)(section[offset + 3]>>3);
            MagazineNumber = (byte)(section[offset + 3]&0x7);

            // Decode
            PageNumberBCD = section[offset + 4];
        }

        public int Length
        {
            get
            {
                // Report constant length
                return 5;
            }
        }

        internal static TeletextItem Create(Section section, int offset, int length)
        {
            // Check 
            if (length < 5) return null;

            // Create
            return new TeletextItem(section, offset);
        }

		internal void CreatePayload(TableConstructor buffer)
		{
			// The language
			buffer.AddLanguage(ISOLanguage);

			// Code field
			buffer.Add((byte)((MagazineNumber & 0x7) | (((int)Type) << 3)));
			buffer.Add(PageNumberBCD);
		}
	}
}
