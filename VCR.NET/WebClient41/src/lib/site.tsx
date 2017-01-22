﻿// Das muss noch geklärt werden: import geht leider nicht mit AMD und sonst können wir das nicht alles in eine Quelldatei bekommen.
import React = __React;
import ReactDOM = __React.__DOM;

namespace JMSLib.ReactUi {

    // Beschreibt eine React.Js Komponente für ein NoUi Präsentationsmodell.
    export interface INoUiComponent<TViewModelType> {
        // Das Präsentationsmodell.
        noui: TViewModelType;
    }

    // Beschreibt einen nicht vorhandenen Zustand einer React.Js Komponente.
    export interface INoDynamicState {
    }

    // Implementierung einer React.Js Komponente für ein NoUi Präsentationsmodell.
    export abstract class NoUiViewEx<TViewModelType, TConfigType extends INoUiComponent<TViewModelType>> extends React.Component<TConfigType, INoDynamicState>
    {
    }

    export abstract class NoUiView<TViewModelType> extends NoUiViewEx<TViewModelType, INoUiComponent<TViewModelType>>
    {
    }

    // Implementierung einer React.Js Komponente für ein NoUi Präsentationsmodell.
    export abstract class NoUiViewExWithSite<TViewModelType extends App.IUi, TConfigType extends INoUiComponent<TViewModelType>> extends NoUiViewEx<TViewModelType, TConfigType> implements App.ISite {
        // Führt die Anmeldung auf Benachrichtigungen aus.
        componentWillMount(): void {
            this.props.noui.setSite(this);
        }

        // Meldet sich von Benachrichtigungen ab.
        componentWillUnmount(): void {
            this.props.noui.setSite(undefined);
        }

        // Fordert eine Aktualisierung der Anzeige an.
        refreshUi(): void {
            this.forceUpdate();
        }
    }

    export abstract class NoUiViewWithSite<TViewModelType extends App.IUi> extends NoUiViewExWithSite<TViewModelType, INoUiComponent<TViewModelType>> {
    }
}