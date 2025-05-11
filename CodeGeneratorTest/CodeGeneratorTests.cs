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
    [InlineData(1_000)]
    [InlineData(1_000_000)]
    [InlineData(10_000_000)]
    [InlineData(15_000_000)]
    public void GenerateSeeds_ShouldReturnUniqueSeeds(int numberOfSeeds)
    {
        var generetedSeeds = _codeGeneratorHelper.GenerateUniqueSeeds(numberOfSeeds);
        var seeds = new HashSet<ulong>(generetedSeeds);
        Assert.Equal(numberOfSeeds, seeds.Count);
    }


    [Theory]
    [InlineData(1)]
    [InlineData(1_000)]
    [InlineData(1_000_000)]
    [InlineData(10_000_000)]
    [InlineData(15_000_000)]
    public void GenerateKeys_ShouldReturnCorrectCount(int count)
    {
        // Act
        var keys = _codeGeneratorHelper.GenerateKeys(count);
        // Assert
        Assert.Equal(count, keys.Distinct().Count());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1_000)]
    [InlineData(1_000_000)]
    [InlineData(10_000_000)]
    [InlineData(15_000_000)]
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
    [InlineData(1_000)]
    [InlineData(1_000_000)]
    [InlineData(10_000_000)]
    [InlineData(15_000_000)]
    public void GenerateKeys_ShouldNotHaveCollision(int count)
    {
        Random random = new Random();
        var bruteForceKeys = new List<string>();
        for (int i = 0; i < count; i++)
        {
            string code = string.Empty;
            for (int j = 0; j < 8; j++)
            {
                int randomIndex = random.Next(Charset.Length);
                code += Charset[randomIndex];
            }
            bruteForceKeys.Add(code);
        }

        var keys = _codeGeneratorHelper.GenerateKeys(count);
        var inculudedKeys = keys.Intersect(bruteForceKeys).ToList();
        if (inculudedKeys.Count > 0)
        {
            Console.WriteLine($"Collision found: {string.Join(", ", inculudedKeys)}");
        }
        var rateCollision = (double)(inculudedKeys.Count * 100) / count;
        Assert.True(rateCollision <= 0.02, $"Collision rate is too high: {rateCollision}");
    }
    [Theory]
    [InlineData(1)]
    [InlineData(1_000)]
    [InlineData(1_000_000)]
    [InlineData(10_000_000)]
    [InlineData(15_000_000)]
    public void GenerateRandomKeys_ShouldNotHaveCollision(int count)
    {
        Random random = new Random();
        int collisionCount = 0;
        for (int i = 0; i < count; i++)
        {
            string code = string.Empty;
            for (int j = 0; j < 8; j++)
            {
                int randomIndex = random.Next(Charset.Length);
                code += Charset[randomIndex];
            }
            if (_codeGeneratorHelper.ValidateKey(code))
            {
                collisionCount++;
            }
        }
        double collisionRate = (ulong)collisionCount / (ulong)count;
        double expectedProbability = CalculateCollisionProbability(count);
        Assert.True(collisionRate >= expectedProbability, $"Çakışma oranı beklenenden yüksek: {collisionRate:P6}");
    }
    [Theory]
    [InlineData(1)]
    [InlineData(1_000)]
    [InlineData(1_000_000)]
    [InlineData(10_000_000)]
    [InlineData(15_000_000)]
    public void GenerateKeys_ShouldNotHaveCollision_Theoretical(int count)
    {
        Random random = new Random();
        int collisionCount = 0;
        for (int i = 0; i < count; i++)
        {
            string code = string.Empty;
            for (int j = 0; j < 8; j++)
            {
                int randomIndex = random.Next(Charset.Length);
                code += Charset[randomIndex];
            }
            if (_codeGeneratorHelper.ValidateKey(code))
            {
                collisionCount++;
            }
        }
        double collisionRate = (ulong)collisionCount / (ulong)count;
        double expectedProbability = CalculateCollisionProbability(count);
        Console.WriteLine($"Gerçek çakışma oranı: {collisionRate:P6}, Teorik: {expectedProbability:P6}");

        Assert.True(collisionRate >= expectedProbability, $"Çakışma oranı beklenenden yüksek: {collisionRate:P6}");
    }
    private static double CalculateCollisionProbability(int codeCount) => (ulong)codeCount / (ulong)Math.Pow(23, 8);// will be 78,364,164,096
}

