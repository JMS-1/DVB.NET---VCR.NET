/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class EditBooleanWithButton extends ComponentWithSite<App.IFlag>  {
        render(): JSX.Element {
            return <div
                className="jmslib-editflagbutton jmslib-command"
                data-jmslib-checked={this.props.noui.value ? "yes" : null}
                onClick={ev => this.onClick(ev)}
                title="">
                {this.props.noui.text}
            </div>;
        }

        private onClick(ev: React.FormEvent): void {
            ev.preventDefault();

            this.props.noui.value = !this.props.noui.value;
        }
    }
}
