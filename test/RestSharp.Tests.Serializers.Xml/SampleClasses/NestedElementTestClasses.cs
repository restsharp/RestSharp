namespace RestSharp.Tests.Serializers.Xml.SampleClasses;

// Test classes for nested element bugs

public class Item {
    public int Id { get; set; }
    public List<Item> SubItems { get; set; }
}

public class ItemContainer {
    public List<Item> Items { get; set; } = new();
}

public class ItemWithGroup {
    public int Id { get; set; }
    public ItemGroup Group { get; set; }
}

public class ItemGroup {
    public List<Item> Items { get; set; }
}

public class ItemsResponse {
    public List<ItemWithGroup> Items { get; set; } = new();
}
