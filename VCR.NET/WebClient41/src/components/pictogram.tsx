/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IPictStatic {
        name: string;

        type?: string;
    }

    interface IPictDynamic {
    }

    export class Pictogram extends React.Component<IPictStatic, IPictDynamic>{
        render(): JSX.Element {
            return <img className="vcrnet-pict" src={`ui/images/${this.props.name}.${this.props.type || "png"}`}></img>;
        }
    }
}
