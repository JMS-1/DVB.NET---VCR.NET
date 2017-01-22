/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IPictStatic {
        name: string;

        description?: string;

        type?: string;

        onClick?: () => void;
    }

    export class Pictogram extends React.Component<IPictStatic, JMSLib.ReactUi.IEmpty>{
        render(): JSX.Element {
            return <img className="vcrnet-pict" alt={this.props.description} src={`ui/images/${this.props.name}.${this.props.type || "png"}`} onClick={this.props.onClick}></img>;
        }
    }
}
