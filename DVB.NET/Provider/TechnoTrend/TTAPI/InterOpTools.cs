using System;


namespace JMS.TechnoTrend
{
    /// <summary>
    /// Some conversion helper for interoperability with MFC extension DLLs.
    /// </summary>
    public static class InterOpTools
    {
        /// <summary>
        /// When using virtual functions with <see cref="ClassHolder.this"/> and some ANSI
        /// MFC extension DLL string data is provided as a zero terminated stream of
        /// 8-bit characters. This method makes this a regular <see cref="string"/>.
        /// </summary>
        /// <param name="aData">Character stream from ANSI MFC extension virtual function call.</param>
        /// <returns>A <see cref="string"/> holding all characters excluding the 
        /// terminating zero.</returns>
        static public string ByteArrayToString( sbyte[] aData )
        {
            // The length
            int len = 0;

            // Find terminator
            while ((len < aData.Length) && (0 != aData[len])) ++len;

            // Create data
            char[] cArray = new char[len];

            // Copy over
            for (int ix = cArray.Length; ix-- > 0; ) cArray[ix] = (char) aData[ix];

            // Done
            return new String( cArray );
        }
    }
}
