import "chart.js/auto";
import { Line, Bar } from 'react-chartjs-2'
import { defaults, Chart } from 'chart.js'
import {YearMonthFromDateTime} from "../../utils/datetime-things.js";
import {getDifficultyColor} from "../../utils/beatmap-things.js";
import {getRankTierColor, getSpeedColor} from "../../utils/score-things.js";
import {useContext, useMemo} from "react";
import {PreferencesContext} from "../Preferences/PreferencesContext.jsx";

function UserStats({data}) {
    const { preferences, setPreferences } = useContext(PreferencesContext);
    const themePreference = useMemo(() => preferences.theme ?? 'dark', [preferences.theme]);

    defaults.font.family = "'Cascadia Mono', monospace";
    defaults.color = themePreference === 'light' ? 'rgb(35, 35, 35)' : 'white';
    
    const gridOptions = {
        x: {
            grid: {
                color: themePreference === 'light' ? 'rgb(90, 90, 90)' : 'rgb(45, 45, 45)'
            },
            border: {
                color: themePreference === 'light' ? 'rgb(90, 90, 90)' : 'rgb(45, 45, 45)'
            }
        },
        y: {
            grid: {
                color: themePreference === 'light' ? 'rgb(90, 90, 90)' : 'rgb(45, 45, 45)'
            },
            border: {
                color: themePreference === 'light' ? 'rgb(90, 90, 90)' : 'rgb(45, 45, 45)'
            }
        }
    }
    
    const responsiveFontPlugin = {
        id: 'responsivefont',
        beforeLayout: (chart) => {
            const chartWidth = chart.width;
            const fontSize = 18;
            const fontSizeTooltip = 16;
            const baseWidth = 600;
            const widthRatio = chartWidth / baseWidth;
            
            const calculateFontSize = (baseSize) => {
                return Math.floor(baseSize * widthRatio);
            }

            // Because chart.js is scuffed and won't instantiate these if there are no custom options specified.
            chart.options.scales.x.ticks.font = {};
            chart.options.scales.y.ticks.font = {};
            chart.options.plugins.legend.labels.font = {};
            chart.options.plugins.title.font = {};
            chart.options.plugins.tooltip.titleFont = {};
            chart.options.plugins.tooltip.bodyFont = {};
            
            chart.options.scales.x.ticks.font.size = calculateFontSize(fontSize);
            chart.options.scales.y.ticks.font.size = calculateFontSize(fontSize);
            chart.options.plugins.legend.labels.font.size = calculateFontSize(fontSize);
            chart.options.plugins.title.font.size = calculateFontSize(fontSize);
            chart.options.plugins.tooltip.titleFont.size = calculateFontSize(fontSizeTooltip);
            chart.options.plugins.tooltip.bodyFont.size = calculateFontSize(fontSizeTooltip);
        }
    };
    
    Chart.register(responsiveFontPlugin);
    
    let history, rankStats, starStats, speedStats;
    
    if (data.history !== null) {
        history = {
            labels: data.history.map((item) => YearMonthFromDateTime(item.month)),
            datasets: [{
                label: 'Count',
                data: data.history.map((item) => item.monthlyCount),
                backgroundColor: themePreference === 'light' ? 'rgb(90, 90, 90)' : 'rgb(180, 180, 180)',
                borderColor: themePreference === 'light' ? 'rgb(90, 90, 90)'  : 'rgb(180, 180, 180)',
                pointRadius: 0
            }]
        };
    }
    
    if (data.rankDistribution !== null) {
        const nonZeroData = data.rankDistribution.filter((item) => item.count > 0);
        rankStats = {
            labels: nonZeroData.map((item) => `Top ${item.rankBound}`),
            datasets: [{
                label: 'Count',
                data: nonZeroData.map((item) => item.count),
                backgroundColor: nonZeroData.map((item) => getRankTierColor(themePreference, item.rankBound))
            }],
        }
    }
    
    if (data.starsDistribution !== null) {
        const nonZeroData = data.starsDistribution.filter((item) => item.count > 0);
        
        starStats = {
            labels: nonZeroData.map((item) => item.srBracket),
            datasets: [{
                label: 'Count',
                data: nonZeroData.map(
                    (item) => item.count
                ),
                backgroundColor: nonZeroData.map((item) => getDifficultyColor(item.srBracket + 0.5))
            }],
        }
    }
    
    if (data.speedDistribution != null) {
        speedStats = {
            labels: data.speedDistribution.map((item) => `${item.speedBracket}x`),
            datasets: [{
                label: 'Count',
                data: data.speedDistribution.map((item) => item.count),
                backgroundColor: data.speedDistribution.map((item) => getSpeedColor(item.speedBracket))
            }]
        }
    }
    
    return (
        <div className="player-stats">
            <div className="chart-wrapper">
                <Line data={history} options={{
                    plugins: {
                        title: {
                            display: true,
                            text: "Accumulated scores count history"
                        },
                        tooltip: {
                            mode: 'index',
                            intersect: false,
                        }
                    },
                    elements: {
                        line: {
                            borderWidth: 4
                        }
                    },
                    scales: gridOptions,
                    responsive: true,
                    maintainAspectRatio: false,
                }}/>
            </div>
            <div className="chart-wrapper">
                <Bar data={rankStats} options={{
                    indexAxis: 'y',
                    plugins: {
                        title: {
                            display: true,
                            text: "Rank distribution",
                        },
                        tooltip: {
                            axis: 'y',
                            mode: 'index',
                            intersect: false,
                        }
                    },
                    scales: gridOptions,
                    responsive: true,
                    maintainAspectRatio: false,
                }}/>
            </div>
            <div className="chart-wrapper">
                <Bar data={starStats} options={{
                    plugins: {
                        title: {
                            display: true,
                            text: "Star rating distribution"
                        },
                        tooltip: {
                            mode: 'index',
                            intersect: false,
                        }
                    },
                    scales: gridOptions,
                    responsive: true,
                    maintainAspectRatio: false,
                }}/>
            </div>
            <div className="chart-wrapper">
                <Bar data={speedStats} options={{
                    plugins: {
                        title: {
                            display: true,
                            text: "Speed distribution"
                        },
                        tooltip: {
                            mode: 'index',
                            intersect: false,
                        }
                    },
                    scales: gridOptions,
                    responsive: true,
                    maintainAspectRatio: false,
                }}/>
            </div>
        </div>
    )
}

export default UserStats;