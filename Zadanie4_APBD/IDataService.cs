namespace Zadanie4_APBD;

public interface IDataService
{
    public Task<int> InsertProduct(Order order);

    public void Insert(Order order);
}