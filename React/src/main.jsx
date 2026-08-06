import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'

import ScoresPage from './components/ScoresPage.jsx';
import UserPage from './components/UserPage.jsx';
import BeatmapsetPage from './components/BeatmapsetPage.jsx';
import ScoreRankingPage from "./components/ScoreRankingPage.jsx";

const allComponents = {
    "ScoresPage": ScoresPage,
    "UserPage": UserPage,
    "BeatmapsetPage": BeatmapsetPage,
    "ScoreRankingPage": ScoreRankingPage,
}

document.querySelectorAll(".react-app").forEach((el) => {
    const componentName = el.getAttribute("data-component");
    
    const Component = allComponents[componentName];
    
    if (Component) {
        const rawProps = el.getAttribute("data-props");
        const props = rawProps ? JSON.parse(rawProps) : {};

        console.log("Props from Razor:", props);

        createRoot(el).render(
            <StrictMode>
                <Component {...props} />
            </StrictMode>,
        );
    }
})
