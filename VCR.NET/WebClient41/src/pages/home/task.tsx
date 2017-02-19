/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Task extends JMSLib.ReactUi.ComponentWithSite<JMSLib.App.ICommand> {
        render(): JSX.Element {
            return <li className="vcrnet-home-task">
                <fieldset>
                    {this.props.children}
                    <div>
                        <JMSLib.ReactUi.ButtonCommand noui={this.props.noui} />
                    </div>
                </fieldset>
            </li>;
        }
    }

}
