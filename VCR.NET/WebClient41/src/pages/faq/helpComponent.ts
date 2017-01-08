/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient {
    export abstract class HelpComponent implements App.NoUi.IHelpComponent {
        abstract getTitle(): string;

        abstract render(page: App.NoUi.IPage): JSX.Element;
    }
}
