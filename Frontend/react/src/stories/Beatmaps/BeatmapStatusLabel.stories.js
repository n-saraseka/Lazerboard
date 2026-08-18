import BeatmapStatusLabel from '../../components/Beatmaps/BeatmapStatusLabel';

import './styles/BeatmapStatusLabel.css';

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