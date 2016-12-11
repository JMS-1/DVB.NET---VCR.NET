/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IRouterStatic {
    }

    interface IRouterDynamic {
    }

    export class Router extends React.Component<IRouterStatic, IRouterDynamic>{
        render(): JSX.Element {
            return <div className="vcrnet-router"><Home /></div>;
        }
    }        
}
