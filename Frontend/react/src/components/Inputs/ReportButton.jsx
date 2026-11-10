import ModalWindow from "../Windows/ModalWindow.jsx";
import {useState} from "react";
import {createReportQueryCommand} from "../../utils/user-things.js";
import DialogWindow from "../Windows/DialogWIndow.jsx";
import ErrorWindow from "../Windows/ErrorWindow.jsx";

function ReportButton({ user }) {
    const [modalOpen, setModalOpen] = useState(false);
    const [successOpen, setSuccessOpen] = useState(false);
    const [errorOpen, setErrorOpen] = useState(false);
    const [error, setError] = useState(null);

    const reportUser = async () =>  {
        const command = createReportQueryCommand(user);

        try {
            const response = await fetch("/api/reports", {
                method: "POST",
                body: JSON.stringify(command),
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                },
            });

            if (response.ok) {
                setModalOpen(false);
                setSuccessOpen(true);
                setError(null);
            }
            else {
                const json = await response.json();
                setModalOpen(false);
                setError(json);
                setErrorOpen(true);
            }
        }
        catch (error) {
            setModalOpen(false);
            setErrorOpen(true);
        }
    }
    
    return (<div className="report-wrapper">
        <div className="report-button" onClick={() => setModalOpen(true)}>Report</div>
        {modalOpen && (
            <ModalWindow titleText={`Report ${user.username}?`}
                         descriptionText="The report will be processed within 15 minutes automatically."
                         confirmText="Let's do it"
                         cancelText="No, go back"
                         confirmAction={reportUser}
                         cancelAction={() => setModalOpen(false)}/>
        )}
        {successOpen && (
            <DialogWindow titleText={`Reported ${user.username} successfully.`} 
                          descriptionText="Thank you for your report!"
                          confirmText="Close"
                          onConfirm={() => setSuccessOpen(false)}/>
        )}
        {errorOpen && (
            <ErrorWindow titleText="An error occured"
                         descriptionText={error === null 
                             ? "An unknown error occured. Please try again later." 
                             : error.message} 
                         confirmText="Close"
                         onConfirm={() => setErrorOpen(false)}/>
        )}
    </div>)
}

export default ReportButton;