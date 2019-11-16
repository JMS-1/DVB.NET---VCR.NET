module VCRServer {

    // Repräsentiert die Klasse GuideSettings
    export interface GuideSettingsContract extends SettingsContract {
        // Das Zeitintervall (in Stunden) für vorgezogene Aktualisierungen
        joinHours: string;

        // Das minimale Intervall (in Stunden) zwischen Aktualisierungen
        minDelay: string;

        // Die maximale Dauer einer Aktualisierung (in Minuten)
        duration: number;

        // Die vollen Stunden, zu denen eine Aktualisierung stattfinden soll
        hours: number[];

        // Alle Quellen, die bei der Aktualisierung mit berücksichtigt werden sollen
        sources: string[];

        // Gesetzt, um auch die britische Programmzeitschrift auszuwerten
        includeUK: boolean;
    }

    export function getGuideSettings(): Promise<GuideSettingsContract> {
        return doUrlCall(`configuration?guide`);
    }

    export function setGuideSettings(data: GuideSettingsContract): Promise<boolean> {
        return doUrlCall(`configuration?guide`, `PUT`, data);
    }

}

