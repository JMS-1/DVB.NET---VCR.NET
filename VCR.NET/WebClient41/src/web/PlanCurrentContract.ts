module VCRServer {

    export interface PlanCurrentContract {
        // Das Gerät, auf dem die Aktivität stattfindet
        device: string;

        // Der Name der Aktivität
        name: string;

        // Die Kennung der Quelle
        source: string;

        // Der Name der Quelle
        sourceName: string;

        // Der Startzeitpunkt in ISO Notation
        start: string;

        // Die Dauer in Sekunden
        duration: number;

        // Gesetzt, wenn Daten aus der Programmzeitschrift für die Dauer der Aktivität vorliegen
        epg: boolean;

        // Eine eindeutige Kennung einer Aufzeichnung zum Abruf der Detailinformationen
        id: string;

        // Eine eindeutige Kennung einer laufenden Aufzeichnung oder Aufgabe, mit Hilfe derer diese beendet werden kann
        referenceId: string;

        // Gesetzt, wenn eine zukünftige Aktivität verspätet beginnen wird
        late: boolean;

        // Zeigt an, dass dieser Eintrag nur ein Platzhalter für ein Gerät ist, für das keine Planungsdaten vorliegen.
        isIdle: boolean;

        // Hinweistext mit einer Größenangabe
        size: string;

        // Die interne laufende Nummer des Aufzeichnungsdatenstroms
        streamIndex: number;

        // Optional die TCP/IP Adresse, an die gerade ein Netzwerkversand stattfindet
        streamTarget: string;

        // Die verbleibende Anzahl von Minuten einer aktiven Aufzeichnung oder Aufgabe
        remainingMinutes: number;
    }

    export function getPlanCurrent(): Promise<PlanCurrentContract[]> {
        return doUrlCall(`plan`);
    }

    export function updateEndTime(device: string, suppressHibernate: boolean, scheduleIdentifier: string, newEnd: Date): Promise<void> {
        return doUrlCall<void, void>(`profile/${device}?disableHibernate=${suppressHibernate}&schedule=${scheduleIdentifier}&endTime=${newEnd.toISOString()}`, `PUT`);
    }

    export function triggerTask(taskName: string): Promise<void> {
        return doUrlCall<void, void>(`plan?${taskName}`, `POST`);
    }

}

