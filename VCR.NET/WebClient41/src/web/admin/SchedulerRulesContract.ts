module VCRServer {

    // Repräsentiert die Klasse SchedulerRules
    export interface SchedulerRulesContract extends SettingsContract {
        // Die Liste der Regeln
        rules: string;
    }

    export function getSchedulerRules(): JMSLib.App.IHttpPromise<SchedulerRulesContract> {
        return doUrlCall(`configuration?rules`);
    }

    export function setSchedulerRules(data: SchedulerRulesContract): JMSLib.App.IHttpPromise<boolean> {
        return doUrlCall(`configuration?rules`, `PUT`, data);
    }

}

