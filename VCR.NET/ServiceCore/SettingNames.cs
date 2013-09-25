

namespace JMS.DVBVCR.RecordingService
{
    /// <summary>
    /// Enthält die Namen aller Konfigurationsparameter.
    /// </summary>
    public enum SettingNames
    {
        /// <summary>
        /// Der TCP/IP Port, an den der Web Server gebunden werden soll.
        /// </summary>
        TCPPort,

        /// <summary>
        /// Das Muster, nach dem die Namen der Aufzeichnungsdateien erzeugt werden.
        /// </summary>
        FileNamePattern,

        /// <summary>
        /// Das primäre Aufzeichnungsverzeichnis. Nur dieses und untergeordnete Verzeichnisse können
        /// zur Ablage von Aufzeichnungsdateien verwerden werden.
        /// </summary>
        VideoRecorderDirectory,

        /// <summary>
        /// Eine Liste von zusätzlichen Verzeichnissen, in denen Aufzeichnungen abgelegt werden dürfen.
        /// </summary>
        AdditionalRecorderPaths,

        /// <summary>
        /// Alle zu verwendenden DVB.NET Geräteprofile. Die einzelnen Namen sind durch einen 
        /// senkrechten Strich getrennt.
        /// </summary>
        Profiles,

        /// <summary>
        /// Die Zeitspanne, nach der archivierte Aufträge automatisch entfernt werden (in Wochen).
        /// </summary>
        ArchiveLifeTime,

        /// <summary>
        /// Die Zeitspanne, nach der Protokolleinträge automatisch entfernt werden (in Wochen).
        /// </summary>
        LogLifeTime,

        /// <summary>
        /// Beschreibt den Umfang der Protokollierung.
        /// </summary>
        LoggingLevel,

        /// <summary>
        /// Wird gesetzt, um die Hardwarezugriff in einen eigenen Prozess auszulagern.
        /// </summary>
        UseExternalCardServer,

        /// <summary>
        /// Die durch Kommas getrennte Liste der Sender, die in der Programmzeitschrift berücksichtigt werden sollen.
        /// </summary>
        EPGStations,

        /// <summary>
        /// Die maximale Laufzeit für die Aktualisierung der Programmzeitschrift.
        /// </summary>
        EPGDuration,

        /// <summary>
        /// Die Programmzeitschrift berücksichtigt auch die englischen Sender.
        /// </summary>
        EPGIncludeFreeSat,

        /// <summary>
        /// Die Liste aller vollen Stunden, zu denen die Programmzeitschrift aktualisiert
        /// werden soll.
        /// </summary>
        EPGHours,

        /// <summary>
        /// Die maximale Laufzeit für die Aktualisierung der Quellen eines Geräteprofils.
        /// </summary>
        ScanDuration,

        /// <summary>
        /// Der Abstand in Tagen zwischen der Aktualisierung der Quellen eines Geräteprofils.
        /// </summary>
        ScanInterval,

        /// <summary>
        /// Die Liste aller vollen Stunden, zu denen die Programmzeitschrift aktualisiert werden soll.
        /// </summary>
        ScanHours,

        /// <summary>
        /// Wird gesetzt, um nach einer Aktualisierung der Quellen das Ergebnis zusammen
        /// zu führen.
        /// </summary>
        MergeScanResult,

        /// <summary>
        /// Wird gesetzt, wenn der VCR.NET Recording Service den Rechner in den Schlafzustand
        /// versetzten darf.
        /// </summary>
        MayHibernateSystem,

        /// <summary>
        /// Wenn der VCR.NET Recording Service einen Schlafzustand einleitet, so wird
        /// dafür S3 (Standby) und nicht S4 (Hibernate) verwendet.
        /// </summary>
        UseStandByForHibernation,

        /// <summary>
        /// Die Vorlaufzeit in Sekunden, die bei dem Aufwachen aus dem Schlafzustand erwartet wird.
        /// </summary>
        HibernationDelay,

        /// <summary>
        /// Optional der Name der Windows Gruppe der Anwender, die Zugiff auf den VCR.NET
        /// Recording Server haben.
        /// </summary>
        RequiredUserRole,

        /// <summary>
        /// Optional der Name der Windows Gruppe von Anwendern, die administrativen Zugriff
        /// auf den VCR.NET Recording Service.
        /// </summary>
        RequiredAdminRole,

        /// <summary>
        /// Optional die Größe für das Zwischenspeichern bei Schreiben in die Datei einer 
        /// Radioaufzeichnung.
        /// </summary>
        TSAudioBufferSize,

        /// <summary>
        /// Optional die Größe für das Zwischenspeichern bei Schreiben in die Datei einer 
        /// Fernsehaufzeichnung geringer Qualität.
        /// </summary>
        TSSDTVBufferSize,

        /// <summary>
        /// Optional die Größe für das Zwischenspeichern bei Schreiben in die Datei einer 
        /// Fernsehaufzeichnung hoher Qualität.
        /// </summary>
        TSHDTVBufferSize,

        /// <summary>
        /// Der minimale Abstand zwischen zwei Aktualisierungen der Programmzeitschrift.
        /// </summary>
        EPGInterval,

        /// <summary>
        /// Der zeitliche Schwellwert für eine vorzeitige Aktualisierung der Programmzeitschrift.
        /// </summary>
        EPGJoinThreshold,

        /// <summary>
        /// Der zeitliche Schwellwert für eine vorzeitige Aktualisierung der Liste der Quellen
        /// (Sendersuchlauf).
        /// </summary>
        ScanJoinThreshold,

        /// <summary>
        /// Verbietet es, aus einem H.264 Bildsignal die Zeitbasis (PCR) abzuleiten.
        /// </summary>
        DisablePCRFromH264Generation,

        /// <summary>
        /// Verbietet es, aus einem MPEG2 Bildsignal die Zeitbasis (PCR) abzuleiten.
        /// </summary>
        DisablePCRFromMPEG2Generation,

        /// <summary>
        /// Die Zeit nach dem erzwungenen Übergang in den Schlafzustand in dem der Rechner
        /// nicht durch den <i>VCR.NET Recording Service</i> aufgeweckt wird (in Minuten).
        /// </summary>
        DelayAfterForcedHibernation,

        /// <summary>
        /// Gesetzt, wenn der Übergang in den Schlafzustand nicht weiter beeinflusst werden soll.
        /// </summary>
        SuppressDelayAfterForcedHibernation,

        /// <summary>
        /// Gesetzt, wenn der Zugriff auf die Web Dienste verschlüsselt erfolgen soll.
        /// </summary>
        UseSSL,

        /// <summary>
        /// Der TCP/IP Port, an den der Web Server bei einer sicheren Verbindung gebunden werden soll.
        /// </summary>
        SSLPort,

        /// <summary>
        /// Gesetzt, wenn auch die unsichere Basic Authentisierung erlaubt ist.
        /// </summary>
        AllowBasic,
    }
}
