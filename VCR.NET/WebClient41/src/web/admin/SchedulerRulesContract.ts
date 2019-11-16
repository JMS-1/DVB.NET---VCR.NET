module VCRServer {

    // Repräsentiert die Klasse SchedulerRules
    export interface SchedulerRulesContract extends SettingsContract {
        // Die Liste der Regeln
        rules: string;
    }

    export function getSchedulerRules(): Promise<SchedulerRulesContract> {
        return doUrlCall(`configuration?rules`);
    }

    export function setSchedulerRules(data: SchedulerRulesContract): Promise<boolean> {
        return doUrlCall(`configuration?rules`, `PUT`, data);
    }

}

