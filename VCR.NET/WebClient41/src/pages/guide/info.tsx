﻿/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class GuideEntryInfo extends JMSLib.ReactUi.Component<App.Guide.IGuideInfo> {
        render(): JSX.Element {
            return <div className="vcrnet-guideentryinfo">
                <table>
                    <tbody>
                        <tr>
                            <td>Name:</td>
                            <td>{this.props.noui.name}</td>
                        </tr>
                        <tr>
                            <td>Sprache:</td>
                            <td>{this.props.noui.language}</td>
                        </tr>
                        <tr>
                            <td>Sender:</td>
                            <td>{this.props.noui.source}</td>
                        </tr>
                        <tr>
                            <td>Beginn:</td>
                            <td>{this.props.noui.startDisplay}</td>
                        </tr>
                        <tr>
                            <td>Ende:</td>
                            <td>{this.props.noui.endDisplay}</td>
                        </tr>
                        <tr>
                            <td>Dauer:</td>
                            <td>{this.props.noui.duration}</td>
                        </tr>
                        <tr>
                            <td>Freigabe:</td>
                            <td>{this.props.noui.rating}</td>
                        </tr>
                        <tr>
                            <td>Inhalt:</td>
                            <td>{this.props.noui.content}</td>
                        </tr>
                        <tr>
                            <td>Beschreibung:</td>
                            <td>{this.props.noui.shortDescription}</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>{this.props.noui.longDescription}</td>
                        </tr>
                    </tbody>
                </table>
                <div>
                    <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.findSimiliar} />
                </div>
            </div>;
        }
    }

}