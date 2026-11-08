import UserSearchBar from "./UserSearchBar.jsx";
import {useCallback, useContext, useEffect, useMemo, useState} from "react";
import UserPreferences from "../Preferences/UserPreferences.jsx";
import {PreferencesContext} from "../Preferences/PreferencesContext.jsx";
import UserButton from "./UserButton.jsx";
import LoginButton from "./LoginButton.jsx";

function Header({currentPage}) {
    const [burgerActive, setBurgerActive] = useState(true);
    const { preferences, setPreferences } = useContext(PreferencesContext);
    const preference = useMemo(() => preferences.theme ?? 'dark',
        [preferences.theme]);
    const [checkedIfLoggedIn, setCheckedIfLoggedIn] = useState(false);
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    const [user, setUser] = useState(null);

    const getUser = useCallback(async () => {
        const response = await fetch("/api/users/me");

        if (response.ok) {
            const json = await response.json();
            setUser(json);
            setIsLoggedIn(true);
        }

        setCheckedIfLoggedIn(true);
    }, [currentPage])

    useEffect(() => {
        getUser();
    }, []);
    
    return <header>
        <div className="logo-navigation">
            <a href="/" className="logo">
                <img src={`/static/images/icon-${preference}.png`} alt="Logo" className="logo-image"/>
                <strong>Lazerboard</strong>
            </a>
            <div className={`burger-icon ${burgerActive ? 'active' : 'inactive'}`} onClick={() => setBurgerActive(!burgerActive)}></div>
        </div>
        <div className="navigation-main">
            <ul className="navigation">
                <li>
                    <a href="/" className={currentPage === "scores" ? 'active' : 'inactive'}>Home page</a>
                </li>
                <li>
                    <a href="/about" className={currentPage === "about" ? 'active' : 'inactive'}>About</a>
                </li>
                <li>
                    <a href="/scoreranking" className={currentPage === "userranking" ? 'active' : 'inactive'}>User ranking</a>
                </li>
                <li>
                    <a href="/millionsranking" className={currentPage === "millionsranking" ? 'active' : 'inactive'}>Mania millions</a>
                </li>
            </ul>
            <div className="navigation-right">
                {checkedIfLoggedIn && (
                    isLoggedIn
                        ? (<UserButton user={user}/>)
                        : (<LoginButton/>)
                )}
                <UserPreferences/>
                <UserSearchBar/>
            </div>
        </div>
    </header>
}

export default Header;