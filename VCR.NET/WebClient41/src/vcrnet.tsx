// Das muss noch geklärt werden: import geht leider nicht mit AMD und sonst können wir das nicht alles in eine Quelldatei bekommen.
import React = __React;
import ReactDOM = __React.__DOM;

namespace VCRNETClient {

    // Beschreibt eine React.Js Komponente für ein NoUi Präsentationsmodell.
    export interface INoUiComponent<TViewModelType> {
        // Das Präsentationsmodell.
        noui: TViewModelType;
    }

    // Beschreibt einen nicht vorhandenen Zustand einer React.Js Komponente.
    export interface INoDynamicState {
    }

    // Implementierung einer React.Js Komponente für ein NoUi Präsentationsmodell.
    export abstract class NoUiView<TViewModelType> extends React.Component<INoUiComponent<TViewModelType>, INoDynamicState>
    {
    }

    // Implementierung einer React.Js Komponente für ein NoUi Präsentationsmodell.
    export abstract class NoUiViewWithSite<TViewModelType extends App.NoUi.INoUiWithSite> extends NoUiView<TViewModelType> implements App.NoUi.INoUiSite {
        componentWillMount(): void {
            this.props.noui.setSite(this);
        }

        componentWillUnmount(): void {
            this.props.noui.setSite(undefined);
        }

        refresh(): void {
            this.forceUpdate();
        }
    }

    // Initialisiert die react.js Laufzeitumgebung.
    export function startup(): void {
        ReactDOM.render(<Main />, document.querySelector(`vcrnet-spa`));
    }
}
