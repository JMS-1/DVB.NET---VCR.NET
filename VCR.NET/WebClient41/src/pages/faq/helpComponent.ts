/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient {
    export abstract class HelpComponent implements App.IHelpComponent {
        abstract getTitle(): string;

        abstract render(page: App.IPage): JSX.Element;
    }
}
