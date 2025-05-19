using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WrldBxScript
{
    public class TerraformCodeGenerator : ICodeGenerator
    {
        private readonly Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> _repositories;

        // Constructor that accepts repositories
        public TerraformCodeGenerator(Dictionary<string, WrldBxObjectRepository<IWrldBxObject>> repositories)
        {
            _repositories = repositories;
        }
        public void GenerateCode(StringBuilder src, string modname)
        {
            src.AppendLine("\tpublic static void init() \n\t{");

            // Add effects-specific generation logic here
            foreach (WrldBxTerraform terraformOption in _repositories["TERRAFORMING"].GetAll.Cast<WrldBxTerraform>())
            {
                AddBlockId(src, terraformOption.id);
                if (terraformOption.explode_strength.HasValue && terraformOption.explode_tile == false)
                {
                    terraformOption.explode_tile = true;
                }
                if (!terraformOption.explode_strength.HasValue && terraformOption.explode_tile == true)
                {
                    WrldBxScript.Warning($"You did not set explode_strength it was given a default value of 1", terraformOption);
                }
                src.Append(
                    $"flash = {terraformOption.flash}," +
                    $"explode_tile = {terraformOption.explode_tile}," +
                    $"applyForce = {terraformOption.applyForce}," +
                    $"force_power = {terraformOption.force_power}," +
                    $"explode_strength = {terraformOption.explode_strength}," +
                    $"damageBuildings = {terraformOption.damageBuildings.ToString().ToLower()}," +
                    $"setFire = {terraformOption.setFire}," +
                    $"addBurned = {terraformOption.addBurned}," +
                    $"shake_intensity = 1f," +
                    $"damage = {terraformOption.damage},"

                    );
                AddReqCodeToBlock(src, terraformOption.id);
            }
            src.Append("\n\t\t}\n\t}\n}");
        }

        public void AddBlockId(StringBuilder src, object name)
        {
            src.Append("AssetManager.terraform.add(new TerraformOptions\n\t{");
            src.Append($"\t\t\nid = {InQuotes(name.ToString())},");
        }

        public void AddReqCodeToBlock(StringBuilder src, object name, string appendage = null)
        {
            src.Append("\t\t\n});");
        }

        private string InQuotes(string str) => $"\"{str}\"";
    }
}
