import {useState, useEffect} from "react";

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
        <span>Score fetcher state: <strong>
                {
                    fetcherState === "unavailable" ? "unreachable"
                        : fetcherState === "seeding" ? "scanning existing leaderboards" : "fetching scores live"
                }
            </strong>
        </span>
    )
}

export default DatabaseState;