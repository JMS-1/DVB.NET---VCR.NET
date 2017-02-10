/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    interface IGuideDetails extends JMSLib.ReactUi.IComponent<App.Guide.IGuideEntry> {
        page: App.IPage;
    }

    export class GuideDetails extends JMSLib.ReactUi.ComponentEx<App.Guide.IGuideEntry, IGuideDetails> {
        render(): JSX.Element {
            return <form className="vcrnet-guideentrydetails">
                <fieldset>
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
                </fieldset>
                <div>
                    <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.createNew} />
                    <Field page={this.props.page} label={`(${this.props.noui.jobSelector.text}:`}><JMSLib.ReactUi.SelectSingleFromList noui={this.props.noui.jobSelector} />)</Field>
                </div>
            </form>;
        }
    }

}
