namespace VCRNETClient.App {
    export abstract class Page {
        abstract getName(): string;

        constructor(protected readonly application: Application) {
        }
    }
}