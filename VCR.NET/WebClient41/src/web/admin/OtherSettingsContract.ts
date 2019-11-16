module VCRServer {

    // Repräsentiert die Klasse OtherSettings
    export interface OtherSettingsContract extends SettingsContract {
        // Gesetzt, um den Übergang in den Schlafzustand erlauben
        mayHibernate: boolean;

        // Gesetzt, um StandBy für den Schlafzustand zu verwenden
        useStandBy: boolean;

        // Die Verweildauer von Aufträgen im Archiv (in Wochen)
        archive: number;

        // Die Verweildauer von Protokollen (in Wochen)
        protocol: number;

        // Die Vorlaufzeit beim Aufwecken aus dem Schlafzustand (in Sekunden)
        hibernationDelay: number;

        // Gesetzt, um eine PCR Erzeugunbg bei H.264 Material zu vermeiden
        noH264PCR: boolean;

        // Gesetzt, um eine PCR Erzeugung aus MPEG-2 Material zu vermeiden
        noMPEG2PCR: boolean;

        // Die minimale Verweildauer im Schlafzustand
        forcedHibernationDelay: number;

        // Gesetzt, wenn die minimale Verweildauer im Schlafzustand ignoriert werden soll
        suppressHibernationDelay: boolean;

        // Gesetzt, um auch das Basic Protokoll zur Autentisierung zu erlauben
        basicAuth: boolean;

        // Gesetzt, um die Verbindung zu verschlüsseln
        ssl: boolean;

        // Der TCP/IP Port für verschlüsselte Verbindungen
        sslPort: number;

        // Der TCP/IP Port für reguläre Anfragen
        webPort: number;

        // Die Art der Protokollierung
        logging: string;
    }

    export function getOtherSettings(): Promise<OtherSettingsContract> {
        return doUrlCall(`configuration?other`);
    }

    export function setOtherSettings(data: OtherSettingsContract): Promise<boolean> {
        return doUrlCall(`configuration?other`, `PUT`, data);
    }

}

