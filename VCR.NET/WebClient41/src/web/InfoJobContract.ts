module VCRServer {

    // Repräsentiert die Klasse InfoJob
    export interface InfoJobContract {
        // Der Name des Auftrags
        name: string;

        // Die eindeutige Kennung des Auftrags
        id: string;

        // Alle Aufzeichnung zum Auftrag
        schedules: InfoScheduleContract[];

        // Gesetzt, wenn der Auftrag noch nicht in das Archiv übertragen wurde
        active: boolean;
    }

    export function getInfoJobs(): Promise<InfoJobContract[]> {
        return doUrlCall(`info?jobs`);
    }

}

