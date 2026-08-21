import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'

import ScoresPage from './components/Pages/ScoresPage.jsx';
import UserPage from './components/Pages/UserPage.jsx';
import BeatmapsetPage from './components/Pages/BeatmapsetPage.jsx';
import ScoreRankingPage from "./components/Pages/ScoreRankingPage.jsx";
import ManiaRankingPage from "./components/Pages/ManiaRankingPage.jsx";
import UserSearchBar from "./components/User/UserSearchBar.jsx";
import DatabaseState from "./components/Misc/DatabaseState.jsx";

const allComponents = {
    "ScoresPage": ScoresPage,
    "UserPage": UserPage,
    "BeatmapsetPage": BeatmapsetPage,
    "ScoreRankingPage": ScoreRankingPage,
    "ManiaRankingPage": ManiaRankingPage,
    "UserSearchBar": UserSearchBar,
    "DatabaseState": DatabaseState,
}

document.querySelectorAll(".react-app").forEach((el) => {
    const componentName = el.getAttribute("data-component");
    
    const Component = allComponents[componentName];
    
    if (Component) {
        const rawProps = el.getAttribute("data-props");
        const props = rawProps ? JSON.parse(rawProps) : {};

        createRoot(el).render(
            <StrictMode>
                <Component {...props} />
            </StrictMode>,
        );
    }
})
