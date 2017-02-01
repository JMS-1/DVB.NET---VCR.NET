/// <reference path="../lib/reactUi.tsx" />

namespace VCRNETClient.Ui {

    export class Admin extends JMSLib.ReactUi.ComponentWithSite<App.IAdminPage>{
        render(): JSX.Element {
            const section = this.props.noui.section;

            return <div className="vcrnet-admin">
                <div className="vcrnet-admin-tabs">
                    <div>
                        {this.props.noui.sections.map(s => <div key={s} data-jmslib-checked={(s === section) ? "yes" : null}>
                            <JMSLib.ReactUi.InternalLink view={`${this.props.noui.route};${s}`}>{this.props.noui.sectionNames[s]}</JMSLib.ReactUi.InternalLink>
                        </div>)}
                    </div>
                    <div>
                        {this.renderSection()}
                    </div>
                    <div />
                </div>
            </div>;
        }

        private renderSection(): JSX.Element {
            switch (this.props.noui.section) {
                case "security":
                    return <AdminSecurity noui={this.props.noui} />;
                case "directories":
                    return <AdminDirectories noui={this.props.noui} />;
                case "guide":
                    return <AdminGuide noui={this.props.noui} />;
                case "devices":
                    return <AdminDevices noui={this.props.noui} />;
                case "sources":
                    return <AdminSources noui={this.props.noui} />;
                case "rules":
                    return <AdminRules noui={this.props.noui} />;
                case "other":
                    return <AdminOther noui={this.props.noui} />;
                default:
                    return null;
            }
        }
    }

}
