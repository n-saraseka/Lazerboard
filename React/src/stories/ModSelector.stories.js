import ModSelector from '../components/ModSelector';

import './ModSelector.css';
import {allMods} from "../utils/score-things.js";

export default {
    title: 'ModSelector',
    component: ModSelector,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

let mockMods = [];
for (const key of Object.keys(allMods)) {
    console.log(allMods[key]);
    const matchingMods = allMods[key].filter(m => m.modes.includes(0));
    matchingMods.forEach(m => {
        mockMods.push({
            acronym: m.acronym,
            active: false,
        })
    })
}

const mockProps = {
    mods: mockMods,
    setMods: (mods) => {}
};

export const Default = {
    args: {
        ...mockProps,
    },
};