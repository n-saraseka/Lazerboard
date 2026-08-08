import ScoreMod from '../components/ScoreMod';

import './ScoreMod.css';

export default {
    title: 'ScoreMod',
    component: ScoreMod,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    acronym: 'DT',
    speedChange: 1.67,
};

export const Default = {
    args: {
        ...mockProps,
    },
};