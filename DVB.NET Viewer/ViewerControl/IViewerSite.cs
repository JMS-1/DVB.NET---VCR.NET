using System;
using System.Windows.Forms;

using JMS.DVB.Favorites;
using JMS.DVB.DirectShow;


namespace JMS.DVB.Viewer
{
    /// <summary>
    /// Signatur für eine Methode, die auf einen Tastendruck reagiert.
    /// </summary>
    public delegate void ViewerKeyStrokeCallback();

    /// <summary>
    /// Mit dieser Schnittstelle kommuniziert das DVB.NET Viewer
    /// <see cref="Control"/> mit den Hilfsklassen, die den
    /// Zugriff auf die unterschiedlichen Datenquellen vermitteln.
    /// </summary>
    public interface IViewerSite : IWin32Window
    {
        /// <summary>
        /// Meldet ein Tastaturbefehl an.
        /// </summary>
        /// <param name="key">Gewünschte Taste.</param>
        /// <param name="handler">Zugehörige Bearbeitungsroutine.</param>
        void SetKeyHandler( Keys key, ViewerKeyStrokeCallback handler );

        /// <summary>
        /// Set die Behandung der Tastatureingabe auf den ursprünglichen Zustand zurück.
        /// </summary>
        void ResetKeyHandlers();

        /// <summary>
        /// Lädt die Senderliste neu.
        /// </summary>
        void Restart();

        /// <summary>
        /// Meldet die aktuell verwendete Quelle.
        /// </summary>
        Adaptor CurrentAdaptor { get; }

        /// <summary>
        /// Transferiert einen Aufruf auf den Windows Hauptthread
        /// der Anwendung.
        /// </summary>
        /// <param name="method">Auszuführende Methode.</param>
        /// <param name="args">Parameter der Methode.</param>
        object Invoke( Delegate method, params object[] args );

        /// <summary>
        /// Zeigt eine einfache Nachricht im OSD an.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="headline">Überschrift für das Nachrichtenfeld.</param>
        /// <param name="realOSD">Gesetzt, wenn das echte OSD verwendet werden soll.</param>
        void ShowMessage( string message, string headline, bool realOSD );

        /// <summary>
        /// Zeigt eine Auswahlliste im OSD an.
        /// </summary>
        /// <param name="headline">Kopfzeile der Liste</param>
        /// <param name="minShow">Minimale Anzahl von Einträgen zum Anzeigen der Liste.</param>
        /// <param name="mode">Die Art des Inhalts der Liste.</param>
        void ShowList( string headline, int minShow, OSDShowMode mode );

        /// <summary>
        /// Zeigt einen Balken an.
        /// </summary>
        /// <param name="percentage">Füllgrad des Balkens in Prozent von 0.0 bis 1.0.</param>
        void ShowPositionInFile( double percentage );

        /// <summary>
        /// Die in <see cref="ShowList"/> verwendete Auswahlliste.
        /// </summary>
        ComboBox ScratchComboBox { get; }

        /// <summary>
        /// Entfernt alle Optionen aus der Konfigurationsliste.
        /// </summary>
        void ResetOptions();

        /// <summary>
        /// Ermittelt alle kontextspezifischen Optionen für die Konfigurationsliste.
        /// </summary>
        void FillOptions();

        /// <summary>
        /// Meldet die Favoritenverwaltung.
        /// </summary>
        ChannelSelector FavoriteManager { get; }

        /// <summary>
        /// Fügt eine Option in die Konfigurationsliste ein.
        /// </summary>
        /// <param name="option"></param>
        void AddOption( OptionDisplay option );

        /// <summary>
        /// Liest oder verändert die Parameter des Videobildes.
        /// </summary>
        PictureParameters PictureParameters { get; set; }

        /// <summary>
        /// Meldet, ob beim Ändern der Direct Show Filter ein Neustart des
        /// Graphen unterstützt wird.
        /// </summary>
        bool CanRestartGraph { get; }
    }
}
