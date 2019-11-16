module VCRServer {

    // Beschreibt einen Eintrag im Aufzeichnungsplan.
    export interface PlanActivityContract {
        // Beginn der Aufzeichnung im ISO Format.
        start?: string;

        // Dauer der Aufzeichung in Sekunden.
        duration: string;

        // Name der Aufzeichnung.
        name: string;

        // Gerät, auf dem aufgezeichnet wird.
        device?: string;

        // Sender, von dem aufgezeichnet wird.
        station?: string;

        // Gesetzt, wenn die Aufzeichnung verspätet beginnt.
        late: boolean;

        // Gesetzt, wenn die Aufzeichnung gar nicht ausgeführt wird.
        lost: boolean;

        // Gesetzt, wenn Informationen aus der Programmzeitschrift vorliegen.
        epg: boolean;

        // Das Gerät, in dessen Programmzeitschrift die Aufzeichnung gefunden wurde.
        epgDevice?: string;

        // Die Quelle zur Aufzeichnung in der Programzeitschrift.
        source?: string;

        // Die eindeutige Kennung der Aufzeichnung.
        id: string;

        // Gesetzt, wenn die Endzeit durch eine Sommer-/Winterzeitumstellung nicht korrekt ist.
        suspectEndTime: boolean;

        // Gesetzt, wenn alle Tonspuren aufgezeichnet werden sollen.
        allAudio: boolean;

        // Gesetzt, wenn Dolby Tonspuren aufgezeichnet werden sollen.
        ac3: boolean;

        // Gesetzt, wenn der Videotext mit aufgezeichnet werden soll.
        ttx: boolean;

        // Gesetzt, wenn DVB Untertitel mit aufgezeichnet werden sollen.
        dvbsub: boolean;

        // Gesetzt, wenn die Aufzeichnung laut Programmzeitschrift gerade läuft.
        epgCurrent: boolean;

        // Aktive Ausnahmeregel für die Aufzeichnung.
        exception?: PlanExceptionContract;
    }

    export function getPlan(limit: number, end: Date): Promise<PlanActivityContract[]> {
        return doUrlCall(`plan?limit=${limit}&end=${end.toISOString()}`);
    }

}

