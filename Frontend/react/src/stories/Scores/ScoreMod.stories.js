import ScoreMod from '../../components/Scores/ScoreMod';

import './styles/ScoreMod.css';

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