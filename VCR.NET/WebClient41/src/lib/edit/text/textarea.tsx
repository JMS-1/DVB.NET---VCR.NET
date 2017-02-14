/// <reference path="../../reactUi.tsx" />

namespace JMSLib.ReactUi {

    interface IEditTextArea extends IComponent<App.IString> {
        rows: number;

        columns: number;
    }

    export class EditTextArea extends ComponentExWithSite<App.IString, IEditTextArea>  {
        render(): JSX.Element {
            return <textarea className="jmslib-edittextarea"
                rows={this.props.rows}
                cols={this.props.columns}
                value={this.props.noui.value}
                title={this.props.noui.message}
                onChange={ev => this.props.noui.value = (ev.target as HTMLTextAreaElement).value} />;
        }
    }
}
