import ModeWedge from '../components/ModeWedge.jsx';

import './ModeWedge.css';

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