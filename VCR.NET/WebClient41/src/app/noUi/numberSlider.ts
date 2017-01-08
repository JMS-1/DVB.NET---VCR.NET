/// <reference path="generic.ts" />

namespace VCRNETClient.App.NoUi {

    export interface INumberSlider {
        readonly position: number;
    }

    export class NumberSlider implements INumberSlider {
        position: number = 0;
    }
}