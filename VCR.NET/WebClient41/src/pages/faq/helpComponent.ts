/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient {
    export abstract class HelpComponent {
        abstract getTitle(): string;

        abstract render(page: App.NoUi.IPage): JSX.Element;
    }
}
