function TimedOut() {
    return (
        <div className="error-wrapper">
            <div className="timeout-icon"></div>
            <strong>Request timed out.</strong>
            <span>Please try again later.</span>
        </div>
    )
}

export default TimedOut;