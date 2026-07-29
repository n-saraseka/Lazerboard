using Moq;
using NUnit.Framework;
using OsuScoreStats.Calculations;
using OsuScoreStats.DbService.Entities;
using OsuScoreStats.DbService.Repositories.Interfaces;
using OsuScoreStats.OsuApi.OsuApiEntities;
using OsuScoreStats.ScoreFetcher;

namespace OsuScoreStats.Tests.ScoreFetcherTests;

[TestFixture]
public class ScoreProcessorTests
{
    private ScoreProcessor _scoreProcessor;
    private Mock<IScoreRepository> _scoreRepository;
    private Mock<ICalculator> _calculator;
    
    [SetUp]
    public void Setup()
    {
        _scoreRepository = new Mock<IScoreRepository>();
        _calculator = new Mock<ICalculator>();
        _scoreProcessor = new ScoreProcessor(_scoreRepository.Object, _calculator.Object);
    }

    [Test]
    public async Task CheckIfSignificantAsync_AllScoresBetter_ReturnsFalse()
    {
        // Arrange
        var score = new APIScore
        {
            Id = 51,
            BeatmapId = 1,
            TotalScore = 10000,
            UserId = 51
        };
        
        var scores = new List<Score>();
        for (int i = 0; i < 50; i++)
        {
            scores.Add(new Score()
            {
                Id = (ulong)i + 1,
                BeatmapId = 1,
                TotalScore = 1000000 - 1000 * i,
                UserId = i + 1
            });
        }

        _scoreRepository.Setup(r => r.GetByBeatmapIdAsync(It.IsAny<int>(), CancellationToken.None))
            .ReturnsAsync(scores);
        
        // Assert
        Assert.IsFalse(await _scoreProcessor.CheckIfSignificantAsync(score, CancellationToken.None));
    }
    
    [Test]
    public async Task CheckIfSignificantAsync_AllBetterLessThan50_ReturnsTrue()
    {
        // Arrange
        var score = new APIScore
        {
            Id = 50,
            BeatmapId = 1,
            TotalScore = 10000,
            UserId = 50
        };
        
        var scores = new List<Score>();
        for (int i = 0; i < 49; i++)
        {
            scores.Add(new Score()
            {
                Id = (ulong)i + 1,
                BeatmapId = 1,
                TotalScore = 1000000 - 1000 * i,
                UserId = i + 1
            });
        }

        _scoreRepository.Setup(r => r.GetByBeatmapIdAsync(It.IsAny<int>(), CancellationToken.None))
            .ReturnsAsync(scores);
        
        // Assert
        Assert.IsTrue(await _scoreProcessor.CheckIfSignificantAsync(score, CancellationToken.None));
    }
    
    [Test]
    public async Task CheckIfSignificantAsync_SomeAreWorse_ReturnsTrue()
    {
        // Arrange
        var score = new APIScore
        {
            Id = 51,
            BeatmapId = 1,
            TotalScore = 970000,
            UserId = 50
        };
        
        var scores = new List<Score>();
        for (int i = 0; i < 50; i++)
        {
            scores.Add(new Score()
            {
                Id = (ulong)i + 1,
                BeatmapId = 1,
                TotalScore = 1000 * i,
                UserId = i + 1
            });
        }

        _scoreRepository.Setup(r => r.GetByBeatmapIdAsync(It.IsAny<int>(), CancellationToken.None))
            .ReturnsAsync(scores);
        
        // Assert
        Assert.IsTrue(await _scoreProcessor.CheckIfSignificantAsync(score, CancellationToken.None));
    }
    
    [Test]
    public async Task CheckIfSignificantAsync_NoScores_ReturnsTrue()
    {
        // Arrange
        var score = new APIScore
        {
            Id = 50,
            BeatmapId = 1,
            TotalScore = 10000,
            UserId = 50
        };
        
        var scores = new List<Score>();

        _scoreRepository.Setup(r => r.GetByBeatmapIdAsync(It.IsAny<int>(), CancellationToken.None))
            .ReturnsAsync(scores);
        
        // Assert
        Assert.IsTrue(await _scoreProcessor.CheckIfSignificantAsync(score, CancellationToken.None));
    }
    
    [Test]
    public async Task CheckIfSignificantBulkAsync_RespectiveGroupExists()
    {
        // Arrange
        var score = new List<APIScore>
        {
            new APIScore()
            {
                Id = 51,
                BeatmapId = 1,
                TotalScore = 10000,
                UserId = 51
            },
            new APIScore()
            {
                Id = 52,
                BeatmapId = 1,
                TotalScore = 990000,
                UserId = 52
            },
            new APIScore()
            {
                Id = 53,
                BeatmapId = 1,
                TotalScore = 1,
                UserId = 1
            }
        };
        
        var scores = new List<Score>();
        for (int i = 0; i < 50; i++)
        {
            scores.Add(new Score()
            {
                Id = (ulong)i + 1,
                BeatmapId = 1,
                TotalScore = 1000000 - 1000 * i,
                UserId = i + 1
            });
        }
        
        var groupedScores = scores.GroupBy(s => s.BeatmapId).ToList();

        _scoreRepository.Setup(r => r.GetByBeatmapIdsAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync(groupedScores);
        
        // Act
        var dict = await _scoreProcessor.CheckIfSignificantBulkAsync(score, CancellationToken.None);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dict.Count, Is.EqualTo(3));
            Assert.IsFalse(dict[51]);
            Assert.IsTrue(dict[52]);
            Assert.IsFalse(dict[53]);
        });
    }
    
    [Test]
    public async Task CheckIfSignificantBulkAsync_RespectiveGroupDoesntExist()
    {
        // Arrange
        var score = new List<APIScore>
        {
            new APIScore()
            {
                Id = 51,
                BeatmapId = 1,
                TotalScore = 10000,
                UserId = 51
            },
            new APIScore()
            {
                Id = 52,
                BeatmapId = 1,
                TotalScore = 990000,
                UserId = 52
            },
            new APIScore()
            {
                Id = 53,
                BeatmapId = 1,
                TotalScore = 1,
                UserId = 1
            }
        };
        
        var scores = new List<Score>();
        
        var groupedScores = scores.GroupBy(s => s.BeatmapId).ToList();

        _scoreRepository.Setup(r => r.GetByBeatmapIdsAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync(groupedScores);
        
        // Act
        var dict = await _scoreProcessor.CheckIfSignificantBulkAsync(score, CancellationToken.None);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dict.Count, Is.EqualTo(3));
            foreach (var key in dict.Keys)
            {
                Assert.IsTrue(dict[key]);
            }
        });
    }

    [Test]
    public async Task CalculateScoreAsync_ScoreHasPp_DoesntCalc()
    {
        // Arrange
        var score = new APIScore
        {
            Id = 1,
            BeatmapId = 1,
            TotalScore = 10000,
            UserId = 11,
            PP = 1
        };
        
        // Act
        await _scoreProcessor.CalculateScoreAsync(score, CancellationToken.None);
        
        // Assert
        _calculator.Verify(c => c.CalculateAsync(It.IsAny<APIScore>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Test]
    public async Task CalculateScoreAsync_ScoreDoesntHavePp_Calculates()
    {
        // Arrange
        var score = new APIScore
        {
            Id = 1,
            BeatmapId = 1,
            TotalScore = 10000,
            UserId = 11,
        };

        _calculator.Setup(c => c.CalculateAsync(It.IsAny<APIScore>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        
        // Act
        await _scoreProcessor.CalculateScoreAsync(score, CancellationToken.None);
        
        // Assert
        _calculator.Verify(c => c.CalculateAsync(It.IsAny<APIScore>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(score.PP, Is.EqualTo(2));
    }
    
    [Test]
    public void CheckIfBetterAlreadyExists_NoScore_ReturnsFalse()
    {
        // Arrange
        var score = new APIScore
        {
            Id = 50,
            BeatmapId = 1,
            TotalScore = 10000,
            UserId = 50
        };
        
        var scores = new List<Score>();
        for (int i = 0; i < 49; i++)
        {
            scores.Add(new Score()
            {
                Id = (ulong)i + 1,
                BeatmapId = 1,
                TotalScore = 1000000 - 1000 * i,
                UserId = i + 1
            });
        }

        _scoreRepository.Setup(r => r.GetByBeatmapIdAsync(It.IsAny<int>(), CancellationToken.None))
            .ReturnsAsync(scores);
        
        // Assert
        Assert.IsFalse(_scoreProcessor.CheckIfBetterAlreadyExists(score, scores));
    }
    
    [Test]
    public void CheckIfBetterAlreadyExists_WorseScoreExists_ReturnsFalse()
    {
        // Arrange
        var score = new APIScore
        {
            Id = 50,
            BeatmapId = 1,
            TotalScore = 10000,
            UserId = 1
        };
        
        var scores = new List<Score>();
        for (int i = 0; i < 50; i++)
        {
            scores.Add(new Score()
            {
                Id = (ulong)i + 1,
                BeatmapId = 1,
                TotalScore = 1000 * i,
                UserId = i + 1
            });
        }

        _scoreRepository.Setup(r => r.GetByBeatmapIdAsync(It.IsAny<int>(), CancellationToken.None))
            .ReturnsAsync(scores);
        
        // Assert
        Assert.IsFalse(_scoreProcessor.CheckIfBetterAlreadyExists(score, scores));
    }
    
    [Test]
    public void CheckIfBetterAlreadyExists_BetterScoreExists_ReturnsTrue()
    {
        // Arrange
        var score = new APIScore
        {
            Id = 50,
            BeatmapId = 1,
            TotalScore = 10000,
            UserId = 1
        };
        
        var scores = new List<Score>();
        for (int i = 0; i < 50; i++)
        {
            scores.Add(new Score()
            {
                Id = (ulong)i + 1,
                BeatmapId = 1,
                TotalScore = 1000000 - 1000 * i,
                UserId = i + 1
            });
        }

        _scoreRepository.Setup(r => r.GetByBeatmapIdAsync(It.IsAny<int>(), CancellationToken.None))
            .ReturnsAsync(scores);
        
        // Assert
        Assert.IsTrue(_scoreProcessor.CheckIfBetterAlreadyExists(score, scores));
    }
}