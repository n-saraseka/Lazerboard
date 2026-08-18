function Error() {
    return (
        <div className="error-wrapper">
            <div className="error-icon"></div>
            <strong>An error occured while processing this request.</strong>
            <span>Please try again later.</span>
        </div>
    )
}

export default Error;