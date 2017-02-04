/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {
    export abstract class HelpComponent implements App.IHelpComponent {
        abstract readonly title: string;

        abstract render(page: App.IPage): JSX.Element;
    }
}
