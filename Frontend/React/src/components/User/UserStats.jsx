import "chart.js/auto";
import { Line, Bar } from 'react-chartjs-2'
import { defaults } from 'chart.js'
import {YearMonthFromDateTime} from "../../utils/datetime-things.js";
import {getDifficultyColor} from "../../utils/beatmap-things.js";
import {getSpeedColor} from "../../utils/score-things.js";

defaults.font.family = "'Cascadia Mono', monospace";
defaults.color = "white";

function UserStats({data}) {
    const history = {
        labels: data.history.map((item) => YearMonthFromDateTime(item.month)),
        datasets: [{
            label: 'Count',
            data: data.history.map((item) => item.monthlyCount),
            backgroundColor: 'rgb(180, 180, 180)',
            borderColor: 'rgb(180, 180, 180)',
            pointRadius: 0
        }] 
    };
    
    const gridOptions = {
        x: {
            grid: {
                color: 'rgb(45, 45, 45)'
            }
        },
        y: {
            grid: {
                color: 'rgb(45, 45, 45)'
            }
        }
    }
    
    const rankStats = {
        labels: data.rankStats.map((item) => `#${item.rankBound}`),
        datasets: [{
            label: 'Count',
            data: data.rankStats.map((item) => item.count),
            backgroundColor: 'rgb(180, 180, 180)',
        }],
    }
    
    const allSrs = Array(11).fill(0).map((_, i) => i);
    const dataSrs = data.starStats.map((item) => item.srBracket);
    
    const starStats = {
        labels: allSrs,
        datasets: [{
            label: 'Count',
            data: allSrs.map(
                (sr) => dataSrs.includes(sr) 
                ? data.starStats.find((i) => i.srBracket === sr).count 
                : 0
            ),
            backgroundColor: allSrs.map((sr) => getDifficultyColor(sr + 0.5))
        }],
    }
    
    const speedStats = {
        labels: data.speedStats.map((item) => `${item.speedBracket}x`),
        datasets: [{
            label: 'Count',
            data: data.speedStats.map((item) => item.count),
            backgroundColor: data.speedStats.map((item) => getSpeedColor(item.speedBracket))
        }]
    }
    
    return (
        <div className="player-stats">
            <div className="chart-wrapper">
                <Line data={history} options={{
                    plugins: {
                        title: {
                            display: true,
                            text: "Top 100 leaderboard count history"
                        }
                    },
                    elements: {
                        line: {
                            borderWidth: 4
                        }
                    },
                    scales: gridOptions
                }}/>
            </div>
            <div className="chart-wrapper">
                <Bar data={rankStats} options={{
                    indexAxis: 'y',
                    plugins: {
                        title: {
                            display: true,
                            text: "Rank distribution",
                        }
                    },
                    scales: gridOptions
                }}/>
            </div>
            <div className="chart-wrapper">
                <Bar data={starStats} options={{
                    plugins: {
                        title: {
                            display: true,
                            text: "Star rating distribution"
                        }
                    },
                    scales: gridOptions
                }}/>
            </div>
            <div className="chart-wrapper">
                <Bar data={speedStats} options={{
                    plugins: {
                        title: {
                            display: true,
                            text: "Speed distribution"
                        }
                    },
                    scales: gridOptions
                }}/>
            </div>
        </div>
    )
}

export default UserStats;