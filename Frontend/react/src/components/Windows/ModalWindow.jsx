import {useState} from "react";

function ModalWindow({titleText, descriptionText, confirmText, cancelText, confirmAction, cancelAction}) {
    const [waitingForAction, setWaitingForAction] = useState(false);
    return (<div className="window-wrapper">
        <div className="window">
            <div className="window-header">{titleText}</div>
            <div className="window-description">{descriptionText}</div>
            <div className="window-controls">
                <button className="confirm-button" 
                        onClick={() => {
                            setWaitingForAction(true);
                            confirmAction()
                        }}
                        disabled={waitingForAction}>{confirmText}</button>
                <button className="cancel-button" onClick={cancelAction} disabled={waitingForAction}>{cancelText}</button>
            </div>
        </div>
    </div>)
}

export default ModalWindow;