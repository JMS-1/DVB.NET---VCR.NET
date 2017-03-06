/// <reference path="../../lib.react/reactUi.tsx" />

namespace VCRNETClient.Ui {

    // Hilfskomponente zum Erstellen von Ract.JS Konfigurationskomponenten.
    export abstract class AdminSection<TSectionType extends App.Admin.ISection> extends JMSLib.ReactUi.ComponentWithSite<TSectionType>{
        // Oberflächenelemente erstellen.
        render(): JSX.Element {
            return <div className="vcrnet-admin-section">
                <h2>{this.title}</h2>
                {this.renderSection()}
                <div><JMSLib.ReactUi.ButtonCommand uvm={this.props.uvm.update} /></div>
            </div>;
        }

        // Die Überschrift für diesen Bereich.
        protected abstract readonly title: string;

        // Oberflächenelemente erstellen.
        protected abstract renderSection(): JSX.Element;
    }

}
