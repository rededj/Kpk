public class UpgradeOption
{
    public string Name { get; set; }
    public int Count { get; set; } = 0;

    public UpgradeOption(string name)
    {
        Name = name;
    }

    public bool CanBeSelected()
    {
        return Count < 5; 
    }
}