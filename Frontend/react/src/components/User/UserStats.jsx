import "chart.js/auto";
import { Line, Bar } from 'react-chartjs-2'
import { defaults } from 'chart.js'
import {YearMonthFromDateTime} from "../../utils/datetime-things.js";
import {getDifficultyColor} from "../../utils/beatmap-things.js";
import {getRankTierColor, getSpeedColor} from "../../utils/score-things.js";
import Loader from "../Misc/Loader.jsx";
import Error from "../Misc/Error.jsx";

defaults.font.family = "'Cascadia Mono', monospace";
defaults.color = "white";

function UserStats({data, loadingData, errorData}) {
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
    
    let history, rankStats, starStats, speedStats;
    
    if (data.history !== null) {
        history = {
            labels: data.history.map((item) => YearMonthFromDateTime(item.month)),
            datasets: [{
                label: 'Count',
                data: data.history.map((item) => item.monthlyCount),
                backgroundColor: 'rgb(180, 180, 180)',
                borderColor: 'rgb(180, 180, 180)',
                pointRadius: 0
            }]
        };
    }
    
    if (data.ranks !== null) {
        rankStats = {
            labels: data.ranks.map((item) => `Top ${item.rankBound}`),
            datasets: [{
                label: 'Count',
                data: data.ranks.map((item) => item.count),
                backgroundColor: data.ranks.map((item) => getRankTierColor(item.rankBound))
            }],
        }
    }
    
    if (data.stars !== null) {
        const nonZeroData = data.stars.filter((item) => item.count > 0);
        
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
    
    if (data.speed != null) {
        speedStats = {
            labels: data.speed.map((item) => `${item.speedBracket}x`),
            datasets: [{
                label: 'Count',
                data: data.speed.map((item) => item.count),
                backgroundColor: data.speed.map((item) => getSpeedColor(item.speedBracket))
            }]
        }
    }
    
    return (
        <div className="player-stats">
            <div className="chart-wrapper">
                { loadingData.history
                    ? <Loader/>
                    : errorData.history
                        ? <Error/>
                        : <Line data={history} options={{
                            plugins: {
                                title: {
                                    display: true,
                                    text: "Top 100 leaderboard count history"
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
                        }}/>
                }
            </div>
            <div className="chart-wrapper">
                {
                    loadingData.ranks
                        ? <Loader/>
                        : errorData.ranks
                            ? <Error/>
                            : <Bar data={rankStats} options={{
                                indexAxis: 'y',
                                plugins: {
                                    title: {
                                        display: true,
                                        text: "Rank distribution",
                                    }
                                },
                                scales: gridOptions,
                                responsive: true
                            }}/>
                }
            </div>
            <div className="chart-wrapper">
                {
                    loadingData.stars
                        ? <Loader/>
                        : errorData.stars
                            ? <Error/>
                            : <Bar data={starStats} options={{
                                plugins: {
                                    title: {
                                        display: true,
                                        text: "Star rating distribution"
                                    }
                                },
                                scales: gridOptions,
                                responsive: true
                            }}/>
                }
            </div>
            <div className="chart-wrapper">
                {
                    loadingData.speed
                        ? <Loader/>
                        : errorData.speed
                            ? <Error/>
                            : <Bar data={speedStats} options={{
                                plugins: {
                                    title: {
                                        display: true,
                                        text: "Speed distribution"
                                    }
                                },
                                scales: gridOptions,
                                responsive: true
                            }}/>
                }
            </div>
        </div>
    )
}

export default UserStats;