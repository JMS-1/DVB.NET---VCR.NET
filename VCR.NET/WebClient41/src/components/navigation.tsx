/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface INavigationStatic {
    }

    interface INavigationDynamic {
        active?: boolean;
    }

    export class Navigation extends React.Component<INavigationStatic, INavigationDynamic>  {
        render(): JSX.Element {
            return <div className="vcrnet-navigation">[Navigation]</div>;
        }
    }
}