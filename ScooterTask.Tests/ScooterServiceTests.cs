using ScooterTask.Exceptions;

namespace ScooterTask.Tests;

public class ScooterServiceTests
{
    private ScooterService _scooterService;
    
    [SetUp]
    public void Setup()
    {
        _scooterService = new ScooterService();
    }

    [Test]
    public void AddScooter_InvalidId_ThrowsInvalidIdException()
    {
        Assert.Throws<InvalidIdException>(() => _scooterService.AddScooter("", 0));
    }
    
    [Test]
    public void AddScooter_IdNull_ThrowsInvalidIdException()
    {
        Assert.Throws<InvalidIdException>(() => _scooterService.AddScooter(null, 0));
    }
    
    [Test]
    public void AddScooter_PriceNegative_ThrowsInvalidPriceException()
    {
        Assert.Throws<InvalidPriceException>(() => _scooterService.AddScooter("1", -1m));
    }
    
    [Test]
    public void RemoveScooter_InvalidId_ThrowsInvalidIdException()
    {
        Assert.Throws<InvalidIdException>(() => _scooterService.RemoveScooter(""));
    }
    
    [Test]
    public void RemoveScooter_IdNull_ThrowsInvalidIdException()
    {
        Assert.Throws<InvalidIdException>(() => _scooterService.RemoveScooter(null));
    }

    [Test]
    public void GetScooters_List_ShouldReturnList()
    {
        var asIs = _scooterService.GetScooters();
        Assert.That(asIs, Is.EqualTo(new List<Scooter>()));
    }
    
    [Test]
    public void GetScooterById_InvalidId_ThrowsInvalidIdException()
    {
        Assert.Throws<InvalidIdException>(() => _scooterService.GetScooterById(""));
    }
    
    [Test]
    public void GetScooterById_IdNull_ThrowsInvalidIdException()
    {
        Assert.Throws<InvalidIdException>(() => _scooterService.GetScooterById(null));
    }

    [Test]
    public void AddScooter_ValidData_IncreasesListSizeByOne()
    {
        _scooterService.AddScooter("1", 1m);
        Assert.That(_scooterService.GetScooters(), Has.Count.EqualTo(1));
    }

    [Test]
    public void GetScooterById_AddScooter_ReturnsCorrectScooter()
    {
        _scooterService.AddScooter("1", 1.5m);
        Assert.Multiple(() =>
        {
            Assert.That(_scooterService.GetScooterById("1").Id, Is.EqualTo("1"));
            Assert.That(_scooterService.GetScooterById("1").PricePerMinute, Is.EqualTo(1.5m));
        });
    }
    
    [Test]
    public void GetScooterById_DifferentId_ThrowsIdNotFoundException()
    {
        _scooterService.AddScooter("1", 1.5m);
        Assert.Throws<IdNotFoundException>(() => _scooterService.GetScooterById("2"));
    }
    
    [Test]
    public void RemoveScooter_DifferentId_ThrowsIdNotFoundException()
    {
        _scooterService.AddScooter("1", 1m);
        Assert.Throws<IdNotFoundException>(() => _scooterService.RemoveScooter("2"));
    }
    
    [Test]
    public void RemoveScooter_CorrectId_DecreasesScootersCount()
    {
        _scooterService.AddScooter("1", 1m);
        _scooterService.RemoveScooter("1");
        Assert.That(_scooterService.GetScooters(), Has.Count.EqualTo(0));
    }
}