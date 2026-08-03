export function getDifficultyColor(difficulty) {
    if (difficulty < 0.1) return '#AAAAAA';
    if (difficulty < 1.25) return '#4290FB';
    if (difficulty < 2) return '#4FC0FF';
    if (difficulty < 2.5) return '#4FFFD5';
    if (difficulty < 3.3) return '#7CFF4F';
    if (difficulty < 4.2) return '#F6F05C';
    if (difficulty < 4.9) return '#FF8068';
    if (difficulty < 5.8) return '#FF4E6F';
    if (difficulty < 6.7) return '#C645B8';
    if (difficulty < 7.7) return '#6563DE';
    if (difficulty < 9) return '#18158E';
    if (difficulty >= 9) return '#000000';
}

export function modeEnumToString(mode) {
    switch (mode) {
        case 0:
            return 'osu';
        case 1:
            return 'taiko';
        case 2:
            return 'catch';
        case 3:
            return 'mania';
    }
}

export function beatmapStatusEnumToText(beatmapStatus) {
    switch (beatmapStatus) {
        case -2:
            return 'Graveyard';
        case -1:
            return 'WIP';
        case 0:
            return 'Pending';
        case 1:
            return 'Ranked';
        case 2:
            return 'Approved';
        case 3:
            return 'Qualified';
        case 4:
            return 'Loved';
    }
}