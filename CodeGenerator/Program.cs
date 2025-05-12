using CodeGenerator;

var charset = "ACDEFGHKLMNPRTXYZ234579";// 8 characters → ~36 bits
var keyGeneratorHelper = new CodeGeneratorHelper(charset);
var createdKeys = keyGeneratorHelper.GenerateKeys(10000);
for (int i = 0; i < createdKeys.Count; i++)
{
    if (keyGeneratorHelper.ValidateKey(createdKeys[i])) continue;
    else throw new Exception($"Key {i + 1}: {createdKeys[i]} is invalid. Validation failed.");
}
Console.WriteLine("All keys are valid.");