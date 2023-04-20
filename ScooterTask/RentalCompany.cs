using ScooterTask.Exceptions;

namespace ScooterTask;

public class RentalCompany : IRentalCompany
{
    public string Name { get; set; }
    private ScooterService _scooterService;
    private Dictionary<DateTime, decimal> _journal;

    public RentalCompany(string name, ScooterService scooterService)
    {
        Name = name;
        _scooterService = scooterService;
        _journal = new Dictionary<DateTime, decimal>();
    }

    public void StartRent(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new InvalidIdException();
        }

        var scooter = _scooterService.GetScooterById(id);
        if (scooter.IsRented)
        {
            throw new ScooterIsUnavailableException();
        }

        scooter.IsRented = true;
        scooter.RentStart = DateTime.Now;
    }

    public decimal EndRent(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new InvalidIdException();
        }

        var scooter = _scooterService.GetScooterById(id);
        if (!scooter.IsRented)
        {
            throw new ScooterIsNotInUseException();
        }

        scooter.IsRented = false;
        scooter.RentEnd = DateTime.Now;
        var totalAmount = CalculatePrice(scooter);
        _journal.TryAdd(scooter.RentEnd, totalAmount);
        return totalAmount;
    }

    public decimal CalculateIncome(int? year, bool includeNotCompletedRentals)
    {
        var totalIncome = 0m;
        if (year == null)
        {
            return _journal.Values.Sum();
        }

        foreach (var key in _journal.Keys.Where(key => key.Year == year))
        {
            if (_journal.TryGetValue(key, out var value))
            {
                totalIncome += value;
            }
        }

        if (!includeNotCompletedRentals)
        {
            return totalIncome;
        }

        var scooters = _scooterService.GetScooters();
        totalIncome += scooters.Where(x => x.IsRented).Sum(scooter =>
            (DateTime.Now.Minute - scooter.RentStart.Minute + 60 * (DateTime.Now.Hour - scooter.RentStart.Hour)) *
            scooter.PricePerMinute);

        return totalIncome;
    }

    private decimal CalculatePrice(Scooter scooter)
    {
        var totalAmount = 0m;

        if (scooter.RentEnd.Day == scooter.RentStart.Day)
        {
            totalAmount += (60 * (scooter.RentEnd.Hour - scooter.RentStart.Hour) + scooter.RentEnd.Minute -
                            scooter.RentStart.Minute) *
                scooter.PricePerMinute >= 20
                    ? 20
                    : (60 * (scooter.RentEnd.Hour - scooter.RentStart.Hour) + scooter.RentEnd.Minute -
                       scooter.RentStart.Minute) * scooter.PricePerMinute;
            return totalAmount;
        }

        for (var date = scooter.RentStart; date <= scooter.RentEnd; date = date.AddMinutes(60 * (23 - date.Hour) + 60 - date.Minute))
        {
            if (date.Day == scooter.RentEnd.Day)
            {
                totalAmount += (60 * (scooter.RentEnd.Hour) + scooter.RentEnd.Minute) *
                    scooter.PricePerMinute >= 20
                        ? 20
                        : (60 * (scooter.RentEnd.Hour) + scooter.RentEnd.Minute) * scooter.PricePerMinute;
            }
            else
            {
                totalAmount += (60 * (23 - date.Hour) + 60 - date.Minute) * scooter.PricePerMinute >= 20
                    ? 20
                    : (60 * (23 - date.Hour) + 60 - date.Minute) * scooter.PricePerMinute;
            }
        }

        return totalAmount;
    }
}