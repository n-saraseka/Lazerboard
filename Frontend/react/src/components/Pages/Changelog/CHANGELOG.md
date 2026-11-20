## Nov 20, 2026

### Web

#### Fixed

- Fixed an issue with the accumulated scores count graph where 
it wouldn't display the line graph if there was only one data point.
- Fixed accuracy display not matching the osu! website and osu!lazer client.
- Fixed irrelevant stats showing on beatmap cards for other modes.

### Backend

#### Fixed
- Fixed an issue with the stats endpoint where it would exclude the final day when
filtering by date.

## Nov 19, 2026

### Web

#### Added

- Added the changelog page. All entries prior to this are mainly from the **#changelog** [Discord](https://discord.com/invite/Z3rWRrznfd) channel and were kept to keep things consistent.
- Implemented custom scrollbar style.

### Backend

#### Fixed

- Fixed an issue where a user's registered user UserGroup could duplicate if they didn't have a recorded registration date before.

## Nov 18, 2026

### Score fetcher

#### Added
- Added ability to specify mods for the leaderboard rescans service (this was necessary in order to start the osu!mania leaderboard rescan).

#### Changed
- Revamped the flow of rescanning leaderboards to account for edge cases regarding new user PBs.

#### Fixed
- Fixed old scores not updating after receiving data with new values from the osu! API.

## Nov 17, 2026

### Score fetcher

#### Fixed
- Various optimizations for DB lookups.
- Fix various issues with restricted user background services failing.

#### Added
- Implement [osu.direct](https://osu.direct/) and [Nekoha](https://mirror.nekoha.moe/) beatmap mirrors support for retrieving beatmap files necessary for PP calculation.

### External APIs

#### Added
- Added API endpoint wrappers for retrieving beatmap files from [osu.direct](https://osu.direct/) and [Nekoha](https://mirror.nekoha.moe/) beatmap mirrors.

## Nov 16, 2026

### Backend

#### Added

- Add **user groups** to the users table. The groups are as follows: User, Registered User, Supporter, Moderator, Administrator.

### Score fetcher

#### Fixed
- Use the same leaderboard refetching logic for both removing and reinstating (un)restricted user scores, hopefully fixing issues with the service failing due to not doing that.

#### Changed
- Now using 24 hours as the "too old" newest score threshold for the live scores service, following the recent outage.

## Nov 15, 2026

### Web

#### Fixed
- Fixed incorrect pagination behavior due to debounce fixes from before.

### Backend

#### Changed
- Include approved maps in ranked beatmaps filter.

## Nov 14, 2026

### Backend

#### Added
- Implement proper cache busting for static files on backend.
- Moved scores count background service from score fetcher service to backend service.

#### Fixed
- The API now returns its own ApiUser DTO for all responses instead of raw DB data.

### Score fetcher

#### Removed
- Removed scores count background service.

## Nov 13, 2026

### Web

#### Changed
- Moved the view preference setting from filters to header.

#### Fixed
- Fixed misses display not working due to a change in the website's API.
- Fixed an issue where the search component would keep showing "no users found" with cleared input.
- Fixed the debounce function not really debouncing requests.
- Fixed weird z-index issues.
- Fixed "Allow other mods" checkbox not wrapping properly on mobile.
- Fixed all weird hover issues with custom selectors.

## Nov 12, 2026

### Web

#### Added
- Added a dedicated page for [mania millions scores](https://lazerboard.melguy.com/millions).

#### Changed
- Changed the title of the "Top 100 scores count history" to "Accumulated scores count history".
- Redesigned header navigation to use subsections for score and ranking pages.

#### Fixed
- Fixed an issue where you couldn't hover over very thin bars in the user's rank distribution chart.

### Backend

#### Added
- Added the API endpoint for mania millions scores.
- Started tracking registered users in the database.