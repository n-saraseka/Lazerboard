function DialogWindow({titleText, descriptionText, confirmText, onConfirm}) {
    return (<div className="window-wrapper">
        <div className="window">
            <div className="window-header">{titleText}</div>
            <div className="window-description">{descriptionText}</div>
            <div className="window-controls">
                <button className="cancel-button" onClick={onConfirm}>{confirmText}</button>
            </div>
        </div>
    </div>)
}

export default DialogWindow;