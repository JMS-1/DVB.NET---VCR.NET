/// <reference path="../../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class AdminDirectories extends JMSLib.ReactUi.ComponentWithSite<App.Admin.IAdminDirectoriesPage>{
        render(): JSX.Element {
            return <div className="vcrnet-admin-directories">
                <h2>Aufzeichnungsverzeichnisse und Dateinamen</h2>
                <div>
                    <JMSLib.ReactUi.EditMultiValueList noui={this.props.noui.directories} />
                    <div>
                        <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.remove} />
                    </div>
                </div>
                <form>
                    <fieldset>
                        <legend>Neues Verzeichnis</legend>
                        <Field page={this.props.noui.page} label={`${this.props.noui.share.text}:`}>
                            <JMSLib.ReactUi.EditText noui={this.props.noui.share} chars={80} />
                        </Field>
                        {this.props.noui.showBrowse ? <div>
                            <i>oder</i>
                            <Field page={this.props.noui.page} label={`${this.props.noui.browse.text}:`}>
                                <JMSLib.ReactUi.EditTextWithList noui={this.props.noui.browse} />
                            </Field>
                        </div> : null}
                        <div>
                            {this.props.noui.showBrowse ? <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.parent} /> : null}
                            <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.add} />
                        </div>
                    </fieldset>
                </form>
                <form>
                    <Field page={this.props.noui.page} label={`${this.props.noui.pattern.text}:`}>
                        <JMSLib.ReactUi.EditText noui={this.props.noui.pattern} chars={60} />
                    </Field>
                </form>
                <div>
                    <JMSLib.ReactUi.ButtonCommand noui={this.props.noui.update} />
                </div>
            </div>;
        }
    }

}
