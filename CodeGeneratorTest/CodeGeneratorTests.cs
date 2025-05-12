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
    public void RandomlyGeneratedCodes_ShouldRarelyBeValid(int count)
    {
        var random = new Random();
        int validCount = 0;

        for (int i = 0; i < count; i++)
        {
            string code = string.Empty;
            for (int j = 0; j < 8; j++)
            {
                int index = random.Next(Charset.Length);
                code += Charset[index];
            }

            if (_codeGeneratorHelper.ValidateKey(code))
            {
                validCount++;
            }
        }

        double validRate = (double)validCount / count;

        // Allow extremely low false positives due to hash collisions
        Assert.True(validRate < 0.00001, $"Too many valid codes from random: {validRate:P6}");
    }

}

