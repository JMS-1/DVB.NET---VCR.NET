/// <reference path="../vcrnet.tsx" />

namespace VCRNETClient {
    interface IPlanStatic {
    }

    interface IPlanDynamic {
    }

    export class Plan extends React.Component<IPlanStatic, IPlanDynamic>{
        render(): JSX.Element {
            return <div className="vcrnet-plan">[PLAN]</div>;
        }
    }
}
