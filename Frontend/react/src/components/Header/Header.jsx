import UserSearchBar from "./UserSearchBar.jsx";
import {useCallback, useContext, useEffect, useMemo, useState} from "react";
import UserPreferences from "../Preferences/UserPreferences.jsx";
import {PreferencesContext} from "../Preferences/PreferencesContext.jsx";
import UserButton from "./UserButton.jsx";
import LoginButton from "./LoginButton.jsx";
import {UserContext} from "../Misc/UserContext.jsx";
import HeaderItemWithSubsection from "./HeaderItemWithSubsection.jsx";

function Header({currentPage}) {
    const [burgerActive, setBurgerActive] = useState(true);
    const { preferences, setPreferences } = useContext(PreferencesContext);
    const preference = useMemo(() => preferences.theme ?? 'dark',
        [preferences.theme]);
    const [checkedIfLoggedIn, setCheckedIfLoggedIn] = useState(false);
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const { currentUser, setCurrentUser } = useContext(UserContext);

    const getUser = useCallback(async () => {
        const response = await fetch("/api/users/me");

        if (response.ok) {
            const json = await response.json();
            setCurrentUser(json);
            setIsLoggedIn(true);
        }

        setCheckedIfLoggedIn(true);
    }, [currentPage]);

    useEffect(() => {
        getUser();
    }, []);
    
    return <header>
        <div className="logo-navigation">
            <a href="/" className="logo">
                <img src={`/images/icon-${preference}.png`} alt="Logo" className="logo-image"/>
                <strong>Lazerboard</strong>
            </a>
            <div className={`burger-icon ${burgerActive ? 'active' : 'inactive'}`} onClick={() => setBurgerActive(!burgerActive)}></div>
        </div>
        <div className="navigation-main">
            <ul className="navigation">
                <li>
                    <HeaderItemWithSubsection 
                        currentPage={currentPage}
                        sectionTitle="Scores" 
                        subItems={[
                            {
                                techTitle: "scores", 
                                url: "/", 
                                title: "Global"
                            },
                            {
                                techTitle: "millionscores",
                                url: "/millions",
                                title: "Mania millions"
                            },
                        ]}/>
                </li>
                <li>
                    <HeaderItemWithSubsection
                        currentPage={currentPage}
                        sectionTitle="User ranking"
                        subItems={[
                            {
                                techTitle: "userranking",
                                url: "/scoreranking",
                                title: "Global"
                            },
                            {
                                techTitle: "millionsranking",
                                url: "/millionsranking",
                                title: "Mania millions"
                            },
                        ]}/>
                </li>
                <li>
                    <a href="/about" className={currentPage === "about" ? 'active' : 'inactive'}>About</a>
                </li>
            </ul>
            <div className="navigation-right">
                <UserPreferences/>
                {checkedIfLoggedIn && (
                    isLoggedIn
                        ? (<UserButton user={currentUser}/>)
                        : (<LoginButton/>)
                )}
                <UserSearchBar/>
            </div>
        </div>
    </header>
}

export default Header;