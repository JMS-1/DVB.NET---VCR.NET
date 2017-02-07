/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IDevice {
        readonly name: string;

        readonly active: JMSLib.App.IValidatedFlag;

        readonly priority: JMSLib.App.IValidatedNumber;

        readonly decryption: JMSLib.App.IValidatedNumber;

        readonly sources: JMSLib.App.IValidatedNumber;
    }

    export class Device implements IDevice {

        readonly name: string;

        readonly active: JMSLib.App.EditFlag;

        readonly priority: JMSLib.App.EditNumber;

        readonly decryption: JMSLib.App.EditNumber;

        readonly sources: JMSLib.App.EditNumber;

        constructor(profile: VCRServer.ProfileContract, onChange: () => void) {
            this.name = profile.name;
            this.active = new JMSLib.App.EditFlag(profile, "active", onChange, null);
            this.priority = new JMSLib.App.EditNumber(profile, "priority", null, null, true, 0, 100);
            this.decryption = new JMSLib.App.EditNumber(profile, "ciLimit", null, null, true, 0, 16);
            this.sources = new JMSLib.App.EditNumber(profile, "sourceLimit", null, null, true, 1, 32);
        }

        validate(defaultDevice: string): void {
            this.active.validate();
            this.priority.validate();
            this.decryption.validate();
            this.sources.validate();

            if (!this.active.value)
                if (this.name === defaultDevice)
                    this.active.message = `Das bevorzugte Geräteprofil muss auch für Aufzeichnungen verwendet werden.`;
        }

        get isValid(): boolean {
            return (this.active.message === ``) && (this.priority.message === ``) && (this.decryption.message === ``) && (this.sources.message === ``);
        }
    }
}