'use strict'

const difficultyDecreasingMods = ["DC", "EZ", "HT", "NF"];
const difficultyIncreasingMods = ["AC", "BL", "DT", "FL", "HD", "HR", "NC", "PF", "SD", "ST", "TC"];
const conversionMods = ["AL", "CL", "DA", "MR", "RD", "SG", "TP"];
const automationMods = ["AP", "RX", "SO"];
const funMods = ["AD", "AS", "BM", "BR", "BU", "DF", "DP", "FR", "GR", "MG", "MU", "NS", "RP", "SI", "SY", "TR", "WD", "WG", "WU"]
const systemMods = ["TD"];

const modCategories = {};
difficultyDecreasingMods.forEach(mod => modCategories[mod] = "difficulty-decrease");
difficultyIncreasingMods.forEach(mod => modCategories[mod] = "difficulty-increase");
conversionMods.forEach(mod => modCategories[mod] = "conversion");
automationMods.forEach(mod => modCategories[mod] = "automation");
funMods.forEach(mod => modCategories[mod] = "fun");
systemMods.forEach(mod => modCategories[mod] = "system");

function getModCategory(mod) {
    return modCategories[mod] || "unknown";
}

const pageContainer = document.querySelector(".container");
const userCard = document.querySelector(".user-card");
const userId = userCard.getAttribute("user-id");
const userCountryCode = userCard.getAttribute("user-country");
const userDisplay = document.querySelector(".user-data");
const userName = document.querySelector(".user-name").innerText;
const userScores = document.querySelector(".scores");
const userScoresCondensedTable = document.querySelector(".scores-table");
const userScoresCondensed = userScoresCondensedTable.querySelector("tbody");

// amount of scores to show (25, 50, 75, 100)
const amountSelect = document.querySelector("#scores-amount");
let currentThreshold = parseInt(amountSelect.value);

amountSelect.addEventListener("change", async () => {
    currentThreshold = parseInt(amountSelect.value);
    await fillWithData(0, 1, currentThreshold, currentSort, (currentDirection === "desc"));
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
    await fillWithData(0, 1, currentThreshold, currentSort, (currentDirection === "desc"));
});

// sort direction (desc or asc)
const sortDirectionSelect = document.querySelector("#scores-sort-direction");
let currentDirection = sortDirectionSelect.value;
sortDirectionSelect.addEventListener("change", async () => {
    currentDirection = sortDirectionSelect.value;
    await fillWithData(0, 1, currentThreshold, currentSort, (currentDirection === "desc"));
});

// Get user scores from the API
async function getScores(mode = 0, amount = 100, sort = "pp", isDesc = true, page = 1) {
    const params = new URLSearchParams();
    params.append("mode", mode.toString());
    params.append("amount", amount.toString());
    params.append("sort", sort);
    params.append("isDesc", isDesc.toString());
    params.append("page", page);
    params.append("userId", userId);
    const response = await fetch(`/api/users/${userId}/scores?` + params.toString(), {
        method: "GET",
        headers: { "Accept": "application/json" },
    });

    if (response.ok) {
        return response.json();
    }
}

// Get count of user scores stored in the DB from the API
async function getUserScoresCount(mode = 0) {
    const params = new URLSearchParams();
    params.append("mode", mode.toString());
    const response = await fetch(`/api/users/${userId}/scores/count?${params.toString()}`, {
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

// create a score card element (for cards view)
function gridScore(score, beatmap, beatmapset) {
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
    rank.innerText = `#${score.mapRank}`;

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
    playerA.href = `/user/${userId}`;

    const playerImg = document.createElement("img");
    playerImg.classList.add("score-player-img");
    playerImg.src = "https://a.ppy.sh/" + score.userId;
    playerImg.alt = userName;
    playerImg.title = userName;

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
function rowScore(score, beatmap, beatmapset) {
    const scoreTr = document.createElement("tr");
    scoreTr.classList.add("score-row");

    const pp = document.createElement("td");
    pp.classList.add("score-row-pp");
    pp.innerText = `${score.pp.toFixed(0)}pp`;

    const rank = document.createElement("td");
    rank.classList.add("score-row-rank");
    rank.innerText = `#${score.mapRank}`;

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

    scoreTr.append(pp, rank, mods, rankedScore, combo, acc, misses, mapImage, mapInfo);
    return scoreTr;
}

// fill page with score data
async function fillWithData(mode = 0, page = 1, amountPerPage = 25, sort = "date", isDesc = true) {
    const scoreCount = await getUserScoresCount(mode);
    if (!document.querySelector(".scores-amount")) {
        generateScoreAmountSpan(scoreCount);
    }
    generateScorePagesUl(scoreCount, amountPerPage);
    
    const scores = await getScores(mode, amountPerPage, sort, isDesc, page);

    let beatmapIds = [];
    scores.forEach(score => beatmapIds.push(score.beatmapId));
    const beatmaps = await getBeatmaps(beatmapIds);

    let beatmapsetIds = [];
    beatmaps.forEach(beatmap => beatmapsetIds.push(beatmap.beatmapsetId));
    const beatmapsets = await getBeatmapsets(beatmapsetIds);
    
    clearData();
    scores.forEach(score => {
            userScores.append(gridScore(
                score,
                beatmaps.find(beatmap =>
                    beatmap.id === score.beatmapId),
                beatmapsets.find(beatmapset =>
                    beatmapset.id === beatmaps.find(beatmap => beatmap.id === score.beatmapId).beatmapsetId))
            );
            userScoresCondensed.append(rowScore(
                score,
                beatmaps.find(beatmap =>
                    beatmap.id === score.beatmapId),
                beatmapsets.find(beatmapset =>
                    beatmapset.id === beatmaps.find(beatmap => beatmap.id === score.beatmapId).beatmapsetId))
            );
        }
    );
    switchView();
}

// get hex code for country code
function getEncodedCountry(countryCode) {
    countryCode = countryCode.toUpperCase();
    const baseCode = 0x1F1E6;

    const code1 = baseCode + (countryCode.charCodeAt(0) - 'A'.charCodeAt(0));
    const code2 = baseCode + (countryCode.charCodeAt(1) - 'A'.charCodeAt(0));

    const hex1 = code1.toString(16).toLowerCase();
    const hex2 = code2.toString(16).toLowerCase();

    return `${hex1}-${hex2}`;
}

// generate <img> element with the user country flag
function generateCountryImage() {
    const countryImg = document.createElement("img");
    countryImg.classList.add("country-img");
    countryImg.src = `https://osu.ppy.sh/assets/images/flags/${getEncodedCountry(userCountryCode)}.svg`;
    countryImg.alt = "Country";
    countryImg.title = userCountryCode;
    
    userDisplay.querySelector(".user-data-name").append(countryImg);
}

// generate <span> element for this score count
function generateScoreAmountSpan(scoreCount) {
    const scoresAmountSpan = document.createElement("span");
    scoresAmountSpan.classList.add("scores-amount");
    scoresAmountSpan.innerText = `${scoreCount} scores stored in the database`;

    userDisplay.append(scoresAmountSpan);
}

// generate pagination element for this score count
function generateScorePagesUl(scoreCount, amountPerPage) {
    const pages = scoreCount / amountPerPage;
    const pagesCount = document.querySelectorAll(".page").length;
    if (pagesCount !== Math.ceil(pages)) {
        const oldScorePagesUl = document.querySelector(".pages");
        if (oldScorePagesUl) {
            oldScorePagesUl.remove();
        }
        if (pages > 1) {
            const scorePagesUl = document.createElement("ul");
            scorePagesUl.classList.add("pages");
            for (let i = 0; i < pages; i++) {
                const li = document.createElement("li");
                li.classList.add("page");
                if (i === 0) li.classList.add("active");
                li.innerText = `${i + 1}`;
                li.addEventListener("click", async () => {
                    const activeLi = document.querySelector(".page.active");
                    if (activeLi !== li) {
                        activeLi.classList.remove("active");
                        li.classList.add("active");
                        await fillWithData(0, parseInt(li.innerText), currentThreshold, currentSort, (currentDirection === "desc"));
                    }
                })
                scorePagesUl.appendChild(li);
            }
            pageContainer.append(scorePagesUl);
        }
    }
}

// clear score data
function clearData() {
    userScores.innerHTML = "";
    userScoresCondensed.innerHTML = "";
}

// switch scores view (cards or condensed)
function switchView() {
    if (currentView === "cards") {
        userScores.style.display = "grid";
        userScoresCondensedTable.style.display = "none";
    }
    else {
        userScores.style.display = "none";
        userScoresCondensedTable.style.display = "table";
    }
}

generateCountryImage();
fillWithData();
