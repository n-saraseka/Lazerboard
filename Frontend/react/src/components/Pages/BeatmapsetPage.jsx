import DifficultyIcon from "../Beatmaps/DifficultyIcon.jsx";
import BeatmapCard from "../Beatmaps/BeatmapCard.jsx";
import BeatmapScores from "../Beatmaps/BeatmapScores.jsx";
import ModeSelector from "../Filters/ModeSelector.jsx";
import {useState, useMemo, useCallback, useEffect} from "react";
import MappedBy from "../Beatmaps/MappedBy.jsx";
import {modeEnumToString} from "../../utils/beatmap-things.js";
import Error from "../Misc/Error.jsx";
import Loader from "../Misc/Loader.jsx";
import {debounce} from "../../utils/server-things.js";

function BeatmapsetPage({beatmapset, beatmaps, selectedBeatmapId, selectedMode}) {
    const allModes = [0, 1, 2, 3];
    const firstBeatmap = beatmaps.find((beatmap) => beatmap.id === selectedBeatmapId);
    const [selectedBeatmap, setSelectedBeatmap] = useState(firstBeatmap);
    const [beatmapScores, setBeatmapScores] = useState([]);
    const [allowedModes, setAllowedModes] = useState(firstBeatmap.mode !== 0 ? [firstBeatmap.mode] : allModes);
    const [currentMode, setCurrentMode] = useState(selectedMode);
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);
    
    async function switchMode(mode) {
        if (!allowedModes.includes(mode)) return;
        setCurrentMode(mode);
        debouncedGetBeatmapScores(selectedBeatmap.id, mode);
    }
    
    async function switchBeatmap(id) {
        if (selectedBeatmap.id === id) return;
        const newBeatmap = beatmaps.find((beatmap) => beatmap.id === id);
        setSelectedBeatmap(newBeatmap);
        setAllowedModes(newBeatmap.mode !== 0 ? [newBeatmap.mode] : allModes);
        const newMode = newBeatmap.mode !== 0 ? newBeatmap.mode : currentMode;
        if (newMode !== currentMode) {
            setCurrentMode(newMode);
        }
        debouncedGetBeatmapScores(id, newMode);
    }
    
    const getBeatmapScores = useCallback(async (id, mode, page = 1) => {
        setIsLoading(true);
        setIsError(false);

        const params = new URLSearchParams();
        params.append("mode", mode.toString());

        try {
            const response = await fetch(`/api/beatmaps/${id}/scores?` + params.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
            });
            if (response.ok) {
                const json = await response.json();
                setBeatmapScores(json.scores);
            }
            else {
                setIsError(true);
            }
        }
        catch (error) {
            setIsError(true);
        }
        setIsLoading(false);
    }, [])

    useEffect(() => {
        getBeatmapScores(selectedBeatmapId, selectedMode);
    }, []);
    
    const debouncedGetBeatmapScores = useMemo(
        () => debounce(getBeatmapScores, 300),
        []
    );
    
    return (<>
        <div className="beatmapset-modes">
            <div className="beatmapset-card" style={
                {background: `url("https://assets.ppy.sh/beatmaps/${beatmapset.id}/covers/cover@2x.jpg") center / cover, rgba(0, 0, 0, 0.7)`}
            }>
                <h1><a href={`https://osu.ppy.sh/beatmapsets/${beatmapset.id}#${modeEnumToString(currentMode)}`}>{`${beatmapset.artist} - ${beatmapset.title}`}</a></h1>
                <MappedBy user={beatmapset.user} rankedDate={beatmapset.rankedDate}/>
                <div className="difficulties">
                    {beatmaps.map((beatmap, index) => (
                        <DifficultyIcon difficulty={beatmap.difficulty}
                                        mode={beatmap.mode}
                                        isActive={selectedBeatmap.id === beatmap.id}
                                        name={beatmap.difficultyName}
                                        onDifficultySwitch={() => switchBeatmap(beatmap.id)}
                                        key={index}/>))
                    }
                </div>
                <BeatmapCard beatmap={selectedBeatmap} beatmapset={beatmapset} selectedMode={currentMode}/>
            </div>
            <div className="mode-selection">
                {allModes.map((mode, index) => (
                    <ModeSelector mode={mode}
                                  allowedModes={allowedModes}
                                  selectedMode={currentMode}
                                  onModeSwitch={() => switchMode(mode)}
                                  key={index}/>
                ))}
            </div>
        </div>
        {isError 
            ? (<Error/>) 
            : (isLoading
                ? (<Loader/>)
                :  <BeatmapScores key={selectedBeatmap.id} scores={beatmapScores}/>)}
        
    </>)
}

export default BeatmapsetPage;