import * as d3 from 'd3';

// Source: https://github.com/ppy/osu-web/blob/7ace30a85a11f297a9d482697526cee2965a7184/resources/js/utils/beatmap-helper.ts#L22-L26
const difficultyColorSpectrum = d3.scaleLinear()
    .domain([0.1, 1.25, 2, 2.5, 3.3, 4.2, 4.9, 5.8, 6.7, 7.7, 9])
    .clamp(true)
    .range(['#4290FB', '#4FC0FF', '#4FFFD5', '#7CFF4F', '#F6F05C', '#FF8068', '#FF4E6F', '#C645B8', '#6563DE', '#18158E', '#000000'])
    .interpolate(d3.interpolateRgb.gamma(2.2));

export function getDifficultyColor(difficulty) {
    if (difficulty < 0.1) return '#AAAAAA';
    if (difficulty >= 9) return '#000000';
    return difficultyColorSpectrum(difficulty);
}

export function modeEnumToString(mode) {
    switch (mode) {
        case 0:
            return 'osu';
        case 1:
            return 'taiko';
        case 2:
            return 'fruits';
        case 3:
            return 'mania';
    }
}

export function modeEnumToDisplayString(mode) {
    switch (mode) {
        case 0:
            return 'osu!';
        case 1:
            return 'osu!taiko';
        case 2:
            return 'osu!catch';
        case 3:
            return 'osu!mania';
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