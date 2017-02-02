/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Admin extends JMSLib.ReactUi.ComponentWithSite<App.IAdminPage>{
        render(): JSX.Element {
            const section = this.props.noui.section;

            return <div className="vcrnet-admin">
                <div className="vcrnet-admin-tabs">
                    <div>
                        {this.props.noui.sectionNames.map(s => <div key={s} data-jmslib-checked={(s === section) ? "yes" : null}>
                            <JMSLib.ReactUi.InternalLink view={`${this.props.noui.route};${s}`}>{this.props.noui.sections[s].display}</JMSLib.ReactUi.InternalLink>
                        </div>)}
                    </div>
                    <div>
                        {this.renderSection()}
                    </div>
                </div>
            </div>;
        }

        private renderSection(): JSX.Element {
            const page = this.props.noui.sections[this.props.noui.section].page;

            switch (this.props.noui.section) {
                case "security":
                    return <AdminSecurity noui={page} />;
                case "directories":
                    return <AdminDirectories noui={page} />;
                case "guide":
                    return <AdminGuide noui={page} />;
                case "devices":
                    return <AdminDevices noui={page} />;
                case "sources":
                    return <AdminSources noui={page} />;
                case "rules":
                    return <AdminRules noui={page} />;
                case "other":
                    return <AdminOther noui={page} />;
                default:
                    return null;
            }
        }
    }

}
