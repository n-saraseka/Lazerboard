import ScoreFilters from '../../components/Filters/ScoreFilters';

import '../styles/ScoreFilters.css';

export default {
    title: 'ScoreFilters',
    component: ScoreFilters,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const currentDate = new Date().toISOString().split("T")[0];

const mockProps = {
    filters: {
        view: 'cards',
        scoresAmount: 25,
        sortBy: 'pp',
        sortDir: 'desc',
        dateStart: currentDate,
        dateEnd: currentDate,
        modes: [0, 1, 2, 3]
    },
    setFilters: (filters) => {},
    refetchScores: () => {}
};

export const Default = {
    args: {
        ...mockProps,
    },
};