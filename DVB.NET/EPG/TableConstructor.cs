using System;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
    public class TableConstructor
    {
        private static Encoding ANSI = Encoding.GetEncoding(1252);

        private List<byte> m_Buffer = new List<byte>(184);

        public TableConstructor()
        {
        }

        public int CreateDynamicLength()
        {
            // Create
            m_Buffer.Add(0);

            // Report
            return m_Buffer.Count - 1;
        }

        public void SetDynamicLength(int dynamicLengthPosition)
        {
            // Store
            m_Buffer[dynamicLengthPosition] = (byte)(m_Buffer.Count - dynamicLengthPosition - 1);
        }

        public void AddLanguage(string isoLanguage)
        {
            // Correct
            if (string.IsNullOrEmpty(isoLanguage))
                isoLanguage = "deu";
            else if (isoLanguage.Length < 3)
                isoLanguage = isoLanguage + new String(' ', 3 - isoLanguage.Length);
            else if (isoLanguage.Length > 3)
                isoLanguage = isoLanguage.Substring(0, 3);

            // Forward
            Add(ANSI.GetBytes(isoLanguage));
        }

        public void Add(byte value)
        {
            // Remember
            m_Buffer.Add(value);
        }

        public void Add(ushort value)
        {
            // Remember
            Add((byte)((value >> 8) & 0xff), (byte)(value & 0xff));
        }

        public void Add(params byte[] bytes)
        {
            // Remember
            if (null != bytes) m_Buffer.AddRange(bytes);
        }

        public byte[] ToArray()
        {
            // Report
            return m_Buffer.ToArray();
        }

        public void Add(Descriptor descriptor)
        {
            // Forward
            descriptor.CreateDescriptor(this);
        }
    }
}
