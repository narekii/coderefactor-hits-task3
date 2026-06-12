namespace AutoServiceApp.Storage;

public interface IDataProvider<T>
{
    List<T> Load(string name);
    void Save(string name, List<T> values);
}
