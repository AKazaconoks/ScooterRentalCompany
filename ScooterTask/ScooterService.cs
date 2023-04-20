using ScooterTask.Exceptions;

namespace ScooterTask;

public class ScooterService : IScooterService
{
    private List<Scooter> _scooters;

    public ScooterService()
    {
        _scooters = new List<Scooter>();
    }

    public void AddScooter(string id, decimal pricePerMinute)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new InvalidIdException();
        }

        if (pricePerMinute < 0)
        {
            throw new InvalidPriceException();
        }

        _scooters.Add(new Scooter(id, pricePerMinute));
    }

    public void RemoveScooter(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new InvalidIdException();
        }

        var scooter = _scooters.FirstOrDefault(s => s.Id == id);
        if (scooter == null)
        {
            throw new IdNotFoundException();
        }

        _scooters.Remove(scooter);
    }

    public IList<Scooter> GetScooters()
    {
        return _scooters;
    }

    public Scooter GetScooterById(string scooterId)
    {
        if (string.IsNullOrEmpty(scooterId))
        {
            throw new InvalidIdException();
        }

        var scooter = _scooters.FirstOrDefault(s => s.Id == scooterId);
        if (scooter == null)
        {
            throw new IdNotFoundException();
        }

        return scooter;
    }
}