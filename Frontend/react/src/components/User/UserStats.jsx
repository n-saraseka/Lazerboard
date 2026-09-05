import "chart.js/auto";
import { Line, Bar } from 'react-chartjs-2'
import { defaults } from 'chart.js'
import {YearMonthFromDateTime} from "../../utils/datetime-things.js";
import {getDifficultyColor} from "../../utils/beatmap-things.js";
import {getSpeedColor} from "../../utils/score-things.js";
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
                backgroundColor: 'rgb(180, 180, 180)',
            }],
        }
    }
    
    if (data.stars !== null) {
        const allSrs = Array(11).fill(0).map((_, i) => i);
        const dataSrs = data.stars.map((item) => item.srBracket);
        
        starStats = {
            labels: allSrs,
            datasets: [{
                label: 'Count',
                data: allSrs.map(
                    (sr) => dataSrs.includes(sr)
                        ? data.stars.find((i) => i.srBracket === sr).count
                        : 0
                ),
                backgroundColor: allSrs.map((sr) => getDifficultyColor(sr + 0.5))
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

    console.log('Data:')
    console.log(data.history);
    console.log(data.ranks);
    console.log(data.speed);
    console.log(data.stars);
    console.log('Processed data:')
    console.log(history);
    console.log(rankStats);
    console.log(speedStats);
    console.log(starStats);
    
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
                                }
                            },
                            elements: {
                                line: {
                                    borderWidth: 4
                                }
                            },
                            scales: gridOptions
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
                                scales: gridOptions
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
                                scales: gridOptions
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
                                scales: gridOptions
                            }}/>
                }
            </div>
        </div>
    )
}

export default UserStats;