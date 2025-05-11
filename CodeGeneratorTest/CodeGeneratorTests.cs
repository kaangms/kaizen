using CodeGenerator;

namespace CodeGeneratorTest;

public class CodeGeneratorTests
{
    private const string Charset = "ACDEFGHKLMNPRTXYZ234579";
    private readonly CodeGeneratorHelper _codeGeneratorHelper;

    public CodeGeneratorTests()
    {
        _codeGeneratorHelper = new CodeGeneratorHelper(Charset);
    }
    [Theory]
    [InlineData(1)]
    [InlineData(1000)]
    [InlineData(1000000)]
    [InlineData(5000000)]
    [InlineData(10000000)]
    public void GenerateSeeds_ShouldReturnUniqueSeeds(int numberOfSeeds)
    {
        var generetedSeeds = _codeGeneratorHelper.GenerateUniqueSeeds(numberOfSeeds);
        var seeds = new HashSet<ulong>(generetedSeeds);
        Assert.Equal(numberOfSeeds, seeds.Count);
    }
   

    [Theory]
    [InlineData(1)]
    [InlineData(1000)]
    [InlineData(1000000)]
    [InlineData(5000000)]
    [InlineData(10000000)]
    public void GenerateKeys_ShouldReturnCorrectCount(int count)
    {
        // Act
        var keys = _codeGeneratorHelper.GenerateKeys(count);
        // Assert
        Assert.Equal(count, keys.Distinct().Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1000)]
    [InlineData(1000000)]
    [InlineData(5000000)]
    [InlineData(10000000)]
    public void GenerateKeys_ShouldReturnUniqueKeys(int count)
    {
        // Act
        var keys = _codeGeneratorHelper.GenerateKeys(count);
        for (int i = 0; i < keys.Count; i++)
        {
            if (_codeGeneratorHelper.ValidateKey(keys[i])) continue;
            else throw new Exception($"Key {i + 1}: {keys[i]} is invalid. Validation failed.");
        }
    }
    [Theory]
    [InlineData(1)]
    [InlineData(1000)]
    [InlineData(1000000)]
    [InlineData(15000000)]
    [InlineData(10000000)]
    public void GenerateKeys_ShouldReturnValidKeys(int count)
    {
         Random random = new Random();
        var bruteForceKeys=new List<string>();
        for (int i = 0; i < count; i++)
        {
            string code = string.Empty ;
            for (int j = 0; j < 8; j++)
            {
                int randomIndex = random.Next(Charset.Length);
                code += Charset[randomIndex];
            }
            bruteForceKeys.Add(code);
        }
       
        var keys = _codeGeneratorHelper.GenerateKeys(count);
        var inculudedKeys = keys.Intersect(bruteForceKeys).ToList();
        var rateCollision = (double)(inculudedKeys.Count *100) / count;
        Assert.True(rateCollision <= 0.02, $"Collision rate is too high: {rateCollision}");
       
    }
}
