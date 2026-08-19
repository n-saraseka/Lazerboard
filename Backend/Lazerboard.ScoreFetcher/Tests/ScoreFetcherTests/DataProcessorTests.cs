using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.ScoreFetcher.OsuEntityToDtoService;
using Lazerboard.ScoreFetcher.Processing;

namespace Lazerboard.ScoreFetcher.Tests.ScoreFetcherTests;

[TestFixture]
public class DataProcessorTests
{
    private DataProcessor _dataProcessor;
    private Mock<IBeatmapsetRepository> _beatmapsetRepository;
    private Mock<IBeatmapRepository> _beatmapRepository;
    private Mock<ICountryRepository> _countryRepository;
    private Mock<IUserRepository> _userRepository;
    private Mock<IScoreRepository> _scoreRepository;
    private Mock<IOsuEntityToDtoService> _osuEntityToDtoService;
    private Mock<ILogger<IDataProcessor>> _logger;

    [SetUp]
    public void Setup()
    {
        _beatmapsetRepository = new();
        _beatmapRepository = new();
        _countryRepository = new();
        _userRepository = new();
        _scoreRepository = new();
        _osuEntityToDtoService = new();
        _logger = new Mock<ILogger<IDataProcessor>>();
        _dataProcessor = new DataProcessor(_beatmapsetRepository.Object, 
            _beatmapRepository.Object, 
            _countryRepository.Object, 
            _userRepository.Object, 
            _scoreRepository.Object, 
            _osuEntityToDtoService.Object,
            _logger.Object);
    }

    [Test]
    public async Task ProcessBeatmapsetsAsync_NewBeatmapsetsAreUnique()
    {
        // Arrange
        var data = new List<APIBeatmapset>
        {
            new APIBeatmapset
            {
                Id = 1
            },
            new APIBeatmapset
            {
                Id = 1
            },
            new APIBeatmapset
            {
                Id = 2
            }
        };

        var dbData = new List<Beatmapset>();
        
        _beatmapsetRepository.Setup(r => r.GetBulkAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync(dbData);
        _osuEntityToDtoService.Setup(e => e.BeatmapsetEntityToDto(It.IsAny<APIBeatmapset>()))
            .Returns<APIBeatmapset>(api => new Beatmapset { Id = api.Id });

        // Act
        await _dataProcessor.ProcessBeatmapsetsAsync(data, CancellationToken.None);
        
        // Assert
        _beatmapsetRepository.Verify(r => r.CreateBulk(It.Is<IEnumerable<Beatmapset>>(dtos => 
            dtos.Count() == 2 &&
            dtos.All(d => d.Id == 1 || d.Id == 2))), Times.Once);
    }
    
    [Test]
    public async Task ProcessBeatmapsAsync_NewBeatmapsetsAreUnique()
    {
        // Arrange
        var data = new List<APIBeatmap>
        {
            new APIBeatmap
            {
                Id = 1
            },
            new APIBeatmap
            {
                Id = 1
            },
            new APIBeatmap
            {
                Id = 2
            }
        };

        var dbData = new List<Beatmap>();
        
        _beatmapRepository.Setup(r => r.GetBulkAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync(dbData);
        _osuEntityToDtoService.Setup(e => e.BeatmapEntityToDto(It.IsAny<APIBeatmap>()))
            .Returns<APIBeatmap>(api => new Beatmap { Id = api.Id });

        // Act
        await _dataProcessor.ProcessBeatmapsAsync(data, CancellationToken.None);
        
        // Assert
        _beatmapRepository.Verify(r => r.CreateBulk(It.Is<IEnumerable<Beatmap>>(dtos => 
            dtos.Count() == 2 &&
            dtos.All(d => d.Id == 1 || d.Id == 2))), Times.Once);
    }
    
    [Test]
    public async Task ProcessUsersAsync_NewUsersAreUnique()
    {
        // Arrange
        var data = new List<APIUser>
        {
            new APIUser
            {
                Id = 1
            },
            new APIUser
            {
                Id = 1
            },
            new APIUser
            {
                Id = 2
            }
        };

        var dbData = new List<User>();
        
        _userRepository.Setup(r => r.GetBulkAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync(dbData);
        _osuEntityToDtoService.Setup(e => e.UserEntityToDto(It.IsAny<APIUser>()))
            .Returns<APIUser>(api => new User { Id = api.Id });

        // Act
        await _dataProcessor.ProcessUsersAsync(data, CancellationToken.None);
        
        // Assert
        _userRepository.Verify(r => r.CreateBulk(It.Is<IEnumerable<User>>(dtos => 
            dtos.Count() == 2 &&
            dtos.All(d => d.Id == 1 || d.Id == 2))), Times.Once);
    }
    
    [Test]
    public async Task ProcessCountriesAsync_NewCountriesAreUnique()
    {
        // Arrange
        var data = new List<APICountry>
        {
            new APICountry
            {
                Code = "US"
            },
            new APICountry
            {
                Code = "US"
            },
            new APICountry
            {
                Code = "GB"
            }
        };

        var dbData = new List<Country>();
        
        _countryRepository.Setup(r => r.GetBulkAsync(It.IsAny<IEnumerable<string>>(), CancellationToken.None))
            .ReturnsAsync(dbData);
        _osuEntityToDtoService.Setup(e => e.CountryEntityToDto(It.IsAny<APICountry>()))
            .Returns<APICountry>(api => new Country { Id = api.Code });

        // Act
        await _dataProcessor.ProcessCountriesAsync(data, CancellationToken.None);
        
        // Assert
        _countryRepository.Verify(r => r.CreateBulk(It.Is<IEnumerable<Country>>(dtos => 
            dtos.Count() == 2 &&
            dtos.All(d => d.Id == "US" || d.Id == "GB"))), Times.Once);
    }
    
    [Test]
    public async Task ProcessScoresAsync_NewScoresAreUnique()
    {
        // Arrange
        var data = new List<APIScore>
        {
            new APIScore
            {
                Id = 1,
                BeatmapId = 1,
                UserId = 1,
                TotalScore = 100,
                Date = new DateTime(2020, 1, 1)
            },
            new APIScore
            {
                Id = 1,
                BeatmapId = 1,
                UserId = 2,
                TotalScore = 100,
                Date = new DateTime(2020, 1, 1)
            },
            new APIScore
            {
                Id = 2,
                BeatmapId = 1,
                UserId = 3,
                TotalScore = 50,
                Date = new DateTime(2020, 3, 1)
            }
        };

        var dbData = new List<Score>();
        
        _scoreRepository.Setup(r => r.GetByBeatmapIdsAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync(dbData);
        _osuEntityToDtoService.Setup(e => e.ScoreEntityToDto(It.IsAny<APIScore>()))
            .Returns<APIScore>(api => new Score
            {
                Id = api.Id,
                BeatmapId = api.BeatmapId,
                TotalScore = api.TotalScore,
                Date = api.Date
            });

        // Act
        await _dataProcessor.ProcessScoresAsync(data, CancellationToken.None);
        
        // Assert
        _scoreRepository.Verify(r => r.CreateBulk(It.Is<IEnumerable<Score>>(dtos => 
            dtos.Count() == 2 &&
            dtos.All(d => d.Id == 1 || d.Id == 2))), Times.Once);
    }
    
    [Test]
    public async Task ProcessScoresAsync_NoPreviousScores_RanksAreSavedProperly()
    {
        // Arrange
        var data = new List<APIScore>
        {
            new APIScore
            {
                Id = 1,
                BeatmapId = 1,
                UserId = 1,
                TotalScore = 100,
                Date = new DateTime(2020, 1, 1)
            },
            new APIScore
            {
                Id = 2,
                BeatmapId = 1,
                UserId = 2,
                TotalScore = 100,
                Date = new DateTime(2020, 2, 1)
            },
            new APIScore
            {
                Id = 3,
                BeatmapId = 1,
                UserId = 3,
                TotalScore = 50,
                Date = new DateTime(2020, 3, 1)
            }
        };

        var dbData = new List<Score>();

        var scoreRanks = new Dictionary<ulong, int>
        {
            { 1, 1 },
            { 2, 2 },
            { 3, 3 }
        };
        
        _scoreRepository.Setup(r => r.GetByBeatmapIdsAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync(dbData);
        _osuEntityToDtoService.Setup(e => e.ScoreEntityToDto(It.IsAny<APIScore>()))
            .Returns<APIScore>(api => new Score
            {
                Id = api.Id,
                BeatmapId = api.BeatmapId,
                TotalScore = api.TotalScore,
                Date = api.Date
            });

        // Act
        await _dataProcessor.ProcessScoresAsync(data, CancellationToken.None);
        
        // Assert
        _scoreRepository.Verify(r => r.CreateBulk(It.Is<IEnumerable<Score>>(dtos => 
            dtos.Count() == 3 &&
            dtos.All(d => d.Rank == scoreRanks[d.Id]))), Times.Once);
    }
    
    [Test]
    public async Task ProcessScoresAsync_NoPreviousScoresAndMoreThan100_Only100AreSaved()
    {
        // Arrange
        var data = new List<APIScore>();

        for (int i = 0; i < 101; i++)
        {
            data.Add(new APIScore
            {
                Id = (ulong)i + 1,
                BeatmapId = 1,
                Mode = Mode.Osu,
                UserId = i + 1,
                TotalScore = 1000000 - i * 1000,
                Date = new DateTime(2020, 1, 1)
            });
        }

        var dbData = new List<Score>();
        
        _scoreRepository.Setup(r => r.GetByBeatmapIdsAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync(dbData);
        _osuEntityToDtoService.Setup(e => e.ScoreEntityToDto(It.IsAny<APIScore>()))
            .Returns<APIScore>(api => new Score
            {
                Id = api.Id,
                BeatmapId = api.BeatmapId,
                TotalScore = api.TotalScore,
                Date = api.Date,
                UserId = api.UserId,
                Mode = api.Mode
            });

        // Act
        await _dataProcessor.ProcessScoresAsync(data, CancellationToken.None);
        
        // Assert
        _scoreRepository.Verify(r => r.CreateBulk(It.Is<IEnumerable<Score>>(dtos => 
            dtos.Count() == 100)), Times.Once);
    }
    
    [Test]
    public async Task ProcessScoresAsync_PreviousScoresExist_RanksAreSavedProperly()
    {
        // Arrange
        var data = new List<APIScore>
        {
            new APIScore
            {
                Id = 4,
                BeatmapId = 1,
                UserId = 1,
                TotalScore = 200,
                Date = new DateTime(2020, 1, 1),
                Mode = Mode.Osu
            },
            new APIScore
            {
                Id = 5,
                BeatmapId = 1,
                UserId = 2,
                TotalScore = 200,
                Date = new DateTime(2020, 2, 1),
                Mode = Mode.Osu
            },
            new APIScore
            {
                Id = 6,
                BeatmapId = 1,
                UserId = 3,
                TotalScore = 150,
                Date = new DateTime(2020, 3, 1),
                Mode = Mode.Osu
            }
        };

        var dbData = new List<Score>
        {
            new Score
            {
                Id = 1,
                BeatmapId = 1,
                UserId = 4,
                TotalScore = 100,
                Date = new DateTime(2020, 3, 1),
                Rank = 1,
                Mode = Mode.Osu
            },
            new Score
            {
                Id = 2,
                BeatmapId = 1,
                UserId = 5,
                TotalScore = 100,
                Date = new DateTime(2020, 4, 1),
                Rank = 2,
                Mode = Mode.Osu
            },
            new Score
            {
                Id = 3,
                BeatmapId = 1,
                UserId = 6,
                TotalScore = 50,
                Date = new DateTime(2020, 5, 1),
                Rank = 3,
                Mode = Mode.Osu
            }
        };

        var scoreRanks = new Dictionary<ulong, int>
        {
            { 4, 1 },
            { 5, 2 },
            { 6, 3 },
            { 1, 4 },
            { 2, 5 },
            { 3, 6 }
        };
        
        _scoreRepository.Setup(r => r.GetByBeatmapIdsAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync(dbData);
        _osuEntityToDtoService.Setup(e => e.ScoreEntityToDto(It.IsAny<APIScore>()))
            .Returns<APIScore>(api => new Score
            {
                Id = api.Id,
                BeatmapId = api.BeatmapId,
                TotalScore = api.TotalScore,
                Date = api.Date
            });

        // Act
        await _dataProcessor.ProcessScoresAsync(data, CancellationToken.None);
        
        // Assert
        _scoreRepository.Verify(r => r.CreateBulk(It.Is<IEnumerable<Score>>(dtos => 
            dtos.Count() == 3 &&
            dtos.All(d => d.Rank == scoreRanks[d.Id]))), Times.Once);
        _scoreRepository.Verify(r => r.UpdateBulk(It.Is<IEnumerable<Score>>(dtos => 
            dtos.Count() == 3 &&
            dtos.All(d => d.Rank == scoreRanks[d.Id]))), Times.Once);
    }
    
    [Test]
    public async Task ProcessScoresAsync_MultipleDuplicatesExist_DuplicatesAreRemoved()
    {
        // Arrange
        var data = new List<APIScore>
        {
            new APIScore
            {
                Id = 4,
                BeatmapId = 1,
                UserId = 1,
                TotalScore = 200,
                Date = new DateTime(2020, 1, 1),
                Mode = Mode.Osu
            },
            new APIScore
            {
                Id = 5,
                BeatmapId = 1,
                UserId = 2,
                TotalScore = 200,
                Date = new DateTime(2020, 2, 1),
                Mode = Mode.Osu
            },
            new APIScore
            {
                Id = 6,
                BeatmapId = 1,
                UserId = 3,
                TotalScore = 150,
                Date = new DateTime(2020, 3, 1),
                Mode = Mode.Osu
            }
        };

        var dbData = data.Select(s => new Score
        {
            Id = s.Id + 3,
            BeatmapId = s.BeatmapId,
            UserId = s.UserId,
            TotalScore = s.TotalScore - 1,
            Date = s.Date,
            Mode = s.Mode
        }).ToList();

        var copy = dbData.Select(s =>
        {
            s.Id += 3;
            s.TotalScore -= 1;
            return s;
        }).ToList();
        
        dbData.AddRange(copy);
        
        _scoreRepository.Setup(r => r.GetByBeatmapIdsAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync(dbData);
        _osuEntityToDtoService.Setup(e => e.ScoreEntityToDto(It.IsAny<APIScore>()))
            .Returns<APIScore>(api => new Score
            {
                Id = api.Id,
                BeatmapId = api.BeatmapId,
                TotalScore = api.TotalScore,
                Date = api.Date,
                UserId = api.UserId,
                Mode = api.Mode
            });

        // Act
        await _dataProcessor.ProcessScoresAsync(data, CancellationToken.None);
        
        // Assert
        var scoreRanks = new Dictionary<ulong, int>
        {
            { 4, 1 },
            { 5, 2 },
            { 6, 3 },
        };

        ulong[] deletedIds = [10, 11, 12];
        
        _scoreRepository.Verify(r => r.CreateBulk(It.Is<IEnumerable<Score>>(dtos => 
            dtos.Count() == 3 &&
            dtos.All(d => d.Rank == scoreRanks[d.Id]))), Times.Once);
        _scoreRepository.Verify(r => r.DeleteBulk(It.Is<IEnumerable<Score>>(dtos =>
                dtos.Count() == 6 
                && dtos.All(d => deletedIds.Contains(d.Id)))), Times.Once);
        _scoreRepository.Verify(r => r.UpdateBulk(It.IsAny<IEnumerable<Score>>()), Times.Never);
    }

    [Test]
    public async Task ProcessScoresAsync_PbsExist_OnlySameUserAndModeScoresAreRemoved()
    {
        var data = new List<APIScore>
        {
            new APIScore
            {
                Id = 5,
                BeatmapId = 1,
                UserId = 1,
                TotalScore = 200,
                Date = new DateTime(2020, 1, 1),
                Mode = Mode.Osu
            },
            new APIScore
            {
                Id = 6,
                BeatmapId = 1,
                UserId = 2,
                TotalScore = 200,
                Date = new DateTime(2020, 2, 1),
                Mode = Mode.Osu
            },
            new APIScore
            {
                Id = 7,
                BeatmapId = 1,
                UserId = 3,
                TotalScore = 150,
                Date = new DateTime(2020, 3, 1),
                Mode = Mode.Mania
            },
        };

        var dbData = new List<Score>();
        for (var i = 0; i < data.Count; i++)
        {
            dbData.Add(new Score
            {
                Id = data[i].Id - 4,
                BeatmapId = data[i].BeatmapId,
                UserId = data[i].UserId,
                TotalScore = data[i].TotalScore - 1,
                Date = new DateTime(2019, 1, i + 1),
                Mode = data[i].Mode
            });
        }
        dbData.Add(new Score
        {
            Id = 4,
            BeatmapId = 1,
            UserId = 3,
            TotalScore = 149,
            Date = new DateTime(2019, 1, 1),
            Mode = Mode.Osu
        });
        
        _scoreRepository.Setup(r => r.GetByBeatmapIdsAsync(It.IsAny<IEnumerable<int>>(), CancellationToken.None))
            .ReturnsAsync(dbData);
        _osuEntityToDtoService.Setup(e => e.ScoreEntityToDto(It.IsAny<APIScore>()))
            .Returns<APIScore>(api => new Score
            {
                Id = api.Id,
                BeatmapId = api.BeatmapId,
                TotalScore = api.TotalScore,
                Date = api.Date,
                UserId = api.UserId,
                Mode = api.Mode
            });
        
        // Act
        await _dataProcessor.ProcessScoresAsync(data, CancellationToken.None);
        
        // Assert
        ulong[] deletedIds = [1, 2, 3];
        _scoreRepository.Verify(r => r.DeleteBulk(It.Is<IEnumerable<Score>>(dtos =>
            dtos.All(d => deletedIds.Contains(d.Id)))), Times.Exactly(2));
        _scoreRepository.Verify(r => r.DeleteBulk(It.Is<IEnumerable<Score>>(dtos =>
            dtos.Any(d => d.Id == 4))), Times.Never);
    } 
}