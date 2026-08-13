import {useState} from "react";
import Error from "../Error.jsx";
import Loader from "../Loader.jsx";
import UserSearchResult from "./UserSearchResult.jsx";

function UserSearchBar() {
    const [isLoading, setIsLoading] = useState(false);
    const [isError, setIsError] = useState(false);
    const [users, setUsers] = useState([])

    async function getUsers(query) {
        setIsLoading(true);
        setIsError(false);
        
        if (query.length === 0) {
            setIsLoading(false);
            setUsers([]);
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
    }

    return (
        <div className="search-bar">
            <div className="search">
                <input type="text" className="search" placeholder="Search users..." onChange={(event) => getUsers(event.target.value)}/>
            </div>
            <div className="search-results">
                {isError
                    ? (<Error/>)
                    : isLoading
                        ? (<Loader/>)
                        : users.length > 0 &&
                        users.map(user => (<UserSearchResult user={user}/>))
                }
            </div>
        </div>
    )
}

export default UserSearchBar;