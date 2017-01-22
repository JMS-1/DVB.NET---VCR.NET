namespace VCRNETClient.App {

    export interface IGuideEntry {
    }

    export class GuideEntry implements IGuideEntry {
        constructor(model: VCRServer.GuideItemContract) {
        }
    }
}