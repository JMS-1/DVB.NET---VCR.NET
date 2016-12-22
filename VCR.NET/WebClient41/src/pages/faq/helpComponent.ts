/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient {
    export abstract class HelpComponent {
        abstract getTitle(): string;

        abstract render(): JSX.Element;
    }
}
