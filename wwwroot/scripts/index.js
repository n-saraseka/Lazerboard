'use strict'

const difficultyDecreasingMods = ["DC", "EZ", "HT", "NF"];
const difficultyIncreasingMods = ["AC", "BL", "DT", "FL", "HD", "HR", "NC", "PF", "SD", "ST", "TC"];
const conversionMods = ["AL", "CL", "DA", "MR", "RD", "SG", "TP"];
const automationMods = ["AP", "RX", "SO"];
const funMods = ["AD", "AS", "BM", "BR", "BU", "DF", "DP", "FR", "GR", "MG", "MU", "NS", "RP", "SI", "SY", "TR", "WD", "WG", "WU"]
const systemMods = ["TD"];
const speedChangeMods = ["DT", "NC", "HT", "DC"];

const modCategories = {};
difficultyDecreasingMods.forEach(mod => modCategories[mod] = "difficulty-decrease");
difficultyIncreasingMods.forEach(mod => modCategories[mod] = "difficulty-increase");
conversionMods.forEach(mod => modCategories[mod] = "conversion");
automationMods.forEach(mod => modCategories[mod] = "automation");
funMods.forEach(mod => modCategories[mod] = "fun");
systemMods.forEach(mod => modCategories[mod] = "system");

// amount of scores to show (25, 50, 75, 100)
const amountSelect = document.querySelector("#scores-amount");
let currentThreshold = parseInt(amountSelect.value);
let oldThreshold = parseInt(amountSelect.value);

amountSelect.addEventListener("change", () => {
    oldThreshold = currentThreshold;
    currentThreshold = parseInt(amountSelect.value);
    if (currentThreshold > oldThreshold) {
        unhideScores();
    }
    else {
        hideScoresAboveThreshold();
    }
})

// view toggle (cards or condensed)
const viewSelect = document.querySelector("#scores-view");
let currentView = viewSelect.value;
viewSelect.addEventListener("change", () => {
    currentView = viewSelect.value;
    switchView();
})

// sort toggle (pp, totalScore or date)
const sortSelect = document.querySelector("#scores-sort");
let currentSort = sortSelect.value;
sortSelect.addEventListener("change", async () => {
    currentSort = sortSelect.value;
    await fillWithData(0, 100, currentDateStart, today, currentSort, (currentDirection === "desc"));
});

// sort direction (desc or asc)
const sortDirectionSelect = document.querySelector("#scores-sort-direction");
let currentDirection = sortDirectionSelect.value;
sortDirectionSelect.addEventListener("change", async () => {
    currentDirection = sortDirectionSelect.value;
    await fillWithData(0, 100, currentDateStart, today, currentSort, (currentDirection === "desc"));
});

const dateRangeSelect = document.querySelector("#date-range");
const today = new Date();
let currentDateStart = new Date();

dateRangeSelect.addEventListener("change", async () => {
    currentDateStart =  new Date();
    switch (dateRangeSelect.value) {
        case "last-week":
            currentDateStart.setDate(currentDateStart.getDate() - 7);
            break;
        case "last-month":
            currentDateStart.setMonth(currentDateStart.getMonth() - 1);
            break;
        case "last-year":
            currentDateStart.setFullYear(currentDateStart.getFullYear() - 1);
            break;
        default:
            break;
    }
    await fillWithData(0, 100, currentDateStart, today, currentSort, (currentDirection === "desc"));
})


// Get mod category based on mod acronym
function getModCategory(mod) {
    return modCategories[mod] || "unknown";
}

function getDateString(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
}

// Get scores from the API
async function getScores(mode = 0, amount = 100, dateStart = currentDateStart, dateEnd = today, sort = "pp", isDesc = true) {
    const params = new URLSearchParams();
    params.append("mode", mode.toString());
    params.append("amount", amount.toString());
    params.append("sort", sort);
    params.append("isDesc", isDesc.toString());
    params.append("dateStart", getDateString(dateStart));
    params.append("dateEnd", getDateString(dateEnd));
    const response = await fetch("/api/scores?" + params.toString(), {
        method: "GET",
        headers: { "Accept": "application/json" },
    });

    if (response.ok) {
        return response.json();
    }
}

// get beatmaps from the API
async function getBeatmaps(beatmapIds) {
    const params = new URLSearchParams();
    beatmapIds.forEach(id => params.append('beatmapIds', id));

    const response = await fetch("/api/beatmaps?" + params.toString(), {
        method: "GET",
        headers: { "Accept": "application/json" },
    });

    if (response.ok) {
        return response.json();
    }
}

// get beatmapsets from the API
async function getBeatmapsets(beatmapsetIds) {
    const params = new URLSearchParams();
    beatmapsetIds.forEach(id => params.append('beatmapsetIds', id));

    const response = await fetch("/api/beatmapsets?" + params.toString(), {
        method: "GET",
        headers: { "Accept": "application/json" },
    });

    if (response.ok) {
        return response.json();
    }
}

// get users from the API
async function getUsers(userIds) {
    const params = new URLSearchParams();
    userIds.forEach(id => params.append('userIds', id));

    const response = await fetch("/api/users?" + params.toString(), {
        method: "GET",
        headers: { "Accept": "application/json" },
    });

    if (response.ok) {
        return response.json();
    }
}

// create a score card element (for cards view)
function gridScore(score, user, beatmap, beatmapset) {
    const scoreDiv = document.createElement("div");
    scoreDiv.classList.add("score");
    scoreDiv.style.background = `url("https://assets.ppy.sh/beatmaps/${beatmap.beatmapsetId}/covers/cover@2x.jpg") center, rgba(0, 0, 0, 0.7)`;

    const leftColumn = document.createElement("div");
    leftColumn.classList.add("score-column", "score-left-column");

    const songName = document.createElement("strong");
    songName.classList.add("score-song-name");
    songName.innerText = `${beatmapset.artist} - ${beatmapset.title}`;

    const difficultyName = document.createElement("a");
    difficultyName.classList.add("score-difficulty-name");
    difficultyName.href = `https://osu.ppy.sh/b/${beatmap.id}`;
    difficultyName.innerText = `[${beatmap.difficultyName}]`;

    const rank = document.createElement("strong");
    rank.classList.add("score-rank");
    rank.innerText = `#${score.rank}`;

    const pp = document.createElement("strong");
    pp.classList.add("score-pp");
    pp.innerText = `${score.pp.toFixed(0)}pp`;

    const scoreTotalDiv = document.createElement("div");
    scoreTotalDiv.classList.add("score-total");
    
    const standardizedScore = document.createElement("a");
    standardizedScore.classList.add("score-primary");
    standardizedScore.href = `https://osu.ppy.sh/scores/${score.id}`;
    standardizedScore.innerText = score.totalScore.toLocaleString('en-US');
    standardizedScore.title = "Standardized score";

    const classicScore = document.createElement("span");
    classicScore.classList.add("score-secondary");
    if (score.legacyTotalScore) {
        classicScore.innerText = score.legacyTotalScore.toLocaleString('en-US');
    }
    else {
        classicScore.innerText = score.classicTotalScore.toLocaleString('en-US');
    }
    classicScore.title = "Classic score";
    
    scoreTotalDiv.append(standardizedScore, classicScore);

    leftColumn.append(songName, difficultyName, rank, pp, scoreTotalDiv);
    scoreDiv.append(leftColumn);

    const rightColumn = document.createElement("div");
    rightColumn.classList.add("score-column", "score-right-column");

    const playerA = document.createElement("a");
    playerA.href = `/user/${user.id}`;

    const playerImg = document.createElement("img");
    playerImg.classList.add("score-player-img");
    playerImg.src = "https://a.ppy.sh/" + score.userId;
    playerImg.alt = user.username;
    playerImg.title = user.username;

    playerA.appendChild(playerImg);

    const mods = document.createElement("div");
    mods.classList.add("score-mods");
    score.modAcronyms.forEach(mod => {
        const modSpan = document.createElement("span");
        modSpan.classList.add("mod", `mod-${getModCategory(mod.slice(0,2))}`);
        let modText = mod;
        if (score.speedChange && speedChangeMods.indexOf(mod) !== -1) {
            modText += `(${score.speedChange})`;
        }
        modSpan.innerText = modText;
        mods.appendChild(modSpan);
    });

    const combo = document.createElement("strong");
    combo.classList.add("score-combo");
    combo.innerText = `${score.combo.toLocaleString('en-US')}x`;

    const accMisses = document.createElement("div");
    accMisses.classList.add("score-acc-misses");

    const acc = document.createElement("span");
    acc.classList.add("score-acc");
    acc.innerText = (score.accuracy * 100).toFixed(2) + "%";
    accMisses.appendChild(acc);

    if (score.misses > 0) {
        const misses = document.createElement("span");
        misses.classList.add("score-misses");
        misses.innerText = score.misses + "x";
        accMisses.appendChild(misses);
    }

    rightColumn.append(playerA, mods, combo, accMisses);
    scoreDiv.append(rightColumn);

    return scoreDiv;
}

// create a score row element (for condensed view)
function rowScore(score, user, beatmap, beatmapset) {
    const scoreTr = document.createElement("tr");
    scoreTr.classList.add("score-row");

    const pp = document.createElement("td");
    pp.classList.add("score-row-pp");
    pp.innerText = `${score.pp.toFixed(0)}pp`;

    const rank = document.createElement("td");
    rank.classList.add("score-row-rank");
    rank.innerText = `#${score.rank}`;

    const playerName = document.createElement("td");
    playerName.classList.add("score-row-player-name");
    const playerA = document.createElement("a");
    playerA.href = `/user/${user.id}`;
    playerA.innerText = user.username;
    playerName.appendChild(playerA);

    const mods = document.createElement("td");
    mods.classList.add("score-row-mods");
    score.modAcronyms.forEach(mod => {
        const modSpan = document.createElement("span");
        modSpan.classList.add("mod", `mod-${getModCategory(mod.slice(0,2))}`);
        let modText = mod;
        if (score.speedChange && speedChangeMods.indexOf(mod) !== -1) {
            modText += `(${score.speedChange})`;
        }
        modSpan.innerText = modText;
        mods.appendChild(modSpan);
    });

    const rankedScore = document.createElement("td");
    rankedScore.classList.add("score-total");

    const standardizedScore = document.createElement("a");
    standardizedScore.classList.add("score-primary");
    standardizedScore.href = `https://osu.ppy.sh/scores/${score.id}`;
    standardizedScore.innerText = score.totalScore.toLocaleString('en-US');
    standardizedScore.title = "Standardized score";

    const classicScore = document.createElement("span");
    classicScore.classList.add("score-secondary");
    if (score.legacyTotalScore) {
        classicScore.innerText = score.legacyTotalScore.toLocaleString('en-US');
    }
    else {
        classicScore.innerText = score.classicTotalScore.toLocaleString('en-US');
    }
    classicScore.title = "Classic score";
    
    rankedScore.append(standardizedScore, classicScore);

    const combo = document.createElement("td");
    combo.classList.add("score-row-combo");
    combo.innerText = `${score.combo.toLocaleString('en-US')}x`;

    const acc = document.createElement("td");
    acc.classList.add("score-row-accuracy");
    acc.innerText = (score.accuracy * 100).toFixed(2) + "%";

    const misses = document.createElement("td");
    misses.classList.add("score-misses");
    if (score.misses) {
        misses.innerText = `${score.misses}x`;
    }

    const mapImage = document.createElement("td");
    mapImage.classList.add("score-row-map-image");
    const mapImg = document.createElement("img");
    mapImg.src = `https://assets.ppy.sh/beatmaps/${beatmapset.id}/covers/cover@2x.jpg`;
    mapImg.addEventListener("error", () => {
        mapImg.removeAttribute("src");
    })
    mapImage.appendChild(mapImg);   

    const mapInfo = document.createElement("td");
    mapInfo.classList.add("score-beatmap");
    const mapA = document.createElement("a");
    mapA.href = `https://osu.ppy.sh/b/${beatmap.id}`;
    mapA.innerText = `${beatmapset.artist} - ${beatmapset.title} [${beatmap.difficultyName}]`;
    mapInfo.appendChild(mapA);

    scoreTr.append(pp, rank, playerName, mods, rankedScore, combo, acc, misses, mapImage, mapInfo);
    return scoreTr;
}

// clear score data
function clearData() {
    const scoresGrid = document.querySelector(".scores");
    const scoresTable = document.querySelector(".scores-table tbody");
    scoresGrid.innerHTML = "";
    scoresTable.innerHTML = "";
}

// fill page with score data
async function fillWithData(mode = 0, amount = 100, dateStart = currentDateStart, dateEnd = today, sort = "pp", isDesc = true) {
    const scores = await getScores(mode, amount, dateStart, dateEnd, sort, isDesc);

    let userIds = [];
    scores.forEach(score => userIds.push(score.userId));
    const users = await getUsers(userIds);

    let beatmapIds = [];
    scores.forEach(score => beatmapIds.push(score.beatmapId));
    const beatmaps = await getBeatmaps(beatmapIds);

    let beatmapsetIds = [];
    beatmaps.forEach(beatmap => beatmapsetIds.push(beatmap.beatmapsetId));
    const beatmapsets = await getBeatmapsets(beatmapsetIds);

    const scoresGrid = document.querySelector(".scores");
    const scoresTable = document.querySelector(".scores-table tbody");
    clearData();
    scores.forEach(score => {
            scoresGrid.append(gridScore(
                score,
                users.find(user => user.id === score.userId),
                beatmaps.find(beatmap =>
                    beatmap.id === score.beatmapId),
                beatmapsets.find(beatmapset =>
                    beatmapset.id === beatmaps.find(beatmap => beatmap.id === score.beatmapId).beatmapsetId))
            );
            scoresTable.append(rowScore(
                score,
                users.find(user => user.id === score.userId),
                beatmaps.find(beatmap =>
                    beatmap.id === score.beatmapId),
                beatmapsets.find(beatmapset =>
                    beatmapset.id === beatmaps.find(beatmap => beatmap.id === score.beatmapId).beatmapsetId))
            );
        }
    );
    switchView();
    hideScoresAboveThreshold();
}

// hide scores above threshold chosen by the amountSelect
function hideScoresAboveThreshold() {
    const scores = document.querySelectorAll(".score");
    const tableScores = document.querySelectorAll(".score-row");

    for (let i = currentThreshold; i < scores.length; i++) {
        scores[i].style.display = "none";
        tableScores[i].style.display = "none";
    }
}

// unhide scores (called in case the threshold switched to a larger value)
function unhideScores() {
    const scores = document.querySelectorAll(".score");
    const tableScores = document.querySelectorAll(".score-row");

    for (let i = oldThreshold; i < Math.min(scores.length, currentThreshold); i++) {
        scores[i].style.display = "flex";
        tableScores[i].style.display = "table-row";
    }
}

// switch scores view (cards or condensed)
function switchView() {
    const scoreCards = document.querySelector(".scores");
    const scoresTable = document.querySelector(".scores-table");
    if (currentView === "cards") {
        scoreCards.style.display = "grid";
        scoresTable.style.display = "none";
    }
    else {
        scoreCards.style.display = "none";
        scoresTable.style.display = "table";
    }
}

fillWithData();