/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    // Schnittstelle zur Pflege eines einzelnen Geräteprofils.
    export interface IDevice {
        // Der Name des Gerätes.
        readonly name: string;

        // Gesetzt, wenn das Gerät für den VCR.NET Recording Service verwendet werden soll.
        readonly active: JMSLib.App.IFlag;

        // Planungspriorität des Gerätes.
        readonly priority: JMSLib.App.INumber;

        // Anzahl der gleichzeitg entschlüsselbaren Quellen.
        readonly decryption: JMSLib.App.INumber;

        // Anzahl der gleichzeitig empfangbaren Quellen.
        readonly sources: JMSLib.App.INumber;
    }

    // Das Präsentationsmodell zur Pflege eines Gerätes.
    export class Device implements IDevice {

        // Der Name des Gerätes.
        readonly name: string;

        // Gesetzt, wenn das Gerät für den VCR.NET Recording Service verwendet werden soll.
        readonly active: JMSLib.App.Flag;

        // Planungspriorität des Gerätes.
        readonly priority: JMSLib.App.Number;

        // Anzahl der gleichzeitg entschlüsselbaren Quellen.
        readonly decryption: JMSLib.App.Number;

        // Anzahl der gleichzeitig empfangbaren Quellen.
        readonly sources: JMSLib.App.Number;

        // Erstellt ein neues Präsentationsmodell.
        constructor(profile: VCRServer.ProfileContract, onChange: () => void, private readonly _defaultDevice: () => string) {
            this.name = profile.name;
            this.priority = new JMSLib.App.Number(profile, "priority")
                .addRequiredValidator()
                .addMinValidator(0)
                .addMaxValidator(100);
            this.decryption = new JMSLib.App.Number(profile, "ciLimit")
                .addRequiredValidator()
                .addMinValidator(0)
                .addMaxValidator(16);
            this.sources = new JMSLib.App.Number(profile, "sourceLimit")
                .addRequiredValidator()
                .addMinValidator(1)
                .addMaxValidator(32);
            this.active = new JMSLib.App.Flag(profile, "active", null, onChange, null, flag => this.validateDefaultDevice(flag));
        }

        // Wird ein Gerät nicht verwendet, so darf es auch nicht als Vorgabegerät eingetragen sein.
        private validateDefaultDevice(active: JMSLib.App.Flag): string {
            if (!active.value)
                if (this.name === this._defaultDevice())
                    return `Das bevorzugte Geräteprofil muss auch für Aufzeichnungen verwendet werden.`;
        }

        // Führt eine Konsistenzprüfung aus.
        validate(): void {
            this.active.validate();
            this.priority.validate();
            this.decryption.validate();
            this.sources.validate();
        }

        // Prüft, ob die Konfiguration des Gerätes gültig ist.
        get isValid(): boolean {
            return (this.active.message === ``) && (this.priority.message === ``) && (this.decryption.message === ``) && (this.sources.message === ``);
        }
    }
}