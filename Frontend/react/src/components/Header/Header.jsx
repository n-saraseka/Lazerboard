import UserSearchBar from "./UserSearchBar.jsx";
import { useState } from "react";
import UserPreferences from "../Preferences/UserPreferences.jsx";

function Header({currentPage}) {
    const [burgerActive, setBurgerActive] = useState(true);
    return <header>
        <div className="logo-navigation">
            <a href="/" className="logo">
                <img src="/static/images/icon-dark.png" alt="Logo" className="logo-image"/>
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
                <UserPreferences/>
                <UserSearchBar/>
            </div>
        </div>
    </header>
}

export default Header;