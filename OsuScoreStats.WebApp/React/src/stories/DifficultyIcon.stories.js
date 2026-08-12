import DifficultyIcon from '../components/DifficultyIcon';

import './DifficultyIcon.css';

export default {
    title: 'DifficultyIcon',
    component: DifficultyIcon,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    difficulty: 2.5,
    isActive: false,
};

export const Default = {
    args: {
        ...mockProps,
    },
};