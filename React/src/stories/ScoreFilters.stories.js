import ScoreFilters from '../components/ScoreFilters';

import './ScoreFilters.css';

export default {
    title: 'ScoreFilters',
    component: ScoreFilters,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    filters: {
        view: 'cards',
        scoresAmount: 25,
        sortBy: 'pp',
        sortDir: 'desc',
        dateStart: '',
        dateEnd: ''
    },
    setFilters: (filters) => {},
    refetchScores: () => {}
};

export const Default = {
    args: {
        ...mockProps,
    },
};