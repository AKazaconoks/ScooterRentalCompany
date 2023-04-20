using System.Reflection;
using Moq;
using Moq.AutoMock;
using NodaTime;
using NodaTime.Testing;
using ScooterTask.Exceptions;

namespace ScooterTask.Tests;

public class RentalCompanyTests
{
    private RentalCompany _rentalCompany;
    private ScooterService _scooterService;
    private Mock _rentalCompanyMock;
    private FieldInfo _journalField;


    [SetUp]
    public void Setup()
    {
        _scooterService = new ScooterService();
        _rentalCompany = new RentalCompany("Test", _scooterService);
        _rentalCompanyMock = new Mock<RentalCompany>("Test", _scooterService);
        _journalField = typeof(RentalCompany).GetField("_journal", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    [Test]
    public void StartRent_InvalidId_ThrowsInvalidIdException()
    {
        Assert.Throws<InvalidIdException>(() => _rentalCompany.StartRent(""));
    }

    [Test]
    public void StartRent_IdNull_ThrowsInvalidIdException()
    {
        Assert.Throws<InvalidIdException>(() => _rentalCompany.StartRent(null));
    }

    [Test]
    public void EndRent_InvalidId_ThrowsInvalidIdException()
    {
        Assert.Throws<InvalidIdException>(() => _rentalCompany.EndRent(""));
    }

    [Test]
    public void EndRent_IdNull_ThrowsInvalidIdException()
    {
        Assert.Throws<InvalidIdException>(() => _rentalCompany.EndRent(null));
    }

    [Test]
    public void StartRent_CorrectId_ChangesScooterIsRented()
    {
        _scooterService.AddScooter("1", 1m);
        _rentalCompany.StartRent("1");
        Assert.That(_scooterService.GetScooterById("1").IsRented, Is.True);
    }

    [Test]
    public void StartRent_RentedScooter_ThrowsScooterIsUnavailableException()
    {
        _scooterService.AddScooter("1", 1m);
        _rentalCompany.StartRent("1");
        Assert.Throws<ScooterIsUnavailableException>(() => _rentalCompany.StartRent("1"));
    }

    [Test]
    public void EndRent_NotRentedScooter_ThrowsScooterIsNotInUseException()
    {
        _scooterService.AddScooter("1", 1m);
        Assert.Throws<ScooterIsNotInUseException>(() => _rentalCompany.EndRent("1"));
    }
    
    [Test]
    public void EndRent_CorrectId_ChangesScooterIsRented()
    {
        _scooterService.AddScooter("1", 1m);
        _rentalCompany.StartRent("1");
        _rentalCompany.EndRent("1");
        Assert.That(_scooterService.GetScooterById("1").IsRented, Is.False);
    }

    [Test]
    public void StartRent_Scooter_ShouldChangeRentStartToNow()
    {
        _scooterService.AddScooter("1", 1m);
        _rentalCompany.StartRent("1");
        Assert.That(_scooterService.GetScooterById("1").RentStart, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void EndRent_Scooter_ShouldChangeRentEndToNow()
    {
        _scooterService.AddScooter("1", 1m);
        _rentalCompany.StartRent("1");
        _rentalCompany.EndRent("1");
        Assert.That(_scooterService.GetScooterById("1").RentEnd, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void EndRent_Scooter_ReturnsAmountForRide()
    {
        _scooterService.AddScooter("1", 1m);
        _rentalCompany.StartRent("1");
        var amount = _rentalCompany.EndRent("1");
        var scooter = _scooterService.GetScooterById("1");
        var timeDifferenceInMinutes = scooter.RentEnd.Minute - scooter.RentEnd.Minute + 60 * (scooter.RentEnd.Hour - scooter.RentStart.Hour);
        Assert.That(amount, Is.EqualTo(timeDifferenceInMinutes * scooter.PricePerMinute));
    }

    [Test]
    public void EndRent_Amount_AddsRideAmountToJournal()
    {

        _journalField.SetValue(_rentalCompanyMock.Object, new Dictionary<DateTime, decimal>());

        var rentalCompany = (RentalCompany)_rentalCompanyMock.Object;
        var journal = (Dictionary<DateTime, decimal>)_journalField.GetValue(_rentalCompanyMock.Object)!;
        
        _scooterService.AddScooter("1", 1m);
        rentalCompany.StartRent("1");
        var amount = rentalCompany.EndRent("1");
        var scooter = _scooterService.GetScooterById("1");
        Assert.That(journal, Is.EqualTo(new Dictionary<DateTime, decimal>(){[scooter.RentEnd] = amount}));
    }

    [Test]
    public void CalculateIncome_NoIncome_ReturnsZero()
    {
        var amount = _rentalCompany.CalculateIncome(null, false);
        Assert.That(amount, Is.EqualTo(0));
    }

    [Test]
    public void CalculateIncome_WithIncome_ReturnsCorrectIncome()
    {
        _journalField.SetValue(_rentalCompanyMock.Object, new Dictionary<DateTime, decimal>()
        {
            [DateTime.Now.AddDays(-1)] = 10m,
            [DateTime.Now.AddHours(-1)] = 20m
        });
        var rentalCompany = (RentalCompany)_rentalCompanyMock.Object;
        var journal = (Dictionary<DateTime, decimal>)_journalField.GetValue(rentalCompany)!;
        Assert.That(rentalCompany.CalculateIncome(null, false), Is.EqualTo(30m));
    }
    
    [Test]
    public void CalculateIncome_WithIncomeAndYear_ReturnsCorrectIncome()
    {
        _journalField.SetValue(_rentalCompanyMock.Object, new Dictionary<DateTime, decimal>()
        {
            [DateTime.Now.AddDays(-1)] = 10m,
            [DateTime.Now.AddHours(-1)] = 20m,
            [DateTime.Now.AddYears(1)] = 30m
        });
        var rentalCompany = (RentalCompany)_rentalCompanyMock.Object;
        var journal = (Dictionary<DateTime, decimal>)_journalField.GetValue(rentalCompany)!;
        Assert.That(rentalCompany.CalculateIncome(2023, false), Is.EqualTo(30m));
    }
    
    [Test]
    public void CalculateIncome_WithIncomeAndYearAndScooterInUse_ReturnsCorrectIncome()
    {
        _journalField.SetValue(_rentalCompanyMock.Object, new Dictionary<DateTime, decimal>()
        {
            [DateTime.Now.AddDays(-1)] = 10m,
            [DateTime.Now.AddHours(-1)] = 20m,
            [DateTime.Now.AddYears(1)] = 30m
        });
        var rentalCompany = (RentalCompany)_rentalCompanyMock.Object;
        var journal = (Dictionary<DateTime, decimal>)_journalField.GetValue(rentalCompany)!;
        _scooterService.AddScooter("1", 1m);
        var scooter = _scooterService.GetScooterById("1");
        scooter.RentStart = DateTime.Now.AddMinutes(-10);
        scooter.IsRented = true;
        Assert.That(rentalCompany.CalculateIncome(2023, true), Is.EqualTo(40m));
    }
    
    [Test]
    public void CalculateIncome_WithIncomeAndYearAndNoScooterInUse_ReturnsCorrectIncome()
    {
        _journalField.SetValue(_rentalCompanyMock.Object, new Dictionary<DateTime, decimal>()
        {
            [DateTime.Now.AddDays(-1)] = 10m,
            [DateTime.Now.AddHours(-1)] = 20m,
            [DateTime.Now.AddYears(1)] = 30m
        });
        var rentalCompany = (RentalCompany)_rentalCompanyMock.Object;
        var journal = (Dictionary<DateTime, decimal>)_journalField.GetValue(rentalCompany)!;
        _scooterService.AddScooter("1", 1m);
        var scooter = _scooterService.GetScooterById("1");
        scooter.RentStart = DateTime.Now.AddMinutes(-10);
        scooter.IsRented = true;
        Assert.That(rentalCompany.CalculateIncome(2023, false), Is.EqualTo(30m));
    }

    [Test]
    public void EndRent_FewDays_ShouldCount20EuroPerDay()
    {
        _scooterService.AddScooter("1", 0.05m);
        var scooter = _scooterService.GetScooterById("1");
        scooter.RentStart = DateTime.Now.AddDays(-2);
        scooter.IsRented = true;

        var amount = _rentalCompany.EndRent("1");
        Assert.That(amount, Is.EqualTo(60m));
    }
    
    [Test]
    public void EndRent_SmallRent_ShouldCountCorrectPrice()
    {
        _scooterService.AddScooter("1", 0.02m);
        var scooter = _scooterService.GetScooterById("1");
        scooter.RentStart = DateTime.Now.AddMinutes(-10);
        scooter.RentStart = scooter.RentStart.AddHours(-1);
        scooter.IsRented = true;

        var amount = _rentalCompany.EndRent("1");
        Assert.That(amount, Is.EqualTo(1.4m));
    }
    
    [Test]
    public void EndRent_BigRentWithinADay_Returns20Euro()
    {
        _scooterService.AddScooter("1", 0.1m);
        var scooter = _scooterService.GetScooterById("1");
        scooter.RentStart = DateTime.Now.AddHours(-10);
        scooter.IsRented = true;

        var amount = _rentalCompany.EndRent("1");
        Assert.That(amount, Is.EqualTo(20m));
    }
    
    [Test]
    public void EndRent_DayBefore5ToMidnightRent_ReturnsCorrectAmount()
    {
        _scooterService.AddScooter("1", 0.1m);
        var scooter = _scooterService.GetScooterById("1");
        scooter.RentStart = DateTime.Today.AddMinutes(-5);
        scooter.IsRented = true;

        var amount = _rentalCompany.EndRent("1");
        Assert.That(amount, Is.EqualTo(20.5m));
    }
}