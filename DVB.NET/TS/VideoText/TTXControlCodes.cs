using System;
using System.Collections.Generic;
using System.Text;

namespace JMS.DVB.TS.VideoText
{
	/// <summary>
	/// Steuercodes f�r die Videotext Ausgabe.
	/// </summary>
	public enum TTXControlCodes: byte
	{
        /// <summary>
        /// Vordergrundfarbe auf schwarz setzen.
        /// </summary>
		AlphaBlack,
		
        /// <summary>
        /// Vordergrundfarbe auf rot setzen.
        /// </summary>
        AlphaRed,
		
        /// <summary>
        /// Vordergrundfarbe auf gr�n setzen.
        /// </summary>
        AlphaGreen,
		
        /// <summary>
        /// Vordergrundfarbe auf gelb setzen.
        /// </summary>
        AlphaYellow,
		
        /// <summary>
        /// Vordergrundfarbe auf blau setzen.
        /// </summary>
        AlphaBlue,
		
        /// <summary>
        /// Vordergrundfarbe auf magenta setzen.
        /// </summary>
        AlphaMagenta,
		
        /// <summary>
        /// Vordergrundfarbe auf cyan setzen.
        /// </summary>
        AlphaCyan,
		
        /// <summary>
        /// Vordergrundfarbe auf wei� setzen.
        /// </summary>
        AlphaWhite,
		
        /// <summary>
        /// Blinken aktivieren.
        /// </summary>
        Flash,
		
        /// <summary>
        /// [tbd]
        /// </summary>
        Steady,
		
        /// <summary>
        /// Ende eines Rechtecks.
        /// </summary>
        EndBox,
		
        /// <summary>
        /// Beginn eines Rechtecks.
        /// </summary>
        StartBox,
		
        /// <summary>
        /// Text in normaler Gr��e darstellen.
        /// </summary>
        NormalSize,
		
        /// <summary>
        /// Text mit Zeichen doppelter H�he darstellen.
        /// </summary>
        DoubleHeight,
		
        /// <summary>
        /// Text mit Zeichen doppelter Breite darstellen.
        /// </summary>
        DoubleWidth,
		
        /// <summary>
        /// Text in doppelter Gr��e darstellen.
        /// </summary>
        DoubleSize,
		
        /// <summary>
        /// Blockgraphikfarbe auf schwarz setzen.
        /// </summary>
        MosaicBlack,
		
        /// <summary>
        /// Blockgraphikfarbe auf rot setzen.
        /// </summary>
        MosaicRed,
		        
        /// <summary>
        /// Blockgraphikfarbe auf gr�n setzen.
        /// </summary>
        MosaicGreen,
		
        /// <summary>
        /// Blockgraphikfarbe auf gelb setzen.
        /// </summary>
        MosaicYellow,
		
        /// <summary>
        /// Blockgraphikfarbe auf blau setzen.
        /// </summary>
        MosaicBlue,
		
        /// <summary>
        /// Blockgraphikfarbe auf magenta setzen.
        /// </summary>
        MosaicMagenta,
		
        /// <summary>
        /// Blockgraphikfarbe auf cyan setzen.
        /// </summary>
        MosaicCyan,
		
        /// <summary>
        /// Blockgraphikfarbe auf wei� setzen.
        /// </summary>
        MosaicWhite,
		
        /// <summary>
        /// [tbd]
        /// </summary>
        Conceal,
		
        /// <summary>
        /// Blockgraphik fortlaufend anzeigen.
        /// </summary>
        ContigousMosaic,
		
        /// <summary>
        /// Kurzzeitig Blockgraphik verwenden.
        /// </summary>
        SeparatedMosaic,
		
        /// <summary>
        /// Modus verlassen.
        /// </summary>
        Escape,
		
        /// <summary>
        /// Hintergrundfarbe auf schwarz setzen.
        /// </summary>
        BlackBackground,
		
        /// <summary>
        /// Hintergrundfarbe setzen.
        /// </summary>
        NewBackground,
		
        /// <summary>
        /// Blockgraphik beginnen.
        /// </summary>
        HoldMosaic,

        /// <summary>
        /// Blockgraphik beenden.
        /// </summary>
        ReleaseMosaic
	}
}
