using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace JMS.DVB.EPG
{
    /// <summary>
    /// Eine Simulation der ISO-6937 Zeichencodierung.
    /// </summary>
    public class StandardProgramGuideEncoding : Encoding
    {
        /// <summary>
        /// Die Microsoft Variante der Zeichencodierung.
        /// </summary>
        private readonly Encoding m_Default = Encoding.GetEncoding( "ISO-8859-15" );

        /// <summary>
        /// Alle Zeichen von 0xa0 bis 0xaf.
        /// </summary>
        private const string GroupA0 = "\xa0¡¢£€¥ §¤‘“«←↑→↓";

        /// <summary>
        /// Alle Zeichen von 0xb0 bis 0xbf.
        /// </summary>
        private const string GroupB0 = "°±²³×μ¶•÷’”»¼½¾¿";

        /// <summary>
        /// Alle Zeichen von 0xc0 bis 0xcf.
        /// </summary>
        private const string GroupC0 = " \x0300\x0301\x0302\x0303\x0304\x0306\x0307\x0308 \x030A\x0327 \x030B\x0328\x030C";

        /// <summary>
        /// Alle Zeichen von 0xd0 bis 0xdf.
        /// </summary>
        private const string GroupD0 = "–¹®©™♪¬¦    ⅛⅜⅝⅞";

        /// <summary>
        /// Alle Zeichen von 0xe0 bis 0xef.
        /// </summary>
        private const string GroupE0 = "ΩÆÐāĦ ĲĿŁØŒōþŦŊŉ";

        /// <summary>
        /// Alle Zeichen von 0xf0 bis 0xff.
        /// </summary>
        private const string GroupF0 = "ĸæđðħıĳŀłøœßÞŧŋ ";

        /// <summary>
        /// Die Abbildung aller Kombinationszeichen.
        /// </summary>
        private static readonly Dictionary<char, Dictionary<byte, char>> DiacriticMap = new Dictionary<char, Dictionary<byte, char>>();

        /// <summary>
        /// Füllt die Abbildung aller Kombinationszeichen.
        /// </summary>
        static StandardProgramGuideEncoding()
        {
            // Set all supported according to current unicode mappings
            AddCombination( '\x0300', 'A', '\x00C0' );
            AddCombination( '\x0301', 'A', '\x00C1' );
            AddCombination( '\x0302', 'A', '\x00C2' );
            AddCombination( '\x0303', 'A', '\x00C3' );
            AddCombination( '\x0308', 'A', '\x00C4' );
            AddCombination( '\x030A', 'A', '\x00C5' );
            AddCombination( '\x0327', 'C', '\x00C7' );
            AddCombination( '\x0300', 'E', '\x00C8' );
            AddCombination( '\x0301', 'E', '\x00C9' );
            AddCombination( '\x0302', 'E', '\x00CA' );
            AddCombination( '\x0308', 'E', '\x00CB' );
            AddCombination( '\x0300', 'I', '\x00CC' );
            AddCombination( '\x0301', 'I', '\x00CD' );
            AddCombination( '\x0302', 'I', '\x00CE' );
            AddCombination( '\x0308', 'I', '\x00CF' );
            AddCombination( '\x0303', 'N', '\x00D1' );
            AddCombination( '\x0300', 'O', '\x00D2' );
            AddCombination( '\x0301', 'O', '\x00D3' );
            AddCombination( '\x0302', 'O', '\x00D4' );
            AddCombination( '\x0303', 'O', '\x00D5' );
            AddCombination( '\x0308', 'O', '\x00D6' );
            AddCombination( '\x0300', 'U', '\x00D9' );
            AddCombination( '\x0301', 'U', '\x00DA' );
            AddCombination( '\x0302', 'U', '\x00DB' );
            AddCombination( '\x0308', 'U', '\x00DC' );
            AddCombination( '\x0301', 'Y', '\x00DD' );
            AddCombination( '\x0300', 'a', '\x00E0' );
            AddCombination( '\x0301', 'a', '\x00E1' );
            AddCombination( '\x0302', 'a', '\x00E2' );
            AddCombination( '\x0303', 'a', '\x00E3' );
            AddCombination( '\x0308', 'a', '\x00E4' );
            AddCombination( '\x030A', 'a', '\x00E5' );
            AddCombination( '\x0327', 'c', '\x00E7' );
            AddCombination( '\x0300', 'e', '\x00E8' );
            AddCombination( '\x0301', 'e', '\x00E9' );
            AddCombination( '\x0302', 'e', '\x00EA' );
            AddCombination( '\x0308', 'e', '\x00EB' );
            AddCombination( '\x0300', 'i', '\x00EC' );
            AddCombination( '\x0301', 'i', '\x00ED' );
            AddCombination( '\x0302', 'i', '\x00EE' );
            AddCombination( '\x0308', 'i', '\x00EF' );
            AddCombination( '\x0303', 'n', '\x00F1' );
            AddCombination( '\x0300', 'o', '\x00F2' );
            AddCombination( '\x0301', 'o', '\x00F3' );
            AddCombination( '\x0302', 'o', '\x00F4' );
            AddCombination( '\x0303', 'o', '\x00F5' );
            AddCombination( '\x0308', 'o', '\x00F6' );
            AddCombination( '\x0300', 'u', '\x00F9' );
            AddCombination( '\x0301', 'u', '\x00FA' );
            AddCombination( '\x0302', 'u', '\x00FB' );
            AddCombination( '\x0308', 'u', '\x00FC' );
            AddCombination( '\x0301', 'y', '\x00FD' );
            AddCombination( '\x0308', 'y', '\x00FF' );
            AddCombination( '\x0304', 'A', '\x0100' );
            AddCombination( '\x0304', 'a', '\x0101' );
            AddCombination( '\x0306', 'A', '\x0102' );
            AddCombination( '\x0306', 'a', '\x0103' );
            AddCombination( '\x0328', 'A', '\x0104' );
            AddCombination( '\x0328', 'a', '\x0105' );
            AddCombination( '\x0301', 'C', '\x0106' );
            AddCombination( '\x0301', 'c', '\x0107' );
            AddCombination( '\x0302', 'C', '\x0108' );
            AddCombination( '\x0302', 'c', '\x0109' );
            AddCombination( '\x0307', 'C', '\x010A' );
            AddCombination( '\x0307', 'c', '\x010B' );
            AddCombination( '\x030C', 'C', '\x010C' );
            AddCombination( '\x030C', 'c', '\x010D' );
            AddCombination( '\x030C', 'D', '\x010E' );
            AddCombination( '\x030C', 'd', '\x010F' );
            AddCombination( '\x0304', 'E', '\x0112' );
            AddCombination( '\x0304', 'e', '\x0113' );
            AddCombination( '\x0306', 'E', '\x0114' );
            AddCombination( '\x0306', 'e', '\x0115' );
            AddCombination( '\x0307', 'E', '\x0116' );
            AddCombination( '\x0307', 'e', '\x0117' );
            AddCombination( '\x0328', 'E', '\x0118' );
            AddCombination( '\x0328', 'e', '\x0119' );
            AddCombination( '\x030C', 'E', '\x011A' );
            AddCombination( '\x030C', 'e', '\x011B' );
            AddCombination( '\x0302', 'G', '\x011C' );
            AddCombination( '\x0302', 'g', '\x011D' );
            AddCombination( '\x0306', 'G', '\x011E' );
            AddCombination( '\x0306', 'g', '\x011F' );
            AddCombination( '\x0307', 'G', '\x0120' );
            AddCombination( '\x0307', 'g', '\x0121' );
            AddCombination( '\x0327', 'G', '\x0122' );
            AddCombination( '\x0327', 'g', '\x0123' );
            AddCombination( '\x0302', 'H', '\x0124' );
            AddCombination( '\x0302', 'h', '\x0125' );
            AddCombination( '\x0303', 'I', '\x0128' );
            AddCombination( '\x0303', 'i', '\x0129' );
            AddCombination( '\x0304', 'I', '\x012A' );
            AddCombination( '\x0304', 'i', '\x012B' );
            AddCombination( '\x0306', 'I', '\x012C' );
            AddCombination( '\x0306', 'i', '\x012D' );
            AddCombination( '\x0328', 'I', '\x012E' );
            AddCombination( '\x0328', 'i', '\x012F' );
            AddCombination( '\x0307', 'I', '\x0130' );
            AddCombination( '\x0302', 'J', '\x0134' );
            AddCombination( '\x0302', 'j', '\x0135' );
            AddCombination( '\x0327', 'K', '\x0136' );
            AddCombination( '\x0327', 'k', '\x0137' );
            AddCombination( '\x0301', 'L', '\x0139' );
            AddCombination( '\x0301', 'l', '\x013A' );
            AddCombination( '\x0327', 'L', '\x013B' );
            AddCombination( '\x0327', 'l', '\x013C' );
            AddCombination( '\x030C', 'L', '\x013D' );
            AddCombination( '\x030C', 'l', '\x013E' );
            AddCombination( '\x0301', 'N', '\x0143' );
            AddCombination( '\x0301', 'n', '\x0144' );
            AddCombination( '\x0327', 'N', '\x0145' );
            AddCombination( '\x0327', 'n', '\x0146' );
            AddCombination( '\x030C', 'N', '\x0147' );
            AddCombination( '\x030C', 'n', '\x0148' );
            AddCombination( '\x0304', 'O', '\x014C' );
            AddCombination( '\x0304', 'o', '\x014D' );
            AddCombination( '\x0306', 'O', '\x014E' );
            AddCombination( '\x0306', 'o', '\x014F' );
            AddCombination( '\x030B', 'O', '\x0150' );
            AddCombination( '\x030B', 'o', '\x0151' );
            AddCombination( '\x0301', 'R', '\x0154' );
            AddCombination( '\x0301', 'r', '\x0155' );
            AddCombination( '\x0327', 'R', '\x0156' );
            AddCombination( '\x0327', 'r', '\x0157' );
            AddCombination( '\x030C', 'R', '\x0158' );
            AddCombination( '\x030C', 'r', '\x0159' );
            AddCombination( '\x0301', 'S', '\x015A' );
            AddCombination( '\x0301', 's', '\x015B' );
            AddCombination( '\x0302', 'S', '\x015C' );
            AddCombination( '\x0302', 's', '\x015D' );
            AddCombination( '\x0327', 'S', '\x015E' );
            AddCombination( '\x0327', 's', '\x015F' );
            AddCombination( '\x030C', 'S', '\x0160' );
            AddCombination( '\x030C', 's', '\x0161' );
            AddCombination( '\x0327', 'T', '\x0162' );
            AddCombination( '\x0327', 't', '\x0163' );
            AddCombination( '\x030C', 'T', '\x0164' );
            AddCombination( '\x030C', 't', '\x0165' );
            AddCombination( '\x0303', 'U', '\x0168' );
            AddCombination( '\x0303', 'u', '\x0169' );
            AddCombination( '\x0304', 'U', '\x016A' );
            AddCombination( '\x0304', 'u', '\x016B' );
            AddCombination( '\x0306', 'U', '\x016C' );
            AddCombination( '\x0306', 'u', '\x016D' );
            AddCombination( '\x030A', 'U', '\x016E' );
            AddCombination( '\x030A', 'u', '\x016F' );
            AddCombination( '\x030B', 'U', '\x0170' );
            AddCombination( '\x030B', 'u', '\x0171' );
            AddCombination( '\x0328', 'U', '\x0172' );
            AddCombination( '\x0328', 'u', '\x0173' );
            AddCombination( '\x0302', 'W', '\x0174' );
            AddCombination( '\x0302', 'w', '\x0175' );
            AddCombination( '\x0302', 'Y', '\x0176' );
            AddCombination( '\x0302', 'y', '\x0177' );
            AddCombination( '\x0308', 'Y', '\x0178' );
            AddCombination( '\x0301', 'Z', '\x0179' );
            AddCombination( '\x0301', 'z', '\x017A' );
            AddCombination( '\x0307', 'Z', '\x017B' );
            AddCombination( '\x0307', 'z', '\x017C' );
            AddCombination( '\x030C', 'Z', '\x017D' );
            AddCombination( '\x030C', 'z', '\x017E' );
            AddCombination( '\x031B', 'O', '\x01A0' );
            AddCombination( '\x031B', 'o', '\x01A1' );
            AddCombination( '\x031B', 'U', '\x01AF' );
            AddCombination( '\x031B', 'u', '\x01B0' );
            AddCombination( '\x030C', 'A', '\x01CD' );
            AddCombination( '\x030C', 'a', '\x01CE' );
            AddCombination( '\x030C', 'I', '\x01CF' );
            AddCombination( '\x030C', 'i', '\x01D0' );
            AddCombination( '\x030C', 'O', '\x01D1' );
            AddCombination( '\x030C', 'o', '\x01D2' );
            AddCombination( '\x030C', 'U', '\x01D3' );
            AddCombination( '\x030C', 'u', '\x01D4' );
            AddCombination( '\x030C', 'G', '\x01E6' );
            AddCombination( '\x030C', 'g', '\x01E7' );
            AddCombination( '\x030C', 'K', '\x01E8' );
            AddCombination( '\x030C', 'k', '\x01E9' );
            AddCombination( '\x0328', 'O', '\x01EA' );
            AddCombination( '\x0328', 'o', '\x01EB' );
            AddCombination( '\x030C', 'j', '\x01F0' );
            AddCombination( '\x0301', 'G', '\x01F4' );
            AddCombination( '\x0301', 'g', '\x01F5' );
            AddCombination( '\x0300', 'N', '\x01F8' );
            AddCombination( '\x0300', 'n', '\x01F9' );
            AddCombination( '\x030F', 'A', '\x0200' );
            AddCombination( '\x030F', 'a', '\x0201' );
            AddCombination( '\x0311', 'A', '\x0202' );
            AddCombination( '\x0311', 'a', '\x0203' );
            AddCombination( '\x030F', 'E', '\x0204' );
            AddCombination( '\x030F', 'e', '\x0205' );
            AddCombination( '\x0311', 'E', '\x0206' );
            AddCombination( '\x0311', 'e', '\x0207' );
            AddCombination( '\x030F', 'I', '\x0208' );
            AddCombination( '\x030F', 'i', '\x0209' );
            AddCombination( '\x0311', 'I', '\x020A' );
            AddCombination( '\x0311', 'i', '\x020B' );
            AddCombination( '\x030F', 'O', '\x020C' );
            AddCombination( '\x030F', 'o', '\x020D' );
            AddCombination( '\x0311', 'O', '\x020E' );
            AddCombination( '\x0311', 'o', '\x020F' );
            AddCombination( '\x030F', 'R', '\x0210' );
            AddCombination( '\x030F', 'r', '\x0211' );
            AddCombination( '\x0311', 'R', '\x0212' );
            AddCombination( '\x0311', 'r', '\x0213' );
            AddCombination( '\x030F', 'U', '\x0214' );
            AddCombination( '\x030F', 'u', '\x0215' );
            AddCombination( '\x0311', 'U', '\x0216' );
            AddCombination( '\x0311', 'u', '\x0217' );
            AddCombination( '\x0326', 'S', '\x0218' );
            AddCombination( '\x0326', 's', '\x0219' );
            AddCombination( '\x0326', 'T', '\x021A' );
            AddCombination( '\x0326', 't', '\x021B' );
            AddCombination( '\x030C', 'H', '\x021E' );
            AddCombination( '\x030C', 'h', '\x021F' );
            AddCombination( '\x0307', 'A', '\x0226' );
            AddCombination( '\x0307', 'a', '\x0227' );
            AddCombination( '\x0327', 'E', '\x0228' );
            AddCombination( '\x0327', 'e', '\x0229' );
            AddCombination( '\x0307', 'O', '\x022E' );
            AddCombination( '\x0307', 'o', '\x022F' );
            AddCombination( '\x0304', 'Y', '\x0232' );
            AddCombination( '\x0304', 'y', '\x0233' );
            AddCombination( '\x0325', 'A', '\x1E00' );
            AddCombination( '\x0325', 'a', '\x1E01' );
            AddCombination( '\x0307', 'B', '\x1E02' );
            AddCombination( '\x0307', 'b', '\x1E03' );
            AddCombination( '\x0323', 'B', '\x1E04' );
            AddCombination( '\x0323', 'b', '\x1E05' );
            AddCombination( '\x0331', 'B', '\x1E06' );
            AddCombination( '\x0331', 'b', '\x1E07' );
            AddCombination( '\x0307', 'D', '\x1E0A' );
            AddCombination( '\x0307', 'd', '\x1E0B' );
            AddCombination( '\x0323', 'D', '\x1E0C' );
            AddCombination( '\x0323', 'd', '\x1E0D' );
            AddCombination( '\x0331', 'D', '\x1E0E' );
            AddCombination( '\x0331', 'd', '\x1E0F' );
            AddCombination( '\x0327', 'D', '\x1E10' );
            AddCombination( '\x0327', 'd', '\x1E11' );
            AddCombination( '\x032D', 'D', '\x1E12' );
            AddCombination( '\x032D', 'd', '\x1E13' );
            AddCombination( '\x032D', 'E', '\x1E18' );
            AddCombination( '\x032D', 'e', '\x1E19' );
            AddCombination( '\x0330', 'E', '\x1E1A' );
            AddCombination( '\x0330', 'e', '\x1E1B' );
            AddCombination( '\x0307', 'F', '\x1E1E' );
            AddCombination( '\x0307', 'f', '\x1E1F' );
            AddCombination( '\x0304', 'G', '\x1E20' );
            AddCombination( '\x0304', 'g', '\x1E21' );
            AddCombination( '\x0307', 'H', '\x1E22' );
            AddCombination( '\x0307', 'h', '\x1E23' );
            AddCombination( '\x0323', 'H', '\x1E24' );
            AddCombination( '\x0323', 'h', '\x1E25' );
            AddCombination( '\x0308', 'H', '\x1E26' );
            AddCombination( '\x0308', 'h', '\x1E27' );
            AddCombination( '\x0327', 'H', '\x1E28' );
            AddCombination( '\x0327', 'h', '\x1E29' );
            AddCombination( '\x032E', 'H', '\x1E2A' );
            AddCombination( '\x032E', 'h', '\x1E2B' );
            AddCombination( '\x0330', 'I', '\x1E2C' );
            AddCombination( '\x0330', 'i', '\x1E2D' );
            AddCombination( '\x0301', 'K', '\x1E30' );
            AddCombination( '\x0301', 'k', '\x1E31' );
            AddCombination( '\x0323', 'K', '\x1E32' );
            AddCombination( '\x0323', 'k', '\x1E33' );
            AddCombination( '\x0331', 'K', '\x1E34' );
            AddCombination( '\x0331', 'k', '\x1E35' );
            AddCombination( '\x0323', 'L', '\x1E36' );
            AddCombination( '\x0323', 'l', '\x1E37' );
            AddCombination( '\x0331', 'L', '\x1E3A' );
            AddCombination( '\x0331', 'l', '\x1E3B' );
            AddCombination( '\x032D', 'L', '\x1E3C' );
            AddCombination( '\x032D', 'l', '\x1E3D' );
            AddCombination( '\x0301', 'M', '\x1E3E' );
            AddCombination( '\x0301', 'm', '\x1E3F' );
            AddCombination( '\x0307', 'M', '\x1E40' );
            AddCombination( '\x0307', 'm', '\x1E41' );
            AddCombination( '\x0323', 'M', '\x1E42' );
            AddCombination( '\x0323', 'm', '\x1E43' );
            AddCombination( '\x0307', 'N', '\x1E44' );
            AddCombination( '\x0307', 'n', '\x1E45' );
            AddCombination( '\x0323', 'N', '\x1E46' );
            AddCombination( '\x0323', 'n', '\x1E47' );
            AddCombination( '\x0331', 'N', '\x1E48' );
            AddCombination( '\x0331', 'n', '\x1E49' );
            AddCombination( '\x032D', 'N', '\x1E4A' );
            AddCombination( '\x032D', 'n', '\x1E4B' );
            AddCombination( '\x0301', 'P', '\x1E54' );
            AddCombination( '\x0301', 'p', '\x1E55' );
            AddCombination( '\x0307', 'P', '\x1E56' );
            AddCombination( '\x0307', 'p', '\x1E57' );
            AddCombination( '\x0307', 'R', '\x1E58' );
            AddCombination( '\x0307', 'r', '\x1E59' );
            AddCombination( '\x0323', 'R', '\x1E5A' );
            AddCombination( '\x0323', 'r', '\x1E5B' );
            AddCombination( '\x0331', 'R', '\x1E5E' );
            AddCombination( '\x0331', 'r', '\x1E5F' );
            AddCombination( '\x0307', 'S', '\x1E60' );
            AddCombination( '\x0307', 's', '\x1E61' );
            AddCombination( '\x0323', 'S', '\x1E62' );
            AddCombination( '\x0323', 's', '\x1E63' );
            AddCombination( '\x0307', 'T', '\x1E6A' );
            AddCombination( '\x0307', 't', '\x1E6B' );
            AddCombination( '\x0323', 'T', '\x1E6C' );
            AddCombination( '\x0323', 't', '\x1E6D' );
            AddCombination( '\x0331', 'T', '\x1E6E' );
            AddCombination( '\x0331', 't', '\x1E6F' );
            AddCombination( '\x032D', 'T', '\x1E70' );
            AddCombination( '\x032D', 't', '\x1E71' );
            AddCombination( '\x0324', 'U', '\x1E72' );
            AddCombination( '\x0324', 'u', '\x1E73' );
            AddCombination( '\x0330', 'U', '\x1E74' );
            AddCombination( '\x0330', 'u', '\x1E75' );
            AddCombination( '\x032D', 'U', '\x1E76' );
            AddCombination( '\x032D', 'u', '\x1E77' );
            AddCombination( '\x0303', 'V', '\x1E7C' );
            AddCombination( '\x0303', 'v', '\x1E7D' );
            AddCombination( '\x0323', 'V', '\x1E7E' );
            AddCombination( '\x0323', 'v', '\x1E7F' );
            AddCombination( '\x0300', 'W', '\x1E80' );
            AddCombination( '\x0300', 'w', '\x1E81' );
            AddCombination( '\x0301', 'W', '\x1E82' );
            AddCombination( '\x0301', 'w', '\x1E83' );
            AddCombination( '\x0308', 'W', '\x1E84' );
            AddCombination( '\x0308', 'w', '\x1E85' );
            AddCombination( '\x0307', 'W', '\x1E86' );
            AddCombination( '\x0307', 'w', '\x1E87' );
            AddCombination( '\x0323', 'W', '\x1E88' );
            AddCombination( '\x0323', 'w', '\x1E89' );
            AddCombination( '\x0307', 'X', '\x1E8A' );
            AddCombination( '\x0307', 'x', '\x1E8B' );
            AddCombination( '\x0308', 'X', '\x1E8C' );
            AddCombination( '\x0308', 'x', '\x1E8D' );
            AddCombination( '\x0307', 'Y', '\x1E8E' );
            AddCombination( '\x0307', 'y', '\x1E8F' );
            AddCombination( '\x0302', 'Z', '\x1E90' );
            AddCombination( '\x0302', 'z', '\x1E91' );
            AddCombination( '\x0323', 'Z', '\x1E92' );
            AddCombination( '\x0323', 'z', '\x1E93' );
            AddCombination( '\x0331', 'Z', '\x1E94' );
            AddCombination( '\x0331', 'z', '\x1E95' );
            AddCombination( '\x0331', 'h', '\x1E96' );
            AddCombination( '\x0308', 't', '\x1E97' );
            AddCombination( '\x030A', 'w', '\x1E98' );
            AddCombination( '\x030A', 'y', '\x1E99' );
            AddCombination( '\x0323', 'A', '\x1EA0' );
            AddCombination( '\x0323', 'a', '\x1EA1' );
            AddCombination( '\x0309', 'A', '\x1EA2' );
            AddCombination( '\x0309', 'a', '\x1EA3' );
            AddCombination( '\x0323', 'E', '\x1EB8' );
            AddCombination( '\x0323', 'e', '\x1EB9' );
            AddCombination( '\x0309', 'E', '\x1EBA' );
            AddCombination( '\x0309', 'e', '\x1EBB' );
            AddCombination( '\x0303', 'E', '\x1EBC' );
            AddCombination( '\x0303', 'e', '\x1EBD' );
            AddCombination( '\x0309', 'I', '\x1EC8' );
            AddCombination( '\x0309', 'i', '\x1EC9' );
            AddCombination( '\x0323', 'I', '\x1ECA' );
            AddCombination( '\x0323', 'i', '\x1ECB' );
            AddCombination( '\x0323', 'O', '\x1ECC' );
            AddCombination( '\x0323', 'o', '\x1ECD' );
            AddCombination( '\x0309', 'O', '\x1ECE' );
            AddCombination( '\x0309', 'o', '\x1ECF' );
            AddCombination( '\x0323', 'U', '\x1EE4' );
            AddCombination( '\x0323', 'u', '\x1EE5' );
            AddCombination( '\x0309', 'U', '\x1EE6' );
            AddCombination( '\x0309', 'u', '\x1EE7' );
            AddCombination( '\x0300', 'Y', '\x1EF2' );
            AddCombination( '\x0300', 'y', '\x1EF3' );
            AddCombination( '\x0323', 'Y', '\x1EF4' );
            AddCombination( '\x0323', 'y', '\x1EF5' );
            AddCombination( '\x0309', 'Y', '\x1EF6' );
            AddCombination( '\x0309', 'y', '\x1EF7' );
            AddCombination( '\x0303', 'Y', '\x1EF8' );
            AddCombination( '\x0303', 'y', '\x1EF9' );
            AddCombination( '\x0338', '=', '\x2260' );
            AddCombination( '\x0338', '<', '\x226E' );
            AddCombination( '\x0338', '>', '\x226F' );

            // Extra needed - strange, may indicate some bug
            AddCombination( '\x0306', 'S', '\x0160' );
            AddCombination( '\x0306', 's', '\x0161' );
        }

        /// <summary>
        /// Ergänzt ein Kombinationszeichen.
        /// </summary>
        /// <param name="marker">Das Diacrit.</param>
        /// <param name="item">Das Standardzeichen.</param>
        /// <param name="result">Das Kombinationszeichen.</param>
        private static void AddCombination( char marker, char item, char result )
        {
            // Load
            Dictionary<byte, char> map;
            if (!DiacriticMap.TryGetValue( marker, out map ))
            {
                // Create
                map = new Dictionary<byte, char>();

                // Remember
                DiacriticMap[marker] = map;
            }

            // Store
            map[(byte) item] = result;
        }

        /// <summary>
        /// Erzeugt eine neue Codierung.
        /// </summary>
        public StandardProgramGuideEncoding()
        {
        }

        /// <summary>
        /// Meldet die maximale Anzahl von verschlüsselten Bytes für einen Zeichenkette
        /// fester Länge.
        /// </summary>
        /// <param name="charCount">Die Länge der Zeichenkette.</param>
        /// <returns>Die maximale Anzahl der benötigten Bytes.</returns>
        public override int GetMaxByteCount( int charCount )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Meldet die maximale Anzahl von Zeichen, die aus einer komprimierten
        /// Sequenz entstehen kann.
        /// </summary>
        /// <param name="byteCount">Die Anzahl der komprimierten Bytes.</param>
        /// <returns>Die maximale Anzahl von Zeichen.</returns>
        public override int GetMaxCharCount( int byteCount )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Komprimiert eine Zeichensequenz.
        /// </summary>
        /// <param name="bytes">Die komprimierten Daten.</param>
        /// <param name="byteIndex">Laufende Nummer des ersten zu nutzenden Wertes.</param>
        /// <param name="byteCount">Anzahl der zu nutzenden Werte.</param>
        /// <param name="chars">Resultierende Zeichenkette.</param>
        /// <param name="charIndex">Laufende Nummer des ersten Zeichens in der resultierenden Zeichenkette.</param>
        /// <returns>Die Anzahl der dekomprimierten Zeichen.</returns>
        public override int GetChars( byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Meldet die Anzahl der Zeichen, die bei einer Dekomprimierung entstehen würden.
        /// </summary>
        /// <param name="bytes">Die komprimierten Daten.</param>
        /// <param name="index">Laufende Nummer des ersten zu nutzenden Wertes.</param>
        /// <param name="count">Anzahl der zu nutzenden Werte.</param>
        /// <returns>Die Anzahl der dekomprimierten Zeichen.</returns>
        public override int GetCharCount( byte[] bytes, int index, int count )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Komprimiert eine Zeichenkette.
        /// </summary>
        /// <param name="chars">Die Zeichenkette.</param>
        /// <param name="charIndex">Die laufende Nummer des ersten zu komprimierenden Zeichens.</param>
        /// <param name="charCount">Die Anzahl der zu komprimierenden Zeichen.</param>
        /// <param name="bytes">Ergebnisfeld für die komprimierten Werte.</param>
        /// <param name="byteIndex">Die laufende Nummer des ersten Wertes im Ergebnisfeld.</param>
        /// <returns>Die Anzahl der erzeugten Werte.</returns>
        public override int GetBytes( char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ermittelt, wieviele Werte für eine Komprimierung einer Zeichenkette notwendig sind.
        /// </summary>
        /// <param name="chars">Die Zeichenkette.</param>
        /// <param name="index">Die laufende Nummer des ersten zu komprimierenden Zeichens.</param>
        /// <param name="count">Die Anzahl der zu komprimierenden Zeichen.</param>
        /// <returns>Die Anzahl der benötigten Werte.</returns>
        public override int GetByteCount( char[] chars, int index, int count )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ermittelt eine Zeichenkette zu einer Sequenz von Bytes.
        /// </summary>
        /// <param name="bytes">Die zu analysierenden Bytes.</param>
        /// <param name="index">Das erste zu verwendende Byte.</param>
        /// <param name="count">Die Anzahl der zu verwendenden Bytes.</param>
        /// <returns>Das gewünschte Ergebnis.</returns>
        public override string GetString( byte[] bytes, int index, int count )
        {
            // Result 
            StringBuilder result = new StringBuilder( count );

            // Construct
            while (count-- > 0)
            {
                // Get the byte
                byte next = bytes[index++];

                // Check area
                if (next < 0xa0)
                    result.Append( (char) next );
                else if (next < 0xb0)
                    result.Append( GroupA0[next - 0xa0] );
                else if (next < 0xc0)
                    result.Append( GroupB0[next - 0xb0] );
                else if (next < 0xd0)
                    if (count-- > 0)
                        result.Append( ResolveDiacryptic( GroupC0[next - 0xc0], bytes[index++] ) );
                    else
                        result.Append( GroupC0[next - 0xc0] );
                else if (next < 0xe0)
                    result.Append( GroupD0[next - 0xd0] );
                else if (next < 0xf0)
                    result.Append( GroupE0[next - 0xe0] );
                else
                    result.Append( GroupF0[next - 0xf0] );
            }

            // Report
            return result.ToString();
        }

        /// <summary>
        /// Erzeugt eine Zeichenkombination.
        /// </summary>
        /// <param name="symbol">Das zu verwendende Symbol.</param>
        /// <param name="item">Das umzuwandeln Zeichen.</param>
        /// <returns>Das umgewandelte Zeichen.</returns>
        private static char ResolveDiacryptic( char symbol, byte item )
        {
            // Read the map
            Dictionary<byte, char> map;
            if (!DiacriticMap.TryGetValue( symbol, out map ))
                return (char) item;

            // Read the char
            char result;
            if (map.TryGetValue( item, out result ))
                return result;
            else
                return (char) item;
        }
    }
}
