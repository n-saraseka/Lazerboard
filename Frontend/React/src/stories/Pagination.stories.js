import Pagination from '../components/Pagination';

import './styles/Pagination.css';

export default {
    title: 'Pagination',
    component: Pagination,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    pages: 10,
    onPageChange: () => {},
    windowSize: 4,
};

export const Default = {
    args: {
        ...mockProps,
    },
};