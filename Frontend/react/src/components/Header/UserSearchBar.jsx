import {useState, useMemo} from "react";
import Error from "../Misc/Error.jsx";
import Loader from "../Misc/Loader.jsx";
import UserSearchResult from "./UserSearchResult.jsx";
import {debounce} from "../../utils/server-things.js";

function UserSearchBar() {
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);
    const [searchInputValue, setSearchInputValue] = useState("");
    const [searchFinished, setSearchFinished] = useState(false);
    const [users, setUsers] = useState([])

    async function getUsers(query) {
        setIsLoading(true);
        setIsError(false);
        setSearchFinished(false);
        
        if (query.length === 0) {
            setIsLoading(false);
            setUsers([]);
            setSearchFinished(true);
            return;
        }

        const params = new URLSearchParams();
        params.append("query", query);

        try {
            const response = await fetch(`/api/users/search?` + params.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
            });

            if (response.ok) {
                const json = await response.json();
                setUsers(json);
            }
            else {
                setUsers([]);
                setIsError(true);
            }
        }
        catch (error) {
            setUsers([]);
            setIsError(true);
        }

        setIsLoading(false);
        setSearchFinished(true);
    }
    
    const debouncedGetUsers = useMemo(() => debounce(getUsers, 300), []);
    
    return (
        <div className="search-bar">
            <div className="search-icon">
                <input type="text" className="search" placeholder="Search users..." value={searchInputValue} onChange={(event) => {
                    setSearchInputValue(event.target.value);
                    debouncedGetUsers(event.target.value);
                }
                }/>
            </div>
            <div className="search-results">
                {isError
                    ? (<div className="search-result"><Error/></div>)
                    : isLoading
                        ? (<div className="search-result"><Loader/></div>)
                        : users.length > 0
                            ? users.map(user => (<UserSearchResult key={user.id} user={user}/>))
                            : (searchFinished && searchInputValue !== "") && <div className="search-result">No users found!</div>
                }
            </div>
        </div>
    )
}

export default UserSearchBar;