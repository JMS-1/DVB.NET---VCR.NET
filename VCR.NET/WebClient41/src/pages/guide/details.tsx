/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    interface IGuideDetails extends JMSLib.ReactUi.IComponent<App.Guide.IGuideEntry> {
        page: App.IPage;
    }

    export class GuideDetails extends JMSLib.ReactUi.ComponentEx<App.Guide.IGuideEntry, IGuideDetails> {
        render(): JSX.Element {
            return <form className="vcrnet-guideentrydetails">
                <fieldset>
                    <GuideEntryInfo noui={this.props.noui} />
                </fieldset>
                <div>
                    <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.createNew} />
                    <Field page={this.props.page} label={`(${this.props.noui.jobSelector.text}:`}><JMSLib.ReactUi.SelectSingleFromList noui={this.props.noui.jobSelector} />)</Field>
                </div>
            </form>;
        }
    }

}
