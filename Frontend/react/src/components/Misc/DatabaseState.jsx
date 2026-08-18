import {useState, useEffect, useCallback} from "react";

function DatabaseState() {
    const [fetcherState, setState] = useState("unavailable");

    const getState = useCallback(async () => {

        try {
            const response = await fetch(`/api/fetcher/seedingstate`, {
                method: "GET",
                headers: { "Accept": "application/json" },
            });

            if (response.ok) {
                const text = await response.text();
                text === "true" ? setState("seeding") : setState("livescores");
            }
            else {
                setState("unavailable");
            }
        }
        catch (error) {
            setState("unavailable");
        }
    }, [])

    useEffect( () => {
        getState();
    }, [getState]);

    return (
        <span>Score fetcher state: <strong>
                {
                    fetcherState === "unavailable" ? "unreachable"
                        : fetcherState === "seeding" ? "fetching live scores" : "scanning existing leaderboards"
                }
            </strong>
        </span>
    )
}

export default DatabaseState;