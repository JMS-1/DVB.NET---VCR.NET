/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class EditBooleanWithButton extends ComponentWithSite<App.IFlag>  {
        render(): JSX.Element {
            return <button className="jmslib-editflagbutton" data-jmslib-checked={this.props.noui.value ? "yes" : null} onClick={ev => this.onClick(ev)}>
                {this.props.noui.text}
            </button>;
        }

        private onClick(ev: React.FormEvent): void {
            ev.preventDefault();

            this.props.noui.value = !this.props.noui.value;
        }
    }
}
