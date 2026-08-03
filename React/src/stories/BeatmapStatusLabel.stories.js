import BeatmapStatusLabel from '../components/BeatmapStatusLabel';

import './BeatmapStatusLabel.css';

export default {
    title: 'BeatmapStatusLabel',
    component: BeatmapStatusLabel,
    parameters: {
        layout: 'centered',
    },
    tags: ['autodocs']
}

const mockProps = {
    status: 2
};

export const Default = {
    args: {
        ...mockProps,
    },
};