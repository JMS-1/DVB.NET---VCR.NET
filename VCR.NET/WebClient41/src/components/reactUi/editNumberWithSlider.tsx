/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {

    export class EditNumberWithSlider extends NoUiView<App.NoUi.INumberSlider>  {
        render(): JSX.Element {
            return <div className="vcrnet-slider">
                <div></div>
                <div><div style={{ left: `${100 * this.props.noui.position}%` }}></div></div>
            </div>;
        }
    }
}
