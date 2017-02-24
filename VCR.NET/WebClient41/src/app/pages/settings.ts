/// <reference path="page.ts" />

namespace VCRNETClient.App {

    export interface ISettingsPage extends IPage {
        readonly planDays: JMSLib.App.INumber;

        readonly maxFavorites: JMSLib.App.INumber;

        readonly guideRows: JMSLib.App.INumber;

        readonly preGuide: JMSLib.App.INumber;

        readonly postGuide: JMSLib.App.INumber;

        readonly dolby: JMSLib.App.IFlag;

        readonly allAudio: JMSLib.App.IFlag;

        readonly ttx: JMSLib.App.IFlag;

        readonly subs: JMSLib.App.IFlag;

        readonly noSleep: JMSLib.App.IFlag;

        readonly backToGuide: JMSLib.App.IFlag;

        readonly type: JMSLib.App.IValueFromList<string>;

        readonly encryption: JMSLib.App.IValueFromList<string>;

        readonly update: JMSLib.App.ICommand;
    }

    export class SettingsPage extends Page implements ISettingsPage {

        private static readonly _types = [
            JMSLib.App.uiValue("RT", "Alle Quellen"),
            JMSLib.App.uiValue("R", "Nur Radio"),
            JMSLib.App.uiValue("T", "Nur Fernsehen")
        ];

        private static readonly _encryptions = [
            JMSLib.App.uiValue("FP", "Alle Quellen"),
            JMSLib.App.uiValue("P", "Nur verschlüsselte Quellen"),
            JMSLib.App.uiValue("F", "Nur unverschlüsselte Quellen")
        ];

        readonly planDays = new JMSLib.App.Number({}, "planDays", "Anzahl der Vorschautage im Aufzeichnungsplan", () => this.refreshUi())
            .addRequiredValidator()
            .addMinValidator(1)
            .addMaxValidator(50);

        readonly maxFavorites = new JMSLib.App.Number({}, "recentSourceLimit", "Maximale Größe der Liste zuletzt verwendeter Sendern", () => this.refreshUi())
            .addRequiredValidator()
            .addMinValidator(1)
            .addMaxValidator(50);

        readonly guideRows = new JMSLib.App.Number({}, "guideRows", "Anzahl der Einträge pro Seite in der Programmzeitschrift", () => this.refreshUi())
            .addRequiredValidator()
            .addMinValidator(10)
            .addMaxValidator(100);

        readonly preGuide = new JMSLib.App.Number({}, "guideAheadStart", "Vorlaufzeit bei Programmierung über die Programmzeitschrift (in Minuten)", () => this.refreshUi())
            .addRequiredValidator()
            .addMinValidator(0)
            .addMaxValidator(240);

        readonly postGuide = new JMSLib.App.Number({}, "guideBeyondEnd", "Nachlaufzeit bei Programmierung über die Programmzeitschrift (in Minuten)", () => this.refreshUi())
            .addRequiredValidator()
            .addMinValidator(0)
            .addMaxValidator(240);

        readonly dolby = new JMSLib.App.Flag({}, "dolby", "Dolby Digital (AC3)");

        readonly allAudio = new JMSLib.App.Flag({}, "languages", "Alle Sprachen");

        readonly ttx = new JMSLib.App.Flag({}, "videotext", "Videotext");

        readonly subs = new JMSLib.App.Flag({}, "subtitles", "DVB Untertitel");

        readonly noSleep = new JMSLib.App.Flag({}, "suppressHibernate", "Beim Abbrechen von Aufzeichnungen bevorzugt den Schlafzustand unterdrücken");

        readonly backToGuide = new JMSLib.App.Flag({}, "backToGuide", "Nach Anlegen einer neuen Aufzeichnung zurück zur Programmzeitschrift");

        readonly type = new JMSLib.App.SelectSingleFromList<string>({}, "typeFilter", null, null, true, SettingsPage._types);

        readonly encryption = new JMSLib.App.SelectSingleFromList<string>({}, "encryptionFilter", null, null, true, SettingsPage._encryptions);

        readonly update = new JMSLib.App.Command(() => this.save(), "Aktualisieren", () => this.isValid);

        constructor(application: Application) {
            super(`settings`, application);
        }

        reset(sections: string[]): void {
            this.update.reset();

            // Einfache Kopie.
            var newProfile = { ...this.application.profile };

            // Tiefe Kopie.
            newProfile.recentSources = newProfile.recentSources.slice();

            // Binden.
            this.maxFavorites.data = newProfile;
            this.backToGuide.data = newProfile;
            this.encryption.data = newProfile;
            this.guideRows.data = newProfile;
            this.postGuide.data = newProfile;
            this.preGuide.data = newProfile;
            this.planDays.data = newProfile;
            this.allAudio.data = newProfile;
            this.noSleep.data = newProfile;
            this.dolby.data = newProfile;
            this.type.data = newProfile;
            this.subs.data = newProfile;
            this.ttx.data = newProfile;

            this.application.isBusy = false;
        }

        get title(): string {
            return `Individuelle Einstellungen ändern`;
        }

        private get isValid(): boolean {
            if (this.planDays.message !== ``)
                return false;
            if (this.guideRows.message !== ``)
                return false;
            if (this.preGuide.message !== ``)
                return false;
            if (this.postGuide.message !== ``)
                return false;
            if (this.maxFavorites.message !== ``)
                return false;

            return true;
        }

        private save(): JMSLib.App.IHttpPromise<void> {
            return VCRServer.setUserProfile(this.planDays.data).then(profile => {
                this.application.profile = profile;
                this.application.gotoPage(null);
            });
        }
    }
}