using System.Collections.Generic;

public class BuildCategory
{
    public BuildType Category;
    public List<BuildableDefinition> Definitions = new();
}