module VCRServer {

    // Repräsentiert die Klasse SourceScanSettings
    export interface SourceScanSettingsContract extends SettingsContract {
        // Das Zeitinterval (in Stunden) für vorgezogene Aktualisierungen
        joinDays: string;

        // Das minimale Intervall (in Tagen) zwischen den Aktualisierungen - negative Werte für eine ausschließlich manuelle Aktualisierung
        interval: number;

        // Die maximale Dauer einer Aktualisierung (in Minuten)
        duration: number;

        // Die vollen Stunden, zu denen eine Aktualisierung stattfinden soll
        hours: number[];

        // Gesetzt, wenn die neu ermittelten Listen mit den alten zusammengeführt werden sollen
        merge: boolean;
    }

    export function getSourceScanSettings(): Promise<SourceScanSettingsContract> {
        return doUrlCall(`configuration?scan`);
    }

    export function setSourceScanSettings(data: SourceScanSettingsContract): Promise<boolean> {
        return doUrlCall(`configuration?scan`, `PUT`, data);
    }

}

