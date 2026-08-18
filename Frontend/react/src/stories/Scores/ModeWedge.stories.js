import ModeWedge from '../../components/Scores/ModeWedge.jsx';

import './styles/ModeWedge.css';

export default {
    title: 'ModeWedge',
    component: ModeWedge,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    mode: 1
};

export const Default = {
    args: {
        ...mockProps,
    },
};