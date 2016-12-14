namespace VCRNETClient.App {
    export abstract class Page {
        abstract getName(): string;

        abstract reset(): void;

        constructor(protected readonly application: Application) {
        }
    }
}