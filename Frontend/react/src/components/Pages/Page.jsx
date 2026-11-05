import Header from '../Header/Header.jsx';
import Footer from '../Misc/Footer.jsx';
import { useState } from "react";
import {PreferencesContext} from "../Header/Preferences/PreferencesContext.jsx";

function Page({currentPage, scoresCount, pageComponent}) {
    const localPreferences = localStorage.getItem("preferences");
    const [preferences, setPreferences] = useState(JSON.parse(localPreferences)
        ?? {
            theme: 'dark',
            preferStandardizedScoring: true
        });
    
    return <>
        <PreferencesContext.Provider value={{ preferences, setPreferences }}>
            <Header currentPage={currentPage}/>
            <div className="container">
                {pageComponent}
            </div>
            <Footer scoresCount={scoresCount}/>
        </PreferencesContext.Provider>
    </>
}

export default Page;