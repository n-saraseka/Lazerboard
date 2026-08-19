import {useState, useEffect} from "react";

function getFetcherStateString(state) {
    if (state === "loading") return "loading...";
    if (state === "unavailable") return "unreachable";
    if (state === "seeding") return "scanning existing leaderboards";
    if (state === "livescores") return "fetching scores live";
    return "unknown";
}

function DatabaseState() {
    const [fetcherState, setFetcherState] = useState("loading");

    useEffect( () => {
        const getState = async () => {
            try {
                const response = await fetch(`/api/fetcher/seedingstate`, {
                    method: "GET",
                    headers: { "Accept": "application/json" },
                });

                if (response.ok) {
                    const text = await response.text();
                    text === "true" ? setFetcherState("seeding") : setFetcherState("livescores");
                }
                else {
                    setFetcherState("unavailable");
                }
            }
            catch (error) {
                setFetcherState("unavailable");
            }
        };
        
        getState();
    }, []);

    return (
        <span>Score fetcher state: <strong>{getFetcherStateString(fetcherState)}</strong>
        </span>
    )
}

export default DatabaseState;