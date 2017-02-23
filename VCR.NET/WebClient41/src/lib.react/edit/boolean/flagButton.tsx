/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    export class EditBooleanWithButton extends ComponentWithSite<App.IFlag>  {
        render(): JSX.Element {
            return <div
                className="jmslib-editflagbutton jmslib-command"
                data-jmslib-checked={this.props.uvm.value ? "yes" : null}
                onClick={ev => this.onClick(ev)}
                title="">
                {this.props.uvm.text}
            </div>;
        }

        private onClick(ev: React.FormEvent): void {
            ev.preventDefault();

            this.props.uvm.value = !this.props.uvm.value;
        }
    }
}
