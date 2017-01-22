/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {
    interface IPictogram {
        name: string;

        description?: string;

        type?: string;

        onClick?: () => void;
    }

    export class Pictogram extends React.Component<IPictogram, IEmpty>{
        static imageRoot: string;

        render(): JSX.Element {
            return <img
                className="vcrnet-pict"
                alt={this.props.description}
                src={`${Pictogram.imageRoot}${this.props.name}.${this.props.type || "png"}`}
                onClick={this.props.onClick}></img>;
        }
    }
}
