import Filters from '../../components/Filters/Filters';

import './styles/ScoreFilters.css';

export default {
    title: 'Filters',
    component: Filters,
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
};

export const Default = {
    args: {
        ...mockProps,
    },
};