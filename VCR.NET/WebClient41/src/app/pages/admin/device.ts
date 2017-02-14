/// <reference path="../admin.ts" />

namespace VCRNETClient.App.Admin {

    export interface IDevice {
        readonly name: string;

        readonly active: JMSLib.App.IFlag;

        readonly priority: JMSLib.App.INumber;

        readonly decryption: JMSLib.App.INumber;

        readonly sources: JMSLib.App.INumber;
    }

    export class Device implements IDevice {

        readonly name: string;

        readonly active: JMSLib.App.Flag;

        readonly priority: JMSLib.App.Number;

        readonly decryption: JMSLib.App.Number;

        readonly sources: JMSLib.App.Number;

        constructor(profile: VCRServer.ProfileContract, onChange: () => void) {
            this.name = profile.name;
            this.active = new JMSLib.App.Flag(profile, "active", null, onChange);
            this.priority = new JMSLib.App.Number(profile, "priority", null, null, true, 0, 100);
            this.decryption = new JMSLib.App.Number(profile, "ciLimit", null, null, true, 0, 16);
            this.sources = new JMSLib.App.Number(profile, "sourceLimit", null, null, true, 1, 32);
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