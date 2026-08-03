import DifficultyIcon from "../components/DifficultyIcon.jsx";
import BeatmapCard from "../components/BeatmapCard.jsx";
import BeatmapScores from "../components/BeatmapScores.jsx";
import ModeSelector from "../components/ModeSelector.jsx";
import {useState} from "react";
import MappedBy from "./MappedBy.jsx";

function BeatmapsetPage({beatmapset, beatmaps, selectedBeatmapId, scores}) {
    const allModes = [0, 1, 2, 3];
    const firstBeatmap = beatmaps.find((beatmap) => beatmap.id === selectedBeatmapId);
    const [selectedBeatmap, setSelectedBeatmap] = useState(firstBeatmap);
    const [beatmapScores, setBeatmapScores] = useState(scores);
    const [allowedModes, setAllowedModes] = useState(firstBeatmap.mode !== 0 ? [firstBeatmap.mode] : allModes);
    const [selectedMode, setSelectedMode] = useState(selectedBeatmap.mode);
    
    async function switchMode(mode) {
        if (!allowedModes.includes(mode)) return;
        setSelectedMode(mode);
        await getBeatmapScores(selectedBeatmap.id, mode);
    }
    
    async function switchBeatmap(id) {
        if (selectedBeatmap.id === id) return;
        const newBeatmap = beatmaps.find((beatmap) => beatmap.id === id);
        setSelectedBeatmap(newBeatmap);
        setAllowedModes(newBeatmap.mode !== 0 ? [newBeatmap.mode] : allModes);
        const newMode = selectedBeatmap.mode !== 0 ? selectedBeatmap.mode : selectedMode;
        if (newMode !== selectedMode) {
            setSelectedMode(newMode);
        }
        await getBeatmapScores(id, newMode);
    }

    async function getBeatmapScores(id, mode) {
        const params = new URLSearchParams();
        params.append("mode", mode.toString());
        const response = await fetch(`/api/beatmaps/${id}/scores?` + params.toString(), {
            method: "GET",
            headers: { "Accept": "application/json" },
        });

        if (response.ok) {
            const json = await response.json();
            setBeatmapScores(json);
        }
    }
    
    return (<>
        <div className="beatmapset-modes">
            <div className="beatmapset-card" style={
                {background: `url("https://assets.ppy.sh/beatmaps/${beatmapset.id}/covers/cover@2x.jpg") center, rgba(0, 0, 0, 0.7)`}
            }>
                <h1><a href={`https://osu.ppy.sh/beatmapsets/${beatmapset.id}`}>{`${beatmapset.artist} - ${beatmapset.title}`}</a></h1>
                <MappedBy user={beatmapset.user}/>
                <div className="difficulties">
                    {beatmaps.map((beatmap, index) => (
                        <DifficultyIcon difficulty={beatmap.difficulty}
                                        mode={beatmap.mode}
                                        isActive={selectedBeatmap.id === beatmap.id}
                                        name={beatmap.difficultyName}
                                        onDifficultySwitch={async () => await switchBeatmap(beatmap.id)}
                                        key={index}/>))
                    }
                </div>
                <BeatmapCard beatmap={selectedBeatmap}/>
            </div>
            <div className="mode-selection">
                {allModes.map((mode, index) => (
                    <ModeSelector mode={mode}
                                  allowedModes={allowedModes}
                                  selectedMode={selectedMode}
                                  onModeSwitch={async () => await switchMode(mode)}
                                  key={index}/>
                ))}
            </div>
        </div>
        <BeatmapScores key={selectedBeatmap.id} scores={beatmapScores}/>
    </>)
}

export default BeatmapsetPage;