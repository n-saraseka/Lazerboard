import Header from '../Header/Header.jsx';
import Footer from '../Misc/Footer.jsx';
import { useState, useEffect } from "react";
import {PreferencesContext} from "../Preferences/PreferencesContext.jsx";

function Page({currentPage, scoresCount, pageComponent}) {
    const localPreferences = localStorage.getItem("preferences");
    const [preferences, setPreferences] = useState(() => {
        try {
            return JSON.parse(localPreferences)
                ?? {
                    theme: 'dark',
                    preferStandardizedScoring: true,
                    globalPreferences: {
                        view: 'cards',
                        modes: Array(4).fill(0).map((m, i) => {
                            return { value: i, enabled: true };
                        }),
                        excludeConverts: true,
                        beatmapStatus: 'all',
                        amount: 25
                    },
                    rankingPreferences: {
                        amount: 10
                    }
                }
        }
        catch (error) {
            return {
                theme: 'dark',
                preferStandardizedScoring: true,
                globalPreferences: {
                    view: 'cards',
                    modes: Array(4).fill(0).map((m, i) => {
                        return { value: i, enabled: true };
                    }),
                    excludeConverts: true,
                    beatmapStatus: 'all',
                    amount: 25
                },
                rankingPreferences: {
                    amount: 10
                }
            }
        }
    });
    useEffect(() => {
        localStorage.setItem("preferences", JSON.stringify(preferences));
    }, [preferences]);
    
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