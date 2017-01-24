/// <reference path="../reactUi.tsx" />

namespace JMSLib.ReactUi {
    interface IInternalLink {
        view: string | (() => void);

        pict?: string
    }

    export class InternalLink extends React.Component<IInternalLink, IEmpty>{
        render(): JSX.Element {
            var target = "javascript:void(0)";
            var click = undefined;

            if (typeof this.props.view === "function")
                click = this.props.view;
            else
                target = "#" + this.props.view;

            return <span>
                {this.props.pict ? <a className="vcrnet-jmslib" href={target} onClick={click}><Pictogram name={this.props.pict} /></a> : null}
                {this.props.children ? <a className="vcrnet-jmslib" href={target} onClick={click}>{this.props.children}</a> : null}
            </span>;
        }
    }
}
