﻿/// <reference path="../../vcrnet.tsx" />

namespace VCRNETClient.Ui {
    export class JobData extends JMSLib.ReactUi.NoUiView<App.NoUi.IJobEditor>{
        render(): JSX.Element {
            return <fieldset className="vcrnet-jobdata">
                <legend>Daten zum Auftrag</legend>

                <Ui.Field page={this.props.noui.page} label={`${this.props.noui.device.text}:`}>
                    <Ui.EditTextWithList noui={this.props.noui.device} />
                    <Ui.EditBoolean noui={this.props.noui.deviceLock} />
                </Ui.Field>

                <Ui.Field page={this.props.noui.page} label={`${this.props.noui.name.text}:`} help="jobsandschedules">
                    <Ui.EditText noui={this.props.noui.name} chars={100} hint="(Jeder Auftrag muss einen Namen haben)" />
                </Ui.Field>

                <Ui.Field page={this.props.noui.page} label={`${this.props.noui.folder.text}:`}>
                    <Ui.EditTextWithList noui={this.props.noui.folder} />
                </Ui.Field>

                <Ui.Field page={this.props.noui.page} label={`${this.props.noui.source.text}:`} help="sourcechooser">
                    <Ui.EditChannel noui={this.props.noui.source} />
                </Ui.Field>

                <Ui.Field page={this.props.noui.page} label={`${this.props.noui.sourceFlags.text}:`} help="filecontents">
                    <Ui.EditChannelFlags noui={this.props.noui.sourceFlags} />
                </Ui.Field>
            </fieldset>;
        }
    }
}
